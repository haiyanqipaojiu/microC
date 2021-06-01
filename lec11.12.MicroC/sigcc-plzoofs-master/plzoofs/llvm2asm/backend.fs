module Backend
(* ll ir compilation -------------------------------------------------------- *)
#nowarn "62"

open Ll
open X86


(* allocated llvmlite function bodies --------------------------------------- *)

(* Generating X86 assembly is tricky, and it helps to split the problem into
   two parts:

   1) Figuring out how to represent states of the LLVMlite machine as
      those of the X86lite machine, i.e. where should we store uid %x
      or global @foo, and

   2) Choosing the X86 instructions that will correspond to LLVMlite
      instructions while maintaining this relationship: For example,
      what sequence of X86 instructions will implement the
      "getelementptr" instruction, assuming that we know the arguments
      are in such and such X86 registers?

   To do this, we will introduce a slightly different representation
   of LLVMlite, where uids, globals, and labels have been replaced
   with their X86 counterparts.  Uids can be mapped onto X86
   registers, stack slots or, for instructions like "store" that do
   not assign to their uid, no storage. Additionally, since in
   LLVMlite labels and uids share a namespace, some uids correspond to
   code labels.

   Rather than working directly with the LLVMlite AST, we will be
   using a flattened "stream of instructions" representation like the
   one you saw in class. Once we have decided how we want to represent
   LLVMlite states in X86, we can convert programs to this
   representation. Then, we will have to choose X86 instructions to
   correspond to the "allocated" LLVMlite instruction stream.
*)
module Alloc =

    (* X86 locations *)
    type loc =
        | LVoid (* no storage *)
        | LReg of X86.reg
        | LStk of int (* a stack offset from %rbp *)
        | LLbl of X86.lbl

    type operand =
        | Null
        | Const of int64
        | Gid of X86.lbl
        | Loc of loc

    type insn =
        | ILbl
        | Binop of bop * ty * operand * operand
        | Alloca of ty
        | Load of ty * operand
        | Store of ty * operand * operand
        | Icmp of Ll.cnd * ty * operand * operand
        | Call of ty * operand * (ty * operand) list
        | Bitcast of ty * operand * ty
        | Gep of ty * operand * operand list
        | Ret of ty * operand option
        | Br of loc
        | Cbr of operand * loc * loc

    (* An allocated function body is just a flattened list of instructions,
    labels, and terminators. All uids, labels, and gids are replaced with the
    associated parts of the x86 machine *)
    type fbody = (loc * insn) list

    (* Converting between Ll function bodies and allocate function bodies given
    two functions
    f : uid -> loc
    g : gid -> X86.lbl *)
    let map_operand f g : Ll.operand -> operand =
        function
        | Ll.Null -> Null
        | Ll.Const i -> Const i
        | Ll.Gid x -> Gid(g x)
        | Ll.Id u -> Loc(f u)

    let map_insn f g : Ll.insn -> insn =
        let mo = map_operand f g in

        function
        | Ll.Binop (b, t, o, o') -> Binop(b, t, mo o, mo o')
        | Ll.Alloca t -> Alloca t
        | Ll.Load (t, o) -> Load(t, mo o)
        | Ll.Store (t, o, o') -> Store(t, mo o, mo o')
        | Ll.Icmp (c, t, o, o') -> Icmp(c, t, mo o, mo o')
        | Ll.Call (t, o, args) -> Call(t, mo o, List.map (fun (t, o) -> t, mo o) args)
        | Ll.Bitcast (t, o, t') -> Bitcast(t, mo o, t')
        | Ll.Gep (t, o, is) -> Gep(t, mo o, List.map mo is)

    let map_terminator f g : Ll.terminator -> insn =
        let mo = map_operand f g in

        function
        | Ll.Ret (t, None) -> Ret(t, None)
        | Ll.Ret (t, Some o) -> Ret(t, Some(mo o))
        | Ll.Br l -> Br(f l)
        | Ll.Cbr (o, l, l') -> Cbr(mo o, f l, f l')

    let of_block f g (b: Ll.block) : fbody =
        List.map (fun (u, i) -> f u, map_insn f g i) b.insns
        @ [ LVoid, map_terminator f g b.terminator ]

    let of_lbl_block f g ((l, b): (Ll.lbl * Ll.block)) =
        (LLbl(Platform.mangle l), ILbl) :: of_block f g b

    let of_cfg f g ((e, bs): Ll.cfg) =
        let ls =
            of_block f g e :: (List.map (of_lbl_block f g) bs)

        Seq.toList (
            seq {
                for r in ls do
                    yield! r
            }
        )


