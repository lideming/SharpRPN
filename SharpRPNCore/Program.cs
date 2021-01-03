using System;

namespace SharpRPN
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "SharpRPN";
            Scope scope = new Scope();
            Repl.Start(scope);
        }
    }
}
