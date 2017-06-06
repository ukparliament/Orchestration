using System.Collections.Generic;
using System.Xml;

namespace Functions.TransformationPartyMnis
{
    public class Settings :ITransformationSettings
    {
        public string OperationName
        {
            get
            {
                return "TransformationPartyMnis";
            }
        }

        public string AcceptHeader
        {
            get
            {
                return "application/atom+xml";
            }
        }

        public XmlNamespaceManager SourceXmlNamespaceManager
        {
            get
            {
                XmlNamespaceManager sourceXmlNamespaceManager = new XmlNamespaceManager(new NameTable());
                sourceXmlNamespaceManager.AddNamespace("atom", "http://www.w3.org/2005/Atom");
                sourceXmlNamespaceManager.AddNamespace("m", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
                sourceXmlNamespaceManager.AddNamespace("d", "http://schemas.microsoft.com/ado/2007/08/dataservices");
                return sourceXmlNamespaceManager;
            }
        }

        public string SubjectRetrievalSparqlCommand
        {
            get
            {
                return @"
            construct{
                ?s a parl:Party.
            }
            where{
                ?s a parl:Party; 
                    parl:partyMnisId @partyMnisId.
            }";
            }
        }

        public Dictionary<string, string> SubjectRetrievalParameters
        {
            get
            {
                return new Dictionary<string, string>
                {
                    { "partyMnisId","atom:entry/atom:content/m:properties/d:Party_Id"}
                };
            }
        }

        public string ExistingGraphSparqlCommand
        {
            get
            {
                return @"
        construct {
        	?party a parl:Party;
                parl:partyMnisId ?partyMnisId;
                parl:partyName ?partyName.
        }
        where {
            bind(@subject as ?party)
        	?party a parl:Party.
            optional {?party parl:partyMnisId ?partyMnisId}
            optional {?party parl:partyName ?partyName}
        }";
            }
        }

        public Dictionary<string, string> ExistingGraphSparqlParameters
        {
            get
            {
                return null;
            }
        }

        public string FullDataUrlParameterizedString(string dataUrl)
        {
            return $"{dataUrl}?$select=Party_Id,Name";
        }
    }
}
