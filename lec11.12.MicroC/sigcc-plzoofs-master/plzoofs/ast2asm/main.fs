open Compile

(* Example source program expressions --------------------------------------- *)

(* X1 + (X2 + (X3 + (X4  (X5 + (X6 + (X6 + (X7 + (X8 + 0)))))))) = 36 *) 
let src1 : Compile.exp =
  (Add(Var "X1",
       Add(Var "X2",
           Add(Var "X3",
               Add(Var "X4",
                   Add(Var "X5",
                       Add(Var "X6",
                           Add(Var "X7",
                               Add(Var "X8", Const 0L)))))))))
 

(* X1 * (X2 * (X3 * (X4  (X5 * (X6 * (X6 * (X7 * (X8 * 1)))))))) = 40320 *)
let src2 : Compile.exp =
  (Mul(Var "X1",
       Mul(Var "X2",
           Mul(Var "X3",
               Mul(Var "X4",
                   Mul(Var "X5",
                       Mul(Var "X6",
                           Mul(Var "X7",
                               Mul(Var "X8", Const 1L)))))))))


(* X8 * X8 = 64 *)
let src3 : Compile.exp =
  (Mul(Var "X8", Var "X8"))

(* - (X8 * X8) = -64 *) 
let src4 : Compile.exp =
  Neg(Mul(Var "X8", Var "X8"))

(* ((1 * X3) + -(X1 * X2)) + ((X4 * X5) + (X6 + X7)) * - (src3 * src3)) = -135167 *)
let src5 : Compile.exp =
  Add(
    Add(
      Mul(Const 1L, Var "X3"),
      Neg(Mul(Var "X1", Var "X2"))),
    Mul(
      Add(Mul(Var "X4", Var "X5"),
          Add(Var "X6", Var "X7")),
      Neg(Mul(src3, src3))
    )
  )


(* compilation -------------------------------------------------------------- *)

let src = src3

(* Resulting x86 program after compilation *)
//let tgt : X86.prog = compile1 src5
// let tgt : X86.prog = compile2 src3
let tgt : X86.prog = compile2 src

// printfn "%A" tgt 
(* Output the resulting s file *)

let s = X86.string_of_prog tgt


// test x86 program
// let s = X86.string_of_prog X86.p1 
// let s = X86.string_of_prog X86.p2 
// let s = X86.string_of_prog (X86.p3 0) 
// let s = X86.string_of_prog (X86.p4 5) 

;; printfn "%s" s
