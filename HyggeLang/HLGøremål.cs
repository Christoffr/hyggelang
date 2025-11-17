using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyggeLang
{
    internal class HLGøremål : IHLCallable
    {
        private readonly Stmt.Gøremål _declaration;

        public HLGøremål(Stmt.Gøremål declaration)
        {
            _declaration = declaration;
        }

        public int Arity { get { return _declaration.@params.Count; } }

        public object? Call(Interpreter interpreter, List<object?> arguments)
        {
            Environment environment = new(interpreter._gloabal);

            for (int i = 0; i < _declaration.@params.Count; i++)
            {
                environment.Define(_declaration.@params[i].Lexeme, arguments[i]);
            }

            interpreter.ExecuteBlock(_declaration.body, environment);
            return null;
        }

        public override string ToString()
        {
            return $"<gm {_declaration.name.Lexeme} >";
        }
    }
}
