// This file is part of www.nand2tetris.org
// and the book "The Elements of Computing Systems"
// by Nisan and Schocken, MIT Press.
// File name: projects/04/Fill.asm

// Runs an infinite loop that listens to the keyboard input.
// When a key is pressed (any key), the program blackens the screen,
// i.e. writes "black" in every pixel;
// the screen should remain fully black as long as the key is pressed. 
// When no key is pressed, the program clears the screen, i.e. writes
// "white" in every pixel;
// the screen should remain fully clear as long as no key is pressed.

// Put your code here.

@8192
D=A
@PixcelNum	
M=D

(INFINITE_LOOP)
@KBD
D=M
	
@KBDON
D;JNE

@KBDOFF	
0;JMP
	
(KBDON)	
@R0
M=-1 // black

@DRAWALL
0;JMP

(KBDOFF)	
@R0
M=0  // White

@DRAWALL
0;JMP
	
(DRAWALL)
@SCREEN
D=A	
@address // pixcel postion
M=D	
	
(DRAWING)
@R0
D=M
@address
A=M
M=D

@address
D=M
@SCREEN
D=D-A
@PixcelNum	
D=D-M
D=D+1	
@INFINITE_LOOP
D;JGE
	
@address
M=M+1
@DRAWING	
0;JMP
