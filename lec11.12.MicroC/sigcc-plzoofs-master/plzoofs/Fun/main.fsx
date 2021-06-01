#r "nuget: FsLexYacc"

let show exp = printfn "\n%A" exp

// 抽象语法，扫描器，解析器
#load "Absyn.fs" "FunPar.fs" "FunLex.fs" "Parse.fs"

// 一阶函数式语言
#load "Fun.fs"

// 解析器
open Parse

"一阶函数式语言" |> show
let e1 = fromString "5+7"
let e2 = fromString "let y = 7 in y + 2 end"
let e3 = fromString "let f x = x + 7 in f 2 end"

("5+7", "let y = 7 in y + 2 end", "let f x = x + 7 in f 2 end")
|> show

(e1, e2, e3) |> show

// 解析并求值
let run e = Fun.eval e []
(run e1, run e2, run e3) |> show

let e4 =
    fromString "let f x = if x < 1 then 1 else  x * f (x-1) in f 10 end"

e4 |> show
run e4 |> show

// 高阶函数式语言
#load "HigherFun.fs"

open Absyn
open HigherFun

"高阶函数式语言" |> show

// 直接对 AST 求值
// let twice f = let g x = f(f(x)) in g end
// in let mul3 z = z*3 in twice mul3 2 end end
"let twice f = let g x = f(f(x)) in g end in let mul3 z = z*3 in twice mul3 2 end end"
|> show

run (
    Letfun(
        "twice",
        "f",
        Letfun("g", "x", Call(Var "f", Call(Var "f", Var "x")), Var "g"),
        Letfun("mul3", "z", Prim("*", Var "z", CstI 3), Call(Call(Var "twice", Var "mul3"), CstI 2))
    )
)
|> show

// 对源程序语法分析，求值
let exp =
    (fromString "let twice f = let g x = f(f(x)) in g end in let mul3 z = z*3 in twice mul3 2 end end")

exp |> show
run exp |> show


"类型检查" |> show

#load "typedfun.fs"
open TypedFun // open module

"类型检查正确" |> show

"let f (x : int) : int = x+1
  in f 12 end "
|> show

ex1 |> show
eval ex1 [] |> show

typeCheck ex1 |> show

"类型检查错误" |> show
exErr1 |> show
// This typecheck should throw exception:
try
    typeCheck exErr1 |> ignore
with e -> show e

typeCheck (Prim("=", CstI 1, CstI 2))


// 多态类型推理
#load "TypeInference.fs"
"类型推理" |> show

"let f x = 1 in f 7 + f false end" |> show

TypeInference.inferType (fromString "let f x = 1 in f 7 + f false end")
|> show

"let twice f = let g x = f(f(x)) in g end in twice end"
|> show

"typeof twice:" |> show

TypeInference.inferType (fromString "let twice f = let g x = f(f(x)) in g end in twice end")
|> show

"let twice f = let g x = f(f(x)) in g end in let mul3 z = z*3 in twice mul3  end end"
|> show

"type of twice mul3:" |> show

TypeInference.inferType (
    fromString "let twice f = let g x = f(f(x)) in g end in let mul3 z = z*3 in twice mul3  end end"
)
|> show
