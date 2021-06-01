// recursive descent parser for grammer
//G13:
//1. S → aSB
//2. S → b
//3. B → a
//4. B → bBa

import java.io.IOException; 
import java.io.InputStreamReader; 

//"abaN" -- accept 
class RDP // Recursive Descent Parser 
{
char inp; 
public static void main (String[] args)throws IOException 
{
InputStreamReader
 stdin = new InputStreamReader(System.in); 
        RDP rdp = new RDP(); 
        rdp.parse(); 
}
void parse ()
{
    inp = getInp(); 
    S (); // Call start nonterminal
if (inp == 'N')accept(); // end of string marker
else reject(); 
}
void S ()
{
    //S -> aSB
    if (inp == 'a')// apply rule 1 
{
        inp = getInp(); 
        S (); 
        B (); 
    }// end rule 1
    
    //S -> b
else if (inp == 'b')
            inp = getInp(); // apply rule 2
else reject(); 
}
void B ()
{
    //B->a
    if (inp == 'a')
        inp = getInp(); // rule 3
    
    //B-> bBa
else if (inp == 'b')// apply rule 4 
{
    inp = getInp(); 
            B(); 
            if (inp == 'a')
                inp = getInp(); 
            else reject(); 
        }// end rule 4
else reject(); 
}

void accept()// Accept the input 
{System.out.println ("accept"); }

void reject()// Reject the input 
{
    System.out.println ("reject"); 
System.exit(0); // terminate parser
}
char getInp()
    {try 
        {return (char)System.in.read(); }
catch (IOException ioe)
        {System.out.println ("IO error " + ioe); }
return '#'; // must return a char
}}