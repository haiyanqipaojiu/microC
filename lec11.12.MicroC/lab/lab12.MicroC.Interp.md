# 2020-2021学年第2学期

##  实 验 报 告

![](zucc.png)

-   课程名称: <u>编程语言原理与编译</u>

-   实验项目: <u>MicroC解释器</u>

-   专业班级: <u>软工1801</u>

-   学生学号: <u>31803112</u>

-   学生姓名: <u>梁泽生</u>

-   实验指导教师:<u>郭鸣</u>

## 实验要求

1. 完成下面各题目
2. 使用Markdown文件完成，推荐Typora
3. 使用[Git](https://learngitbranching.js.org/)工具管理作业代码、文本文件

## 实验内容


### 1. 阅读课件 [MicroC实现,解释器 - 编程语言与编译](http://sigcc.gitee.io/plc2021/#/06/microc.interp)

###  2. 阅读 [计算的本质](https://bb.zucc.edu.cn/bbcswebdav/users/j04014/books/Understanding.Computation)第1 2 3章

-   [计算的本质](https://bb.zucc.edu.cn/bbcswebdav/users/j04014/books/Understanding.Computation)
-   请说明大步语义，小步语义的区别
-   小步语义定义了一种在一个计算步骤中一次评估表达式的方法。从形式上来说，表达语言的一个小步语义是一个关系称为*归约*关系。小步语义详细描述了表达式所发生的事情。它可以给出无限链的精确描述，甚至是非终止程序。终止程序是这样的程序，即以*值*终止，使得终止。
-   大步语义在中间。表达式语言和一组值上的一大步语义是关系。它将表达式与它的值相关（如果语言是不确定的，则可能有多个值）。通常，特殊值用于非终止表达式。

### 3. 阅读课件 2.call.by.parameters.pdf

- 请说明 Call by reference, Call by value的区别

-  Call by reference：

- 参数在函数调用之前进行计算。

  函数接收一个参数的副本对函数中的变量所做的改变在外部是不可见的优点:速度缺点:不稳定

-    Call by value:

-   参数在函数调用之前进行计算

    函数接收参数的副本

    变量作为指针传递

    对函数中变量所做的更改在外部可见

    优点:速度快，节省一些内存，副作用也有可能

    当你想要它们的时候

    缺点:副作用是可能的，当你不想要他们

-   (选做)请说明什么是Call by need 

### 4. 阅读简单命令式语言代码`imp.zip`(自选)

-   理解命令式语言**存储模型**
-   写出函数`setSto` `getSto` 的类型声明
-   请说明 命令式语言与函数式语言**执行模型**的不同之处

### 5. 阅读`MicroC` 解释器代码

-   请说明 抽象语法树中 对**左值和右值**的表示方式
-   `右值`是`常规值` ,如赋值语句中右边的 `17, true` Rvalue is "normal" value, right-hand side of assignment: 17, true
-   `左值`是`位置`,如赋值语句左边的 `x,a[2]` Lvalue is "location", left-hand side of assignment: x, a[2]
-   请说明 表达式`a[i] = x` **左值求值**和**右值求值**的过程,需要调用解释器的哪些方法
-   a[i]左值
-   x右值
-   请写出 `MicroC` 解释器中以下3个函数的类型声明,说明每个参数的含义

```fsharp
eval
// 求值函数  求出expr值 返回结果 int 与被修改的store
eval: expr -> locEnv -> gloEnv -> store -> int * store  

exec
//执行函数 执行 stmt 返回被修改的store
let rec exec stmt locEnv gloEnv store : store

access
//右值函数，返回右值地址address和store
access: access -> locEnv -> gloEnv -> store -> address * store 
```

-   用解释器 运行 `ex9.c` 给出运行结果. 说明递归调用过程.

-   gitee.com/sigcc/plzoofs microc目录 完成 `ReadME.md`中的A部分.

###  预习下章 micro C stack machine 指令系统 重点理解

```bash
LDI
STI
GETBP
GETSP
CALL
RET
等指令
```

请使用编译器 输出 ex9.c的指令代码

// micro-C example 9 -- return a result via a pointer argument

void main(int i) {
  int r;
  fac(i, &r);
  print r;
}

void fac(int n, int *res) {
  // print &n;			// Show n's address
  if (n == 0)
    *res = 1;
  else {
    int tmp;
    fac(n-1, &tmp);
    *res = tmp * n;
  }
}



```fsharp

```