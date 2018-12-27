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

    public class Analysis
    {
        Cruise gocd;
        Dictionary<string,IEnumerable<string>> EnvironmentToAgents;

        public static Analysis OfXMLFile(string filename)
        {
            return new Analysis(FromXMLFile(filename));
        }

        Analysis(Cruise g)
        {
            gocd = g;
            EnvironmentToAgents = gocd.Environments
                .Environment.ToDictionary(
                    e => e.Name,
                    e=> e.Agents.Physical.Select(a => a.Uuid)
                )
            ;
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
