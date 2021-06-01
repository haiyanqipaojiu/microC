(* Programming language concepts for software developers, 2010-08-28 *)

(* Representing object language expressions using recursive datatypes *)
module SimpleExpr

// 表达式 递归数据类型
type expr = 
  | CstI of int
  | Prim of string * expr * expr;;

let e1 = CstI 17;;
let e2 = Prim("-", CstI 3, CstI 4);;
let e3 = Prim("+", Prim("*", CstI 7, CstI 9), CstI 10);;


(* Evaluating expressions using recursive functions *)
//表达式递归求值函数
let rec eval (e : expr) : int =
    match e with
    | CstI i -> i
    | Prim("+", e1, e2) -> eval e1 + eval e2
    | Prim("*", e1, e2) -> eval e1 * eval e2
    | Prim("-", e1, e2) -> eval e1 - eval e2
    | Prim _            -> failwith "unknown primitive";;      // _ 表示任意值


let e1v = eval e1;;
let e2v = eval e2;;
let e3v = eval e3;;

// printfn  "%A" (e1v,e2v,e3v);;

(* Changing the meaning of subtraction *)
// 自定义减法操作的语义

let rec evalm (e : expr) : int =
    match e with
    | CstI i -> i
    | Prim("+", e1, e2) -> evalm e1 + evalm e2
    | Prim("*", e1, e2) -> evalm e1 * evalm e2
    | Prim("-", e1, e2) -> 
      let res = evalm e1 - evalm e2
      if res < 0 then 0 else res 
    | Prim _            -> failwith "unknown primitive";;


let e4v = evalm (Prim("-", CstI 10, CstI 27));;

// printfn  "%A" e4v;;