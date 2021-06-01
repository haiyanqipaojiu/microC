	.text
	.globl	program
program:
	pushq	%rbp
	movq	%rsp, %rbp
	movq	72(%rbp), %rax
	pushq	%rax
	movq	72(%rbp), %rax
	popq	%r10
	imulq	%r10, %rax
	popq	%rbp
	retq	
