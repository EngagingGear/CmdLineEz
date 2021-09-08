using System;
using System.IO;
using CmdLineEzNs; // This is he namespace to use

namespace HelloWorld
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var cmdLine = new CmdLineEz()
                    .Config("configFile")
                    .Flag("capitalize")
                    .Param("filename")
                    .Param("param1")
                    .Param("param2")
                    .Required("filename")
                    .AllowTrailing();
                cmdLine.Process(args);
                using (var fileStream = new FileStream(cmdLine.ParamVal("filename"), FileMode.OpenOrCreate))
                {
                    using (var streamWriter = new StreamWriter(fileStream))
                    {
                        foreach (var item in cmdLine.TrailingVal())
                            if (cmdLine.FlagVal("capitalize") ?? false)
                                streamWriter.WriteLine(item.ToUpper());
                            else
                                streamWriter.WriteLine(item);

                        streamWriter.WriteLine(cmdLine.ParamVal("param1"));
                    }
                }
            }
            catch (ArgumentException e)
            {
                Console.WriteLine("Invalid command argument: " + e.Message);
            }
        }
    }
}