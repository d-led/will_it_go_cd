using System;
using CommandLine;
using wigc.analysis;
using ConsoleTableExt;
using System.Linq;
using System.Collections.Generic;

namespace willitgocd
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                Parser.Default.Settings.MaximumDisplayWidth = 80;
                return Parser.Default.ParseArguments<
                    XmlArguments,
                    ServerArguments
                >(args)
                .MapResult(
                    (XmlArguments opts) =>
                    {
                        ProcessXml(opts.Filename);
                        return 0;
                    },
                    (ServerArguments opts) =>
                    {
                        ProcessServerXml(opts);
                        return 0;
                    },
                    err => 1
                )
                ;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"<<<OOPS>>: an error happened: {e}");
                return 1;
            }
        }

        private static void ProcessServerXml(ServerArguments opts)
        {
            Console.WriteLine($"Processing config of {opts.Server}");
            proceedWith(AnalysisOfServerXML(opts));
        }

        static void ProcessXml(string filename)
        {
            Console.WriteLine($"Processing {filename}");
            proceedWith(AnalysisOfXMLFile(filename));
        }

        static Analysis AnalysisOfXMLFile(string filename)
        {
            return new Analysis(new FileConfig(filename));
        }

        static Analysis AnalysisOfServerXML(ServerArguments opts)
        {
            return new Analysis(new ServerConfig(opts.Server, opts.Username, opts.Password, opts.SkipVerify));
        }

        static void proceedWith(Analysis analysis)
        {
            new ConsoleReport(analysis)
                .Run()
            ;
        }
    }


    [Verb("xml", HelpText = "parse a GoCD XML file and analyze")]
    class XmlArguments
    {
        [Option('f', "filename", Required = true, HelpText = "Path to the XML file to parse")]
        public string Filename { get; set; }
    }

    [Verb("server", HelpText = "parse a GoCD XML config from an URL and analyze")]
    class ServerArguments
    {
        [Option('s', "server", Required = true, HelpText = "e.g. http://localhost:8153")]
        public string Server { get; set; }

        [Option('u', "username", Required = false)]
        public string Username { get; set; }

        [Option('p', "password", Required = false)]
        public string Password { get; set; }

        [Option('k', "skipverify", Required = false)]
        public bool SkipVerify { get; set; }
    }
}
