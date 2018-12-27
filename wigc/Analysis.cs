using System.IO;
using System.Xml.Serialization;
using System.Linq;
using System.Collections.Generic;

namespace wigc
{
    public class Analysis {
        Cruise gocd;

        public static Analysis ofXMLFile(string filename) {
            return new Analysis(fromXMLFile(filename));
        }

        Analysis(Cruise g) {
            gocd = g;
        }

        public IEnumerable<Agent> Agents {
            get {
                return gocd.Agents.Agent;
            }
        }

        static Cruise fromXMLFile(string filename) {
            XmlSerializer serializer = new XmlSerializer(typeof(Cruise));
            using (var reader = new FileStream(filename, FileMode.Open))
            {
                return (Cruise)serializer.Deserialize(reader);
            }
        }
    }
}
