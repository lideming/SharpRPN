using System;
using System.Collections;
using System.Collections.Generic;

namespace SharpRPN
{

    public class Scope
    {
        Stack _stack = new Stack();
        public Stack Stack => _stack;
        Dictionary<string, object> _vars = new Dictionary<string, object>();
        public Dictionary<string, object> Vars => _vars;

        public virtual bool TryGetVar(string name, out object obj)
        {
            return Vars.TryGetValue(name, out obj);
        }

        public virtual void SetVar(string name, object value)
        {
            Vars[name] = value;
        }


        public Scope()
        {
            Functions.InitScope(this);
        }

        public void Input(string rpncode)
        {
            Interpreter.Input(rpncode, this);
        }

        public void Input(AST cb)
        {
            Interpreter.Input(cb, this);
        }

        public void Input(AST.CodeBlock cb)
        {
            Interpreter.Input(cb, this);
        }

        public void AddFunction(string name, Function func)
        {
            this._vars.Add(name, func);
        }

        public void AddFunction(string name, Func<object> func)
        {
            this.AddFunction(name, (s) => {
                var v = func();
                if (v != null)
                    s.Push(v);
            });
        }

        public void AddFunction<T>(string name, Action<T> func)
        {
            this.AddFunction(name, (s) => {
                s.CheckArgsCount(1);
                var arg = s.Pop();
                if (arg is T == false) Functions.ThrowArgsTypeError();
                func((T)arg);
            });
        }

        public void AddFunction<T>(string name, Func<T, object> func)
        {
            this.AddFunction(name, (s) => {
                s.CheckArgsCount(1);
                var arg = s.Pop();
                if (arg is T == false) Functions.ThrowArgsTypeError();
                var v = func((T)arg);
                s.Push(v);
            });
        }

        public void AddFunction<T1, T2>(string name, Func<T1, T2, object> func)
        {
            this.AddFunction(name, (s) => {
                s.CheckArgsCount(1);
                var arg2 = s.Pop();
                var arg1 = s.Pop();
                if (arg1 is T1 == false || arg2 is T2 == false) Functions.ThrowArgsTypeError();
                var v = func((T1)arg1, (T2)arg2);
                s.Push(v);
            });
        }

        public void AddFunctionO(string name, Action<object> func)
        {
            this.AddFunction(name, (s) => {
                s.CheckArgsCount(1);
                func(s.Pop());
            });
        }

        public void AddFunctionO(string name, Func<object, object> func)
        {
            this.AddFunction(name, (s) => {
                s.CheckArgsCount(1);
                var v = func(s.Pop());
                if (v != null)
                    s.Push(v);
            });
        }

        public void AddFunctionO(string name, Func<object, object, object> func)
        {
            AddFunction(name, (s) => {
                s.CheckArgsCount(2);
                var b = s.Pop();
                var v = func(s.Pop(), b);
                if (v != null)
                    s.Push(v);
            });
        }

        public void AddFunctionD(string name, Func<dynamic, object> func)
        {
            AddFunctionO(name, func);
        }

        public void AddFunctionD(string name, Func<dynamic, dynamic, object> func)
        {
            AddFunctionO(name, func);
        }

        public void Push(object obj)
        {
            _stack.Push(obj);
        }

        public object Pop()
        {
            return _stack.Pop();
        }

        public T Pop<T>()
        {
            try {
                return (T)_stack.Pop();
            } catch (Exception) {
                throw new FunctionException("wrong arg type, need " + typeof(T).Name);
            }
        }

        public double PopNum()
        {
            return Functions.CastToNum(_stack.Pop());
        }

        public void CheckArgsCount(int count)
        {
            if (_stack.Count < count) {
                Functions.ThrowTooFewArgs();
            }
        }

        public Action<string> Write;
        public void write(string a)
        {
            if (Write != null) {
                Write(a);
            } else {
                Console.Write(a);
            }
        }

        public Action<string> WriteLine;
        public void writeLine(string a)
        {
            if (WriteLine != null) {
                WriteLine(a);
            } else {
                Console.WriteLine(a);
            }
        }

        public Func<string> ReadLine;
        public string readLine()
        {
            if (ReadLine != null) {
                return ReadLine();
            } else {
                return Console.ReadLine();
            }
        }
    }
}