(* locals and layout -------------------------------------------------------- *)

(* One key problem in compiling the LLVM IR is how to map its local
   identifiers to X86 abstractions.  For the best performance, one
   would want to use an X86 register for each LLVM %uid that is assigned
   a value.  However, since there are an unlimited number of %uids and
   only 16 registers, doing so effectively is quite difficult. We will
   see later in the course how _register allocation_ algorithms can do a
   good job at this.

   A simpler, but less performant, implementation is to map each %uid
   in the LLVM source to a _stack slot_ (i.e. a region of memory in
   the stack). Since LLVMlite, unlike real LLVM, permits %uid locals
   to store only 64-bit data, each stack slot is an 8-byte value.

   [ NOTE: For compiling LLVMlite, even i1 data values should be
   represented as a 8-byte quad. This greatly simplifies code
   generation. ]

   We call the datastructure that maps each %uid to its stack slot a
   'stack layout'. A stack layout maps a uid to an Alloc.loc that represents
   where it will be stored. Recall that some uids identify instructions that
   do not assign a value, whereas others name code blocks. These are mapped to
   Alloc.LVoid, and Alloc.LLbl, respectively. For this compilation strategy,
   uids that are assigned values will always be assigned an offset from ebp
   (in bytes) that corresponds to a storage slot in the stack.
*)
type layout = (uid * Alloc.loc) list

type tdc = (tid * ty) list

(* Once we have a layout, it's simple to generate the allocated version of our
   LLVMlite program *)
let assoc x ls =
    //    printfn "%A" (x,ls)
    snd (List.find (fst >> ((=) x)) ls)

let alloc_cfg (layout: layout) (g: Ll.cfg) : Alloc.fbody =
    Alloc.of_cfg (fun x -> assoc x layout) (fun l -> Platform.mangle l) g

(* streams of x86 instructions ---------------------------------------------- *)

type x86elt =
    | I of X86.ins
    | L of (X86.lbl * bool)

type x86stream = x86elt list

let lift : X86.ins list -> x86stream = List.map (fun i -> I i) >> List.rev

let (>@) x y = y @ x
let (>+) x y = y :: x

let prog_of_x86stream : x86stream -> X86.prog =
    let rec loop p iis =
        function
        | [] ->
            (match iis with
             | [] -> p
             | _ -> failwith "stream has no initial label")
        | (I i) :: s' -> loop p (i :: iis) s'
        | (L (l, globals)) :: s' ->
            loop
                ({ lbl = l
                   globals = globals
                   asm = Text iis }
                 :: p)
                []
                s'

    loop [] []


(* compiling operands  ------------------------------------------------------ *)

(* LLVM IR instructions support several kinds of operands.

   LL local %uids live in stack slots, whereas global ids live at
   global addresses that must be computed from a label.  Constants are
   immediately available, and the operand Null is the 64-bit 0 value.

   You might find it useful to implement the following helper function,
   whose job is to generate the X86 operand corresponding to an allocated
   LLVMlite operand.
 *)

let compile_operand_base (b: X86.reg) : Alloc.operand -> X86.operand =
    function
    | Alloc.Null -> Imm(Lit 0L)
    | Alloc.Const c -> Imm(Lit c)
    | Alloc.Gid l -> Ind3(Lbl l, b)
    | Alloc.Loc l ->
        (match l with
         | Alloc.LReg r -> Reg r
         | Alloc.LStk s -> Ind3(Lit(int64 s), b)
         | Alloc.LLbl lb -> Imm(Lbl(Platform.mangle lb))
         | _ -> failwith "Cannot use this as an operand")

