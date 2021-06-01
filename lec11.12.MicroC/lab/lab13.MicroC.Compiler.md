# 2020-2021学年第2学期

##  实 验 报 告

![](zucc.png)

-   课程名称: <u>编程语言原理与编译</u>

-   实验项目: <u>MicroC编译器</u>

-   专业班级: <u>软工1801</u>

-   学生学号: <u>31803112</u>

-   学生姓名: <u>梁泽生</u>

-   实验指导教师:<u>郭鸣</u>

## 实验要求

1. 完成下面各题目
2. 使用Markdown文件完成，推荐Typora
3. 使用[Git](https://learngitbranching.js.org/)工具管理作业代码、文本文件

## 实验内容

### 1.  阅读课件 [MicroC实现,编译器 - 编程语言与编译](http://sigcc.gitee.io/plc2021/#/07/microc.compiler)

### 2.  阅读`MicroC` 解释虚拟机指令集

-   LDI
-   将 栈帧上 某位置的值入栈 `s,i --> s,v ; v = s(i)`
-   `i`值为相对栈底的偏移量，从 `0` 开始索引, 如 `s(0)` 表示栈底的第一个值。
-   访问局部变量 `x` 的值: `x + 3`
-   STI
-   将 值写入栈上某个位置 `s,i,v ---> s,v ; s(i) <= v`
-   该指令用于赋值语句`x`: `x = y + 3`
-   GETBP
-   GETBP得到当前栈帧基地址bp
    bp+0对应函数第1个参数/局部变量v1
    bp+1......第2.....V2
-   GETSP
-   n > 0 增长栈 分配空间
-   n < 0 减少栈 释放空间
-   语句块内的变量声明 `{ int tmp; ....  }` 在块入口 生成 `INCSP 1`,块出口生成 `INCSP -1`
-   CALL
-   例 call m a
-   `m`是函数参数个数, `a` 是函数跳转目标地址
-   call 执行后，将 返回地址`r`，上个栈帧原来`bp`值保存到栈上,参数v1---vm拷贝到栈上。
-   新 `bp` 寄存器的值 指向当前栈帧基地址，即从函数参数开始的地址
-   RET
-   `RET m` 与 `CALL m` 对应，从`bp` 开始算，第`m`个值，就是`v`
-   `bp+0` --> v1

请说明上面指令的作用

### 3.  完成 lab中 `ReadME.md 任务`B`



```sh
# 编译microc编译器,并用microc编译器 编译 ex3.c 

dotnet clean  microc.fsproj
dotnet build  microc.fsproj

dotnet run -p microc.fsproj ex3.c

Micro-C Stack VM compiler v 1.0.0.1 of 2017-12-2
Compiling ex3.c to ex3.out
StackVM code:
[LDARGS; CALL (1,"L1"); STOP; Label "L1"; INCSP 1; GETBP; CSTI 1; ADD; CSTI 0;
 STI; INCSP -1; GOTO "L3"; Label "L2"; GETBP; CSTI 1; ADD; LDI; PRINTI; INCSP -1;
 GETBP; CSTI 1; ADD; GETBP; CSTI 1; ADD; LDI; CSTI 1; ADD; STI; INCSP -1;
 INCSP 0; Label "L3"; GETBP; CSTI 1; ADD; LDI; GETBP; CSTI 0; ADD; LDI; LT;
 IFNZRO "L2"; INCSP -1; RET 0]
Numeric code in file:
        ex3.out  
 Please run with VM.
 
# cat ex3.out  #ex3.out机器码
24 19 1 5 25 15 1 13 0 1 1 0 0 12 15 -1 16 43 13 0 1 1 11 22 15 -1 13 0 1 1 13 0 1 1 11 0 1 1 12 15 -1 15 0 13 0 1 1 11 13 0 0 1 11 7 18 18 15 -1 21 0

#gcc machine.c -o machine  #用gcc编译器生成生成c 虚拟机 machine
# ./machine ex3.out 10    #用虚拟机machine 执行ex3.out机器码
0 1 2 3 4 5 6 7 8 9 Used   0.000 cpu seconds
```

// micro-C example 3

void main(int n) { 
  int i; 
  i=0; 
  while (i < n) { 
    print i; 
    i=i+1;
  } 
}

### 4.  请阅读 `ex9.trace.0.txt` `ex9.trace.3.txt`理解 源代码 和 指令的对应关系

```sh
 ./machine.exe ex9.out 0
 ./machine.exe -trace ex9.out 3
```
[运行示例参见](http://sigcc.gitee.io/plc2021/#/05/microc.compiler?id=the-code-generated-for-ex9c)

栈帧下标从0开始
 |
 v
[ ]{0: INCSP 1}               // int t;  没有使用的全局变量
[ 0 ]{2: LDARGS}              // main 命令行参数 i = 0    
[ 0 0 ]{3: CALL 1 7}          // 调用 位于7 的 main(0) 函数 参数个数为1
                              // 栈帧内容 [6 -999 0] 的解释
                              // CALL 1 7 拿掉栈上1个参数 0 
                              // 放上 返回地址: 6
                                      old bp: -999   默认初始bp值为 -999
                                       参数i : 0
[ 0 6 -999 0 ]{7: INCSP 1}    // 给 int  r; 留空间
[ 0 6 -999 0 0 ]{9: GETBP}    // bp = 3   
[ 0 6 -999 0 0 3 ]{10: CSTI 0}   bp + 0   main参数 i的位置
[ 0 6 -999 0 0 3 0 ]{12: ADD}
[ 0 6 -999 0 0 3 ]{13: LDI}   //  得到参数 i=0
[ 0 6 -999 0 0 0 ]{14: GETBP}
[ 0 6 -999 0 0 0 3 ]{15: CSTI 1}  bp+1  main局部变量r的位置 
[ 0 6 -999 0 0 0 3 1 ]{17: ADD}   bp+1   &r=4
[ 0 6 -999 0 0 0 4 ]{18: CALL 2 35}   //调用 位于 35 的fac(0,4)  4是r的在栈上的地址 &r
                                      // CALL 2 35 拿掉栈上的两个参数 0 4
[ 0 6 -999 0 0 21 3 0 4 ]{35: GETBP}
[ 0 6 -999 0 0 21 3 0 4 7 ]{36: CSTI 0}
[ 0 6 -999 0 0 21 3 0 4 7 0 ]{38: ADD}
[ 0 6 -999 0 0 21 3 0 4 7 ]{39: LDI}   // 得到 fac 参数n的值 =0
[ 0 6 -999 0 0 21 3 0 4 0 ]{40: CSTI 0}   
[ 0 6 -999 0 0 21 3 0 4 0 0 ]{42: EQ}     //  n = 0 ?
[ 0 6 -999 0 0 21 3 0 4 1 ]{43: IFZERO 57}  // n != 0  else 转到 57 条件为FALSE 则跳转
[ 0 6 -999 0 0 21 3 0 4 ]{45: GETBP}         // n=0 执行
[ 0 6 -999 0 0 21 3 0 4 7 ]{46: CSTI 1}
[ 0 6 -999 0 0 21 3 0 4 7 1 ]{48: ADD}  
[ 0 6 -999 0 0 21 3 0 4 8 ]{49: LDI}    // *res的左值 = 4  
[ 0 6 -999 0 0 21 3 0 4 4 ]{50: CSTI 1}
[ 0 6 -999 0 0 21 3 0 4 4 1 ]{52: STI}  // *res = 1  注意: STI对栈上位置4 赋值 1
[ 0 6 -999 0 1 21 3 0 4 1 ]{53: INCSP -1}
            ^^^
[ 0 6 -999 0 1 21 3 0 4 ]{55: GOTO 97}
[ 0 6 -999 0 1 21 3 0 4 ]{97: INCSP 0}  //fac 没有局部变量
[ 0 6 -999 0 1 21 3 0 4 ]{99: RET 1}    //从 fac(0,4)返回 撤销栈帧  return; RET m-1                                         //                    fac参数个数 m=2
[ 0 6 -999 0 1 4 ]{21: INCSP -1}       //  fac(0); 丢弃 fac(0);的值 
[ 0 6 -999 0 1 ]{23: GETBP}             
[ 0 6 -999 0 1 3 ]{24: CSTI 1}
[ 0 6 -999 0 1 3 1 ]{26: ADD}          //得到r的偏移地址  
[ 0 6 -999 0 1 4 ]{27: LDI}           // 得到main 的值 r
[ 0 6 -999 0 1 1 ]{28: PRINTI}       //输出 r
1 [ 0 6 -999 0 1 1 ]{29: INCSP -1}   // 丢弃 PRINTI 的值 (r值) 
[ 0 6 -999 0 1 ]{31: INCSP -1}  // 丢弃 main 的局部变量 r
[ 0 6 -999 0 ]{33: RET 0}          //  从main(0) 中 return; RET m-1
                                             // main 参数个数 m=1 
[ 0 0 ]{6: STOP}             // 执行结束                  





### 5.  请用 运行下面的命令,仿照4 写出 虚拟机代码的注释

-   machine.exe ex5.out 5
-   machine -trace ex5.out 5

### 6. 选做下面 1 项或多项

#### Exercise 8.3

This abstract syntax for preincrement `++e` and predecrement `--e` was
introduced in Exercise 7.4:

```fsharp
type expr =
...
| PreInc of access (* C/C++/Java/C ++i or ++a[e] *)
| PreDec of access (* C/C++/Java/C --i or --a[e] *)
```
Modify the compiler (`function cExpr`) to generate code for `PreInc(acc)` and
`PreDec(acc)`. To parse micro-C source programs containing these expressions,
you also need to modify the lexer and parser.
It is tempting to expand `++e` to the assignment expression `e = e+1`, but that
would evaluate e twice, which is wrong. Namely, e may itself have a side effect, as
in `++arr[++i]`.
Hence e should be computed only once. For instance, `++i` should compile to
something like this: 

`<code to compute address of i>;, DUP, LDI, CSTI 1, ADD, STI`, 

where the address of `i` is computed once and then duplicated.
Write a program to check that this works. If you are brave, try it on expressions of
the form `++arr[++i]` and check that i and the elements of arr have the correct
values afterwards.

#### Exercise 8.6

Extend the lexer, parser, abstract syntax and compiler to implement\
switch statements such as this one:

```fsharp

switch (month) {
    case 1:
        { days = 31; }
    case 2:
        { days = 28; if (y%4==0) days = 29; }
    case 3:
        { days = 31; }
}
```

Unlike in C, there should be no fall-through from one case to the next: after the

last statement of a case, the code should jump to the end of the switch statement.

The parenthesis after switch must contain an expression. The value after a case

must be an integer constant, and a case must be followed by a statement block.

A switch with n cases can be compiled using n labels, the last of which is at the

very end of the switch. For simplicity, do not implement the break statement or

the default branch.

#### Exercise 8.7

(Would be convenient) Write a disassembler that can display a machine code program in a more readable way. You can write it in Java, using a variant

of the method insname from Machine.java.

#### Exercise 8.9

Extend the language and compiler to accept initialized declarations\
such as
```fsharp
int i = j + 32;
```
Doing this for local variables (inside functions) should not be too hard. For global
ones it requires more changes.