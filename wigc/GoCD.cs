using System;
using System.Xml.Serialization;
using System.Collections.Generic;

// https://xmltocsharp.azurewebsites.net
namespace wigc
{
   /* 
    Licensed under the Apache License, Version 2.0
    
    http://www.apache.org/licenses/LICENSE-2.0
    */
[XmlRoot(ElementName="backup")]
	public class Backup {
		[XmlAttribute(AttributeName="emailOnSuccess")]
		public string EmailOnSuccess { get; set; }
		[XmlAttribute(AttributeName="emailOnFailure")]
		public string EmailOnFailure { get; set; }
	}

	[XmlRoot(ElementName="server")]
	public class Server {
		[XmlElement(ElementName="backup")]
		public Backup Backup { get; set; }
		[XmlAttribute(AttributeName="artifactsdir")]
		public string Artifactsdir { get; set; }
		[XmlAttribute(AttributeName="agentAutoRegisterKey")]
		public string AgentAutoRegisterKey { get; set; }
		[XmlAttribute(AttributeName="webhookSecret")]
		public string WebhookSecret { get; set; }
		[XmlAttribute(AttributeName="commandRepositoryLocation")]
		public string CommandRepositoryLocation { get; set; }
		[XmlAttribute(AttributeName="serverId")]
		public string ServerId { get; set; }
		[XmlAttribute(AttributeName="tokenGenerationKey")]
		public string TokenGenerationKey { get; set; }
	}

	[XmlRoot(ElementName="git")]
	public class Git {
		[XmlAttribute(AttributeName="url")]
		public string Url { get; set; }
	}

	[XmlRoot(ElementName="config-repo")]
	public class Configrepo {
		[XmlElement(ElementName="git")]
		public Git Git { get; set; }
		[XmlAttribute(AttributeName="pluginId")]
		public string PluginId { get; set; }
		[XmlAttribute(AttributeName="id")]
		public string Id { get; set; }
	}

	[XmlRoot(ElementName="config-repos")]
	public class Configrepos {
		[XmlElement(ElementName="config-repo")]
		public List<Configrepo> Configrepo { get; set; }
	}

	[XmlRoot(ElementName="materials")]
	public class Materials {
		[XmlElement(ElementName="git")]
		public Git Git { get; set; }
		[XmlElement(ElementName="pipeline")]
		public List<Pipeline> Pipeline { get; set; }
	}

	[XmlRoot(ElementName="exec")]
	public class Exec {
		[XmlAttribute(AttributeName="command")]
		public string Command { get; set; }
		[XmlElement(ElementName="arg")]
		public List<string> Arg { get; set; }
		[XmlElement(ElementName="runif")]
		public Runif Runif { get; set; }
	}

	[XmlRoot(ElementName="runif")]
	public class Runif {
		[XmlAttribute(AttributeName="status")]
		public string Status { get; set; }
	}

	[XmlRoot(ElementName="tasks")]
	public class Tasks {
		[XmlElement(ElementName="exec")]
		public List<Exec> Exec { get; set; }
		[XmlElement(ElementName="fetchartifact")]
		public List<Fetchartifact> Fetchartifact { get; set; }
	}

	[XmlRoot(ElementName="artifact")]
	public class Artifact {
		[XmlAttribute(AttributeName="type")]
		public string Type { get; set; }
		[XmlAttribute(AttributeName="src")]
		public string Src { get; set; }
	}

	[XmlRoot(ElementName="artifacts")]
	public class Artifacts {
		[XmlElement(ElementName="artifact")]
		public Artifact Artifact { get; set; }
	}

	[XmlRoot(ElementName="job")]
	public class Job {
		[XmlElement(ElementName="tasks")]
		public Tasks Tasks { get; set; }
		[XmlElement(ElementName="artifacts")]
		public Artifacts Artifacts { get; set; }
		[XmlAttribute(AttributeName="name")]
		public string Name { get; set; }
		[XmlElement(ElementName="resources")]
		public Resources Resources { get; set; }
	}

	[XmlRoot(ElementName="jobs")]
	public class Jobs {
		[XmlElement(ElementName="job")]
		public List<Job> Job { get; set; }
	}