let compile_operand o = compile_operand_base Rbp o



(* compiling instructions  ------------------------------------------------- *)

(* | Addq | Subq | Imulq | Xorq | Orq | Andq
            | Shlq | Sarq | Shrq *)
let compile_bop : Ll.bop -> X86.opcode =
    function
    | Add -> Addq
    | Sub -> Subq
    | Mul -> Imulq
    | Shl -> Shlq
    | Lshr -> Shrq
    | Ashr -> Sarq
    | And -> Andq
    | Or -> Orq
    | Xor -> Xorq

let cmpl_binop (l: Alloc.loc) (b: bop) (t: ty) (op1: Alloc.operand) (op2: Alloc.operand) : X86.ins list =
    let x_op1 = compile_operand op1 in
    let x_op2 = compile_operand op2 in
    let dest = compile_operand (Alloc.Loc l) in
    let x_bop = compile_bop b in

    [ Movq, [ x_op1; Reg R10 ]
      x_bop, [ x_op2; Reg R10 ]
      Movq, [ Reg R10; dest ] ]


(* Helper function to compile LLVM CC to X86 CC *)
let cmpl_cnd : Ll.cnd -> X86.cnd =
    function
    | Ll.Eq -> Eq
    | Ll.Ne -> Neq
    | Ll.Sgt -> Gt
    | Ll.Sge -> Ge
    | Ll.Slt -> Lt
    | Ll.Sle -> Le

(* - Alloca: needs to return a pointer into the stack *)
let cmpl_alloca (l: Alloc.loc) (t: ty) : X86.ins list =
    let dest = compile_operand (Alloc.Loc l) in

    [ Pushq, [ Imm(Lit 0L) ]
      Movq, [ Reg Rsp; Reg R11 ]
      Movq, [ Reg R11; dest ] ]

let cmpl_load (l: Alloc.loc) (t: ty) (op: Alloc.operand) : X86.ins list =
    let dest = compile_operand (Alloc.Loc l) in

    (match op with
     | Alloc.Const _
     | Alloc.Null -> failwith "invalid pointers"
     | Alloc.Gid gl ->
         let x_op = compile_operand_base Rip op
         [ Movq, [ x_op; Reg R11 ] ]
     | Alloc.Loc lo ->
         let x_op = compile_operand (Alloc.Loc lo)

         [ Movq, [ x_op; Reg R10 ]
           Movq, [ Ind2 R10; Reg R11 ] ])
    @ [ Movq, [ Reg R11; dest ] ]

let cmpl_store (t: ty) (src: Alloc.operand) (dst_p: Alloc.operand) : X86.ins list =
    let x_src = compile_operand src in

    [ Movq, [ x_src; Reg R11 ] ]
    @ (match dst_p with
       | Alloc.Const _
       | Alloc.Null -> failwith "invalid pointers"
       | Alloc.Gid gl ->
           let x_dst_p = compile_operand_base Rip dst_p
           [ Movq, [ Reg R11; x_dst_p ] ]
       | Alloc.Loc lo ->
           let x_dst_p = compile_operand (Alloc.Loc lo)

           [ Movq, [ x_dst_p; Reg R10 ]
             Movq, [ Reg R11; Ind2 R10 ] ])

(* - Br should jump *)
let cmpl_br (l: Alloc.loc) : X86.ins list =
    let dest = compile_operand (Alloc.Loc l) in
    [ Jmp, [ dest ] ]

(* - Cbr branch should treat its operand as a boolean conditional
*)
let cmpl_cbr (op: Alloc.operand) (l1: Alloc.loc) (l2: Alloc.loc) : X86.ins list =
    let x_op = compile_operand op in
    let x_lbl1 = compile_operand (Alloc.Loc l1) in
    let x_lbl2 = compile_operand (Alloc.Loc l2) in

    [ Movq, [ Imm(Lit 0L); Reg R11 ]
      Movq, [ x_op; Reg R10 ]
      Cmpq, [ Reg R11; Reg R10 ]
      J Eq, [ x_lbl2 ]
      Jmp, [ x_lbl1 ] ]

