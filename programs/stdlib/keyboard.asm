; Keyboard routines.
;
; Include near the start of a program, behind an initial JMP/JMP16 over the routines.
; R3 is the return address for all routines.

.include "io.asm"
.include "common.asm"

; stdlib_read_line
; Inputs:  R0 = destination buffer address, R1 = max characters before terminator
; Outputs: R1 = character count. The buffer is zero-terminated.
; Clobbers: R0, R2, R3, flags, STDLIB_TMP0-STDLIB_TMP3
stdlib_read_line:
LDI R2, STDLIB_TMP0
ST [R2], R0
LDI R2, STDLIB_TMP1
ST [R2], R3
LDI R2, STDLIB_TMP2
ST [R2], R1
LDI R1, 0
LDI R2, STDLIB_TMP3
ST [R2], R1

stdlib_read_line_loop:
LDI R2, STDLIB_TMP3
LD R2, [R2]
LDI R1, STDLIB_TMP2
LD R1, [R1]
CLF
CMP R2, R1
JE stdlib_read_line_done

LDI R0, IO_KEYBOARD
OUT ADDR, R0
IN DATA, R2
LDI R0, 0
CLF
CMP R2, R0
JE stdlib_read_line_loop
LDI R0, 13
CLF
CMP R2, R0
JE stdlib_read_line_done

LDI R0, STDLIB_TMP0
LD R0, [R0]
LDI R1, STDLIB_TMP3
LD R1, [R1]
ADD R0, R1
ST [R0], R2

LDI R0, STDLIB_TMP3
LD R1, [R0]
LDI R2, 1
ADD R1, R2
ST [R0], R1
JMP stdlib_read_line_loop

stdlib_read_line_done:
LDI R0, STDLIB_TMP0
LD R0, [R0]
LDI R1, STDLIB_TMP3
LD R1, [R1]
ADD R0, R1
LDI R2, 0
ST [R0], R2

LDI R0, STDLIB_TMP3
LD R1, [R0]
LDI R0, STDLIB_TMP1
LD R3, [R0]
JMPR R3