	[XmlRoot(ElementName="stage")]
	public class Stage {
		[XmlElement(ElementName="jobs")]
		public Jobs Jobs { get; set; }
		[XmlAttribute(AttributeName="name")]
		public string Name { get; set; }
	}

	[XmlRoot(ElementName="resources")]
	public class Resources {
		[XmlElement(ElementName="resource")]
		public List<string> Resource { get; set; }
	}

	[XmlRoot(ElementName="pipeline")]
	public class Pipeline {
		[XmlElement(ElementName="materials")]
		public Materials Materials { get; set; }
		[XmlElement(ElementName="stage")]
		public List<Stage> Stage { get; set; }
		[XmlAttribute(AttributeName="name")]
		public string Name { get; set; }
	}

	[XmlRoot(ElementName="fetchartifact")]
	public class Fetchartifact {
		[XmlElement(ElementName="runif")]
		public Runif Runif { get; set; }
		[XmlAttribute(AttributeName="artifactOrigin")]
		public string ArtifactOrigin { get; set; }
		[XmlAttribute(AttributeName="srcfile")]
		public string Srcfile { get; set; }
		[XmlAttribute(AttributeName="pipeline")]
		public string Pipeline { get; set; }
		[XmlAttribute(AttributeName="stage")]
		public string Stage { get; set; }
		[XmlAttribute(AttributeName="job")]
		public string Job { get; set; }
	}

	[XmlRoot(ElementName="pipelines")]
	public class Pipelines {
		[XmlElement(ElementName="pipeline")]
		public List<Pipeline> Pipeline { get; set; }
		[XmlAttribute(AttributeName="group")]
		public string Group { get; set; }
	}

	[XmlRoot(ElementName="physical")]
	public class Physical {
		[XmlAttribute(AttributeName="uuid")]
		public string Uuid { get; set; }
	}

	[XmlRoot(ElementName="agents")]
	public class Agents {
		[XmlElement(ElementName="physical")]
		public List<Physical> Physical { get; set; }
		[XmlElement(ElementName="agent")]
		public List<Agent> Agent { get; set; }
	}

	[XmlRoot(ElementName="environment")]
	public class Environment {
		[XmlElement(ElementName="agents")]
		public Agents Agents { get; set; }
		[XmlElement(ElementName="pipelines")]
		public Pipelines Pipelines { get; set; }
		[XmlAttribute(AttributeName="name")]
		public string Name { get; set; }
	}

	[XmlRoot(ElementName="environments")]
	public class Environments {
		[XmlElement(ElementName="environment")]
		public List<Environment> Environment { get; set; }
	}

	[XmlRoot(ElementName="agent")]
	public class Agent {
		[XmlElement(ElementName="resources")]
		public Resources Resources { get; set; }
		[XmlAttribute(AttributeName="hostname")]
		public string Hostname { get; set; }
		[XmlAttribute(AttributeName="ipaddress")]
		public string Ipaddress { get; set; }
		[XmlAttribute(AttributeName="uuid")]
		public string Uuid { get; set; }
	}

	[XmlRoot(ElementName="cruise")]
	public class Cruise {
		[XmlElement(ElementName="server")]
		public Server Server { get; set; }
		[XmlElement(ElementName="config-repos")]
		public Configrepos Configrepos { get; set; }
		[XmlElement(ElementName="pipelines")]
		public Pipelines Pipelines { get; set; }
		[XmlElement(ElementName="environments")]
		public Environments Environments { get; set; }
		[XmlElement(ElementName="agents")]
		public Agents Agents { get; set; }
		[XmlAttribute(AttributeName="xsi", Namespace="http://www.w3.org/2000/xmlns/")]
		public string Xsi { get; set; }
		[XmlAttribute(AttributeName="noNamespaceSchemaLocation", Namespace="http://www.w3.org/2001/XMLSchema-instance")]
		public string NoNamespaceSchemaLocation { get; set; }
		[XmlAttribute(AttributeName="schemaVersion")]
		public string SchemaVersion { get; set; }
	}

}
