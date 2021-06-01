# 函数式语言实现-解释器

- 一阶函数式语言
- 类型检查
- 高阶函数式语言
- 类型推理

## 文件列表

- Absyn.fs 抽象语法定义
- FunLex.fsl  词法分析生成器文件
  - Funlex.fs  扫描器
- FunPar.fsy  语法分析生成器文件
  - FunPar.fs   分析器
- Fun.fs             一阶函数式语言Fun 实现
- HigherFun.fs  高阶函数式语言Fun 实现
- TypedFun.fs  类型检查实现
- TypeInference.fs  类型推理实现
- Parse.fs  辅助函数
  - ParseAndRun.fs  Fun 例子
  - ParseAndRunHigher.fs 高阶Fun 例子
  - ParseAndType.fs  类型推理例子
- main.fsx  F# 主脚本文件
- fun.fsproj 项目文件

## 构建

构建项目 `fun.fsproj`

- 构建扫描器、分析器
- 执行`main.fsx`脚本

```sh
dotnet build
dotnet run
```

## 命令行

### A. 加载求值器 Loading the micro-ML evaluator, with abstract syntax only

加载求值器，对抽象语法树求值

```sh
dotnet fsi Absyn.fs Fun.fs

open Absyn;;
open Fun;;
let res = run (Prim("+", CstI 5, CstI 7));;
#q;;
```

### B. 生成词法扫描器 语法分析器 运行求值器  Generating and compiling the lexer and parser, and loading them

一阶函数式语言

- 函数声明，函数调用
- 只有表达式，没有语句
  - 语法，语义 比命令式语言简洁
- 函数不可以作为参数

```sh
# 注意修改路径
# 生成扫描器
dotnet "C:\Users\gm\.nuget\packages\fslexyacc\10.2.0\build\/fslex/netcoreapp3.1\fslex.dll"  -o "FunLex.fs" --module FunLex --unicode FunLex.fsl

# 生成解析器
dotnet "C:\Users\gm\.nuget\packages\fslexyacc\10.2.0\build\/fsyacc/netcoreapp3.1\fsyacc.dll"  -o "FunPar.fs" --module FunPar FunPar.fsy

# 命令行运行程序
dotnet fsi 

#r "nuget: FsLexYacc";;  //命令行添加包引用
#load "Absyn.fs"  "FunPar.fs" "FunLex.fs" "Parse.fs" "Fun.fs" "ParseAndRun.fs";;

# 解析器
open Parse;;
let e1 = fromString "5+7";;
let e2 = fromString "let y = 7 in y + 2 end";;
let e3 = fromString "let f x = x + 7 in f 2 end";;

# 解析并求值
open ParseAndRun;;
run (fromString "5+7");;
run (fromString "let y = 7 in y + 2 end");;
run (fromString "let f x = x + 7 in f 2 end");;

```

### D. 高阶函数式语言

- 高阶函数式语言解释器
  - 函数可以做为参数

```sh
dotnet fsi 

#r "nuget: FsLexYacc";;   
#load "Absyn.fs"  "HigherFun.fs";;

open HigherFun;;
eval ex1 [];;
open Absyn;;
run (Letfun ("twice", "f",
Letfun ("g", "x", Call (Var "f", Call (Var "f", Var "x")), Var "g"),
Letfun ("mul3", "z", Prim ("*", Var "z", CstI 3),
Call (Call (Var "twice",Var "mul3"),CstI 2))));;
#q;;
```

### E. Using the lexer, parser and higher-order evaluator together

高阶函数式语言

- 词法，语法，解释器
- `twice`是高阶函数
  - mul3 是twice的参数

```sh
dotnet fsi 

#r "nuget: FsLexYacc";;  
#load "Absyn.fs"  "FunPar.fs" "FunLex.fs" "Parse.fs" "HigherFun.fs" "ParseAndRunHigher.fs";;

open ParseAndRunHigher;;
run (fromString @"let twice f = let g x = f(f(x)) in g end 
in let mul3 z = z*3 in twice mul3 2 end end");;
#q;;
```

F. 多态类型推理  Using the lexer, parser and polymorphic type inference together:

```sh
dotnet fsi 
#r "nuget: FsLexYacc";;  
#load "Absyn.fs"  "FunPar.fs" "FunLex.fs" "Parse.fs" "TypeInference.fs" "ParseAndType.fs"

open ParseAndType;;
inferType (fromString "let f x = 1 in f 7 + f false end");;
#q;;

```
