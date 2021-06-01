(* File Assembly/X64.fs

   Instructions and assembly code emission for a x86 machine.
   sestoft@itu.dk * 2017-05-01

   We use some aspects of Niels Kokholm's SML version (March 2002).

   This compiler takes a less template-based approach closer to the
   x86 spirit:

   * We use 32 bit (aka double word) protected mode code.

   * Expressions are compiled to register-based code without use of
     the stack.

   * All local variables and parameters are stored in the stack.

   * All function arguments are passed on the stack.

   * There is no optimized register allocation across expressions and statements. 

   * We use all 32-bit registers of the x86-64 architecture.  

   * We use the native x86 call and ret instructions, which means that
     we must pust some prologue code at each function start to obey
     the calling conventions of the abstract machine.  This is the
     most important reason for splitting labels into ordinary labels
     and function entry point labels.  *)

module X64

(* The MacOS and Windows linkers expect an underscore (_) before
   external and global names, whereas the Linux/gcc linker does not. *)

// open System.Runtime.InteropServices
// let isLinux  = 
//     RuntimeInformation.IsOSPlatform(OSPlatform.Linux)



let printi    = "printi"
let printc    = "printc"
let checkargc = "checkargc"
let asm_main  = "asm_main"

type label = string

type flabel = string

type reg64 =
    | Rax | Rcx | Rdx | Rbx | Rsi | Rdi | Rsp | Rbp

type rand =
    | Cst of int                        (* immrdiate qword n               *)
    | Reg of reg64                      (* register rbx                    *)
    | Ind of reg64                      (* register indirect [rbx]         *)
    | RbpOff of int                     (* rbp offset indirect [rbp - n]   *)
    | Glovars                           (* stackbase [glovars]             *)

type x86 =
    | Label of label                    (* symbolic label; pseudo-instruc. *)
    | FLabel of flabel * int            (* function label, arity; pseudo.  *)
    | Ins of string                     (* eg. sub rsp, 4                  *)
    | Ins1 of string * rand             (* eg. push rax                    *)
    | Ins2 of string * rand * rand      (* eg. add rax, [rbp - 32]         *)
    | Jump of string * label            (* eg. jz near lab                 *)
    | PRINTI                            (* print [rsp] as integer          *)
    | PRINTC                            (* print [rsp] as character        *)

let fromReg reg =
    match reg with
    | Rax  -> "rax"
    | Rcx  -> "rcx"
    | Rdx  -> "rdx"
    | Rbx  -> "rbx"
    | Rsi  -> "rsi"
    | Rdi  -> "rdi"
    | Rsp  -> "rsp"
    | Rbp  -> "rbp"

let operand rand : string =
    match rand with
        | Cst n    -> string n
        | Reg reg  -> fromReg reg
        | Ind reg  -> "[" + fromReg reg + "]"
        | RbpOff n -> "[rbp - " + string n + "]"
        | Glovars  -> "[glovars]"

(* The five registers that can be used for temporary values in i386.
Allowing EDX requires special handling across IMUL and IDIV *)

let temporaries =
    [Rcx; Rdx; Rbx; Rsi; Rdi]

let mem x xs = List.exists (fun y -> x=y) xs

let getTemp pres : reg64 option =
    let rec aux available =
        match available with
            | []          -> None
            | reg :: rest -> if mem reg pres then aux rest else Some reg
    aux temporaries

(* Get temporary register not in pres; throw exception if none available *)

let getTempFor (pres : reg64 list) : reg64 =
    match getTemp pres with
    | None     -> failwith "no more registers, expression too complex"
    | Some reg -> reg

let pushAndPop reg code = [Ins1("push", Reg reg)] @ code @ [Ins1("pop", Reg reg)]

(* Preserve reg across code, on the stack if necessary *)

let preserve reg pres code =
    if mem reg pres then
       pushAndPop reg code
    else
        code

(* Preserve all live registers around code, eg a function call *)

let rec preserveAll pres code =
    match pres with
    | []          -> code
    | reg :: rest -> preserveAll rest (pushAndPop reg code)

(* Generate new distinct labels *)

