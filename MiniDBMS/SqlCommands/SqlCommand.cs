using MiniDBMS.Context;
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
        
        public SqlCommand(string[] cmd)
        {
            _command = cmd;
        }

        public abstract void Execute(SqlExecutionContext context);
        public abstract void Parse();
    }

}