(*  - Icmp:  the Set instruction may be of use.  Depending on how you
     compile Cbr, you may want to ensure that the value produced by
     Icmp is exactly 0 or 1.
  *)
let cmpl_icmp (l: Alloc.loc) (c: Ll.cnd) (t: ty) (op1: Alloc.operand) (op2: Alloc.operand) : X86.ins list =
    let cc = cmpl_cnd c in
    let dest = compile_operand (Alloc.Loc l) in
    let x_op1 = compile_operand op1 in
    let x_op2 = compile_operand op2 in

    [ Movq, [ x_op1; Reg R10 ]
      Movq, [ x_op2; Reg R11 ]
      Movq,
      [ Imm(Lit 0L)
        dest ] (* zero-init dest *)
      Cmpq, [ Reg R11; Reg R10 ]
      Set cc, [ dest ] ]


(* - Bitcast: does nothing interesting at the assembly level *)
let cmpl_bitcast (l: Alloc.loc) (t1: ty) (op: Alloc.operand) (t2: ty) : X86.ins list =
    let dest = compile_operand (Alloc.Loc l) in

    (match op with
     | Alloc.Gid g -> let x_op = compile_operand_base Rip op in [ Leaq, [ x_op; Reg R11 ] ]
     | _ -> let x_op = compile_operand op in [ Movq, [ x_op; Reg R11 ] ])
    @ [ Movq, [ Reg R11; dest ] ]

(*
- Ret should properly exit the function: freeing stack space,
     restoring the value of %rbp, and putting the return value (if
     any) in %rax.
*)
let cmpl_ret (t: ty) (op: Alloc.operand option) : X86.ins list =
    let i =
        (match op with
         | Some o ->
             let x_op = compile_operand o in
             [ Movq, [ x_op; Reg Rax ] ]
         | None -> [])

    i
    @ [ Movq, [ Reg Rbp; Reg Rsp ]
        Popq, [ Reg Rbp ]
        Retq, [] ]


let cmpl_ilbl l : x86elt list =
    (match l with
     | Alloc.LLbl lb -> [ L(lb, false) ]
     | _ -> failwith "don't know what to do with you")



(* compiling call  ---------------------------------------------------------- *)

(* You will probably find it helpful to implement a helper function that
   generates code for the LLVM IR call instruction.

   The code you generate should follow the x64 System V AMD64 ABI
   calling conventions, which places the first six 64-bit (or smaller)
   values in registers and pushes the rest onto the stack.  Note that,
   since all LLVM IR operands are 64-bit values, the first six
   operands will always be placed in registers.  (See the notes about
   compiling fdecl below.)

   [ NOTE: It is the caller's responsibility to clean up arguments
   pushed onto the stack, so you must free the stack space after the
   call returns. ]

   [ NOTE: Don't forget to preserve caller-save registers (only if
   needed). ]
*)


(* This helper function computes the location of the nth incoming
   function argument: either in a register or relative to %rbp,
   according to the calling conventions.  You might find it useful for
   compile_fdecl.

   [ NOTE: the first six arguments are numbered 0 .. 5 ]
*)
//TODO
let arg_loc_base (n: int) (r: X86.reg) : operand =
    if Platform.isWindows then
        (match n with
         | 0 -> Reg Rcx
         | 1 -> Reg Rdx
         | 2 -> Reg R08
         | 3 -> Reg R09
         | _ -> Ind3(Lit(int64 (8 * (n + 2))), r))
    else
        (match n with
         | 0 -> Reg Rdi
         | 1 -> Reg Rsi
         | 2 -> Reg Rdx
         | 3 -> Reg Rcx
         | 4 -> Reg R08
         | 5 -> Reg R09
         | _ -> Ind3(Lit(int64 (8 * (n - 4))), r))

