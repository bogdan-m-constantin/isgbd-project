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
            Console.WriteLine("Current Database: " +(_context.CurrentDatabase ?? "-"));
            Console.WriteLine(_context.Catalog);
            try
            {
                string cmd = ReadCommand();
                if (ValidateCommand(cmd))
                {
                    var sqlCmd = cmd.ParseAsSqlCommand();
                       sqlCmd.Execute(_context);
                    Console.WriteLine("Command executed successfully");
                }
                else
                    throw new Exception("Invalid command given");
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
                _logger.Error(e, "Exception: ");
            }
            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
            Console.Clear();
        }

        private bool ValidateCommand(string cmd)
        {
            return true;
        }

        private string ReadCommand()
        {
            Console.WriteLine("Your Command:");
            string command = "\\";
            while (command.EndsWith("\\")){
                var line = Console.ReadLine() ?? "";
                command += line.Trim();
            }
            return command.Replace("\\", " ");

        }
    }
}
