; Memory routines.
;
; Include near the start of a program, behind an initial JMP/JMP16 over the routines.
; R3 is the return address for all routines.

.include "common.asm"

; stdlib_memcpy
; Inputs: R0 = destination address, R1 = source address, STDLIB_MEMCPY_LENGTH = byte count
; Output: none
; Clobbers: R0, R1, R2, flags, STDLIB_TMP0-STDLIB_TMP3
.equ STDLIB_MEMCPY_LENGTH, STDLIB_TMP4

stdlib_memcpy:
LDI R2, STDLIB_TMP0
ST [R2], R0
LDI R2, STDLIB_TMP1
ST [R2], R1
LDI R2, STDLIB_MEMCPY_LENGTH
LD R2, [R2]
LDI R0, STDLIB_TMP2
ST [R0], R2
LDI R2, 0
LDI R0, STDLIB_TMP3
ST [R0], R2

stdlib_memcpy_loop:
LDI R0, STDLIB_TMP3
LD R2, [R0]
LDI R0, STDLIB_TMP2
LD R0, [R0]
CLF
CMP R2, R0
JE stdlib_memcpy_done

LDI R0, STDLIB_TMP1
LD R0, [R0]
LD R1, [R0]
LDI R0, STDLIB_TMP0
LD R0, [R0]
ST [R0], R1

LDI R0, STDLIB_TMP1
LD R1, [R0]
LDI R2, 1
ADD R1, R2
ST [R0], R1

LDI R0, STDLIB_TMP0
LD R1, [R0]
LDI R2, 1
ADD R1, R2
ST [R0], R1

LDI R0, STDLIB_TMP3
LD R1, [R0]
LDI R2, 1
ADD R1, R2
ST [R0], R1
JMP stdlib_memcpy_loop

stdlib_memcpy_done:
JMPR R3
