## 预备软件
dotnet 5.0

#### Linux

sudo apt-get install build-essential nasm gcc-multilib

#### Win32
https://www.nasm.us/pub/nasm/releasebuilds/2.15.05/win32/

https://github.com/jmeubank/tdm-gcc/releases/download/v9.2.0-tdm-1/tdm-gcc-9.2.0.exe

## 构建命令

```sh
#生成 ex1.asm
dotnet run ex1.c

# win32
nasm -f win32 ex1.asm -o ex1.o # 生成目标文件    -f 指定文件格式
gcc -m32 -c driver.c    # 生成运行时库
gcc -m32 driver.o ex1.o -o try1  # 链接 库文件 与目标文件 生成可执行程序

./try1 8
8 7 6 5 4 3 2 1
Ran 0.000 s


# linux
nasm -f elf ex1.asm  # linux 目标文件格式 elf
gcc -m32 -c driver.c  # 运行时库
gcc -m32 driver.o ex1.o -o try11  # 链接库
./try11 8

#macos  -f macho

# one-liner Exec任务脚本
dotnet build -t:ccrun
```

## 名称 mangle

windows macos 32位 汇编，符号名称前面加 下划线  `_`

- 如  `.c` 源代码中函数名 `program`   `.asm` 汇编代码中使用  `_program` 方可以正确链接
- linux 无需处理

## micro C asm 调用规范
- 调用函数前 oldbp 入栈

## Bugs

*(p+3) 指针解引用 不支持指针表达式运算