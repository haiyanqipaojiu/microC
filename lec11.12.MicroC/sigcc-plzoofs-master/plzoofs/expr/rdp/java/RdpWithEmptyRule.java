// recursive descent parser for grammer
// G14:
// 1. S → a A S
// 2. S → b
// 3. A → c A S
// 4. A → ε     Follow(A) = {a,b}

import java.io.IOException; 
import java.io.InputStreamReader; 

//"acbbN" -- accept 
class RDP // Recursive Descent Parser 
{

char inp; 
void parse ()
{
    inp = getInp(); 
    S (); 
    if (inp == ’N’)accept(); 
    else reject(); 
}

void S ()
{
    if (inp == ’a’)// apply rule 1 
{
        inp = getInp(); 
        A(); 
        S(); 
    }
// end rule 1
    else if (inp == ’b’)
            inp = getInp(); // apply rule 2
else reject(); 
}

void A ()
{
    if (inp == ’c’)// apply rule 3 
{
        inp = getInp(); 
        A (); 
        S (); 
    }
// end rule 3
    else    if (inp == ’a’ || inp == ’b’); // apply rule 4
else reject(); 
}

void accept()// Accept the input 
    {
    System.out.println ("accept"); 
    }

void reject()// Reject the input 
{
    System.out.println ("reject"); 
    System.exit(0); // terminate parser
}

char getInp()
    {
        try 
        {
            return (char)System.in.read(); 
        }
        catch (IOException ioe)
        {
            System.out.println ("IO error " + ioe); 
        }
        return '#'; // must return a char
    }  
}