using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using VDS.RDF;

namespace Functions
{
    public static class TripleGenerator
    {
        public static void GenerateTriple(Graph graph, IUriNode subject, string predicate, XDocument doc, string xPath, XmlNamespaceManager namespaceManager, string dataType = null)
        {
            IEnumerable<ILiteralNode> objectNodes = getNode(doc, xPath, namespaceManager, graph, dataType);
            if ((objectNodes != null) && (objectNodes.Any()))
                foreach (ILiteralNode node in objectNodes)
                    graph.Assert(subject, graph.CreateUriNode(predicate), node);
        }

        private static string giveMeDatePart(string date)
        {
            if (date.IndexOf("T") > 0)
                date = date.Substring(0, date.IndexOf("T"));
            return date;
        }

        private static IEnumerable<ILiteralNode> getNode(XDocument doc, string xPath, XmlNamespaceManager namespaceManager, Graph graph, string dataType = null)
        {
            ILiteralNode objectNode = null;
            IEnumerable<XElement> elements = doc.XPathSelectElements(xPath, namespaceManager)
                .Where(e => string.IsNullOrWhiteSpace(e.Value) == false);
            foreach (XElement element in elements)
            {
                if (dataType != null)
                {
                    IUriNode dataTypeUri = graph.CreateUriNode(dataType);
                    string value;
                    if (dataType == "xsd:date")
                        value = giveMeDatePart(element.Value.Trim());
                    else
                        value = element.Value.Trim();
                    yield return objectNode = graph.CreateLiteralNode(value, dataTypeUri.Uri);
                }
                else
                    yield return objectNode = graph.CreateLiteralNode(element.Value.Trim());
            }
        }
    }
}
