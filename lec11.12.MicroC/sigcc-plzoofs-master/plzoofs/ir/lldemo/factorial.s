	.text
	.file	"factorial.ll"
	.globl	factorial                       # -- Begin function factorial
	.p2align	4, 0x90
	.type	factorial,@function
factorial:                              # @factorial
	.cfi_startproc
# %bb.0:
	movq	%rdi, -8(%rsp)
	movq	$1, -16(%rsp)
	cmpq	$0, -8(%rsp)
	jle	.LBB0_3
	.p2align	4, 0x90
.LBB0_2:                                # %then
                                        # =>This Inner Loop Header: Depth=1
	movq	-8(%rsp), %rax
	movq	-16(%rsp), %rcx
	imulq	%rax, %rcx
	movq	%rcx, -16(%rsp)
	decq	%rax
	movq	%rax, -8(%rsp)
	cmpq	$0, -8(%rsp)
	jg	.LBB0_2
.LBB0_3:                                # %end
	movq	-16(%rsp), %rax
	retq
.Lfunc_end0:
	.size	factorial, .Lfunc_end0-factorial
	.cfi_endproc
                                        # -- End function
	.globl	main                            # -- Begin function main
	.p2align	4, 0x90
	.type	main,@function
main:                                   # @main
	.cfi_startproc
# %bb.0:
	pushq	%rax
	.cfi_def_cfa_offset 16
	movq	$0, (%rsp)
	movl	$5, %edi
	callq	factorial
	popq	%rcx
	.cfi_def_cfa_offset 8
	retq
.Lfunc_end1:
	.size	main, .Lfunc_end1-main
	.cfi_endproc
                                        # -- End function
	.section	".note.GNU-stack","",@progbits
