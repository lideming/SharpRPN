using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
