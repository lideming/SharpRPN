using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpRPN
{
    class Functions
    {

        public static double CastToNum(object obj)
        {
            return (double)obj;
        }

        public static void ThrowTooFewArgs()
        {
            throw new TooFewArguments();
        }

        public static void ThrowArgsError() => throw new FunctionException("args error.");

        public static void ThrowArgsTypeError() => throw ArgsTypeError();

        public static FunctionException ArgsTypeError() => new FunctionException("wrong args type.");

        public static void InitScope(Scope scope)
        {
            scope.SetVar("true", true);
            scope.SetVar("false", false);
            scope.SetVar("True", true);
            scope.SetVar("False", false);
            scope.AddFunction("null", (s) => s.Push(null));
            scope.AddFunctionD("+", (a, b) => {
                return a + b;
            });
            scope.AddFunctionD("-", (a, b) => {
                return a - b;
            });
            scope.AddFunctionD("*", (a, b) => {
                return a * b;
            });
            scope.AddFunctionD("/", (a, b) => {
                return a / b;
            });
            scope.AddFunctionD("^", (a, b) => {
                if (a is double || b is double) {
                    return Math.Pow(a, b);
                }
                return a ^ b;
            });
            scope.AddFunctionD("%", (a, b) => {
                return a % b;
            });
            scope.AddFunctionD("&", (a, b) => a & b);
            scope.AddFunctionD("|", (a, b) => a | b);
            scope.AddFunctionD("<", (a, b) => a < b);
            scope.AddFunctionD("<=", (a, b) => a <= b);
            scope.AddFunctionD(">", (a, b) => a > b);
            scope.AddFunctionD(">=", (a, b) => a >= b);
            scope.AddFunctionO("==", (a, b) => a.Equals(b));
            scope.AddFunctionO("tostr", (a) => a.ToString());
            scope.AddFunctionO("tonum", (a) => {
                try {
                    return Convert.ToDouble(a);
                } catch (Exception) {
                    ThrowArgsError();
                }
                return null;
            });
            scope.AddFunctionO("toint", (a) => {
                try {
                    return Convert.ToInt32(a);
                } catch (Exception) {
                    ThrowArgsError();
                }
                return null;
            });
            scope.AddFunctionO("toint64", (a) => {
                try {
                    return Convert.ToInt64(a);
                } catch (Exception) {
                    ThrowArgsError();
                }
                return null;
            });
            scope.AddFunction("tovar", (string str) => {
                return new Var(str);
            });
            scope.AddFunctionO("gettypename", (a) => a.GetType().Name);
            scope.AddFunctionO("gettypefullname", (a) => a.GetType().FullName);
            scope.AddFunction("drop", (a) => { a.CheckArgsCount(1); a.Pop(); });
            scope.AddFunction("dup", (a) => { a.CheckArgsCount(1); a.Push(a.Peek()); });
            scope.AddFunction("swap", (a) => {
                a.CheckArgsCount(2);
                var oa = a.Pop();
                var ob = a.Pop();
                a.Push(oa);
                a.Push(ob);
            });
            scope.AddFunction("pick", (a) => {
                var num = a.Pop<int>();
                if (num <= 0 || num > a.Stack.Count) throw new ArgumentOutOfRangeException();
                object[] temp = new object[num];
                for (int i = 0; i < temp.Length; i++) {
                    temp[i] = a.Pop();
                }
                for (int i = temp.Length - 1; i >= 0; i--) {
                    a.Push(temp[i]);
                }
                a.Push(temp[num - 1]);
            });
            scope.AddFunction("depth", (a) => { a.Push(a.Stack.Count); });
            scope.AddFunction("clear", (a) => { a.Clear(); });
            scope.AddFunctionO("print", (a) => scope.write(a.ToString()));
            scope.AddFunctionO("println", (a) => scope.writeLine(a.ToString()));
            scope.AddFunction("input", () => scope.readLine());
            scope.AddFunction("eval", (s) => {
                eval(s);
            });
            scope.AddFunction("evalif", (s) => {
                s.CheckArgsCount(2);
                var condition = s.Pop();
                if (condition.Equals(true) || condition.Equals(1)) {
                    eval(s);
                }
            });
            scope.AddFunction("evalifelse", (s) => {
                s.CheckArgsCount(3);
                var condition = s.Pop();
                var str2 = s.Pop();
                var str = s.Pop();
                if (str == null || str2 == null) ThrowArgsTypeError();
                var truthy = condition.Equals(true) || condition.Equals(1);
                s.Push(truthy ? str : str2);
                eval(s);
            });
            scope.AddFunction("ifte", (s) => {
                s.CheckArgsCount(3);
                var condition = s.Pop();
                var v2 = s.Pop();
                var v1 = s.Pop();
                if (condition.Equals(true) || condition.Equals(1))
                    s.Push(v1);
                else
                    s.Push(v2);
            });
            scope.AddFunction("sto", (s) => {
                s.CheckArgsCount(2);
                var v = s.Pop();
                var value = s.Pop();
                if (v is Var va)
                    s.SetVar(va.name, value);
                else if (v is string str)
                    s.SetVar(str, value);
                else
                    ThrowArgsTypeError();
            });
            scope.AddFunction("purge", (s) => {
                s.CheckArgsCount(1);
                var v = s.Pop();
                if (v is Var va)
                    s.Vars.Remove(va.name);
                else if (v is string str)
                    s.Vars.Remove((string)v);
                else
                    ThrowArgsTypeError();
            });
            scope.AddFunction("rcl", (s) => {
                s.CheckArgsCount(1);
                var arg = s.Pop();
                var varName = arg as string ?? (arg as Var)?.name ?? throw ArgsTypeError();
                if (!s.TryGetVar(varName, out var value)) throw new FunctionException("undefinded name.");
                s.Push(value);
            });
            scope.AddFunctionO("len", (arr) => {
                if (arr is ICollection col)
                    return col.Count;
                else if (arr is string str)
                    return str.Length;
                else
                    throw ArgsTypeError();
            });
            scope.AddFunctionO("get", (arr, index) => {
                if (arr is IList list && index is int idx)
                    return list[idx];
                else if (arr is string str && index is int idx2)
                    return str[idx2];
                else
                    throw ArgsTypeError();
            });
            scope.AddFunction("vars", (s) => {
                var names = new List<object>(s.Vars.Count);
                foreach (var item in s.Vars.Keys) {
                    names.Add(item);
                }
                s.Push(new List(names));
            });
        }

        private static void eval(Scope s)
        {
            s.CheckArgsCount(1);
            var arg = s.Pop();
            if (arg is string str) {
                s.Input(str);
            } else if (arg is Function func) {
                func(s);
            } else if (arg is AST.CodeBlock cb) {
                s.Input(cb);
            } else {
                ThrowArgsTypeError();
            }
        }
    }

    class FunctionException : Exception
    {
        public FunctionException()
        {

        }

        public FunctionException(string msg) : base(msg)
        {

        }
    }

    class TooFewArguments : FunctionException
    {
        public TooFewArguments() : base("too few arguments.")
        {

        }
    }

    class Var
    {
        public string name;
        public Var(string name)
        {
            this.name = name;
        }
        public override string ToString()
        {
            return name;
        }
    }

    class List : IList, IList<object>
    {
        public List<object> list;
        public List()
        {
            list = new List<object>();
        }
        public List(List<object> baseList)
        {
            list = baseList;
        }

        public object this[int index]
        {
            get {
                return ((IList)this.list)[index];
            }

            set {
                ((IList)this.list)[index] = value;
            }
        }

        public int Count
        {
            get {
                return ((IList)this.list).Count;
            }
        }

        public bool IsFixedSize
        {
            get {
                return ((IList)this.list).IsFixedSize;
            }
        }

        public bool IsReadOnly
        {
            get {
                return ((IList)this.list).IsReadOnly;
            }
        }

        public bool IsSynchronized
        {
            get {
                return ((IList)this.list).IsSynchronized;
            }
        }

        public object SyncRoot
        {
            get {
                return ((IList)this.list).SyncRoot;
            }
        }

        public int Add(object value)
        {
            return ((IList)this.list).Add(value);
        }

        public void Clear()
        {
            ((IList)this.list).Clear();
        }

        public bool Contains(object value)
        {
            return ((IList)this.list).Contains(value);
        }

        public void CopyTo(object[] array, int arrayIndex)
        {
            ((IList<object>)this.list).CopyTo(array, arrayIndex);
        }

        public void CopyTo(Array array, int index)
        {
            ((IList)this.list).CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return ((IList)this.list).GetEnumerator();
        }

        public int IndexOf(object value)
        {
            return ((IList)this.list).IndexOf(value);
        }

        public void Insert(int index, object value)
        {
            ((IList)this.list).Insert(index, value);
        }

        public void Remove(object value)
        {
            ((IList)this.list).Remove(value);
        }

        public void RemoveAt(int index)
        {
            ((IList)this.list).RemoveAt(index);
        }

        public override string ToString()
        {
            return "[ " + string.Join(" ", list) + " ]";
        }

        void ICollection<object>.Add(object item)
        {
            ((IList<object>)this.list).Add(item);
        }

        IEnumerator<object> IEnumerable<object>.GetEnumerator()
        {
            return ((IList<object>)this.list).GetEnumerator();
        }

        bool ICollection<object>.Remove(object item)
        {
            return ((IList<object>)this.list).Remove(item);
        }
    }
}