let arg_loc (n: int) : operand = arg_loc_base n Rbp


let compile_call_helper (i: X86.ins list * int) (os: ty * Alloc.operand) : X86.ins list * int =
    let ins, count = i in
    let typ, op = os in

    let arg_ins =
        (match op with
         | Alloc.Gid g ->
             let x_op = compile_operand_base Rip op
             [ Leaq, [ x_op; Reg R10 ] ]
         | _ ->
             let x_op = compile_operand op
             [ Movq, [ x_op; Reg R10 ] ])


    let dest = arg_loc count

    let new_ins =
        if count < 6 then
            [ Movq, [ Reg R10; dest ] ]
        else
            [ Pushq, [ Reg R10 ] ]

    (ins @ arg_ins @ new_ins, count + 1)


let gen_arg_list (os: (ty * Alloc.operand) list) : (ty * Alloc.operand) list =
    (match os with
     | a1 :: a2 :: a3 :: a4 :: a5 :: a6 :: tl -> [ a1; a2; a3; a4; a5; a6 ] @ List.rev tl
     | _ -> os)


let compile_call (fo: Alloc.operand) (os: (ty * Alloc.operand) list) : x86stream =
    let args = gen_arg_list os

    let arg_ins, _ =
        List.fold compile_call_helper ([], 0) (args)

    let fn =
        (match fo with
         | Alloc.Gid g -> Imm(Lbl g)
         | _ -> failwith "wrong type")

    let num_args = int64 ((8 * List.length os))

    let call_ins =
        [ Callq, [ fn ] ]
        @ if num_args > 0L then
              [] (* [Addq, [Imm (Lit num_args); Reg Rsp]]  *)
          else
              []

    lift (arg_ins @ call_ins)



(* compiling getelementptr (gep)  ------------------------------------------- *)

(* The getelementptr instruction computes an address by indexing into
   a datastructure, following a path of offsets.  It computes the
   address based on the size of the data, which is dictated by the
   data's type.

   To compile getelmentptr, you must generate x86 code that performs
   the appropriate arithemetic calculations.
*)

(* [size_ty] maps an LLVMlite type to a size in bytes.
    (needed for getelementptr)

   - the size of a struct is the sum of the sizes of each component
   - the size of an array of t's with n elements is n * the size of t
   - all pointers, I1, and I64 are 8 bytes
   - the size of a named type is the size of its definition
   - Void, i8, and functions have undefined sizes according to LLVMlite.
     Your function should simply return 0 in those cases
*)
let rec size_ty tdecls t : int =
    (match t with
     | I1
     | I64
     | Ptr _ -> 8
     | Array (n, t_elm) -> n * (size_ty tdecls t_elm)
     | Struct ts -> List.fold (fun sum c -> sum + size_ty tdecls c) 0 ts
     | Namedt s -> size_ty tdecls (assoc s tdecls)
     | _ -> 0)

(* Generates code that computes a pointer value.

   1. o must be a pointer of type t=*t'

   2. the value of o is the base address of the calculation

   3. the first index in the path is treated as the index into an array
     of elements of type t' located at the base address

   4. subsequent indices are interpreted according to the type t':

     - if t' is a struct, the index must be a constant n and it
       picks out the n'th element of the struct. [ NOTE: the offset
       within the struct of the n'th element is determined by the
       sizes of the types of the previous elements ]

     - if t' is an array, the index can be any operand, and its
       value determines the offset within the array.

     - if t' is any other type, the path is invalid

     - make sure you can handle named types!

   5. if the index is valid, the remainder of the path is computed as
      in (4), but relative to the type f the sub-element picked out
      by the path so far
*)
open Ll

let idx_helper (ii: int * int * int * tdc) (el: Ll.ty) : (int * int * int * tdc) =
    let (n, i, acc, td) = ii

    if i < n then
        let new_size = size_ty td el
        n, i + 1, acc + new_size, td
    else
        n, i + 1, acc, td

