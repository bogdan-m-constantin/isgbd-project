using MiniDBMS.Context;
using MiniDBMS.Utils;
using Raven.Client.Documents;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            Stopwatch? sw = null;
            try
            {
                string cmd = ReadCommand();
                if (ValidateCommand(cmd))
                {
                    var sqlCmd = cmd.ParseAsSqlCommand();
                    sw=  Stopwatch.StartNew();
                       sqlCmd.Execute(_context);
                    sw.Stop();
                    TimeSpan e = sw.Elapsed;
                    Console.WriteLine($"Command executed successfully in  {(int)e.TotalSeconds}.{e.Milliseconds:000}s");
                }
                else
                    throw new Exception("Invalid command given");
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
                sw?.Stop();
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
