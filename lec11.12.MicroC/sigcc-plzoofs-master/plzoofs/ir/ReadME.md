## 说明

- ir1-ir5 **表达能力逐渐增加的** 中间表示IR
  - 表达式
  - 命令
  - 控制流
  - 函数调用
  - 全局声明
- run.fsx 执行脚本
- ir-by-hand.fs  F# 模拟中间语言

```sh
dotnet build
dotnet run
```

## Intermediate Representations

### IR1: Expressions 表达式

- simple arithmetic expressions, immutable global variables
  - 操作 `Add Mul`
  - 操作数 `Id Const Var`
    - 临时变量 `Id of uid`
    - 全局变量 `Var of string`
- 将嵌套表达式 转换为 `let` 指令序列
  - `Let of uid * bop * opn * opn`
- 建立**临时变量**保存嵌套表达式的中间结果
  - 临时变量按序编号 0 1 2 ...（对应到寄存器）
  - SSA 单赋值形式
- 重要思想
  - 高级语言的高层表示向机器语言的底层表示的转换
    - 嵌套的语法树 ---> 线性指令序列
    - 无限数目的变量 ---> 有限数目的寄存器
    - 变量名称  --->  内存地址  或 寄存器
    - 复杂的数据类型 --->  无类型 （整型、浮点型、位操作、字节操作）
    - 高级语言特性（闭包、垃圾回收、协程、安全检查、语言库...）---> 运行时支持
  - 如何通过**层次结构**，实现上述转换



```F#
type uid = int        (* Unique identifiers for temporaries. *)
type var = string      
type opn =            (* syntactic values / operands *)
    | Id of uid
    | Const of int64
    | Var of var
type bop =            (* binary operations *)
    | Add
    | Mul
(* instructions *)
(* note that there is no nesting of operations! *)
type insn = Let of uid * bop * opn * opn
type program = { insns: insn list; ret: opn }
```

### IR2: Commands 命令

- global mutable variables
  - `type var = string`
- commands for update and sequencing
- load store 结构
  - `Load of uid * var` 从内存变量（内存地址）加载到临时变量（寄存器）
  - 运算的操作数只是操作临时变量
  - `Store of var * opn` 将临时变量保存到内存变量

```F#
    type uid = int             (* Unique identifiers for temporaries. *)
    type var = string
    type opn =
        | Id of uid
        | Const of int64
    type bop =
        | Add
        | Mul
    type insn =
        | Let of uid * bop * opn * opn
        | Load of uid * var          // NEW
        | Store of var * opn         // NEW
    type program = { insns: insn list }
```

### IR3: Local control flow 局部控制流

- conditional commands & while loops
  - 比较指令 `ICmp of uid * cmpop * opn * opn`
- basic blocks
  - 基本块内部只包括代码序列 `insn list`
  - 基本块终结指令`terminator`是转移指令（条件转移 `Cbr`、无条件转移 `Br`、返回`Ret`）
- cfg 控制流图组成
  - 唯一的 `entry block`
  - `lbl * block list`

```F#
type uid = string          
type var = string
type lbl = string                          // NEW
type opn =
    | Id of uid
    | Const of int64
type bop =
    | Add
    | Mul
type cmpop =                (* comparison operations*) // NEW
    | Eq
    | Lt
type insn =
    | Let of uid * bop * opn * opn
    | Load of uid * var
    | Store of var * opn
    | ICmp of uid * cmpop * opn * opn   // NEW
type terminator =                       // NEW
    | Ret
    | Br of lbl              (* unconditional branch *)
    | Cbr of opn * lbl * lbl (* conditional branch *)
type block =                 (* Basic blocks *)      // NEW
    { insns: insn list
      terminator: terminator }
(* Control Flow Graph: a pair of an entry block and a set labeled blocks *)
type cfg = block * (lbl * block) list    // NEW 唯一入口block
type program = cfg                       // NEW
```

### IR4: Procedures (top-level functions)过程/函数

- 函数声明
  - 名称  `fn_name`
  - 参数列表 `param: uid list`
- local state 局部变量
  - `Alloca of uid` 在栈上分配局部变量
- call stack 调用堆栈
  - `Call of uid * fn_name * (opn list)`
  - `Call` 不是 terminator

```F#
type uid = string (* Unique identifiers for temporaries. *)
type var = string
type lbl = string
type fn_name = string
type opn =
    | Id of uid
    | Const of int64
type bop =
    | Add
    | Mul
type cmpop =
    | Eq
    | Lt
type insn =
    | Let of uid * bop * opn * opn
    | Load of uid * var
    | Store of var * opn
    | ICmp of uid * cmpop * opn * opn
    | Call of uid * fn_name * (opn list)  // NEW
    | Alloca of uid                       // NEW
type terminator =
    | Ret
    | Br of lbl                (* unconditional branch *)
    | Cbr of opn * lbl * lbl   (* conditional branch *)
type block =
    { insns: insn list
      terminator: terminator }
type cfg = block * (lbl * block) list
type fdecl =                               // NEW
    { name: fn_name
      param: uid list
      cfg: cfg }
type program = { fdecls: fdecl list }      // NEW
```

### IR5: ”almost” LLVM IR

- 全局变量
- 全局函数

```F#
type uid = string (* Unique identifiers for temporaries. *)
type lbl = string
type gid = string
type opn =
    | Id of uid
    | Const of int64
type bop =
    | Add
    | Mul
type cmpop =
    | Eq
    | Lt
type insn =
    | Binop of bop * opn * opn (* Rename let to binop *)
    | Load of gid
    | Store of gid * opn
    | ICmp of cmpop * opn * opn
    | Call of gid * (opn list)
    | Alloca
type terminator =
    | Ret
    | Br of lbl (* unconditional branch *)
    | Cbr of opn * lbl * lbl (* conditional branch *)
type block =
    { insns: (uid * insn) list
      terminator: terminator }
type cfg = block * (lbl * block) list
type gdecl = GInt of int64                 // NEW
type fdecl = { param: uid list; cfg: cfg } // NEW
type program =
    { gdecls: (gid * gdecl) list           // NEW
      fdecls: (gid * fdecl) list }         // NEW
```

## 参考资源

|      | Upenn CIS 341 - Compilers      |                                                              |
| ---- | ------------------------------ | ------------------------------------------------------------ |
|      | IRs I                  | [lec06.pdf](https://www.seas.upenn.edu/~cis341/current/lectures/lec06.pdf) |
|      | IRs II  | [lec07.pdf](https://www.seas.upenn.edu/~cis341/current/lectures/lec07.pdf) |