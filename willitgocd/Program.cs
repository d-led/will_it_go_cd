using System;
using CommandLine;
using wigc.analysis;
using ConsoleTableExt;
using System.Linq;

namespace willitgocd
{
    class Program
    {
        static int Main(string[] args)
        {
            var p = new Program();
            try {
                Parser.Default.Settings.MaximumDisplayWidth = 80;
                return Parser.Default.ParseArguments<
                    XmlArguments
                >(args)
                .MapResult(
                    (XmlArguments opts) =>
                    {
                        p.ProcessXml(opts.Filename);
                        return 0;
                    },
                    err => 1
                )
                ;
            } catch (Exception e) {
                Console.Error.WriteLine(e);
                return 1;
            }
        }

        private void ProcessXml(string filename)
        {
            Console.WriteLine($"Processing {filename}");
            proceedWith(Analysis.OfXMLFile(filename));
        }

        private void proceedWith(Analysis analysis) {
            var agents = analysis.Agents.Select(a=>new {
                Hostname = a.Hostname,
                Ip = a.Ip,
                Resources = String.Join(", ", a.Resources),
                Environments = String.Join(", ", a.Environments),
                Uuid = a.Uuid
            });

            ConsoleTableBuilder
                .From(agents.ToList())
                .WithFormat(ConsoleTableBuilderFormat.Minimal)
                .ExportAndWriteLine()
            ;
        }
    }

    
    [Verb("xml", HelpText="parse a GoCD XML file and analyze")]
    class XmlArguments {
        [Option('f', "filename", Required = true, HelpText = "Path to the XML file to parse")]
        public string Filename {get; set;}
    }
}
