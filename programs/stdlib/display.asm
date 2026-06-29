; Display/text routines.
;
; Include near the start of a program, behind an initial JMP/JMP16 over the routines.
; R3 is the return address for all public routines.

.include "io.asm"
.include "common.asm"

; stdlib_print_char
; Inputs:  R1 = ASCII character, R2 = display byte address
; Output:  R2 = next display byte address
; Clobbers: R0, R1, flags, STDLIB_TMP0
stdlib_print_char:
LDI R0, STDLIB_TMP0
ST [R0], R2

LDI R0, IO_DISPLAY
OUT ADDR, R0

MOV R0, R1
SHL R0
SHL R0
SHL R0
LDI R2, FONT_BASE, R1
ADD R0, R2
LDI R2, STDLIB_TMP0
LD R2, [R2]

LD R1, [R0]
OUT ADDR, R2
OUT DATA, R1
LDI R1, 1
ADD R0, R1
LDI R1, BYTES_PER_ROW
ADD R2, R1

LD R1, [R0]
OUT ADDR, R2
OUT DATA, R1
LDI R1, 1
ADD R0, R1
LDI R1, BYTES_PER_ROW
ADD R2, R1

LD R1, [R0]
OUT ADDR, R2
OUT DATA, R1
LDI R1, 1
ADD R0, R1
LDI R1, BYTES_PER_ROW
ADD R2, R1

LD R1, [R0]
OUT ADDR, R2
OUT DATA, R1
LDI R1, 1
ADD R0, R1
LDI R1, BYTES_PER_ROW
ADD R2, R1

LD R1, [R0]
OUT ADDR, R2
OUT DATA, R1
LDI R1, 1
ADD R0, R1
LDI R1, BYTES_PER_ROW
ADD R2, R1

LD R1, [R0]
OUT ADDR, R2
OUT DATA, R1
LDI R1, 1
ADD R0, R1
LDI R1, BYTES_PER_ROW
ADD R2, R1

LD R1, [R0]
OUT ADDR, R2
OUT DATA, R1
LDI R1, 1
ADD R0, R1
LDI R1, BYTES_PER_ROW
ADD R2, R1

LD R1, [R0]
OUT ADDR, R2
OUT DATA, R1

LDI R0, STDLIB_TMP0
LD R2, [R0]
LDI R0, 1
ADD R2, R0
JMPR R3

; stdlib_print_string
; Inputs:  R0 = zero-terminated string address, R2 = display byte address
; Output:  R2 = next display byte address
; Clobbers: R0, R1, R3, flags, STDLIB_TMP0-STDLIB_TMP2
stdlib_print_string:
LDI R1, STDLIB_TMP1
ST [R1], R3
LDI R1, STDLIB_TMP2
ST [R1], R0

stdlib_print_string_loop:
LDI R0, STDLIB_TMP2
LD R0, [R0]
LD R1, [R0]
LDI R0, 0
CLF
CMP R1, R0
JE stdlib_print_string_done

LDI R3, stdlib_print_string_after_char, R0
JMP stdlib_print_char

stdlib_print_string_after_char:
LDI R0, STDLIB_TMP2
LD R1, [R0]
LDI R3, 1
ADD R1, R3
ST [R0], R1
JMP stdlib_print_string_loop

stdlib_print_string_done:
LDI R0, STDLIB_TMP1
LD R3, [R0]
JMPR R3
