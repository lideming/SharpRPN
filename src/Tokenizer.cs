using System;
using System.Collections.Generic;
using System.Text;

namespace SharpRPN
{

    public enum TokenType
    {
        None,
        Unknown,
        Keyword,
        Int,
        Float,
        Double,
        String,
        Identifier,
        Open,
        Close,
    }

    public class Token
    {
        public object obj;
        public string asStr => obj as string;
        public TokenType type;
        public int pos;
        public int len;

        public Token(TokenType type, int pos, object obj = null, int endpos = -1)
        {
            this.type = type;
            this.pos = pos;
            this.obj = obj;
            if (endpos != -1) {
                len = endpos - pos;
            } else {
                len = -1;
            }
        }

        public bool IsNumber => type == TokenType.Int | type == TokenType.Float | type == TokenType.Double;
    }

    public class Tokenizer
    {
        private object _runninglock = new object();
        private List<Token> tokens;
        public static List<string> Keywords = new List<string>();
        public static List<string> Opens = new List<string>();
        public static List<string> Closes = new List<string>();
        public static List<char> SpecialChar = new List<char>();

        int cur;
        string input;

        static Tokenizer()
        {
            Opens.AddRange(new[] { "{" });
            Closes.AddRange(new[] { "}" });
            Keywords.AddRange(new[] { "ifthen", "else", "?", ":" });
            SpecialChar.AddRange(new[] { '"', '\'', '{', '}', '?', ':' });
        }

        public Result Input(string input)
        {
            lock (_runninglock) {
                return _input(input);
            }
        }

        private Result _input(string input)
        {
            tokens = new List<Token>();
            cur = 0;
            this.input = input;
            var len = input.Length;
            while (cur < input.Length) {
                var begin = cur;
                var ch = input[cur];
                if (isSpace(ch)) {
                    cur++;
                    continue;
                }
                if (ch == '"' || ch == '\'') {
                    handleString(ch);
                } else if (char.IsDigit(ch) || ch == '-' || ch == '.') {
                    handleNumber();
                } else {
                    var w = getNextWord();
                    if (Keywords.Contains(w)) {
                        addToken(TokenType.Keyword, w, begin, cur);
                    } else if (Opens.Contains(w)) {
                        addToken(TokenType.Open, w, begin, cur);
                    } else if (Closes.Contains(w)) {
                        addToken(TokenType.Close, w, begin, cur);
                    } else {
                        addToken(TokenType.Identifier, w, begin, cur);
                    }
                }
            }
            var result = new Result();
            result.Tokens = tokens;
            result.Input = input;
            return result;
        }

        private void handleNumber()
        {
            int begin = cur;
            var w = getNextWord();
            if (!w.EndsWith("f") && (w.Contains(".") || w.Contains("e") || w.EndsWith("d"))) {
                double num;
                if (double.TryParse(w.TrimEnd('d'), out num)) {
                    addToken(TokenType.Double, num, begin, cur);
                } else {
                    throw new TokenizerException("invaild number.", cur);
                }
            } else if (w.EndsWith("f")) {
                float num;
                if (float.TryParse(w.TrimEnd('f'), out num)) {
                    addToken(TokenType.Float, num, begin, cur);
                } else {
                    throw new TokenizerException("invaild number.", cur);
                }
            } else {
                int num;
                if (int.TryParse(w, out num)) {
                    addToken(TokenType.Int, num, begin, cur);
                } else {
                    throw new TokenizerException("invaild number.", cur);
                }
            }
        }

        private void handleString(char mark)
        {
            var begin = cur;
            int strLength = measureString(mark);
            char[] s = new char[strLength];
            int spos = 0;
            bool escaped = false;
            cur++;
            while (true) {
                var ch = input[cur++];
                if (escaped) {
                    if (ch == '"') {
                        ch = '"';
                    } else if (ch == '\'') {
                        ch = '\'';
                    } else if (ch == '\\') {
                        ch = '\\';
                    } else if (ch == 'r') {
                        ch = '\r';
                    } else if (ch == 'n') {
                        ch = '\n';
                    } else if (ch == 't') {
                        ch = '\t';
                    }
                    escaped = false;
                } else if (ch == '\\') {
                    escaped = true;
                    continue;
                } else if (ch == mark) {
                    break;
                }
                s[spos++] = ch;
            }
            addToken(TokenType.String, new string(s), begin, cur);
        }

        private int measureString(char mark)
        {
            int strLength = 0;
            bool escaped = false;
            bool succeed = false;
            for (int i = cur + 1; i < input.Length; i++) {
                var ch = input[i];
                if (escaped) {
                    escaped = false;
                    continue;
                }
                if (ch == '\\') {
                    escaped = true;
                } else if (ch == mark) {
                    succeed = true;
                    break;
                }
                strLength++;
            }
            if (succeed == false) {
                throw new TokenizerException("missing quotation mark.", cur);
            }

            return strLength;
        }

        StringBuilder sb = new StringBuilder();
        private string getNextWord()
        {
            sb.Clear();
            while (true) {
                if (cur >= input.Length)
                    break;
                var ch = input[cur];
                cur++;
                if (isSpace(ch) && sb.Length > 0) {
                    break;
                }
                bool isSpecialChar = SpecialChar.Contains(ch);
                if (isSpecialChar) {
                    if (sb.Length == 0)
                        return ch.ToString();
                    cur--;
                    return sb.ToString();
                }
                sb.Append(ch);
            }
            return sb.ToString();
        }

        private static bool isSpace(char ch)
        {
            return ch == ' ' || ch == '\t' || ch == '\r' || ch == '\n';
        }

        private void addToken(TokenType type, object obj = null, int begin = -2, int end = -2)
        {
            if (begin == -2) {
                begin = cur;
            }
            if (end == -2) {
                end = cur;
            }
            tokens.Add(new Token(type, begin, obj, end));
        }

        class TokenizerException : Exception
        {
            public int Position { get; }
            public TokenizerException(string message, int pos) : base(message + " at ch " + pos)
            {
                Position = pos;
            }
        }

        public class Result
        {
            public List<Token> Tokens;
            public string Input;
        }
    }
}
