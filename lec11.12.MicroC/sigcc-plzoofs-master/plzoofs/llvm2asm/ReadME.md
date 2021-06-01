## 文件说明

- ll.fs llvmlite 类型定义

- lllexer.fsl llvmlite 词法分析定义

- llparser.fsy llvmlite 语法分析定义

- x86.fs x86 汇编类型定义
  - string_of_prog 输出 asm 字符串

- backend.fs  llvm2x86 编译器
  - compile_prog 编译x86入口

- driver.fs 驱动程序

- main.fs 主程序
  - process_ll_file 处理 .ll   文件入口

- platform.fs 平台相关处理

- args.fs  命令行参数处理

- llprograms 目录 llvm 示例程序

- output目录 汇编代码输出目录

## 软件安装

- [dotnet 5](https://dotnet.microsoft.com/download/dotnet/5.0)
- Windows gcc
  - [TDM-GCC 9.2.0](https://jmeubank.github.io/tdm-gcc/articles/2020-03/9.2.0-release)

## 构建步骤

```sh
# 构建
dotnet build

# 默认编译到 output/*.s , -g 显示调试信息
dotnet run ./llprograms/call1.ll
dotnet run -g ./llprograms/call1.ll

# Windows 查看输出，汇编、链接并运行
cd output
gcc -O0 -o call1  call1.s
cat call1.s
call1
echo %errorlevel%
# --> 17


# linux 查看输出，汇编、链接并运行
cd output
gcc -O0 -o call1  call1.s
cat call1.s
./call1
echo $?
# --> 17

```

## Cutter

可在Cutter中查看反汇编代码，和反编译的`.c`代码

## 参考资源

|      | Upenn CIS 341 - Compilers      |                                                              |
| ---- | ------------------------------ | ------------------------------------------------------------ |
|      | IRs III / LLVM                 | [lec08.pdf](https://www.seas.upenn.edu/~cis341/current/lectures/lec08.pdf) |
|      | Structured Data in the LLVM IR | [lec09.pdf](https://www.seas.upenn.edu/~cis341/current/lectures/lec09.pdf) |
