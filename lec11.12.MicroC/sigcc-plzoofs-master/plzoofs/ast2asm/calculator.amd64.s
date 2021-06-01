	.text
	.globl	program
program:
	pushq	%rbp
	movq	%rsp, %rbp
	movq	24(%rbp), %rax
	pushq	%rax
	movq	24(%rbp), %rax
	popq	%r10
	imulq	%r10, %rax
	popq	%rbp
	retq	
