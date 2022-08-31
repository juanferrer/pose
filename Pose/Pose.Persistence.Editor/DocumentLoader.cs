using System.IO;
using Extensions.JSON;
using Pose.Framework.Messaging;

namespace Pose.Persistence.Editor
{
    public static class DocumentLoader
    {
        public static Domain.Documents.Document LoadFromFile(IMessageBus messageBus, string filePath)
        {
            var doc = File.ReadAllText(filePath).Deserialize<Document>();
            return DomainDocumentBuilder.CreateDocument(messageBus, doc, filePath);
        }
    }
}
