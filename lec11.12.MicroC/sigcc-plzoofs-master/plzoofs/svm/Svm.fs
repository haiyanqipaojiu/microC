module Svm

//
//带变量定义的表达式
//

type expr = 
  | CstI of int
  | Var of string
  | Let of string * expr * expr
  | Prim of string * expr * expr;;

// 17 + 17 ;
let e1 = Prim ("+", CstI 17, CstI 17)

// 2 + 2 * 3 ;
let e2 = Prim ("+", CstI 2, Prim("*", CstI 2, CstI 3))


// z = 17;  z + z ;
let e3 = Let("z", CstI 17, Prim("+", Var "z", Var "z"));;

// z = 5 - 4; z * 100; 
let e4 = Let("z", Prim("-", CstI 5, CstI 4), 
             Prim("*", CstI 100, Var "z"));;
             
//
// 逆波兰栈式虚拟机器指令集 
// RDup 单目运算，复制栈上的一个操作数
// RSwap 交换栈顶上两个操作数的位置
//
type rinstr =
  | RCstI of int
  | RAdd 
  | RSub
  | RMul 
  | RDup
  | RSwap;;

(* A simple stack machine for evaluation of variable-free expressions
   in postfix form *)

//栈式虚拟机实现 ,用于对 RPN 求值 ,注意递归调用 reval
// 求值表现为对栈的操作
let rec reval (inss : rinstr list) (stack : int list) : int =
    match (inss, stack) with 
    | ([], v :: _) -> v
    | ([], [])     -> failwith "reval: no result on stack!"
    | (RCstI i :: insr,             stk)  -> reval insr (i::stk)
    | (RAdd    :: insr, i2 :: i1 :: stkr) -> reval insr ((i1+i2)::stkr)
    | (RSub    :: insr, i2 :: i1 :: stkr) -> reval insr ((i1-i2)::stkr)
    | (RMul    :: insr, i2 :: i1 :: stkr) -> reval insr ((i1*i2)::stkr)
    | (RDup    :: insr,       i1 :: stkr) -> reval insr (i1 :: i1 :: stkr)
    | (RSwap   :: insr, i2 :: i1 :: stkr) -> reval insr (i1 :: i2 :: stkr)
    | _ -> failwith "reval: too few operands on stack";;

// 虚拟器 运行时 求值环境为 空表 []
// rexec 是 reval 的封装函数
let rexec inss = reval inss []

//
// 10 7 7 * +  =>  10 + 7 * 7 = 59
//

let rpn1 = rexec [RCstI 10; RCstI 7; RDup; RMul; RAdd] ;;
let rpn2 = rexec [RCstI 10; RCstI 7; RCstI 7; RMul; RAdd] ;;


(* Compilation of a variable-free expression to a rinstr list *)
//
// 将基本表达式expr ，不支持 变量声明
// 编译生成 RPN 指令
//
let rec rcomp (e : expr) : rinstr list =
    match e with
    | CstI i            -> [RCstI i]
    | Var _             -> failwith "rcomp cannot compile Var"
    | Let _             -> failwith "rcomp cannot compile Let"
    | Prim("+", e1, e2) -> rcomp e1 @ rcomp e2 @ [RAdd]
    | Prim("*", e1, e2) -> rcomp e1 @ rcomp e2 @ [RMul]
    | Prim("-", e1, e2) -> rcomp e1 @ rcomp e2 @ [RSub]
    | Prim _            -> failwith "unknown primitive";;
            
(* Correctness: eval e []  equals  reval (rcomp e) [] *)


// 17 + 17 => 34
// let e1 = Prim ("+", CstI 17, CstI 17);;
rcomp e1;;

rexec (rcomp e1) ;;
(rcomp  >> rexec)  e1;;  //函数复合
e1 |> rcomp |>rexec;; // 数据流 data pipeline

// can not comiple Var
// let ez = Prim ("+", Var "z", CstI 17);;
// rcomp ez;;