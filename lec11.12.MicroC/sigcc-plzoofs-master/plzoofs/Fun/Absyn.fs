(* Fun/Absyn.fs * Abstract syntax for micro-ML, a functional language *)

module Absyn 

type expr = 
  | CstI of int   // 整数
  | CstB of bool  // 布尔数
  | Var of string   // 变量访问
  | Let of string * expr * expr // 局部变量声明，定义
  | Prim of string * expr * expr  // 基本操作 算术 * + - 、关系 < = 
  | If of expr * expr * expr    // if 表达式
  | Letfun of string * string * expr * expr    (* (f, x, fBody, letBody) *)  // 函数声明，定义
  | Call of expr * expr   // 函数调用
