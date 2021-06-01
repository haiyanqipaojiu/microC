
## 说明

源文件列表

```sh
program.fs 主程序入口
SimpleExpr.fs 简单的表达式解释器，操作AST
SimpleExprVar.fs 带变量的表达式解释器
Intcomp.fs    let 表达式 
              自由变量与封闭性
              变量代换，变量捕获
              将AST中的名称编译为地址 （De Bruijn Index） 编译环境
Svm.fs        栈式虚拟机编译器，编译生成 栈式虚拟机指令
SvmWithVar.fs 带变量的栈式虚拟机编译器

```

运行命令如下

```sh
dotnet run -p svm.fsproj
```

请仔细查看 `program.fs`，特别是理解`SvmWithVar.fs`，掌握变量如何编译到栈上的。
- Simple Interpreter    
- Simple Interpreter With Var
- Intcomp let 表达式，名称到地址的转换
- Svm Compiler
- Svm Compiler With Var