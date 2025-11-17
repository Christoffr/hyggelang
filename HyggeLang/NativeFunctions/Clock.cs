using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyggeLang.NativeClasses
{
    internal class Clock : IHLCallable
    {
        public int Arity { get { return 0; } }

        public object? Call(Interpreter interpreter, List<object?> arguments)
        {
            return DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }
    }
}
