// recursive descent parser for grammer
// G14:
// 1. S → a A S
// 2. S → b
// 3. A → c A S
// 4. A → ε     Follow(A) = {a,b}

exception ParseError

// let matchInput   t ts =
//     match ts with
//     t::trr-> trr
//     | _-> raise   ParseError

let rec S =
    function
    | 'a' :: tr -> (A >> S) tr //rule 1
    | 'b' :: tr -> tr //rule 2
    | _ -> raise ParseError

and A =
    function
    | 'c' :: tr -> (A >> S) tr // rule 3
    | 'a' :: tr -> 'a' :: tr // rule 4
    | 'b' :: tr -> 'b' :: tr // rule 4
    | _ -> raise ParseError

let parse ts =
    match S ts with
    | [] -> printfn "Parse OK!"
    | _ -> raise ParseError

// accept "acbb"
parse [ 'a'; 'c'; 'b'; 'b' ]
parse [ 'a'; 'b'; 'b'; 'a'; 'a' ]
