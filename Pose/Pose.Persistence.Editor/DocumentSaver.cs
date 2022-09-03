using Extensions.JSON;
using System.IO;

namespace Pose.Persistence.Editor
{
    public static class DocumentSaver
    {
        // The Persistence assembly had access to the internals of the domain, so it can read the data.

        public static void SaveDocument(Domain.Documents.Document document)
        {
            var doc = ProtoDocumentBuilder.CreateProtobufDocument(document);
            Save(doc.Serialize(), document.Filename);
        }

        public static void Save(string json, string filename)
        {
            File.WriteAllText(filename, json);
        }
    }
}
