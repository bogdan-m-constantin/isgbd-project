using MiniDBMS.Context;
using MiniDBMS.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDBMS.SqlCommands
{
    public abstract class SqlCommand
    {
        protected readonly string[] _command;
        public abstract string CorrectSyntax { get; }
        public SqlCommand(string[] cmd)
        {
            _command = cmd;
        }

        public abstract void Execute(SqlExecutionContext context);
        public abstract void Parse();
        protected void ThrowInvalidSyntaxError()
        {
            throw new InvalidSyntaxException($"Invalid syntax. Correct syntax is: {CorrectSyntax}");
        }
    }

}