let idx tdecls (c: int64) (t_lst: Ll.ty list) : int64 =
    let int_c = (int32 c)

    let _, _, acc, _ =
        List.fold idx_helper (int_c, 0, 0, tdecls) t_lst

    int64 acc

let rec gep_helper tdecls (t: Ll.ty) (path: Alloc.operand list) : X86.ins list =
    match path with
    | h :: tl ->
        let h_op = compile_operand_base Rbp h

        match t with
        | Struct st ->
            match h with
            | Alloc.Const c ->
                [ Addq, [ Imm(Lit(idx tdecls c st)); Reg Rcx ] ]
                @ (gep_helper tdecls (List.item (int c) st) tl)
            | _ -> failwith "cannot use this as an index"
        | Array (a, tp) ->
            let s = size_ty tdecls tp

            [ Movq, [ h_op; Reg R10 ]
              Imulq, [ Imm(Lit(int64 s)); Reg R10 ]
              Addq, [ Reg R10; Reg Rcx ] ]
            @ (gep_helper tdecls tp tl)
        | Namedt tp -> gep_helper tdecls (assoc tp tdecls) path
        | _ -> failwith "cannot calculate an offset with this type"
    | [] -> []


let compile_getelementptr tdecls (t: Ll.ty) (o: Alloc.operand) (os: Alloc.operand list) : x86stream =
    match t with
    | Ptr p ->
        let s = size_ty tdecls p

        let insns =
            match os with
            | h :: tl ->
                let h_op = compile_operand_base Rbp h in (* the index of t' *)

                [ Movq, [ Imm(Lit 0L); Reg Rcx ]
                  Movq, [ h_op; Reg R10 ]
                  Imulq, [ Imm(Lit(int64 s)); Reg R10 ]
                  Addq, [ Reg R10; Reg Rcx ] ]
                @ (gep_helper tdecls p tl)
            | [] -> []
            @ match o with
              | Alloc.Gid g ->
                  let baseadr = compile_operand_base Rip o
                  [ Leaq, [ baseadr; Reg R11 ] ]
              | _ ->
                  let baseadr = compile_operand_base Rbp o
                  [ Movq, [ baseadr; Reg R11 ] ]

        lift (
            insns
            @ (* Rcx contains the offset *) [ Addq, [ Reg Rcx; Reg R11 ] ]
        )
    | _ -> failwith "not a pointer"



let cmpl_gep tdecls (l: Alloc.loc) (t: ty) (op1: Alloc.operand) (opl: Alloc.operand list) : x86stream =
    let ins = compile_getelementptr tdecls t op1 opl
    let dest = compile_operand (Alloc.Loc l)
    lift ([ Movq, [ Reg R11; dest ] ]) @ ins

(* compiling instructions within function bodies ---------------------------- *)

(* An Alloc.fbody value is a list of LLVM lite labels, instructions,
   and terminators.  The compile_fbody function can process each of these
   in sequence, generating a corresponding stream of x86 instructions.

   The result of compiling a single LLVM instruction might be many x86
   instructions.  We have not determined the structure of this code
   for you. Some of the instructions require only a couple assembly
   instructions, while others require more.  We have suggested that
   you need at least compile_operand, compile_call, and compile_gep
   helpers; you may introduce more as you see fit.

   Here are a few tips:

   - The goal of this project is _not_ to produce efficient code. Emit
     extra moves liberally, using Rax and Rcx as scratch registers.
     You should aim for correctness first, making sure you don't
     violate restrictions of x86-64 assembly (e.g. the number of
     memory operands allowed for an instruction!)

   - The type of x86streams and their operations make appending to a
     stream efficient. You might find it useful to define a tail-
     recursive helper function that passes an output stream as an
     accumulator.

    type fbody = (loc * insn) list

    type loc =
    | LVoid                       (* no storage *)
    | LReg of X86.reg             (* x86 register *)
    | LStk of int                 (* a stack offset from %rbp *)
    | LLbl of X86.lbl             (* an assembler label *)
*)

let cmpl_call (l: Alloc.loc) (t: ty) (op: Alloc.operand) (args: (ty * Alloc.operand) list) : x86stream =
    let insns = compile_call op args in

    let pre =
        (match l with
         | Alloc.LVoid -> []
         | _ ->
             let dest = compile_operand (Alloc.Loc l)
             lift [ Movq, [ Reg Rax; dest ] ])

    pre @ insns




let compile_insn tdecls (l: Alloc.loc) (i: Alloc.insn) : x86stream =
    (match i with
     | Alloc.ILbl -> cmpl_ilbl l
     | Alloc.Binop (b, t, opr1, opr2) -> lift <| cmpl_binop l b t opr1 opr2
     | Alloc.Alloca t -> lift <| cmpl_alloca l t
     | Alloc.Load (t, opr) -> lift <| cmpl_load l t opr
     | Alloc.Store (t, opr1, opr2) -> lift <| cmpl_store t opr1 opr2
     | Alloc.Icmp (llcnd, t, opr1, opr2) -> lift <| cmpl_icmp l llcnd t opr1 opr2
     | Alloc.Call (t, opr, args) -> cmpl_call l t opr args
     | Alloc.Bitcast (t1, opr, t2) -> lift <| cmpl_bitcast l t1 opr t2
     | Alloc.Gep (t, opr1, opr_list) -> cmpl_gep tdecls l t opr1 opr_list
     | Alloc.Ret (t, opr_option) -> lift <| cmpl_ret t opr_option
     | Alloc.Br l -> lift <| cmpl_br l
     | Alloc.Cbr (opr, l1, l2) -> lift <| cmpl_cbr opr l1 l2)



let compile_body_helper
    (l: x86stream * (tid * ty) list)
    (el: Alloc.loc * Alloc.insn)
    : (x86stream * ((tid * ty) list)) =
    let _l, tdecls = l
    let lo, li = el
    (compile_insn tdecls lo li @ _l, tdecls)

let compile_fbody tdecls (af: Alloc.fbody) : x86stream =
    let insn, _ =
        List.fold compile_body_helper ([], tdecls) af

    insn

(* compile_fdecl ------------------------------------------------------------ *)

(* We suggest that you create a helper function that computes the
   layout for a given function declaration.

   - each function argument should be copied into a stack slot
   - in this (inefficient) compilation strategy, each local id
     is also stored as a stack slot.
   - uids associated with instructions that do not assign a value,
     such as Store and a Call of a Void function should be associated
     with Alloc.LVoid
   - LLVMlite uids and labels share a namespace. Block labels you encounter
     should be associated with Alloc.Llbl

*)


let layout_insn_classifier (m: layout * int) (l: uid * insn) : layout * int =
    let map, count = m
    let new_count = count - 8
    let u, i = l

    (match i with
     | Store (x, _, _) ->
         (match x with
          | _ -> (map @ [ (u, Alloc.LVoid) ], count))
     | Call (x, _, _) ->
         (match x with
          | Void -> (map @ [ (u, Alloc.LVoid) ], count)
          | _ -> (map @ [ (u, Alloc.LStk count) ], new_count))
     | _ -> (map @ [ (u, Alloc.LStk count) ], new_count))

let label_block_helper (m: layout * int) (b: lbl * block) : layout * int =
    let map, count = m
    let label, blk = b
    List.fold layout_insn_classifier (map @ [ (label, Alloc.LLbl label) ], count) blk.insns

let args_helper (u: uid) (m: layout * int * int) : layout * int * int =
    let map, count, arg_count = m

    if count > 6 then
        (map @ [ (u, Alloc.LStk(8 * (count - 5))) ], count - 1, arg_count)
    else (* first 6 args *)
        let mul = (-8 * (arg_count - count + 1))
        (map @ [ (u, Alloc.LStk mul) ], count - 1, arg_count)

let stack_layout (f: Ll.fdecl) : layout =
    let entry_blk, lbld_blks = f.cfg
    let args_count = List.length f.param

    let map_w_args, _, _ =
        List.foldBack args_helper f.param ([], args_count, args_count)

    let map_w_locals, c =
        List.fold layout_insn_classifier (map_w_args, -8 * (args_count + 1)) entry_blk.insns

    let final_map, _ =
        List.fold label_block_helper (map_w_locals, c) lbld_blks

    final_map

(* The code for the entry-point of a function must do several things:

   - since our simple compiler maps local %uids to stack slots,
     compiling the control-flow-graph body of an fdecl requires us to
     compute the layout (see the discussion of locals and layout). Use
     the provided alloc_cfg function to produce an allocated function
     body.

   - the function code should also comply with the calling
     conventions, typically by moving arguments out of the parameter
     registers (or stack slots) into local storage space.  For our
     simple compilation strategy, that local storage space should be
     in the stack. (So the function parameters can also be accounted
     for in the layout.)

   - the function entry code should allocate the stack storage needed
     to hold all of the local stack slots.
*)

let push_helper (l: X86.ins list * int) (u: uid) : (X86.ins list * int) =
    let insns, count = l
    let new_ins = [ (Pushq, [ arg_loc_base count Rbp ]) ]
    (new_ins @ insns, count + 1)

let gen_push_args_to_stack (arg_list: uid list) : X86.ins list =
    let push_insns, _ = List.fold push_helper ([], 0) arg_list
    push_insns


let count_helper (c: int) (el: uid * Alloc.loc) : int =
    let u, l = el in

    (match l with
     | Alloc.LStk s -> c + 1
     | _ -> c)

let count_local_variables (c: Ll.cfg) : int =
    let entry_blk, lbld_blks = c

    let map, _ =
        List.fold layout_insn_classifier ([], 0) entry_blk.insns

    let final_map, _ =
        List.fold label_block_helper (map, 0) lbld_blks

    List.fold count_helper 0 final_map

let generate_prologue (f: Ll.fdecl) : X86.ins list =
    let arg_list = f.param
    let num_vars = count_local_variables f.cfg

    [ Pushq, [ Reg Rbp ]
      Movq, [ Reg Rsp; Reg Rbp ] ]
    @ gen_push_args_to_stack arg_list
      @ if num_vars > 0 then
            [ Subq,
              [ Imm(Lit(int64 (8 * (num_vars))))
                Reg Rsp ] ]
        else
            []

let compile_fdecl tdecls (g: gid) (f: Ll.fdecl) : x86stream =
    let l = stack_layout f
    let prologue = generate_prologue f
    let fbody = alloc_cfg l f.cfg
    let body_insn = compile_fbody tdecls fbody

    body_insn
    @ (lift prologue) @ [ L(Platform.mangle g, true) ]

(* compile_gdecl ------------------------------------------------------------ *)

(* Compile a global value into an X86 global data declaration and map
   a global uid to its associated X86 label.
*)

let flatten ls =
    Seq.toList (
        seq {
            for r in ls do
                yield! r
        }
    )

let rec compile_ginit =
    function
    | GNull -> [ Quad(Lit 0L) ]
    | GGid gid -> [ Quad(Lbl(Platform.mangle gid)) ]
    | GInt c -> [ Quad(Lit c) ]
    | GString s -> [ Asciz s ]
    | GArray gs
    | GStruct gs -> List.map compile_gdecl gs |> flatten

and compile_gdecl (_, g) = compile_ginit g

(* compile_prog ------------------------------------------------------------- *)

let compile_prog
    { tdecls = tdecls
      gdecls = gdecls
      fdecls = fdecls }
    : X86.prog =

    let g =
        fun (lbl, gdecl) -> Asm.data (Platform.mangle lbl) (compile_gdecl gdecl)

    let f =
        fun (name, fdecl) -> prog_of_x86stream (compile_fdecl tdecls name fdecl) in

    (List.map g gdecls)
    @ (List.map f fdecls |> flatten)
