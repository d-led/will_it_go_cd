using System.IO;
using System.Xml.Serialization;
using System.Linq;
using System.Collections.Generic;
using System;

namespace wigc.analysis
{
    public struct Agent
    {
        public IEnumerable<string> Resources { get; internal set; }
        public IEnumerable<string> Environments { get; internal set; }
        public string Hostname { get; internal set; }
        public string Ip { get; internal set; }

        public string Uuid { get; internal set; }
    }

    public struct Job
    {
        public string Pipeline { get; internal set; }
        public string Stage { get; internal set; }
        public string Name { get; internal set; }
        public IEnumerable<string> RequiredResources { get; internal set; }

        public IEnumerable<string> Environments { get; internal set; }
    }

    public struct AgentScope
    {
        public Agent Agent { get; internal set; }
        public IEnumerable<analysis.Job> Jobs { get; internal set; }
    }

    public struct JobAgents
    {
        public Job Job {get; internal set; }
        public IEnumerable<Agent> Agents {get; internal set;}
    }

    public class Analysis
    {
        Cruise gocd;
        Dictionary<string, IEnumerable<string>> EnvironmentToAgents;
        Dictionary<string, IEnumerable<string>> AgentToEnvironments;
        Dictionary<string, IEnumerable<string>> PipelineToEnvironments;

        IEnumerable<analysis.Job> AllJobs;

        public static Analysis OfXMLFile(string filename)
        {
            return new Analysis(FromXMLFile(filename));
        }

        Analysis(Cruise g)
        {
            gocd = g;

            // mapping environments to agents
            EnvironmentToAgents = gocd.Environments != null ? gocd.Environments
                .Environment.ToDictionary(
                    e => e.Name,
                    e => e.Agents.Physical.Select(a => a.Uuid)
                )
                :
                new Dictionary<string, IEnumerable<string>>()
            ;

            // mapping agents to environments
            AgentToEnvironments = EnvironmentToAgents
                .SelectMany(e => e.Value.Select(a => new[] { a, e.Key }))
                .GroupBy(e => e[0])
                .ToDictionary(group => group.Key, group => group.Select(e => e[1]))
            ;

            // mapping pipelines to environments
            PipelineToEnvironments = gocd.Environments != null ? gocd.Environments
                .Environment.SelectMany(e =>
                    e.Pipelines.Pipeline.Select(p => new[] { p.Name, e.Name })
                )
                .GroupBy(e => e[0])
                .ToDictionary(group => group.Key, group => group.Select(e => e[1]))
                :
                new Dictionary<string, IEnumerable<string>>()
            ;

            // collecting jobs
            AllJobs = gocd.Pipelines.Pipeline.SelectMany(p =>
            {
                return p.Stage.SelectMany(s =>
                {
                    return s.Jobs.Job.Select(j => new analysis.Job
                    {
                        Pipeline = p.Name,
                        Name = j.Name,
                        Stage = s.Name,
                        RequiredResources = (j.Resources != null && j.Resources.Resource != null) ? j.Resources.Resource : Enumerable.Empty<string>(),
                        Environments = PipelineToEnvironments.ContainsKey(p.Name) ? PipelineToEnvironments[p.Name] : Enumerable.Empty<string>()
                    });
                });
            }).ToArray();
        }

        public IEnumerable<analysis.Agent> Agents
        {
            get
            {
                return gocd.Agents.Agent.Select(a => new analysis.Agent
                {
                    Hostname = a.Hostname,
                    Ip = a.Ipaddress,
                    Resources = (a.Resources != null && a.Resources.Resource != null) ? a.Resources.Resource : Enumerable.Empty<string>(),
                    Environments = EnvironmentsForAgent(a),
                    Uuid = a.Uuid,
                });
            }
        }

        public IEnumerable<analysis.AgentScope> AgentScopes
        {
            get
            {
                return Agents.Select(a => new analysis.AgentScope
                {
                    Agent = a,
                    Jobs = JobsExecutableBy(a)
                });
            }
        }

        public IEnumerable<analysis.JobAgents> AgentsAvailableToJobs
        {
            get
            {
                return AgentScopes
                    .SelectMany(s => s.Jobs.Select(j => new {Job = j, Agent = s.Agent}))
                    .GroupBy(s=>s.Job)
                    .Select(g => new analysis.JobAgents {
                        Job = g.Key,
                        Agents = g.Select(e=>e.Agent)
                    })
                ;
            }
        }

        private IEnumerable<Job> JobsExecutableBy(Agent a)
        {
            return AllJobs.Where(j =>
                SameOrNoEnvironment(a.Uuid, j)
                &&
                AgentProvidesResources(a, j)
            );
        }

        private bool AgentProvidesResources(Agent a, Job j)
        {
            if (j.RequiredResources.Count() == 0)
                return true;

            return new HashSet<string>(a.Resources)
                .Intersect(j.RequiredResources)
                // all required resources should be satisfied
                .Count() == j.RequiredResources.Count()
            ;
        }

        private bool SameOrNoEnvironment(string AgentUuid, Job j)
        {
            try
            {
                bool AgentNotInEnvironment = !AgentToEnvironments.ContainsKey(AgentUuid);
                bool PipelineNotInEnvironment = !PipelineToEnvironments.ContainsKey(j.Pipeline);

                // if both not in an environment, all ok
                if (AgentNotInEnvironment && PipelineNotInEnvironment)
                    return true;

                // either one is not in an environment
                if (AgentNotInEnvironment || PipelineNotInEnvironment)
                    return false;

                var AgentEnvironments = new HashSet<string>(AgentToEnvironments[AgentUuid]);
                var JobEnvironments = new HashSet<string>(PipelineToEnvironments[j.Pipeline]);

                if (AgentEnvironments.Count == 0 && JobEnvironments.Count == 0)
                    return true;

                // the pipeline and the agent should share at least one environment 
                return AgentEnvironments.Intersect(JobEnvironments).Count() > 0;
            }
            catch (Exception e)
            {
                // should not happen
                Console.Error.WriteLine($"Should not have happened: {e}");
                return false;
            }
        }

        private IEnumerable<string> EnvironmentsForAgent(wigc.Agent a)
        {
            return EnvironmentToAgents
                .Where(e =>
                    e.Value.Contains(a.Uuid)
                )
                .Select(e => e.Key);
        }

        static Cruise FromXMLFile(string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Cruise));
            using (var reader = new FileStream(filename, FileMode.Open))
            {
                return (Cruise)serializer.Deserialize(reader);
            }
        }

        public bool AgentWithoutEnvironment(string uuid)
        {
            return !AgentToEnvironments.ContainsKey(uuid)
                || AgentToEnvironments[uuid].Count() == 0;
        }
    }
}
