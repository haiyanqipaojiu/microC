#r "nuget: fslexyacc"
#load "absyn.fs";;
#load "exprparser.fs";;
#load "exprlexer.fs";;

open FSharp.Text.Lexing

let show e = 
  printfn "%A" e

let test s =
    let lexbuf = LexBuffer<char>.FromString (s+"\n") in
    Exprparser.main Exprlexer.token lexbuf;;

test "a + b" |> show

//优先级正确
test "a + b*c" |> show
