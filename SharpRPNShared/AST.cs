using System;
using System.Collections.Generic;

namespace SharpRPN
{

    public class AST
    {
        public CodeBlock codeBlock;

        public AST()
        {
            codeBlock = new CodeBlock();
        }

        public static AST FromTokens(Tokenizer.Result trr)
        {
            return new Parser().Parse(trr);
        }

        public class Parser
        {
            List<Token> tokens;
            int cur;
            Token curToken => tokens[cur];
            Token nextToken => tokens[cur + 1];
            int tokenslen;
            string inputString;
            AST ast;

            public Parser()
            {

            }

            public AST Parse(Tokenizer.Result trr)
            {
                ast = new AST();
                inputString = trr.Input;
                tokens = trr.Tokens;
                tokenslen = tokens.Count;
                ast.codeBlock = parseBlock();
                return ast;
            }

            private CodeBlock parseBlock(string closeWith = null)
            {
                CodeBlock codeBlock = new CodeBlock();
                codeBlock.inputString = inputString;
                codeBlock.token = curToken;
                var nodes = new List<Node>();
                codeBlock.codes = nodes;
                var begin = cur;
                if (closeWith != null) cur++;
                for (; cur < tokenslen; cur++) {
                    var t = tokens[cur];
                    if (t.type == TokenType.Open) {
                        if (t.obj as string == "{") {
                            nodes.Add(parseBlock("}"));
                        }
                    } else if (t.type == TokenType.Close) {
                        if (closeWith != null && t.obj as string == closeWith) {
                            codeBlock.lengthOfCodeBlock = curToken.pos + curToken.len - tokens[begin].pos;
                            return codeBlock;
                        } else {
                            throw getException("unexpected close");
                        }
                    } else if (t.type == TokenType.Keyword) {
                        string s = t.obj as string;
                        if (s == "ifthen" || s == "?") {
                            var ift = new Ifthen() { token = curToken };
                            ift.ifTrue = tryGetNextCodeBlock();
                            if (ift.ifTrue == null) throw getException("syntax error");
                            if (nextTokenType == TokenType.Keyword
                                && nextToken.asStr == (s == "ifthen" ? "else" : ":")) {
                                cur++;
                                ift.@else = tryGetNextCodeBlock();
                                if (ift.@else == null) throw getException("syntax error");
                            }
                            nodes.Add(ift);
                        } else {
                            throw getException("unexpected token");
                        }
                    } else {
                        nodes.Add(parseObj());
                    }
                }
                if (closeWith == null) {
                    return codeBlock;
                } else {
                    cur--;
                    throw getException($"missing \"{closeWith}\"");
                }
            }

            private CodeBlock tryGetNextCodeBlock()
            {
                var next = tryGetNextObj();
                if (next is Value || next is Name) {
                    next = new CodeBlock() {
                        inputString = inputString,
                        codes = new List<Node>(new[] { next }),
                        token = curToken,
                    };
                }
                return next as CodeBlock;
            }

            Node getNextObj()
            {
                cur++;
                return parseObj();
            }

            Node tryGetNextObj()
            {
                cur++;
                if (cur >= tokens.Count) { cur--; return null; }
                return parseObj();
            }

            Node parseObj()
            {
                var t = tokens[cur];
                if (t.type == TokenType.Open) {
                    if (t.obj as string == "{") {
                        return parseBlock("}");
                    } else {
                        throw getException("unexpected open");
                    }
                } else if (t.type == TokenType.Close) {
                    throw getException("unexpected close");
                } else if (t.type == TokenType.Keyword) {
                    throw getException("unexpected keyword");
                } else if (t.IsNumber || t.type == TokenType.String) {
                    return new Value() { value = t.obj, token = t };
                } else if (t.type == TokenType.Identifier) {
                    return new Name() { name = (string)t.obj, token = t };
                } else {
                    throw getException("unknown token");
                }
            }

            TokenType nextTokenType
            {
                get {
                    if (cur + 1 < tokens.Count) {
                        return tokens[cur + 1].type;
                    } else {
                        return TokenType.None;
                    }
                }
            }

            string getTokenErrorString(int tokenIndex)
            {
                var t = tokens[tokenIndex];
                string codepiece;
                var str = inputString;
                if (t.len > 0)
                    codepiece = str.Substring(t.pos, t.len);
                else if (t.pos + 10 < str.Length)
                    codepiece = str.Substring(t.pos, 10);
                else
                    codepiece = str.Substring(t.pos, str.Length - t.pos);
                return $"ch {t.pos + 1} t {tokenIndex + 1}: \"{codepiece}\"";
            }

            void throwException(string message)
            {
                throw getException(message);
            }

            SharpRPNException getException(string message)
            {
                return new SharpRPNException(message, curToken, inputString);
            }
        }

        public class Node
        {
            public Token token;
        }

        public class CodeBlock : Node
        {
            public List<Node> codes;
            public string inputString;
            public int lengthOfCodeBlock;

            public override string ToString()
            {
                return inputString.Substring(token.pos, lengthOfCodeBlock);
            }
        }

        public class Name : Node
        {
            public string name;
        }

        public class Value : Node
        {
            public object value;
        }

        public class Ifthen : Node
        {
            public CodeBlock ifTrue;
            public CodeBlock @else;
        }
    }

    public class SharpRPNException : Exception
    {
        public SharpRPNException(string message, Token token, string inputString, Exception inner = null)
            : base(message + " at " + getTokenInfo(token, inputString), inner)
        {

        }

        public static string getTokenInfo(Token token, string inputString)
        {
            var t = token;
            string codepiece;
            var str = inputString;
            if (t.len > 0)
                codepiece = str.Substring(t.pos, t.len);
            else if (t.pos + 10 < str.Length)
                codepiece = str.Substring(t.pos, 10);
            else
                codepiece = str.Substring(t.pos, str.Length - t.pos);
            return $"ch {t.pos + 1} : \"{codepiece}\"";
        }
    }
}
