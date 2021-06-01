# 说明

## microsml 特性

- 高阶函数
- 类型检查
- 类型推理
- 异常处理
- 列表
- 尾递归
- 运行时支持
  - 库程序
  - 堆分配
  - 垃圾回收
- 编译到 栈式虚拟机
- 基于续算（continuation）的优化

## 文件列表

Absyn.fs  抽象语法
Comp.fs  编译器
Contcomp.fs 优化编译器
FunLex.fsl  词法声明
FunPar.fsy  语法声明
HigherFun.fs 高阶函数
Machine.fs  虚拟机指令
MicroSMLC.fs 编译入口
ParseTypeAndRun.fs 驱动
TypeInference.fs 类型推理
msmlmachine.c 虚拟机 运行时  垃圾回收
main.fsx 测试脚本
queen.fsx 测试脚本
exn*.sml 测试案例代码
exn*.out 输出虚拟机指令，数字形式

## 构建

```sh
#构建并运行
dotnet build -t:ccrun

# 分步构建
#编译 ex01.sml
dotnet run ex01.sml

#优化编译
dotnet run -opt ex01.sml

#编译并求值
dotnet run -eval ex01.sml

#编译并输出AST
dotnet run -verbose test.sml

#编译虚拟机 (32位)
gcc -m32 -o msmlmachine.exe msmlmachine.c

#执行字节码
msmlmachine.exe ex01.out

#执行字节码，并显示跟踪信息
msmlmachine.exe -trace ex01.out 

#生成续算优化编译器
microsml.fsproj 注释 Comp.fs 使用 ContComp.fs 
```

## fsx 脚本

```sh
dotnet fsi main.fsx
```
