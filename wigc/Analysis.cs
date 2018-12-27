using System.IO;
using System.Xml.Serialization;
using System.Linq;
using System.Collections.Generic;
using System;

namespace wigc.analysis
{
    public class Agent
    {
        public IEnumerable<string> Resources { get; internal set; }
        public IEnumerable<string> Environments { get; internal set; }
        public string Hostname { get; internal set; }
        public string Ip { get; internal set; }

        public string Uuid { get; internal set; }
    }

    public class Job
    {
        public string Pipeline {get; internal set; } 
        public string Stage { get; internal set; }
        public string Name { get; internal set; }
        public IEnumerable<string> RequiredResources { get; internal set; }
    }

    public class AgentScope
    {
        public string Id { get; internal set; }
        public IEnumerable<analysis.Job> Jobs { get; internal set; }
    }


    public class Analysis
    {
        Cruise gocd;
        Dictionary<string,IEnumerable<string>> EnvironmentToAgents;
        Dictionary<string,IEnumerable<string>> AgentToEnvironments;
        Dictionary<string,IEnumerable<string>> PipelineToEnvironments;

        IEnumerable<analysis.Job> AllJobs;

        public static Analysis OfXMLFile(string filename)
        {
            return new Analysis(FromXMLFile(filename));
        }

        Analysis(Cruise g)
        {
            gocd = g;

            // mapping environments to agents
            EnvironmentToAgents = gocd.Environments
                .Environment.ToDictionary(
                    e => e.Name,
                    e=> e.Agents.Physical.Select(a => a.Uuid)
                )
            ;

            // mapping agents to environments
            AgentToEnvironments = EnvironmentToAgents
                .SelectMany(e => e.Value.Select(a => new[]{a, e.Key}))
                .GroupBy(e => e[0])
                .ToDictionary(group=> group.Key, group => group.Select(e=> e[1]))
            ;

            // mapping pipelines to environments
            PipelineToEnvironments = gocd.Environments
                .Environment.SelectMany(e =>
                    e.Pipelines.Pipeline.Select(p=>new[]{p.Name, e.Name})
                )
                .GroupBy(e => e[0])
                .ToDictionary(group=> group.Key, group => group.Select(e=> e[1]))
            ;

            // collecting jobs
            AllJobs = gocd.Pipelines.Pipeline.SelectMany(p=> {
                return p.Stage.SelectMany(s => {
                    return s.Jobs.Job.Select(j => new analysis.Job {
                        Pipeline = p.Name,
                        Name = j.Name,
                        Stage = s.Name,
                        RequiredResources = (j.Resources!=null && j.Resources.Resource!=null) ? j.Resources.Resource : Enumerable.Empty<string>()
                    });
                });
            }).ToArray();
        }

        public IEnumerable<analysis.Agent> Agents
        {
            get
            {
                return gocd.Agents.Agent.Select(a => new analysis.Agent {
                    Hostname = a.Hostname,
                    Ip = a.Ipaddress,
                    Resources = (a.Resources!=null && a.Resources.Resource!=null) ? a.Resources.Resource : Enumerable.Empty<string>(),
                    Environments = EnvironmentsForAgent(a),
                    Uuid = a.Uuid,
                });
            }
        }

        public IEnumerable<analysis.AgentScope> JobsToAgents
        {
            get
            {
                return Agents.Select(a => new analysis.AgentScope{
                    Id = $"{a.Hostname}: {String.Join(",",a.Resources)} ({a.Uuid})",
                    Jobs = JobsExecutableBy(a.Uuid)
                });
            }
        }

        private IEnumerable<Job> JobsExecutableBy(string AgentUuid)
        {
            // todo resources
            return AllJobs.Where(j => SameOrNoEnvironment(AgentUuid,j));
        }

        private bool SameOrNoEnvironment(string AgentUuid, Job j)
        {
            try {
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

                if (AgentEnvironments.Count==0 && JobEnvironments.Count == 0)
                    return true;

                return AgentEnvironments.Intersect(JobEnvironments).Count() > 0;
            } catch (Exception e) {
                // should not happen
                Console.Error.WriteLine($"Should not have happened: {e}");
                return false;
            }
        }

        private IEnumerable<string> EnvironmentsForAgent(wigc.Agent a)
        {
            return EnvironmentToAgents
                .Where(e=>
                    e.Value.Contains(a.Uuid)
                )
                .Select(e=>e.Key);
        }

        static Cruise FromXMLFile(string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Cruise));
            using (var reader = new FileStream(filename, FileMode.Open))
            {
                return (Cruise)serializer.Deserialize(reader);
            }
        }
    }
}
