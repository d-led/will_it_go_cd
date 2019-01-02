using System.IO;
using System.Xml.Serialization;
using wigc;

public class FileConfig : ConfigSource
{
    private string filename;

    public FileConfig(string filename)
    {
        this.filename = filename;
    }

    public Cruise Fetch()
    {
        XmlSerializer serializer = new XmlSerializer(typeof(Cruise));
        using (var reader = new FileStream(filename, FileMode.Open))
        {
            return (Cruise)serializer.Deserialize(reader);
        }
    }
}
