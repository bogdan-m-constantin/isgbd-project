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
        private object lockObj = new object();
        public int finsihed = 0;
        public Controller(ILogger logger, SqlExecutionContext context)
        {
            _logger = logger;
            _context = context;
        }
        public void LoadData()
        {
            Random random = new Random();
            Stopwatch? sw = null;
            ExecuteCommand("USE DATABASE DB",sw);
            

            for (int i = 30000; i <= 10000 ; i++)
            {
                var sql = $"INSERT INTO Groups (GroupId, Specialization, Language) VALUES ({i},\"Group {i}\", \"{(random.NextDouble() > 0.6 ? "RO" :"EN")}\")";
                Console.WriteLine($"Loaded group {i} / 10000 ({(i / 100.0).ToString("0.000")})%");
                ExecuteCommand(sql,sw);
               
            }
         
            for (int i = 0; i < 10000000; i++)
            {

                var sql = $"INSERT INTO Students (StudentId, GroupId, Name, Email, Mark) VALUES ({i},{random.Next(1, 10000)},\"Student {i}\", \"student{i}@email.ro\", {(random.NextDouble() * 9 + 1).ToString("0.00")})\r\n";
                try
                {
                    ExecuteCommand(sql, sw);
                    if(i%100  == 0)
                    {
                        Console.Clear();
                        Console.WriteLine($"{((i / 10000.0).ToString("0.000"))}%");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

            }

        }
        public void ExecuteCommand(string cmd, Stopwatch? sw)
        {
            if (ValidateCommand(cmd))
            {
                var sqlCmd = cmd.ParseAsSqlCommand();
                sw = Stopwatch.StartNew();
                sqlCmd.Execute(_context);
                sw.Stop();
                TimeSpan e = sw.Elapsed;
                //Console.WriteLine($"Command executed successfully in  {(int)e.TotalSeconds}.{e.Milliseconds:000}s");
            }
            else
                throw new Exception("Invalid command given");
        }
        public void Loop()
        {
            Console.WriteLine("Current Database: " +(_context.CurrentDatabase ?? "-"));
            Console.WriteLine(_context.Catalog);
            Stopwatch? sw = null;
            try
            {
                string cmd = ReadCommand().Trim();
                if(cmd == "LOAD_TEST_DATA")
                {
                    LoadData();
                    return;
                }
                ExecuteCommand(cmd,sw);
            }catch(Exception e)
            {
                Console.WriteLine(e);
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
