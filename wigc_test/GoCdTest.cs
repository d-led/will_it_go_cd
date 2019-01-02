using System;
using System.Collections.Generic;
using Xbehave;
using FluentAssertions;
using wigc.analysis;
using System.Xml.Serialization;
using System.IO;
using System.Linq;
using wigc;

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
                    analysis = new Analysis(new FileConfig(withEnvironments));
                });

            "Where two out of four agents belong to an environment"
                .x(()=>{
                   analysis.Agents.Where(a=>a.Environments.Count()==1).Should().HaveCount(2); 
                });

            "Then no agent not associated with an environment can build a job associated with one"
                .x(() => {
                    foreach(var scope in analysis.AgentScopes
                        .Where(e=>analysis.AgentWithoutEnvironment(e.Agent.Uuid))) {
                            scope
                                .Jobs
                                .Where(j => j.Environments.Count()>0)
                                .Should().BeEmpty();
                        }
                });

            "And jobs associated with an environment can only run on agents part of at least one environment"
                .x(() => {
                    foreach(var scope in analysis.AgentScopes
                        .Where(e=>!analysis.AgentWithoutEnvironment(e.Agent.Uuid))) {
                            scope
                                .Jobs
                                .Where(j => j.Environments.Count()>0)
                                .Should().NotBeEmpty();
                        }
                });

            // more important properties?
        }

        [Scenario]
        public void ReplacingParametersInStrings() {
            "GoCD replaces tokens in configuration strings"
                .x(()=>{
                    var dict = new Dictionary<string,string>{
                        {"unit", "42"},
                        {"unit_resource", "my_resource"},
                        {"#{unit_resource}", "x"},
                    };

                    var ip = new ParameterInterpolator(dict);
                    ip.Substitute("unit").Should().Be("unit");
                    ip.Substitute("#{unit_resource}").Should().Be("my_resource");
                    ip.Substitute("unit_resource").Should().Be("unit_resource");

                    // spaces not allowed (yet)
                    ip.Substitute("#{unit_resource} ").Should().Be("#{unit_resource} ");
                });
        }
    }
}
