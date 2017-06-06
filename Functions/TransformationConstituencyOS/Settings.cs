using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using VDS.RDF.Query;

namespace Functions.TransformationConstituencyOS
{
    public class Settings : ITransformationSettings
    {
        public string OperationName
        {
            get
            {
                return "TransformationConstituencyOS";
            }
        }

        public string AcceptHeader
        {
            get
            {
                return "application/rdf+xml";
            }
        }

        public XmlNamespaceManager SourceXmlNamespaceManager
        {
            get
            {
                XmlNamespaceManager sourceXmlNamespaceManager = new XmlNamespaceManager(new NameTable());
                sourceXmlNamespaceManager.AddNamespace("admingeo", "http://data.ordnancesurvey.co.uk/ontology/admingeo/");
                sourceXmlNamespaceManager.AddNamespace("rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
                sourceXmlNamespaceManager.AddNamespace("owl", "http://www.w3.org/2002/07/owl#");
                sourceXmlNamespaceManager.AddNamespace("geometry", "http://data.ordnancesurvey.co.uk/ontology/geometry/");
                sourceXmlNamespaceManager.AddNamespace("rdfs", "http://www.w3.org/2000/01/rdf-schema#");
                sourceXmlNamespaceManager.AddNamespace("wgs84", "http://www.w3.org/2003/01/geo/wgs84_pos#");
                return sourceXmlNamespaceManager;
            }
        }

        public string SubjectRetrievalSparqlCommand
        {
            get
            {
                return @"
            construct{
                ?s a parl:ConstituencyGroup.
            }
            where{
                ?s a parl:ConstituencyGroup; 
                    parl:constituencyGroupOnsCode @constituencyGroupOnsCode.
            }";
            }
        }

        public Dictionary<string, string> SubjectRetrievalParameters
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    {"constituencyGroupOnsCode", "rdf:RDF/rdf:Description/admingeo:gssCode" }
                };
            }
        }

        public string ExistingGraphSparqlCommand
        {
            get
            {
                return @"
        construct {
        	?constituencyGroup a parl:ConstituencyGroup;
                parl:constituencyGroupOnsCode ?constituencyGroupOnsCode;
                parl:constituencyGroupHasConstituencyArea ?constituencyArea.
            ?constituencyArea a parl:ConstituencyArea;
                parl:constituencyAreaExtent ?constituencyAreaExtent;
                parl:constituencyAreaLatitude ?constituencyAreaLatitude;
                parl:constituencyAreaLongitude ?constituencyAreaLongitude.
        }
        where {
            bind(@subject as ?constituencyGroup)
            ?constituencyGroup a parl:ConstituencyGroup;
                parl:constituencyGroupOnsCode ?constituencyGroupOnsCode;
                parl:constituencyGroupHasConstituencyArea ?constituencyArea.
            ?constituencyArea a parl:ConstituencyArea;
                parl:constituencyAreaExtent ?constituencyAreaExtent;
                parl:constituencyAreaLatitude ?constituencyAreaLatitude;
                parl:constituencyAreaLongitude ?constituencyAreaLongitude.
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
            string osSparqlCommand = @"
                PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                PREFIX geometry: <http://data.ordnancesurvey.co.uk/ontology/geometry/>
                PREFIX admingeo: <http://data.ordnancesurvey.co.uk/ontology/admingeo/>
                PREFIX wgs84: <http://www.w3.org/2003/01/geo/wgs84_pos#>
                PREFIX owl: <http://www.w3.org/2002/07/owl#>
        
                CONSTRUCT { 
        	        @constituency rdfs:label ?label;
        		        admingeo:gssCode ?gssCode;
        		        wgs84:lat ?lat;
        		        wgs84:long ?long;
        		        owl:sameAs ?sameAs;
        		        geometry:asGML ?gml.
                } WHERE {
        	        @constituency rdfs:label ?label;
        		        admingeo:gssCode ?gssCode;
        		        wgs84:lat ?lat;
        		        wgs84:long ?long;
        		        owl:sameAs ?sameAs;
        		        geometry:extent ?extent.
        	        ?extent geometry:asGML ?gml;
                }";
            SparqlParameterizedString osSparql = new SparqlParameterizedString(osSparqlCommand);
            osSparql.SetUri("constituency", new Uri(dataUrl));
            return $"http://data.ordnancesurvey.co.uk/datasets/os-linked-data/apis/sparql?query={WebUtility.UrlEncode(osSparql.ToString())}";
        }
    }

}
