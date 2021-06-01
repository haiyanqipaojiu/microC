
// RDP for Expr Grammar
// 1. Expr → Term Elist
// 2. Elist → + Term Elist
// 3. Elist → ε
// 4. Term → Factor Tlist
// 5. Tlist → * Factor Tlist
// 6. Tlist → ε
// 7. Factor → ( Expr )
// 8. Factor → var

int inp;
final int var = 256;
final int endmarker = 257;
void Expr() { if (inp==’(’ || inp==var) // apply rule 1
{
    Term();
    Elist();
} // end rule 1
else reject();
}
void Elist() {
  if (inp ==’+’) // apply rule 2
  {
    getInp();
    Term();
    Elist();
  } // end rule 2
  else if (inp ==’)’ || inp==endmarker)
        ; // apply rule 3, null statement
  else
    reject();
}
void Term() { if (inp==’(’ || inp==var) // apply rule 4
{
    Factor();
    Tlist();
} // end rule 4
else reject();
}
void Tlist() {
  if (inp ==’*’) // apply rule 5
  {
    getInp();
    Factor();
    Tlist();
  } // end rule 5
  else if (inp ==’+’ || inp ==’)’|| inp==endmarker)
        ; // apply rule 6, null statement
  else
    reject();
}
void Factor() { if (inp==’(’) // apply rule 7
{
    getInp();
    Expr();
    if (inp ==’)’) getInp();
    else
      reject();
} // end rule 7
else if (inp==var) getInp(); // apply rule 8
else reject();
}