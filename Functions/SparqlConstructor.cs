using System;
using System.Collections.Generic;
using VDS.RDF;
using VDS.RDF.Query;

namespace Functions
{
    public class SparqlConstructor
    {
        public SparqlParameterizedString Sparql { get; private set; }
        private static NodeFactory nodeFactory = new NodeFactory();

        public SparqlConstructor(string sparqlText, Dictionary<string, INode> parameters)
        {
            Sparql = new SparqlParameterizedString(sparqlText);
            foreach (KeyValuePair<string, INode> parameter in parameters)
                Sparql.SetParameter(parameter.Key, parameter.Value);
        }

        public static INode GetNode()
        {
            return string.Empty.ToLiteral(nodeFactory);
        }

        public static INode GetNode(string value, Uri dateType)
        {
            return nodeFactory.CreateLiteralNode(value, dateType);
        }

        public static INode GetNode(Uri value)
        {
            return nodeFactory.CreateUriNode(value);
        }

        public static INode GetNode(string value)
        {
            return value.ToLiteral(nodeFactory);
        }

        public static INode GetNode(bool value)
        {
            return value.ToLiteral(nodeFactory);
        }

        public static INode GetNode(decimal value)
        {
            return value.ToLiteral(nodeFactory);
        }

        public static INode GetNode(double value)
        {
            return value.ToLiteral(nodeFactory);
        }

        public static INode GetNode(int value)
        {
            return value.ToLiteral(nodeFactory);
        }

        public static INode GetNodeDate(DateTimeOffset value)
        {
            DateTimeOffset dt = value;
            dt = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dt, "GMT Standard Time");
            return dt.ToLiteralDate(nodeFactory);
        }

        public static INode GetNodeTime(DateTimeOffset value)
        {
            return value.ToLiteralTime(nodeFactory);
        }

        public static INode GetNodeDateTime(DateTimeOffset value)
        {
            return value.ToLiteral(nodeFactory, true);
        }
    }
}
