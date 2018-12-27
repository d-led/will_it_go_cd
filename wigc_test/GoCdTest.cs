using System;
using Xbehave;
using FluentAssertions;
using wigc;
using System.Xml.Serialization;
using System.IO;
using System.Linq;

namespace wigc_test
{
    public class GoCdTest
    {
        const string withEnvironments = "../../../data/with_environments.xml";

        [Scenario]
        public void TestWithEnvironments()
        {
            "Given nothing"
                .x(()=>42.Should().Be(42));

            var analysis = Analysis.ofXMLFile(withEnvironments);
        }
    }
}
