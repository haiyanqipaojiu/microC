// recursive descent parser
// for grammer
//   1. S → aSB
//   2. S → b
//   3. B → a
//   4. B → bBa


exception ParseError

let matchInput t ts =
    match ts with
    | t :: trr -> trr
    | _ -> raise ParseError

let rec S =
    function
    | 'a' :: tr -> (S >> B) tr
    | 'b' :: tr -> tr
    | _ -> raise ParseError

and B =
    function
    | 'a' :: tr -> tr
    | 'b' :: tr -> (B >> matchInput 'a') tr
    | _ -> raise ParseError

let parse ts =
    match S ts with
    | [] -> printfn "Parse OK!"
    | _ -> raise ParseError

parse [ 'a'; 'b'; 'a' ]
parse [ 'a'; 'b'; 'b'; 'a'; 'a' ]
parse [ 'a'; 'b'; 'b' ]
