#load "ir1.fs"
#load "main1.fs"

open Main1 

let show e =
 printfn "%A\n" e

"表达式" |> show 
"
(*
    (1 + X4) + (3 + (X1 * 5) )
*)
" |> show

"ast1" |>show 
Main1.p |> show  

"let ir1"|> show
Main1.ir |> show 

"ir1 code" |> show
Main1.s |> show 


#load "ir2.fs"
#load "main2.fs"
"命令 赋值/顺序" |>show
"
(*
    X1 := (1 + X4) + (3 + (X1 * 5) ) ;
    Skip ;
    X2 := X1 * X1 ;
*)
" |> show

"ast2" |>show 
Main2.p |> show  

"let ir2"|> show
Main2.ir |> show 

"ir2 code" |> show
Main2.s |> show 

#load "ir3.fs"
#load "main3.fs"
"基本块 while" |>show

"
(*
    X2 := X1 + X2;
    IFNZ X2 THEN
        X1 := X1 + 1
    ELSE
        X2 := X1
    X2 := X2 * X1
*)
" |> show 
"ast3" |>show 
Main3.p |> show  

"let ir3"|> show
Main3.ir |> show 

"ir3 code" |> show
Main3.s |> show 


"基本块 while" |>show

"
(*
    X1 := 6;
    X2 := 1;
    WhileNZ X1 DO
    X2 := X2 * X1;
    X1 := X1 + (-1);
    DONE
*)
" |> show 
"ast3" |>show 
Main3.q |> show  

"let ir3"|> show
Main3.qir |> show 

"ir3 code" |> show
Main3.qs |> show 