module rdpexpr

(*
该文法比较简单，每个分支 可以用 符号区分，没有涉及到 First Follow 集的计算
<expr> ::= <term> | <term> + <expr>
             | <term> - <expr>
<term> ::= <factor> | <factor> * <term>
             | <factor> / <term>
<factor> ::= <id> | ( <expr> )

Tokens: +  -  *  /  (  )  <id>
*)
type token =
    | Id_token of string
    | Left_parenthesis
    | Right_parenthesis
    | Times_token
    | Divide_token
    | Plus_token
    | Minus_token

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

// expr : token list -> (expr * token list)
// term : token list -> (term * token list)
// factor : token list -> (factor * token list)
// 每个递归分析函数对应一个文法的非终结符号

// 每个函数返回的是 元组类型
// （解析结果ast，余下的token）
let rec expr tokens =
    (match term tokens with
     | (term_parse, tokens_after_term) ->
         (match tokens_after_term with
          | (Plus_token :: tokens_after_plus) ->
              (match expr tokens_after_plus with
               | (expr_parse, tokens_after_expr) -> (Plus_Expr(term_parse, expr_parse), tokens_after_expr))
          | (Minus_token :: tokens_after_minus) ->
              (match expr tokens_after_minus with
               | (expr_parse, tokens_after_expr) -> (Minus_Expr(term_parse, expr_parse), tokens_after_expr))
          | _ -> (Term_as_Expr term_parse, tokens_after_term)))

and term tokens =
    (match factor tokens with
     | (factor_parse, tokens_after_factor) ->
         (match tokens_after_factor with
          | (Times_token :: tokens_after_times) ->
              (match term tokens_after_times with
               | (term_parse, tokens_after_term) -> (Mult_Term(factor_parse, term_parse), tokens_after_term))
          | (Divide_token :: tokens_after_divide) ->
              (match term tokens_after_divide with
               | (term_parse, tokens_after_term) -> (Div_Term(factor_parse, term_parse), tokens_after_term))
          | _ -> (Factor_as_Term factor_parse, tokens_after_factor)))

and factor tokens =
    (match tokens with
     | (Id_token id_name :: tokens_after_id) -> (Id_as_Factor id_name, tokens_after_id)
     | (Left_parenthesis :: tokens) ->
         (match expr tokens with
          | (expr_parse, tokens_after_expr) ->
              (match tokens_after_expr with
               | Right_parenthesis :: tokens_after_rparen ->
                   (Parenthesized_Expr_as_Factor expr_parse, tokens_after_rparen)
               | _ -> raise (Failure "No matching rparen"))))

// (a+b)*c-d
let exp =
    [ Left_parenthesis
      Id_token "a"
      Plus_token
      Id_token "b"
      Right_parenthesis
      Times_token
      Id_token "c"
      Minus_token
      Id_token "d" ]

expr exp

// ( a + b * c - d
let err =
    [ Left_parenthesis
      Id_token "a"
      Plus_token
      Id_token "b"
      Times_token
      Id_token "c"
      Minus_token
      Id_token "d" ]

expr err

// a + b ) * c - d
let partial =
    [ Id_token "a"
      Plus_token
      Id_token "b"
      Right_parenthesis
      Times_token
      Id_token "c"
      Minus_token
      Id_token "d" ]

expr partial

(*

val it : expr * token list =
   // a + b
  (Plus_Expr
     (Factor_as_Term (Id_as_Factor "a"),
      Term_as_Expr (Factor_as_Term (Id_as_Factor "b"))),
  // 剩下的 tokens
   [Right_parenthesis; Times_token; Id_token "c"; Minus_token; Id_token "d"])
// 返回 元组 （部分 Ast，余下的Tokens）

*)
