; Draws HELLO WORLD at the top-left of the display.
; Assemble with BYTES_PER_ROW=(SCREEN_WIDTH / 8).

JMP16 Start, R0, R1
.org 0x20
.include "stdlib/display.asm"

Start:
LDI R0, Message, R2
LDI R2, 0
LDI R3, End, R1
JMP stdlib_print_string

End:
HALT R0, R1

Message:
.asciz "HELLO WORLD"

.org FONT_BASE
.incbin "assets/font8x8.bin"
