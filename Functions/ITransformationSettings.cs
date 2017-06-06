using System.Collections.Generic;
using System.Xml;

namespace Functions
{
    public interface ITransformationSettings
    {
        string OperationName { get; }
        string AcceptHeader { get; }
        XmlNamespaceManager SourceXmlNamespaceManager { get; }
        string SubjectRetrievalSparqlCommand { get; }
        Dictionary<string, string> SubjectRetrievalParameters { get; }
        string ExistingGraphSparqlCommand { get; }
        Dictionary<string, string> ExistingGraphSparqlParameters { get; }

        string FullDataUrlParameterizedString(string dataUrl);
    }
}