	.text
	.file	"binary_gcd.ll"
	.globl	binary_gcd                      # -- Begin function binary_gcd
	.p2align	4, 0x90
	.type	binary_gcd,@function
binary_gcd:                             # @binary_gcd
	.cfi_startproc
# %bb.0:
	pushq	%rax
	.cfi_def_cfa_offset 16
	movq	%rdi, %rax
	cmpq	%rsi, %rdi
	je	.LBB0_13
# %bb.1:                                # %term1
	testq	%rax, %rax
	jne	.LBB0_2
# %bb.14:                               # %ret_v
	movq	%rsi, %rax
	popq	%rcx
	.cfi_def_cfa_offset 8
	retq
.LBB0_2:                                # %term2
	.cfi_def_cfa_offset 16
	testq	%rsi, %rsi
	je	.LBB0_13
# %bb.3:                                # %gcd
	movl	%eax, %ecx
	notb	%cl
	testb	$1, %cl
	je	.LBB0_4
# %bb.11:                               # %u_even
	shrq	%rax
	testb	$1, %sil
	jne	.LBB0_10
# %bb.12:                               # %both_even
	shrq	%rsi
	movq	%rax, %rdi
	callq	binary_gcd
	addq	%rax, %rax
.LBB0_13:                               # %ret_u
	popq	%rcx
	.cfi_def_cfa_offset 8
	retq
.LBB0_4:                                # %u_odd
	.cfi_def_cfa_offset 16
	movq	$-1, %rcx
	xorl	%esi, %ecx
	testb	$1, %cl
	je	.LBB0_5
# %bb.9:                                # %v_even
	shrq	%rsi
.LBB0_10:                               # %v_even
	movq	%rax, %rdi
	jmp	.LBB0_8
.LBB0_5:                                # %v_odd
	movq	%rax, %rdi
	subq	%rsi, %rdi
	jle	.LBB0_6
# %bb.7:                                # %u_gt
	shrq	%rdi
	jmp	.LBB0_8
.LBB0_6:                                # %v_gt
	subq	%rax, %rsi
	shrq	%rsi
	movq	%rsi, %rdi
	movq	%rax, %rsi
.LBB0_8:                                # %u_gt
	callq	binary_gcd
	popq	%rcx
	.cfi_def_cfa_offset 8
	retq
.Lfunc_end0:
	.size	binary_gcd, .Lfunc_end0-binary_gcd
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
	movl	$21, %edi
	movl	$15, %esi
	callq	binary_gcd
	popq	%rcx
	.cfi_def_cfa_offset 8
	retq
.Lfunc_end1:
	.size	main, .Lfunc_end1-main
	.cfi_endproc
                                        # -- End function
	.section	".note.GNU-stack","",@progbits
