module Driver

open System.IO
open FSharp.Text.Lexing
open Printf

open Platform
open Ll
open Llparser
open Debug

#nowarn "62"
let dashes n = String.replicate n "-"

let print_endline s = printfn "%A" s

let print_banner s =
    let rec dashes n =
        if n = 0 then
            ""
        else
            "-" ^ (dashes (n - 1)) in

    printf "\n%s %s\n" (dashes (79 - (String.length s))) s

let print_ll file ll_ast =
    print_banner file
    print_endline (Ll.string_of_prog ll_ast)

let print_x86 file asm_str =
    print_banner file
    print_endline asm_str

let read_file (file: string) : string =
    // let lines = ref [] in
    // let channel = open_in file in
    // try while true do
    //     lines := input_line channel :: !lines

    // with End_of_file ->
    //   close_in channel;
    //   String.concat "\n" (List.rev !lines)
    File.ReadAllText(file)


let write_file (file: string) (out: string) =
    // let channel = open_out file in
    // fprintf channel "%s" out;
    // close_out channel
    if not (File.Exists "output") then
        (Directory.CreateDirectory "output" |> ignore)
    else
        ()

    File.WriteAllText(file, out + "\n")

let token buf =
    let tk = Lllexer.token buf

    if !print_token then
        printf "%A, " tk
    else
        ()

    tk

let parse_file filename =
    let mutable program : Ll.prog =
        { tdecls = []
          gdecls = []
          fdecls = [] }

    let lexbuf =
        read_file filename |> LexBuffer<char>.FromString

    try

        if !print_token then
            print_banner filename
        else
            ()

        program <- Llparser.prog token lexbuf

        if !print_token then
            printfn "\n"
        else
            ()

    with ex ->
        raise (
            System.Exception(

                System.String.Format(
                    "Parse failed at line {0}, column {1} ({2})",

                    lexbuf.StartPos.Line,
                    lexbuf.StartPos.Column,

                    new System.String(lexbuf.Lexeme)
                )
            )
        )

    program

let run_executable arg pr =
    let cmd = sprintf "%s%s %s" dot_path pr arg in
    sh cmd (fun _ i -> i)

let run_executable_to_tmpfile arg pr tmp =
    let cmd =
        sprintf "%s%s %d > %s 2>&1" dot_path pr arg tmp in

    sh cmd ignore_error

let string_of_file (f: System.IO.Stream) : string =
    let sr = new System.IO.StreamReader(f)
    sr.ReadToEnd()


let run_program (args: string) (executable: string) (tmp_out: string) : string =
    // let _ =
    //   let cmd = sprintf "%s%s %s > %s 2>&1" dot_path executable args tmp_out in
    //   sh cmd ignore_error
    // in
    // let fi = open_in tmp_out in
    // let result = string_of_file fi in
    // let _ = close_in fi in
    //   result
    "TODO"
