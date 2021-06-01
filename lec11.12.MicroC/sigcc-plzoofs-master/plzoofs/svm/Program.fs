// open SimpleExpr
// open SimpleExprVar
// open SvmWithVar

[<EntryPoint>]
let main argv =

    //无变量表达式语法树求值
    // 3 - 4  -> -1
    printfn "Main:\nSimple interpreter:"
    let e2 = SimpleExpr.e2 
    printfn "ast:e2: %A"  e2
    printfn "eval:e2: %A" ( SimpleExpr.eval e2)
    printfn "evalm:e2: %A" ( SimpleExpr.evalm e2) //改变求值语义
    SimpleExpr.evalm e2 |> printfn "evalm:e2: %A" // |> 管道操作符，简化代码

    //带变量的表达式时语法树求值，此时需要环境 env 提供变量-值的绑定
    let env = [("a", 3); ("b", 5); ("c", 7); ("baf", 666); ("b", 11)]

    printfn "\nSimple interpreter with Var\nenv: %A"  env
    // b --> 5 以先查找到的为准
    // a --> 3
    // b * 9 + a  -> 48
    let varexp = SimpleExprVar.e3
    let varexpv = SimpleExprVar.eval varexp env
    printfn "exp: b * 9 + a  -> 48"
    printfn "ast:varexp: %A"  varexp
    printfn "eval:varexp: %A"  varexpv
    
(*
let z = 17  in z + z
               ^^^^^
              自由变量
let e1 = Let("z", CstI 17, Prim("+", Var "z", Var "z"))
抽象语法树 声明变量用 let （z是左值） ，使用变量用 Var（z是右值）

let z = 22 * z in 5 * z
             ^^       ^^
          自由变量    绑定变量
let e8 = Let("z", Prim("*", CstI 22, Var "z"), Prim("*", CstI 5, Var "z"));;

*)

    printfn "\nLet expression:"
    printfn "src: %A" Intcomp.e2src
    printfn "ast: %A" Intcomp.e2
    printfn "eval result: %A \n" (Intcomp.eval Intcomp.e2 [])
    printfn "src: %A" Intcomp.e8src
    printfn "src': %A" Intcomp.e8src'
    printfn "ast: %A" Intcomp.e8
    let env = [("z",1)]
    printfn "env: %A" env
    printfn "eval result: %A " (Intcomp.eval Intcomp.e8 env)
    
    printfn "\n表达式的替换: "
    printfn $"exp: {Intcomp.e6}"
    printfn $"env1:{Intcomp.e6env1} //常量"
    printfn $"s1:{Intcomp.e6s1}\n"
    printfn $"env2:{Intcomp.e6env2} //表达式"
    printfn $"s2:{Intcomp.e6s2}\n"
    printfn $"env3:{Intcomp.e6env3} //带变量的表达式"
    printfn $"s3:{Intcomp.e6s3}"
    
    printfn "\n表达式封闭性: "
    printfn $"src: {Intcomp.e1src}"
    printfn $"ast: {Intcomp.e1}\nclosed: {Intcomp.closed1 Intcomp.e1}\n"
    printfn $"src: {Intcomp.e8src}"
    printfn $"exp: {Intcomp.e8}\nclosed: {Intcomp.closed1 Intcomp.e8}"

    printfn "\n表达式中的自由变量: "
    printfn $"exp: {Intcomp.e1}\nfreevars: {Intcomp.freevars Intcomp.e1}\n"
    printfn $"exp: {Intcomp.e8}\nfreevars: {Intcomp.freevars Intcomp.e8}"

    printfn "\n表达式替换中的自由变量捕获: "
    printfn $"exp: {Intcomp.e9}"
    printfn $"env1:{Intcomp.e9env1}"
    printfn $"s1:{Intcomp.e9s1}// 替换错误！z 从自由变量，变成了绑定变量\n"
    printfn $"env2:{Intcomp.e9env2}"
    printfn $"s2:{Intcomp.e9s2}\n"

    printfn "\n表达式替换中重新命名let中的绑定变量，防止自由变量捕获: "
    printfn $"src: {Intcomp.e7src}"
    printfn $"ast: {Intcomp.e7}"
    printfn $"env1:{Intcomp.e7env1}"
    printfn $"s1:{Intcomp.e7s1a}\n"
    
    let e9 = Intcomp.e9
    let env= Intcomp.e9env1
    printfn $"ast: {e9}"
    printfn $"env: {env}"
    printfn $"e9s1:{Intcomp.subst e9 env} //let表达式的绑定变量已经重新命名，防止env中的变量被捕获\n"

 (* Correctness: eval e []  equals  teval (tcomp e []) [] *)
    printfn "\nCompiler to Target Ast (remove variable name):"
    printfn "ast: %A" Intcomp.e2
    printfn "eval result: %A " (Intcomp.eval Intcomp.e2 [])
    let tast = (Intcomp.tcomp Intcomp.e2 [])
    printfn "\ntarget ast: %A" tast
    printfn "teval result: %A" (Intcomp.teval tast [])
