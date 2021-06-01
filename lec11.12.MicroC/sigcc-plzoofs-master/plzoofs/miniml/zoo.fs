module Zoo

open FSharp.Text.Lexing

// port ML code to F#
let int_of_string = int
let string_of_int = string
let string_of_bool = string

let error kind loc = 
  printfn $"{kind}: {loc}"

let print_parens ?(max_level=9999) ?(at_level=0) ppf =
  if max_level < at_level then
    begin
      Format.fprintf ppf "(@[" ;
      Format.kfprintf (fun ppf -> Format.fprintf ppf "@])") ppf
    end
  else
    begin
      Format.fprintf ppf "@[" ;
      Format.kfprintf (fun ppf -> Format.fprintf ppf "@]") ppf
    end
    
module Lexing = 

let lexeme = LexBuffer<_>.LexemeString

let newline (lexbuf: LexBuffer<_>) = 
  lexbuf.EndPos <- lexbuf.EndPos.NextLine
}