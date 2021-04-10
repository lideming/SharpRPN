using System;
using System.Collections;
using System.Collections.Generic;

namespace SharpRPN
{
    class Repl
    {
        public static void Start(Scope scope)
        {
            Tokenizer tr = new Tokenizer();
            Tokenizer.Result trr;
            while (true) {
                trr = null;
                string line;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Input: ");
                line = Console.ReadLine();
                //Console.Clear();
                //Console.WriteLine(line);
                Console.ResetColor();
                if (line == null)
                    break;

                if (line.Length > 0)
                    try {
                        trr = tr.Input(line);
                        var ast = AST.FromTokens(trr);
                        scope.Input(ast.codeBlock);
                    } catch (Exception e) {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(e.ToString());
                        Console.ResetColor();
                    }
                WriteStack(scope.Stack);
                if (scope.TryGetVar("_showtokens_", out var v) && v as string == "1" && trr != null) {
                    WriteTokens(trr.Tokens);
                }
            }
        }

        private static void WriteTokens(List<Token> tokens)
        {
            int i = 1;
            Console.WriteLine("===TokenBegin===");
            foreach (var item in tokens) {
                Console.Write($"{i++,-5}");
                Console.Write($"{item.type,-12}");
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine(item.obj ?? "(null)");
                Console.ResetColor();
            }
            Console.WriteLine("====TokenEnd====");
        }

        private static void WriteStack(Stack stack)
        {
            if (stack.Count == 0) {
                Console.WriteLine("Stack empty");
                return;
            }
            int i = 1;
            Console.WriteLine("Stack {");
            foreach (var item in stack) {
                Console.Write("  ");
                Console.Write(i++);
                Console.Write(":\t");
                Console.Write(item);
                Console.Write("\t");
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine(item?.GetType().Name ?? "(null)");
                Console.ResetColor();
            }
            Console.WriteLine("}");
            Console.WriteLine();
        }
    }
}
