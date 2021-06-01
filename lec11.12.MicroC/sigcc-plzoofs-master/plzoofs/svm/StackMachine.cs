
using System;

class Machine {
   const int 
    SCST = 0, SVAR = 1, SADD = 2, SSUB = 3, SMUL = 4, SPOP = 5, SSWAP = 6;
  
  public static void Main(string[] args) {
     int[] rpn1 = { SCST, 17, SVAR, 0, SVAR, 1, SADD, SSWAP, SPOP };
    Console.WriteLine(seval(rpn1));
     int[] rpn2 = { SCST, 17, SCST, 22, SCST, 100, SVAR, 1, SMUL, 
			 SSWAP, SPOP, SVAR, 1, SADD, SSWAP, SPOP };
    Console.WriteLine(seval(rpn2));
  }

  static int seval(int[] code) {
    int[] stack = new int[1000];	// evaluation and env stack
    int sp = -1;			// pointer to current stack top

    int pc = 0;				// program counter
    int instr;				// current instruction

    while (pc < code.Length) 
      switch (instr = code[pc++]) {
      case SCST:
	stack[sp+1] = code[pc++]; sp++; break;
      case SVAR:
	stack[sp+1] = stack[sp-code[pc++]]; sp++; break;
      case SADD: 
	stack[sp-1] = stack[sp-1] + stack[sp]; sp--; break;
      case SSUB: 
	stack[sp-1] = stack[sp-1] - stack[sp]; sp--; break;
      case SMUL: 
	stack[sp-1] = stack[sp-1] * stack[sp]; sp--; break;
      case SPOP: 
	sp--; break;
      case SSWAP: 
	{ int tmp     = stack[sp]; 
	  stack[sp]   = stack[sp-1]; 
	  stack[sp-1] = tmp;
	  break;
	}
      default:			
	throw new Exception("Illegal instruction " + instr 
				   + " at address " + (pc-1));
      }
    return stack[sp];      
  }
}
