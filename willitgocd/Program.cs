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
                    XmlArguments
                >(args)
                .MapResult(
                    (XmlArguments opts) =>
                    {
                        ProcessXml(opts.Filename);
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

        static void ProcessXml(string filename)
        {
            Console.WriteLine($"Processing {filename}");
            proceedWith(AnalysisOfXMLFile(filename));
        }

        static Analysis AnalysisOfXMLFile(string filename)
        {
            return new Analysis(new FileConfig(filename));
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
}
