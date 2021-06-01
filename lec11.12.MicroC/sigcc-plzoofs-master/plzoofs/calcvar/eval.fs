(** Evaluation of expressions, given as big step semantics. *)
module Eval  
open Syntax


(** [eval env e] evaluates the expression [e] to an integer,
    where [env] is an association list mapping variables to their values.
    It raises an expressions if division by zero occurs. *)
//* 在环境 env 中查找变量 x 的值 
let rec lookup x env = 
    match env with
     | [] -> failwith "unknown variable"
     | head :: tail -> 
               if fst head  = x then snd head 
                         else lookup x tail

let eval env =
  let rec eval = function
    | Variable x ->
      (try
         // List.find (fst >> ((=) x)) >> snd env
         lookup x env
       with
         | Failure(msg) -> printfn $"{msg} {x}";0
      )   
    | Numeral n -> n
    | Plus (e1, e2) -> eval e1 + eval e2
    | Minus (e1, e2) -> eval e1 - eval e2
    | Times (e1, e2) -> eval e1 * eval e2
    | Divide (e1, e2) ->
      let n2 = eval e2 in
        if n2 <> 0 then eval e1 / n2 else failwith "division by zero"
    | Negate e -> - (eval e)
  in
    eval
