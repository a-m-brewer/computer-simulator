; Writes each display RAM byte's address as its pixel byte.
; Assemble with BYTES_PER_FRAME=(SCREEN_WIDTH / 8) * SCREEN_HEIGHT.

.equ DISPLAY, 0x07

LDI R2, BYTES_PER_FRAME, R1
DATA R0, 1

DATA R1, DISPLAY
OUT ADDR, R1

DATA R3, 0

Loop:
OUT ADDR, R3
OUT DATA, R3
ADD R3, R0
CLF
CMP R3, R2
JE End
JMP Loop

End:
HALT