let (resetLabels, newLabel) = 
    let lastlab = ref -1
    ((fun () -> lastlab := 0), (fun () -> (lastlab := 1 + !lastlab; "L" + string(!lastlab))))

(* Convert one bytecode instr into x86 instructions in text form and pass to out *)

let x86instr2int out instr =
    let outlab lab = out (lab + ":\t\t\t\t;Label\n")
    let outins ins = out ("\t" + ins + "\n")
    match instr with
      | Label lab -> outlab lab
      | FLabel (lab, n)  -> out (lab + ":\t\t\t\t;start set up frame\n" +
                                 "\tpop rax\t\t\t; retaddr\n" +
                                 "\tpop rbx\t\t\t; oldbp\n" +
                                 "\tsub rsp, 8\n" +
                                 "\tmov rsi, rsp\n" +
                                 "\tmov rbp, rsp\n" +
                                 "\tadd rbp, " + string(8*n) + "\t\t; 8*arity\n" +
                                 lab + "_pro_1:\t\t\t; slide arguments\n" +
                                 "\tcmp rbp, rsi\n" +
                                 "\tjz " + lab + "_pro_2\n" +
                                 "\tmov rcx, [rsi+16]\n" +
                                 "\tmov [rsi], rcx\n" +
                                 "\tadd rsi, 8\n" +
                                 "\tjmp " + lab + "_pro_1\n" +
                                 lab + "_pro_2:\n" +
                                 "\tsub rbp, 8\n" +
                                 "\tmov [rbp+16], rax\n" +
                                 "\tmov [rbp+8], rbx\n" +
                                 lab + "_tc:\t;end set up frame\n")
      | Ins ins               -> outins ins
      | Ins1 (ins, op1)       -> outins (ins + " " + operand op1)
      | Ins2 (ins, op1, op2)  -> outins (ins + " " + operand op1 + ", " + operand op2)
      | Jump (ins, lab)       -> outins (ins + " near " + lab)
      | PRINTI         -> List.iter outins [ "call " + printi]
      | PRINTC         -> List.iter outins [ "call " + printc]

(* Convert instruction list to list of assembly code fragments *)
 
let code2x86asm (code : x86 list) : string list =
    let bytecode = ref []
    let outinstr i   = (bytecode := i :: !bytecode)
    List.iter (x86instr2int outinstr) code;
    List.rev (!bytecode)

let stdheader = "EXTERN " + printi + "\n" +
                "EXTERN " + printc + "\n" +
                "EXTERN " + checkargc + "\n" +
                "GLOBAL " + asm_main + "\n" +
                "section .data\n" +
                "\tglovars dd 0\n" +
                "section .text\n"

let beforeinit argc = asm_main + ":\n" +
                      "\tpush rbp\n" +
                      "\tmov rbp, rsp\n" +
                      "\tmov qword [glovars], rsp\n" +
                      "\tsub qword [glovars], 8\n" +
                      "\t;check arg count:\n" +
                      "\tpush qword [rbp+16]\n" +
                      "\tpush qword " + string(argc) + "\n" +
                      "\tcall " + checkargc + "\n" +
                      "\tadd rsp, 16\n" +
                      "\t; allocate globals:\n"

let pushargs = "\t;set up command line arguments on stack:\n" +
                "\tmov rcx, [rbp+16]\n" +
                "\tmov rsi, [rbp+24]\n" +
                "_args_next:\n" +
                "\tcmp rcx, 0\n" +
                "\tjz _args_end\n" +
                "\tpush qword [rsi]\n" +
                "\tadd rsi, 8\n" +
                "\tsub rcx, 1\n" +
                "\tjmp _args_next               ;repeat until --rcx == 0\n" +
                "_args_end:\n" +
                "\tsub rbp, 8                   ; make rbp point to first arg\n"

let popargs =   "\t;clean up stuff pushed onto stack:\n" +
                "\tmov rsp, qword [glovars]\n" +
                "\tadd rsp, 8\n" +
                "\tmov rsp, rbp\n" +
                "\tpop rbp\n" +
                "\tret\n"
