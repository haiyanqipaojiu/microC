# Makefile for compiling Micro-C programs to x86 assembly on MacOS 10.12
# sestoft@itu.dk * 2017-04-15

EXAMPLES = ex1 ex2 ex3 ex4 ex5 ex6 ex7 ex8 ex9 ex10 ex11 ex11count ex12 ex13 ex14 \
	ex15 ex16 ex17 ex18 ex19 ex20 ex21 ex22 ex23 ex24 ex25

all: microccasm.exe driver.o ${EXAMPLES}

microccasmtemp.exe: CLex.fs CPar.fs Absyn.fs Parse.fs X86Temp.fs X86CompTemp.fs ParseAndCompTemp.fs MicroCCAsmTemp.fs
	fsharpc -r FsLexYacc.Runtime.dll  Absyn.fs CPar.fs CLex.fs Parse.fs \
		X86Temp.fs X86CompTemp.fs ParseAndCompTemp.fs MicroCCAsmTemp.fs -o microccasmtemp.exe

microccasm.exe: CLex.fs CPar.fs Absyn.fs Parse.fs X86.fs X86Comp.fs ParseAndComp.fs MicroCCAsm.fs
	fsharpc -r FsLexYacc.Runtime.dll  Absyn.fs CPar.fs CLex.fs Parse.fs \
		X86.fs X86Comp.fs ParseAndComp.fs MicroCCAsm.fs -o microccasm.exe

driver.o: driver.c
	gcc -arch i386 -c driver.c

examples: ${EXAMPLES}

cleanexamples:
	rm -f *.asm *.o ${EXAMPLES}

clean:
	rm -f CLex.fs CPar.fs CPar.fsi microccasm.exe microccasmtemp.exe *.asm *.o ${EXAMPLES}

.SUFFIXES : .c .o .asm .fs .fsl .fsy

# To disable make's built-in rules, which invoke the wrong C compiler
% : %.c
%.o : %.c
% : %.o

# Useful rules follow

% : %.o driver.o
	gcc -arch i386 -Wl,-no_pie driver.o $< -o $*

%.o : %.asm
	nasm -f macho $<

%.asm : %.c microccasm.exe
	mono microccasm.exe $<

%.fs : %.fsl
	fslex --unicode $<

%.fs : %.fsy
	fsyacc --module $* $<

