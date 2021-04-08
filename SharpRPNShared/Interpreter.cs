using System;
using System.Collections;
using static SharpRPN.AST;

namespace SharpRPN
{
    public delegate void Function(Scope ir);

    public class Interpreter
    {
        public Interpreter()
        {
        }

        public static void Input(string str, Scope scope)
        {
            if (str == null || str == "") return;
            var tr = new Tokenizer();
            var tokens = tr.Input(str);
            var ast = FromTokens(tokens);
            Input(ast, scope);
        }

        public static void Input(AST ast, Scope scope)
        {
            _runCodeBlock(ast.codeBlock, scope);
        }

        public static void Input(CodeBlock cb, Scope scope)
        {
            _runCodeBlock(cb, scope);
        }

        private static void _runCodeBlock(CodeBlock cb, Scope scope)
        {
            var str = cb.inputString;
            var len = cb.codes.Count;
            for (int i = 0; i < len; i++) {
                var node = cb.codes[i];
                try {
                    if (node is Name name) {
                        handleName(name.name, scope);
                    } else if (node is Value val) {
                        scope.Push(val.value);
                    } else if (node is Ifthen ift) {
                        if ((bool)scope.Pop() == true) {
                            _runCodeBlock(ift.ifTrue, scope);
                        } else {
                            if (ift.@else != null) {
                                _runCodeBlock(ift.@else, scope);
                            }
                        }
                    } else if (node is CodeBlock codeblk) {
                        scope.Push(codeblk);
                    }
                } catch (Exception e) {
                    throw getException($"exception", node.token, str, e);
                }
            }
        }

        private static SharpRPNException getException(string message, Token token, string inputString, Exception inner = null)
        {
            return new SharpRPNException(message, token, inputString, inner);
        }

        private static string getTokenInfo(string str, Token t)
        {
            return SharpRPNException.getTokenInfo(t, str);
        }

        private static void handleName(string name, Scope scope)
        {
            if (name.StartsWith("'") && name.EndsWith("'")) {
                scope.Push(new Var(name.Trim('\'')));
            } else {
                var v = scope.GetVar(name);
                if (v != null) {
                    if (v is Function func) {
                        invokeFunction(name, func, scope);
                    } else if (v is CodeBlock codeblk) {
                        _runCodeBlock(codeblk, scope);
                    } else {
                        scope.Push(v);
                    }
                } else {
                    throw new Exception("unknown name.");
                }
            }
        }

        private static void invokeFunction(string name, Function function, Scope scope)
        {
            function(scope);
        }
    }
}
