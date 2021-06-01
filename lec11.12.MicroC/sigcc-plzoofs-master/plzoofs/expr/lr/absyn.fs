module Absyn

type expr =
    | Term_as_Expr of term
    | Plus_Expr of (term * expr)
    | Minus_Expr of (term * expr)

and term =
    | Factor_as_Term of factor
    | Mult_Term of (factor * term)
    | Div_Term of (factor * term)

and factor =
    | Id_as_Factor of string
    | Parenthesized_Expr_as_Factor of expr
