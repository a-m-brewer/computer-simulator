; Math routines.
;
; Include near the start of a program, behind an initial JMP/JMP16 over the routines.
; R3 is the return address for all routines.

.include "common.asm"

; stdlib_mul
; Inputs:  R0 = multiplicand, R1 = multiplier
; Output:  R0 = low 16 bits of product
; Clobbers: R1, R2, flags, STDLIB_TMP0-STDLIB_TMP1
stdlib_mul:
LDI R2, STDLIB_TMP0
ST [R2], R0
LDI R2, STDLIB_TMP1
ST [R2], R1
LDI R0, 0
LDI R1, 0

stdlib_mul_loop:
LDI R2, STDLIB_TMP1
LD R2, [R2]
CLF
CMP R1, R2
JE stdlib_mul_done
LDI R2, STDLIB_TMP0
LD R2, [R2]
ADD R0, R2
LDI R2, 1
ADD R1, R2
JMP stdlib_mul_loop

stdlib_mul_done:
JMPR R3

; stdlib_div
; Inputs:  R0 = dividend, R1 = divisor
; Outputs: R0 = quotient, R1 = remainder. Divisor 0 returns quotient 0 and remainder dividend.
; Clobbers: R2, flags, STDLIB_TMP0-STDLIB_TMP1
stdlib_div:
LDI R2, 0
CLF
CMP R1, R2
JE stdlib_div_zero
LDI R2, STDLIB_TMP0
ST [R2], R1
MOV R1, R0
LDI R0, 0

stdlib_div_loop:
LDI R2, STDLIB_TMP0
LD R2, [R2]
CLF
CMP R1, R2
JA stdlib_div_subtract
JE stdlib_div_subtract
JMP stdlib_div_done

stdlib_div_subtract:
LDI R2, STDLIB_TMP1
ST [R2], R0
LDI R2, STDLIB_TMP0
LD R2, [R2]
NOT R2
LDI R0, 1
ADD R2, R0
ADD R1, R2
LDI R2, STDLIB_TMP1
LD R0, [R2]
LDI R2, 1
ADD R0, R2
JMP stdlib_div_loop

stdlib_div_zero:
MOV R1, R0
LDI R0, 0

stdlib_div_done:
JMPR R3
