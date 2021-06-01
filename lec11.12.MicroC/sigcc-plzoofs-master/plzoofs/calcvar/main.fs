module Main 

open System
open FSharp.Text.Lexing
open Syntax

[<EntryPoint>]
let main argv =
    let mutable debug = false
    let info action = 
        if debug then action () else ()

    try
        let f = Array.find ((=) "-g") argv
        debug <- true 
    with :? System.Exception as ex -> ()

    info (fun ()->printfn "argv: %A" argv)

    let mutable ctx = CalcVar.initial_environment
    
    // 封装系统的 Lexer.token 
    // 提供调试信息
    let token  =
        fun buf ->  
            let res = Lexer.token buf;
            info (fun () ->printfn "%A" (string res)) ; 
            res
    
    // 命令行求值 REPL 主循环
    let rec loop () = 
        printf $"{CalcVar.name}>"
        let input =  Console.ReadLine()
        let lexbuf = LexBuffer<char>.FromString input 
        try 
            let cmd = Parser.toplevel token lexbuf in
                ctx <- CalcVar.exec ctx cmd ;
                info (fun () -> printfn "%A" (cmd,ctx) )
        with e ->
            printfn "Errors"       
        loop ()
    loop ()   
