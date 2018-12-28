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
            Separator();         
            ShowAgents();
            Separator();
            ShowJobsBuiltByAgents();
            Separator();
            ShowAgentsAvailableToJobs();
            Separator();
            ShowJobsWithoutAgents();
        }

        void ShowJobsWithoutAgents()
        {
            if (analysis.JobsWithoutAgents.Count() == 0)
                return;

            Console.WriteLine("<<OOPS>>: the following jobs do not have an agent available to them!\n");
            ShowJobs(analysis.JobsWithoutAgents);
        }

        void ShowAgentsAvailableToJobs()
        {
            Console.WriteLine("Agents available to jobs: \n");

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
            Console.WriteLine("Jobs built by agents: \n");
            
            var jobsBuiltByAgents = analysis.AgentScopes;
            foreach (var agentCapability in jobsBuiltByAgents)
            {
                var a = agentCapability.Agent;
                var Id = PrintableIdOf(a);

                Console.WriteLine($"Jobs that can be built by {Id}:");

                if (agentCapability
                    .Jobs.Count() == 0)
                {
                    Console.WriteLine("<<OOPS>>: no jobs will run on this agent!\n");
                    continue;
                }

                ShowJobs(agentCapability.Jobs);
            }
        }

        private static void ShowJobs(IEnumerable<Job> selectedJobs)
        {
            var jobs = selectedJobs
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

        static void Separator()
        {
            Console.WriteLine();
            Console.WriteLine(new String('=', 100));
            Console.WriteLine();
        }

        void ShowAgents()
        {
            Console.WriteLine("Agents:\n");
            if (analysis.Agents.Count() == 0) {
                Console.Error.WriteLine($"<<<OOPS>>: no agents are configured");
                return;
            }

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
