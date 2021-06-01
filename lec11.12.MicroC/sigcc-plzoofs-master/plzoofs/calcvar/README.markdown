A simple arithmetic calculator with variables. It handles addition, subtraction,
negation, multiplication and division of integers.

The language is interactive, it cannot load files. Example interaction:

    calc_var -- programming languages zoo
    Type Ctrl-D to exit
    calc_var> 2+2
    4
    calc_var> x=10
    calc_var> y=x+20
    calc_var> 3 * x + 7
    37
    calc_var> z
    Error: unknown variable z
    calc_var> x=20
    calc_var> x + y
    50
    calc_var> x / 0
    Error: division by zero

## 带变量的四则运算计算器

## 构建命令

```sh

dotnet clean
dotnet run 
dotnet run -g 调试模式，显示词元、语法树

```
## 文件列表
- calcvar.fsproj 项目文件
- syntax.fs 抽象语法树
- lexer.fsl 词法说明文件
  - lexer.fs fslex 自动生成的词法扫描器程序
- parser.fsy 语法说明文件
  - parser.fs fsyacc 自动生成的语法分析器程序
- eval.fs  求值器/解释器
- main.fs 主程序  


注意理解 lexer.fsl,parser.fsy文件的内容 :

- lexer.fsl lexeme 函数，fslexyacc 库提供的函数

    - `let lexeme = LexBuffer<_>.LexemeString` 从lexbuf 得到当前的字符串

- parser.fsy 的优先级，结合性定义的方法

  ```sh
  /* Precedence and associativity 
  优先级从上到下，依次递增
  */ 
  %left PLUS MINUS  //加法，减法，优先级相同，左结合 ，最低优先级
  %left TIMES DIVIDE //乘法，除法，优先级相同，比减法，减法高
  %nonassoc UMINUS   //单目减是非结合，最高优先级   
  //表达式 -2*3 先算 -2再算*3 ，尽管运算结果同，但是语法树不同。
  //请问如果，让*先算，如何修改优先级，请自己修改parser.fsy文件并实验
  ```
  
- 注意 单目减操作（负号）的产生式规则，最后的优先级指示 `%prec UMINUS`

    ```sh
     | MINUS  expression %prec UMINUS       { Negate $2 }
                // %prec UMINUS 表示该条产生式规则的优先级与 UMINUS 相同
    ```
    
- 由于优先级的关系，词法分析器里面的同一个 `MINUS` 词元，对应到语法树，可以是 `Minus 节点`，或者是`Negate节点`，如下 所示

    ```sh
    dotnet run -g 
calc_var> - 5 - 5*3
"MINUS"    //词元同
"NUMERAL 5"
"MINUS"    //词元同
"NUMERAL 5"
"TIMES"
"NUMERAL 3"
"EOF"
-20
    Expression (Minus (Negate (Numeral 5), Times (Numeral 5, Numeral 3)))
                ^^^^   ^^^^^^^^^^^^^^^^^^
                语法树节点不同
    
    ```
## 查看环境中值的存储

函数式语言中，默认情况下，环境中的绑定的值是不可更改的，原来声明的y = 3 会一直保留在环境中，`main.fs lookup`函数查找环境中最近的值。

```sh

~ calcvar>dotnet run -g
argv: [|"-g"|]
calc_var>y = 3
"VARIABLE "y""
"EQUAL"    
"NUMERAL 3"
"EOF"      
(Definition ("y", Numeral 3), [("y", 3)])
calc_var>y = y + 3
"VARIABLE "y""
"EQUAL"
"VARIABLE "y""
"PLUS"
"NUMERAL 3"
"EOF"
(Definition ("y", Plus (Variable "y", Numeral 3)), [("y", 
6); ("y", 3)])  #  ("y", 3) 一直都在
calc_var>x = y + 3  # x赋值，查找y的绑定，最近值是6
"VARIABLE "x""
"EQUAL"
"VARIABLE "y""
"PLUS"
"NUMERAL 3"
"EOF"
(Definition ("x", Plus (Variable "y", Numeral 3)),        
 [("x", 9); ("y", 6); ("y", 3)])    
calc_var>x     # 访问变量 x
"VARIABLE "x""
"EOF"
9
(Expression (Variable "x"), [ ("x", 9); ("y", 6); ("y", 3)])
```
## %prec修饰符

`%prec`修饰符声明了某个**规则的优先级**，通过指定某个终结符而该终结符的优先级将用于该规则。没有必要在该规则出现这个终结符。（就是说这个**终结符可以是臆造的**，在系统中可能并没有实际的对应体，只是为了**用于指定该规则的优先级**）。下面是优先级的语法：

**%prec terminal-symbol**

并且这个声明必须写在该规则的后面（看下面的例子）。这个声明的效果就是把该**终结符**所具有的优先级赋予**该规则**，而这个优先级将会覆盖在普通方式下推断出来的该规则的优先级。这个更改过的规则优先级会影响规则如何解决冲突。

下面就是解决单目减号的优先级问题。首先，定义一个名为`UMINUS`的虚构的终结符，并为之声明一个优先级。实际上并没有这种类型的终结符，但是这个终结符仅仅为其的优先级服务。

```sh

%left '+' '-'

%left '*'

%left UMINUS
```

现在`UMINUS`的优先级可如此地用于规则：

```sh
exp: ...

| expr '-' exp

...

| '-' exp %prec UMINUS
```