(* File Imp/Naive.fs 
 A naive imperative language with for- and while-loops
   sestoft@itu.dk * 2009-11-17
*)
// module Naive 

(* A naive store is a map from names (strings) to values (ints) *)
//字典做为存储,类型声明
type naivestore = Map<string,int>

// 空存储,空的字典对象
// val emptystore : Map<string,int> = map []
let emptystore : Map<string,int> = Map.empty

// 从存储取值 ,参数为变量名，返回值为 该变量绑定的值
// val getSto : store:naivestore -> x:string -> int
let getSto (store : naivestore) x = store.Item x

// 赋值存储 ,setSto会返回一个新的字典对象，新的存储
// val setSto : store:naivestore -> k:string * v:int -> Map<string,int> 
// store.Add  : ('a * 'b -> Map<'a,'b>) 参数是一个 tuple (k,v)
let setSto (store : naivestore) (k, v) = store.Add (k, v)


(* Abstract syntax for expressions *)
// 表达式类型定义，支持 常数、变量、基本运算
type expr = 
  | CstI of int                     //常量
  | Var of string                  //变量
  | Prim of string * expr * expr  //基本运算

// 表达式求值函数eval    
let rec eval e (store : naivestore) : int =
    match e with
      | CstI i -> i
      | Var x  -> getSto store x
      | Prim(ope, e1, e2) ->
        let i1 = eval e1 store
        let i2 = eval e2 store
        match ope with        //模式匹配 基本运算
          | "*"  -> i1 * i2
          | "+"  -> i1 + i2
          | "-"  -> i1 - i2
          | "==" -> if i1 = i2 then 1 else 0
          | "<"  -> if i1 < i2 then 1 else 0
          | _    -> failwith "unknown primitive"

//语句类型定义
type stmt =  
  | Asgn of string * expr          //赋值语句
  | If of expr * stmt * stmt       //if 语句
  | Block of stmt list             //语句块 ，复合语句
  | For of string * expr * expr * stmt  //for语句
  | While of expr * stmt                //while语句
  | Print of expr                      //输出语句

//语句执行函数 exec 
//调用了求值eval函数，对语句中的表达式求值
//返回类型是 store
let rec exec stmt (store : naivestore) : naivestore =
    match stmt with
    | Asgn(x, e) -> 
        setSto store (x, eval e store)  //变量保存到store
    | If(e1, stmt1, stmt2) ->   // e1是分支条件 
                                // stmt1是True分支，stmt2是False分支
        if eval e1 store <> 0 then exec stmt1 store
                              else exec stmt2 store
    | Block stmts ->          //语句块 定义loop 递归函数 遍历语句块里面的语句
        let rec loop ss sto = 
              match ss with 
              | []     -> sto
              | s1::sr -> loop sr (exec s1 sto) //执行第一条语句，更新store
                                                //递归调用
        loop stmts store
    | For(x, estart, estop, stmt) -> 
        let start = eval estart store in
        let stop  = eval estop  store in
        let rec loop i sto =       //循环变量
                if i > stop then sto                        //更新变量
                            else loop (i+1) (exec stmt (setSto sto (x, i))) in
          loop start store 
    | While(e, stmt) -> 
        let rec loop sto =
          if eval e sto = 0 then sto
              else loop (exec stmt sto) in
          loop store
    | Print e -> 
        (printf "%d\n" (eval e store); store)

let run stmt = 
    // 执行的初始环境是 空存储 emptystore
    let _ = exec stmt emptystore
    ()

(* Example programs *)

// 源程序
// sum = 0;
// for i=0 to 100 do
//   sum = sum + i;
// print sum;

//语法树
let ex1 =
    Block[Asgn("sum", CstI 0);
          For("i", CstI 0, CstI 100, 
              Asgn("sum", Prim("+", Var "sum", Var "i")));
          Print (Var "sum")];;

// i = 1;
// sum = 0;
// while sum < 10 do
//   print sum;
//   sum = sum + i;
//   i = i + 1;
// end
// print i;
// print sum;

let ex2 =
    Block[Asgn("i", CstI 1);
          Asgn("sum", CstI 0);
          While (Prim("<", Var "sum", CstI 10),
                 Block[Print (Var "sum");
                       Asgn("sum", Prim("+", Var "sum", Var "i"));
                       Asgn("i", Prim("+", CstI 1, Var "i"))]);
          Print (Var "i");
          Print (Var "sum")];;

// open Naive;;
run ex1;;
run ex2;;    
