## 文件说明

- `x86.fs` x86 指令定义
  - 寄存器、操作码、操作数
  - x86 汇编代码结构
  - x86 汇编字符串表示
  - `p1 - p4`  x86 示例代码
- `compile.fs` ast to x86 编译器
  - `compile1` 直接编译
  - `compile2` 先编译到栈式 IR，再编译到x86
- `platform.fs` win/linux 平台判定
- `main.fs` 主入口，`src1-src5` ast示例程序

程序编译出 calculator 程序，接受命令行参数X1,X2...X8，根据main.fs 中 src1-src5的抽象语法树对这8个参数求值。
- 可以自己修改main.fs修改几个表达式，看看程序是否能正确返回值
- x86.fs 有几个x86案例程序p1-p4，试试自己写几个类似的汇编程序

## 构建

`windows`下需安装 `gcc` https://jmeubank.github.io/tdm-gcc/

请修改 `main.fs`  测试其他的程序 `src1 - src5`

```sh
dotnet build  # 构建编译器

dotnet run > calculator.s # 编译到汇编代码

gcc -O0 -fno-asynchronous-unwind-tables -S runtime.c # 编译运行时库，运行时库收集并传递命令行参数给 calculator 中的 program 函数

gcc -g -o calculator runtime.s calculator.s  # 汇编、链接 并生成可执行程序

./calculator 1 2 3 4 5 6 7 8
1,2,3,4,5,6,7,8
program returned: 64

# one-liner 
dotnet build && dotnet run > calculator.s &&
gcc -O0 -fno-asynchronous-unwind-tables -S runtime.c &&
gcc -g -o calculator runtime.s calculator.s  &&
./calculator 1 2 3 4 5 6 7 8

```

## compile.fs

编译到x86 的主程序，分别有直接编译AST(compile1) 与间接编译到IR(compile2) 两种方式

### compile1

直接将 ast 编译为 x86 指令
- compile_exp 递归调用自身，并调用
  - compile_var 编译变量
  - compile_op  编译操作
  

- `let rec compile_exp (e:exp) : X86.ins list`
- `rax`用于编译时传递结果，编译表达式前，先将当前`rax`内容入栈
- `r10` 保存二元操作的临时中间结果

### compile2

先编译到 栈式IR ，然后编译到x86

- 栈式IR指令定义 `type insn`
- ast 编译到栈式IR `let rec flatten (e:exp) : program`
- 栈式IR编译到x86指令 `let rec compile_insn (i:insn) : X86.ins list`
  - 直接使用了 x86 的栈
    - 每条栈式IR 指令，都将运算结果上栈
    - rax ，r10 寄存器 做二元运算

## 调用约定call convention

由于参数传递方式的差异，`Windows`，`Linux` 下编译，会生成不同的 `.s`文件，具体请参考

- [MSx64](https://docs.microsoft.com/en-us/cpp/build/x64-calling-convention?view=msvc-160#parameter-passing)
- [AMD64](https://courses.cs.washington.edu/courses/cse378/10au/sections/Section1_recap.pdf)

## 调试工具 [Cutter](https://cutter.re/)

- 显示
  - Global Call Graph
  - Call Graph
- 开始调试，输入命令行参数
  - 支持反向调试 Reverse Debugging

## 来源

 [Upenn CIS341](https://www.seas.upenn.edu/~cis341/current/) lec05
