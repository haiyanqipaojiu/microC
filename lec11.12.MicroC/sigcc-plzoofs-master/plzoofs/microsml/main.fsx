#r "nuget: fslexyacc"

#load "Absyn.fs" "FunPar.fs" "FunLex.fs" "TypeInference.fs" "HigherFun.fs" "Machine.fs" "Comp.fs" "ParseTypeAndRun.fs"

open System.IO
open ParseTypeAndRun

let show e = printfn "\n%A" e

"fromString" |> show

let ex01 = @"
fun f x = x + g 4
and g x = x

begin
  print (f 1)
end
"
ex01 |> show
fromString ex01 |> show

"run ex01" |> show
run ex01 |> show

let compile fname =
    "fromFile " + fname |> show
    File.ReadAllText fname |> show
    "AST:" |> show
    fromFile fname |> show

    "compile to file" |> show

    compProg' (false, true, true, true, fromFile fname, fname + ".out")
    |> show

compile "ex01.sml"
compile "exn01.sml"
