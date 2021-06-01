	.text
	.syntax unified
	.eabi_attribute	67, "2.09"	@ Tag_conformance
	.eabi_attribute	6, 1	@ Tag_CPU_arch
	.eabi_attribute	8, 1	@ Tag_ARM_ISA_use
	.eabi_attribute	34, 1	@ Tag_CPU_unaligned_access
	.eabi_attribute	17, 1	@ Tag_ABI_PCS_GOT_use
	.eabi_attribute	20, 1	@ Tag_ABI_FP_denormal
	.eabi_attribute	21, 1	@ Tag_ABI_FP_exceptions
	.eabi_attribute	23, 3	@ Tag_ABI_FP_number_model
	.eabi_attribute	24, 1	@ Tag_ABI_align_needed
	.eabi_attribute	25, 1	@ Tag_ABI_align_preserved
	.eabi_attribute	38, 1	@ Tag_ABI_FP_16bit_format
	.eabi_attribute	14, 0	@ Tag_ABI_PCS_R9_use
	.file	"binary_gcd.ll"
	.globl	binary_gcd                      @ -- Begin function binary_gcd
	.p2align	2
	.type	binary_gcd,%function
	.code	32                              @ @binary_gcd
binary_gcd:
	.fnstart
@ %bb.0:
	push	{r11, lr}
	mov	lr, r0
	mov	r12, r1
	eor	r0, r1, r3
	eor	r1, lr, r2
	orrs	r0, r1, r0
	beq	.LBB0_4
@ %bb.1:                                @ %term1
	orrs	r0, lr, r12
	bne	.LBB0_3
@ %bb.2:                                @ %ret_v
	mov	r0, r2
	mov	r1, r3
	pop	{r11, lr}
	mov	pc, lr
.LBB0_3:                                @ %term2
	orrs	r0, r2, r3
	bne	.LBB0_5
.LBB0_4:                                @ %ret_u
	mov	r0, lr
	mov	r1, r12
	pop	{r11, lr}
	mov	pc, lr
.LBB0_5:                                @ %gcd
	mvn	r0, lr
	tst	r0, #1
	beq	.LBB0_8
@ %bb.6:                                @ %u_even
	tst	r2, #1
	beq	.LBB0_10
@ %bb.7:                                @ %ue_vo
	lsrs	r1, r12, #1
	rrx	r0, lr
	b	.LBB0_15
.LBB0_8:                                @ %u_odd
	mvn	r0, #0
	eor	r0, r0, r2
	tst	r0, #1
	beq	.LBB0_11
@ %bb.9:                                @ %v_even
	lsrs	r3, r3, #1
	rrx	r2, r2
	mov	r0, lr
	mov	r1, r12
	b	.LBB0_15
.LBB0_10:                               @ %both_even
	lsrs	r1, r12, #1
	rrx	r0, lr
	lsrs	r3, r3, #1
	rrx	r2, r2
	bl	binary_gcd
	lsl	r1, r1, #1
	orr	r1, r1, r0, lsr #31
	lsl	r0, r0, #1
	pop	{r11, lr}
	mov	pc, lr
.LBB0_11:                               @ %v_odd
	subs	r0, r2, lr
	sbcs	r0, r3, r12
	bge	.LBB0_13
@ %bb.12:                               @ %u_gt
	subs	r0, lr, r2
	sbc	r1, r12, r3
	b	.LBB0_14
.LBB0_13:                               @ %v_gt
	subs	r0, r2, lr
	mov	r2, lr
	sbc	r1, r3, r12
	mov	r3, r12
.LBB0_14:                               @ %ue_vo
	lsrs	r1, r1, #1
	rrx	r0, r0
.LBB0_15:                               @ %ue_vo
	bl	binary_gcd
	pop	{r11, lr}
	mov	pc, lr
.Lfunc_end0:
	.size	binary_gcd, .Lfunc_end0-binary_gcd
	.fnend
                                        @ -- End function
	.globl	main                            @ -- Begin function main
	.p2align	2
	.type	main,%function
	.code	32                              @ @main
main:
	.fnstart
@ %bb.0:
	push	{r11, lr}
	mov	r0, #21
	mov	r1, #0
	mov	r2, #15
	mov	r3, #0
	bl	binary_gcd
	pop	{r11, lr}
	mov	pc, lr
.Lfunc_end1:
	.size	main, .Lfunc_end1-main
	.fnend
                                        @ -- End function
	.section	".note.GNU-stack","",%progbits
