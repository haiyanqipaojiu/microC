
let print_endline = printfn "%s\n"
let string_of_int = string

//Table 模块 实现了变量存储
//变量名称 值的绑定关系 用 列表 结构存储
// 使用方式如下：
// let store = Table.set Table.empty "b" 9
// let store' = Table.set store "a" 3
// [("a",3) ; ("b", 9)]

// get 返回的是option值，封装了空值None
// val get : t:('a * 'b) list -> k:'a -> 'b option 

module Table =

  type ('k, 'v) t = ('k * 'v) list
  let empty = []
  let set t k v = (k, v) :: t
  let get t k =
    let rec search = function
      | [] -> None
      | (key, v) :: _ when key = k -> Some v
      | (_, _) :: rest -> search rest
    in
      search t

//直线式语言模块Spl
module Spl = 

  //变量的类型
  type id = string

  //操作符的类型 + - * /
  type binop =
    | Plus
    | Minus
    | Times
    | Div

  // 语句的类型，支持 顺序复合语句，赋值语句，输出语句
  // 语句stm 与 表达式 exp 的类型定义是 相互递归
  // 因此需要使用  type  ... and ... 的结构
  type stm =         //语句
    | CompoundStm of stm * stm  //顺序复合语句，用于构造多条语句的程序
    | AssignStm of id * exp     //赋值
    | PrintStm of exp list     //输出
  and exp =                  //表达式表示可以求值运算
    | IdExp of id             // 变量
    | NumExp of int           // 数值
    | OpExp of exp * binop * exp  // 基本运算
    | EseqExp of stm * exp        // 顺序表达式，用于混合语句与表达式

  exception Unknown_identifier of string  // Table中未查到变量定义，抛出此异常

  let interp_binop op v1 v2 =
    match op with
    | Plus  -> v1 + v2
    | Minus -> v1 - v2
    | Times -> v1 * v2
    | Div   -> v1 / v2

   //interp_stm: stm的解释函数
  //解释器是递归结构，结构与语法类型定义类似
  //根据当前的语句stm，做相应的解释，同时返回 环境tbl
  // tbl 在赋值语句中会被更新
  // tbl 更新是 immutable 每次返回新的tbl
  let rec interp_stm tbl_0 stm =
    match stm with
    | PrintStm exps ->       //输出语句
        let (tbl_1, val_ints) =
          List.foldBack (fun e (tbl0, vs) ->
                let (tbl1, v) = interp_exp tbl0 e in
                (tbl1, v :: vs)
            )  exps (tbl_0, [])
        in
        let val_strings = List.map string_of_int val_ints  in
        print_endline (String.concat " " val_strings );
        tbl_1
    | AssignStm (id, e) ->      //赋值语句
        let (tbl_1, v) = interp_exp tbl_0 e in
        Table.set tbl_1 id v  //返回新的 table
    | CompoundStm (s1, s2) ->   //顺序语句
            //解释 s1 ，更新 tbl_0 为 tbl_1
            //在新环境 tbl_1中解释 s2
        let tbl_1 = interp_stm tbl_0 s1 in
        interp_stm tbl_1 s2

   // interp_exp: 表达式 exp 的解释器函数
  and interp_exp tbl_0 exp =
    match exp with
    | IdExp id ->
        match Table.get tbl_0 id with
        | Some v -> (tbl_0, v)
        | None   -> raise (Unknown_identifier id)  //抛出异常
    | NumExp n -> (tbl_0, n)
    | OpExp (e1, op, e2) ->
        let (tbl_1, v1) = interp_exp tbl_0 e1 in
        let (tbl_2, v2) = interp_exp tbl_1 e2 in
        (tbl_2, interp_binop op v1 v2)
    | EseqExp (s, e) ->
        let tbl_1 = interp_stm tbl_0 s in
        interp_exp tbl_1 e
    
  
  //解释器 主程序
  //在初始空环境 Table.empty 开始解释 stm
  let interp stm : unit =
    ignore (interp_stm (Table.empty) stm)

let spl_prog_noprint =
(*  a := 5 + 3;
 *  b := 10 * a
 *)
  Spl.CompoundStm
    ( Spl.AssignStm
        ("a", Spl.OpExp (Spl.NumExp 5, Spl.Plus, Spl.NumExp 3))
    , Spl.AssignStm
        ("b", Spl.OpExp (Spl.NumExp 10, Spl.Times, Spl.IdExp "a"))
    )

Spl.interp spl_prog_noprint


(*  a := 5 + 3;           AssignStm
 *  b := (print(a, a - 1), 10 * a);  AssignStm  EseqExp
 *  print(b)                PrintStm
 *  运行结果
 *  8 7 
 *  80
 *)

