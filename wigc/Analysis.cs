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
        IEnumerable<analysis.Job> AllJobs;

        public static Analysis OfXMLFile(string filename)
        {
            return new Analysis(FromXMLFile(filename));
        }

        Analysis(Cruise g)
        {
            gocd = g;

            // mapping the environments to agents
            EnvironmentToAgents = gocd.Environments
                .Environment.ToDictionary(
                    e => e.Name,
                    e=> e.Agents.Physical.Select(a => a.Uuid)
                )
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
                    Resources = a.Resources.Resource,
                    Environments = EnvironmentsForAgent(a),
                    Uuid = a.Uuid,
                });
            }
        }

        public IEnumerable<analysis.AgentScope> JobsToAgents
        {
            get
            {
                return gocd.Agents.Agent.Select(a => new analysis.AgentScope{
                    Id = $"{a.Hostname} ({a.Uuid})",
                    Jobs = AllJobs
                });
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
