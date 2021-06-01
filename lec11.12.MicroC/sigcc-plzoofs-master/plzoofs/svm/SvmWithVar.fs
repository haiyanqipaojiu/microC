module SvmWithVar
//
//带变量定义的表达式类型定义
//
type expr = 
  | CstI of int
  | Var of string             //变量使用
  | Let of string * expr * expr //声明局部变量
  | Prim of string * expr * expr

// 17 + 17 ;
let e1 = Prim ("+", CstI 17, CstI 17)

// z = 17;  z + z ;
// let e2 = Let("z", CstI 17, Prim("+",CstI 10, Var "z"));;
let e2 = Let("z", CstI 17, Prim("+",Var "z", Var "z"))

// z = 5 - 4;  100 * z; 
let e3 = Let("z", Prim("-", CstI 5, CstI 4), 
             Prim("*", CstI 100, Var "z"))
             
// SVar int 将变量从栈 []上取出
// int 为位置索引
// 变量的实现，将变量保存到栈，这里是将变量保存到 
// 运算栈stack ，在栈上既保存运算的结果，也保存变量
// 也可以单独设置一个 变量栈，保存变量

type sinstr =
  | SCstI of int                        (* push integer           *)
  | SVar of int                         (* push variable from stack position int *)
  | SAdd                                (* pop args, push sum     *)
  | SSub                                (* pop args, push diff.   *)
  | SMul                                (* pop args, push product *)
  | SPop                                (* pop value/unbind var   *)
  | SSwap                             (* exchange top and next  *)

//栈式虚拟机求值，就是对 指令序列 inns的每条指令按次序求值，并把指令的执行结果放到运算栈stack上
let rec seval (inss : sinstr list) (stack : int list) =
    match (inss, stack) with
    | ([], v :: _) -> v   // 指令执行完毕，栈顶的内容就是最终结果
    | ([], [])     -> failwith "seval: no result on stack"
    | (SCstI i :: insr,          stk) -> seval insr (i :: stk) 
                                     // 立即数入栈
    | (SVar i  :: insr,          stk) -> seval insr (List.nth stk i :: stk) 
                                     // SVar i指令从栈上偏移位置i找到变量的值，放到栈上 
    | (SAdd    :: insr, i2::i1::stkr) -> seval insr (i1+i2 :: stkr) //加法
    | (SSub    :: insr, i2::i1::stkr) -> seval insr (i1-i2 :: stkr) //减法
    | (SMul    :: insr, i2::i1::stkr) -> seval insr (i1*i2 :: stkr) //乘法
    | (SPop    :: insr,    _ :: stkr) -> seval insr stkr            //出栈
    | (SSwap   :: insr, i2::i1::stkr) -> seval insr (i1::i2::stkr)  //交换栈顶内容
    | _ -> failwith "seval: too few operands on stack"

// 虚拟器 运行时 求值环境为 空表 []
let sexec inss = seval inss []
(* Map variable name to variable index at compile-time *)

let rec getindex vs x = 
    match vs with 
    | []    -> failwith "Variable not found"
    | y::yr -> if x=y then 0 else 1 + getindex yr x


(* A compile-time variable environment representing the state of
   the run-time stack. *)

//运算栈，存储栈合一，栈上有运算值 Value 和绑定变量 Bound 'x' 'y'...
type Stackvalue =
  | Value                               (* A computed value *)
  | Bound of string;;                   (* A bound variable *)

(* Compilation to a list of instructions for a unified-stack machine *)

//cenv 是编译过程中构造的变量编译环境，用于跟踪在程序执行过程中，绑定变量的相对位置

let rec scomp (e : expr) (cenv : Stackvalue list) : sinstr list =
    // printfn "e,cenv %A" (e,cenv)
    match e with
    | CstI i -> [SCstI i]
    | Var x  -> [SVar (getindex cenv (Bound x))]
    | Let(x, erhs, ebody) -> 
          scomp erhs cenv @ scomp ebody (Bound x :: cenv) @ [SSwap; SPop]
          //erhs 在栈上生成了 绑定值 Bound x，ebody的编译环境cenv需要增加此值
          //在ebody 编译完成后，[SSwap;SPop]保留 ebody 的值，同时将绑定值从栈上弹出

    | Prim("+", e1, e2) -> 
          scomp e1 cenv @ scomp e2 (Value :: cenv) @ [SAdd] 
          // 编译 e1 ，e1 执行后会在栈上生成值 Value
          // 需要更新编译环境 cenv
    | Prim("-", e1, e2) -> 
          scomp e1 cenv @ scomp e2 (Value :: cenv) @ [SSub] 
    | Prim("*", e1, e2) -> 
          scomp e1 cenv @ scomp e2 (Value :: cenv) @ [SMul] 
    | Prim _ -> failwith "scomp: unknown operator"

let scompiler expr = scomp expr []

let s1 = scompiler e1
// printfn "%A" s1
sexec s1

let s2 = scompiler e2
// printfn "%A" s2
sexec s2

let s3 = scompiler e3
// printfn "%A" s3
sexec s3

(* Output the integers in list inss to the text file called fname: *)

let intsToFile (inss : sinstr list ) (fname : string) = 
    let text = String.concat ", " (List.map string inss)
    // System.Console.WriteLine(text);
    System.IO.File.WriteAllText(fname, text)

// intsToFile (scompiler e3) "e3.instr.txt" 
