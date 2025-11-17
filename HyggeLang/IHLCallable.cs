using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyggeLang
{
    internal interface IHLCallable
    {
        int Arity { get; }

        object? Call(Interpreter interpreter, List<object?> arguments);
    }
}
