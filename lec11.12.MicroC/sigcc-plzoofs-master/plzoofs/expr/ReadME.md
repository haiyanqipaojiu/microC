### 表达式文法的分析器

```sh
<expr> ::= <term> | <term> + <expr>
             | <term> - <expr>
<term> ::= <factor> | <factor> * <term>
             | <factor> / <term>
<factor> ::= <id> | ( <expr> )
```

rdp/rdpexpr.fs 递归下降分析实现
lr/*  fslexyacc LR实现