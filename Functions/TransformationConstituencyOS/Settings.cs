using System;
using System.Collections.Generic;
using System.Net;
using VDS.RDF.Query;

namespace Functions.TransformationConstituencyOS
{
    public class Settings : ITransformationSettings
    {

        public string AcceptHeader
        {
            get
            {
                return "application/rdf+xml";
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
                ?s parl:constituencyGroupOnsCode @constituencyGroupOnsCode.
            }";
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
            ?constituencyGroup parl:constituencyGroupOnsCode ?constituencyGroupOnsCode.
            optional {
                ?constituencyGroup parl:constituencyGroupHasConstituencyArea ?constituencyArea.
                optional {?constituencyArea parl:constituencyAreaExtent ?constituencyAreaExtent}
                optional {?constituencyArea parl:constituencyAreaLatitude ?constituencyAreaLatitude}
                optional {?constituencyArea parl:constituencyAreaLongitude ?constituencyAreaLongitude}
            }
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