//分步构造程序
// 赋值语句 a := 5 + 3
let stm1 = Spl.AssignStm ("a", Spl.OpExp (Spl.NumExp 5, Spl.Plus, Spl.NumExp 3)) 

// 顺序表达式 (print(a, a - 1), 10 * a)
// 组合了语句 print(a,a-1) 表达式 10 * a
let seqexp = Spl.EseqExp  ( Spl.PrintStm
                [ Spl.IdExp "a"; Spl.OpExp (Spl.IdExp "a", Spl.Minus, Spl.NumExp 1)]
              , Spl.OpExp (Spl.NumExp 10, Spl.Times, Spl.IdExp "a"))

// 赋值语句 b := (print(a, a - 1), 10 * a)
let stm2' =  Spl.AssignStm ( "b" , seqexp) 
//同上
let stm2 =  Spl.AssignStm ( "b" , Spl.EseqExp ( Spl.PrintStm
                [ Spl.IdExp "a"; Spl.OpExp (Spl.IdExp "a", Spl.Minus, Spl.NumExp 1)]
                , Spl.OpExp (Spl.NumExp 10, Spl.Times, Spl.IdExp "a")))

let stm3 = Spl.PrintStm [Spl.IdExp "b"]

//复合语句 CompoundStm 组合 上面的 3条语句，构造顺序结构
//CompoundStm
let spl_prog_orig' =   Spl.CompoundStm (stm1, 
                 Spl.CompoundStm (stm2,stm3))

// 同上写到一起
let spl_prog_orig = Spl.CompoundStm ( Spl.AssignStm ("a", Spl.OpExp (Spl.NumExp 5, Spl.Plus, Spl.NumExp 3)), 
    Spl.CompoundStm ( 
       Spl.AssignStm ( "b" , Spl.EseqExp (
             Spl.PrintStm [ Spl.IdExp "a"; Spl.OpExp (Spl.IdExp "a", Spl.Minus, Spl.NumExp 1)]
             , Spl.OpExp (Spl.NumExp 10, Spl.Times, Spl.IdExp "a")
            )
        )
      , Spl.PrintStm [Spl.IdExp "b"]
      )
  )

(*  a := 5 + 3;           AssignStm
 *  b := (print(a, a - 1), 10 * a);  AssignStm  EseqExp
 *  print(b)                PrintStm
 *)
print_endline "BEGIN Spl.interp spl_prog_orig";
Spl.interp spl_prog_orig;
print_endline "END Spl.interp spl_prog_orig"




// let () =
//   let string_of_maxargs int_opt =
//     match int_opt with
//     | Some n -> string_of_int n
//     | None   -> "N/A"
//   in
//     // Printf.printf "maxargs : spl_prog_orig -> %s\n"
//     //   (string_of_maxargs (Spl.maxargs spl_prog_orig));
//     // Printf.printf "maxargs : spl_prog_noprint -> %s\n"
//     //   (string_of_maxargs (Spl.maxargs spl_prog_noprint));
//     print_endline "BEGIN Spl.interp spl_prog_orig";
//     Spl.interp spl_prog_orig;
//     print_endline "END Spl.interp spl_prog_orig"



  (* 01.p.1: Write ML function (maxargs : stm -> int) that tells the
   * maximum number of arguments of any print statement within any
   * subexpression of a given statement. For example, maxargs(prog)
   * is 2.
   *)
// let maxargs stm =
//   let opt_max_update opt n =
//     match opt with
//     | None   -> Some n
//     | Some m -> Some (max m n)
//   let opt_max_merge a b =
//     match a, b with
//     | None  , None   -> None
//     | None  , b      -> b
//     | Some _, None   -> a
//     | Some _, Some n -> opt_max_update a n
  
//   let rec check_stm max_opt stm =
//     match stm with
//     | PrintStm exps ->
//         List.fold_left check_exp (opt_max_update max_opt (List.length exps)) exps
//     | AssignStm (_, e) ->
//         check_exp max_opt e
//     | CompoundStm (s1, s2) ->
//         opt_max_merge (check_stm max_opt s1) (check_stm max_opt s2)
//   and check_exp max_opt exp =
//     match exp with
//     | IdExp _ | NumExp _ -> max_opt
//     | OpExp (e1, _, e2) ->
//         opt_max_merge (check_exp max_opt e1) (check_exp max_opt e2)
//     | EseqExp (s, e) ->
//         opt_max_merge (check_stm max_opt s) (check_exp max_opt e)
//   check_stm None stm