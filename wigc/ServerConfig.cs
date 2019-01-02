using System;
using System.IO;
using System.Xml.Serialization;
using wigc;
using System.Net;
using System.Net.Security;

public class ServerConfig : ConfigSource
{
    private string ServerUrl;
    public string Username;
    public string Password;
    public bool SkipVerify;

    public RemoteCertificateValidationCallback CertCallback { get; }

    public ServerConfig(string ServerUrl, string Username = "", string Password = "", bool SkipVerify = false)
    {
        this.ServerUrl = ServerUrl;
        this.Username = Username;
        this.Password = Password;
        this.SkipVerify = SkipVerify;
        this.CertCallback = (sender, cert, chain, sslPolicyErrors) => true;

        // global hack
        if (SkipVerify)
            ServicePointManager.ServerCertificateValidationCallback += CertCallback;
    }

    ~ServerConfig()
    {
        // being "nice"
        if (SkipVerify)
            ServicePointManager.ServerCertificateValidationCallback -= CertCallback;
    }

    public Cruise Fetch()
    {
        var uri = new Uri(new Uri(ServerUrl), "/go/api/admin/config.xml");
        var request = WebRequest.Create(uri);

        if (!string.IsNullOrWhiteSpace(Username))
        {
            request.Credentials = new CredentialCache { {
                uri, "Basic",
                new NetworkCredential(Username, Password)
            }};
        }

        XmlSerializer serializer = new XmlSerializer(typeof(Cruise));
        using (var reader = request.GetResponse().GetResponseStream())
        {
            return (Cruise)serializer.Deserialize(reader);
        }
    }
}