(*
Compiler to Target Ast (remove variable name):  
ast: Let ("z", CstI 1, Prim ("+", Let ("z", CstI 2, Prim ("*", CstI 10, Var "z")), Var "z"))
eval result: 21

target ast: TLet (TCstI 1, TPrim ("+", TLet (TCstI 2, TPrim ("*", TCstI 10, TVar 0)), TVar 0))
teval result: 21                                                            ^^^^^      ^^^^^^
注意：两个 Tvar 0 的值不同，对应不同的Let 绑定                               内部的 z=2， 外部的z = 1 
*)

    
    // 无变量栈式虚拟机编译器 
    printfn "\nSvm Compiler:" 
    //将树状的（嵌套）AST 转换为线性指令（分支转移）
    let display expr = 
        printfn "ast: %A" expr
        printfn "instr: %A" (Svm.rcomp expr) 
        Svm.rexec (Svm.rcomp expr)  |> printfn "exec: %A"
        (Svm.rcomp  >> Svm.rexec)  expr  //函数复合
        expr |> Svm.rcomp |>Svm.rexec // 数据流 data pipeline
        ()

        
    printfn "\nexp: 17 + 17"
    display Svm.e1

    printfn "\nexp: 2 + 2 * 3" //运算次序 
    display Svm.e2


    //带变量栈式虚拟机编译器
    printfn "\n Svm Compiler with Var:"
    let displayVar expr = 
        printfn "ast: %A" expr
        let instr = SvmWithVar.scompiler expr
        printfn "instr: %A " instr
        printfn "exec: %A" (SvmWithVar.sexec instr) 
        ()

    
    printfn "\nexp: 17 + 17"
    displayVar SvmWithVar.e1
    // printfn "%A" SvmWithVar.e1

    printfn "\nexp: z = 17;  z + z ;"
    displayVar SvmWithVar.e2
(*
exp: z = 17;  z + z ;
ast: Let ("z", CstI 17, Prim ("+", Var "z", Var "z"))
instr: [SCstI 17; SVar 0; SVar 1; SAdd; SSwap; SPop]   //指令序列运算结束后，栈顶是Let 表达式的结果34
          [17]     [17]    [17]   [17]   [34]   [34]   
          ^^^      [17]    [17]   [34]   [17]
         变量z             [17]    ^^^         
exec: 34                          add的结果
 
 注意关键⭐⭐⭐： 变量z 在 z + z 表达式中分别编译为 SVar 0  与 SVar 1，这是因为 程序运行时 栈的内容发生了变化，变量z 的位置相对栈顶的值也发生了变化。
*)


    printfn "\nexp: z = 5 - 4;  100 * z;"
    displayVar SvmWithVar.e3
    //生成指令文件
    SvmWithVar.intsToFile (SvmWithVar.scompiler SvmWithVar.e3) "e3.instr.txt" 

    0 
