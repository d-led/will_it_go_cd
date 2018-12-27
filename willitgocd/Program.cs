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
                Console.Error.WriteLine(e);
                return 1;
            }
        }

        Analysis analysis;

        Program(Analysis a)
        {
            analysis = a;
        }

        static void ProcessXml(string filename)
        {
            Console.WriteLine($"Processing {filename}");
            proceedWith(Analysis.OfXMLFile(filename));
        }

        static void proceedWith(Analysis analysis)
        {
            new Program(analysis)
                .Run()
            ;
        }

        private void Run()
        {
            ShowAgents();
            Separator();
            ShowJobsBuiltByAgents();
            Separator();
            ShowAgentsAvailableToJobs();
        }

        void ShowAgentsAvailableToJobs()
        {
            var jobs = analysis.AgentsAvailableToJobs;
            foreach (var job in jobs)
            {
                var j = job.Job;
                var Id = $"'{j.Pipeline} :: {j.Stage} :: {j.Name}'  r:({String.Join(",", j.RequiredResources)}) e:({String.Join(",", j.Environments)})";

                Console.WriteLine($"Agents available to: {Id}:");
                ShowAgents(job.Agents);
            }
        }

        static string PrintableIdOf(Agent a)
        {
            return $"{a.Hostname}: r:({String.Join(",", a.Resources)}) e:({String.Join(",", a.Environments)}) ({a.Uuid})";
        }

        void ShowJobsBuiltByAgents()
        {
            var jobsBuiltByAgents = analysis.AgentScopes;
            foreach (var agentCapability in jobsBuiltByAgents)
            {
                var a = agentCapability.Agent;
                var Id = PrintableIdOf(a);

                Console.WriteLine($"Jobs that can be built by {Id}:");

                var jobs = agentCapability
                    .Jobs
                    .Select(j => new
                    {
                        Pipeline = j.Pipeline,
                        Stage = j.Stage,
                        Name = j.Name,
                        RequiredResources = String.Join(", ", j.RequiredResources),
                        Environments = String.Join(", ", j.Environments)
                    })
                    .ToList();

                ConsoleTableBuilder
                    .From(jobs)
                    .WithFormat(ConsoleTableBuilderFormat.Minimal)
                    .ExportAndWriteLine()
                ;
            }
        }

        static void Separator()
        {
            Console.WriteLine();
            Console.WriteLine(new String('=', 100));
            Console.WriteLine();
        }

        void ShowAgents()
        {
            Console.WriteLine("Agents:\n");
            ShowAgents(analysis.Agents);
        }

        static void ShowAgents(System.Collections.Generic.IEnumerable<Agent> AllAgents)
        {
            var agents = AllAgents.Select(a => new
            {
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


    [Verb("xml", HelpText = "parse a GoCD XML file and analyze")]
    class XmlArguments
    {
        [Option('f', "filename", Required = true, HelpText = "Path to the XML file to parse")]
        public string Filename { get; set; }
    }
}
