using System;
using Xbehave;
using FluentAssertions;
using wigc.analysis;
using System.Xml.Serialization;
using System.IO;
using System.Linq;

namespace wigc_test
{
    public class GoCdTest
    {
        const string withEnvironments = "../../../data/with_environments.xml";

        [Scenario]
        public void TestWithEnvironments(Analysis analysis)
        {
            "Given a config with environmens"
                .x(()=>{
                    analysis = Analysis.OfXMLFile(withEnvironments);
                });

            "Where two out of four agents belong to an environment"
                .x(()=>{
                   analysis.Agents.Where(a=>a.Environments.Count()==1).Should().HaveCount(2); 
                });
        }
    }
}
