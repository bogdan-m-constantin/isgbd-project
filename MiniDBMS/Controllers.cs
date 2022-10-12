using MiniDBMS.Context;
using MiniDBMS.Utils;
using Raven.Client.Documents;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDBMS
{
    
    public class Controller
    {
        private readonly ILogger _logger;
        private readonly SqlExecutionContext _context;
        public Controller(ILogger logger, SqlExecutionContext context)
        {
            _logger = logger;
            _context = context;
        }

        public void Loop()
        {
            string cmd = ReadCommand();
            if (ValidateCommand(cmd))
            {
                var sqlCmd = cmd.ParseAsSqlCommand();
                sqlCmd.Execute(_context);
            }
            else
                throw new Exception("Invalid command given");
        }

        private bool ValidateCommand(string cmd)
        {
            return true;
        }

        private string ReadCommand()
        {
            Console.WriteLine("Your Command:");
            return Console.ReadLine() ?? "";
        }
    }
}
