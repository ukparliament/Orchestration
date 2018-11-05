namespace VDS.RDF
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using VDS.RDF.Query.Builder;
    using VDS.RDF.Query.Patterns;
    using VDS.RDF.Update.Commands;

    internal static class GraphDiffReportExtensions
    {
        /// <summary>
        /// Converts a <see cref="GraphDiffReport">diff</see> to an equivalent <see cref="ModifyCommand">SPARQL Update query</see>
        /// </summary>
        /// <param name="diff">The <see cref="GraphDiffReport">diff</see> to convert</param>
        /// <returns>A <see cref="ModifyCommand">SPARQL Update query</see> that represents the <see cref="GraphDiffReport">diff</see></returns>
        internal static ModifyCommand AsUpdate(this GraphDiffReport diff)
        {
            var delete = new GraphPatternBuilder();
            var insert = new GraphPatternBuilder();
            var where = new GraphPatternBuilder();

            // Groud removed triples are added as is to both delete and where clauses
            foreach (var t in diff.RemovedTriples)
            {
                delete.AddTriplePattern(t);
                where.AddTriplePattern(t);
            }

            foreach (var g in diff.RemovedMSGs)
            {
                // Blank nodes in non-ground removed triples are converted to variables and added to both delete and where clauses
                foreach (var t in g.Triples)
                {
                    delete.AddVariablePattern(t);
                    where.AddVariablePattern(t);
                }

                // An ISBLANK filter is added for each blank node in non-ground removed triples
                foreach (var n in g.BlankNodes())
                {
                    where.AddBlankNodeFilter(n);
                }
            }

            // Added triples (ground or not) are added as is to the insert clause
            foreach (var t in diff.AllAddedTriples())
            {
                insert.AddTriplePattern(t);
            }

            return new ModifyCommand(
                delete.BuildGraphPattern(),
                insert.BuildGraphPattern(),
                where.BuildGraphPattern());
        }

        private static void AddTriplePattern(this IGraphPatternBuilder builder, Triple t)
        {
            builder
                .Where(tripleBuilder =>
                    tripleBuilder
                        .Subject(t.Subject)
                        .PredicateUri(t.Predicate as IUriNode)
                        .Object(t.Object));
        }

        private static void AddVariablePattern(this IGraphPatternBuilder builder, Triple t)
        {
            builder
                .Where(tripleBuilder =>
                    tripleBuilder
                        .Subject(t.Subject.AsVariable())
                        .PredicateUri(t.Predicate as IUriNode)
                        .Object(t.Object.AsVariable()));
        }

        private static void AddBlankNodeFilter(this IGraphPatternBuilder builder, IBlankNode n)
        {
            builder.Filter(node =>
                node.IsBlank(n.InternalID));
        }

        private static INode AsVariable(this INode n)
        {
            return n is IBlankNode blank ? new NodeFactory().CreateVariableNode(blank.InternalID) : n;
        }

        private static IEnumerable<IBlankNode> BlankNodes(this IGraph g)
        {
            return g.Nodes.BlankNodes();
        }

        private static IEnumerable<Triple> AllAddedTriples(this GraphDiffReport diff)
        {
            return diff.AddedMSGs.SelectMany(msg => msg.Triples).Union(diff.AddedTriples);
        }

        private static GraphPattern BuildGraphPattern(this GraphPatternBuilder builder)
        {
            var buildGraphPattern = typeof(GraphPatternBuilder).GetMethod(nameof(BuildGraphPattern), BindingFlags.Instance | BindingFlags.NonPublic);

            return buildGraphPattern.Invoke(builder, new[] { null as INamespaceMapper }) as GraphPattern;
        }
    }
}
