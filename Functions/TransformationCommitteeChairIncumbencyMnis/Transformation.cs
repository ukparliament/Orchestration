using Parliament.Model;
using Parliament.Rdf.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using VDS.RDF;
using VDS.RDF.Query;

namespace Functions.TransformationCommitteeChairIncumbencyMnis
{
    public class Transformation : BaseTransformationXml<Settings, XDocument>
    {
        public override BaseResource[] TransformSource(XDocument doc)
        {
            Incumbency mnisIncumbency = new Incumbency();
            XElement element = doc.Descendants(d + "MemberCommitteeChair_Id").SingleOrDefault();
            if ((element == null) || (element.Parent == null))
                return null;

            XElement elementIncumbency = element.Parent;

            mnisIncumbency.FormalBodyChairIncumbencyMnisId = elementIncumbency.Element(d + "MemberCommitteeChair_Id").GetText();
            mnisIncumbency.IncumbencyStartDate = elementIncumbency.Element(d + "StartDate").GetDate();

            XElement elementMemberCommittee = doc.Descendants(d + "Member_Id").SingleOrDefault();
            if (elementMemberCommittee != null)
                generateIncumbencyMemberAndCommittee(mnisIncumbency, elementMemberCommittee);

            mnisIncumbency.IncumbencyEndDate = elementIncumbency.Element(d + "EndDate").GetDate();

            return new BaseResource[] { mnisIncumbency };
        }

        public override Dictionary<string, INode> GetKeysFromSource(BaseResource[] deserializedSource)
        {
            string formalBodyChairIncumbencyMnisId = deserializedSource.OfType<Incumbency>()
                .SingleOrDefault()
                .FormalBodyChairIncumbencyMnisId;
            return new Dictionary<string, INode>()
            {
                { "formalBodyChairIncumbencyMnisId", SparqlConstructor.GetNode(formalBodyChairIncumbencyMnisId) }
            };
        }

        public override BaseResource[] SynchronizeIds(BaseResource[] source, Uri subjectUri, BaseResource[] target)
        {
            foreach (BaseResource incumbency in source)
                incumbency.Id = subjectUri;

            return source;
        }

        private void generateIncumbencyMemberAndCommittee(Incumbency mnisIncumbency, XElement elementMemberCommittee)
        {
            string memberId = elementMemberCommittee.Parent.Element(d + "Member_Id").GetText();
            if (string.IsNullOrWhiteSpace(memberId) == false)
            {
                Uri memberUri = IdRetrieval.GetSubject("memberMnisId", memberId, false, logger);
                mnisIncumbency.IncumbencyHasPerson = new Person()
                {
                    Id = memberUri
                };
            }
            string committeeId = elementMemberCommittee.Parent.Element(d + "Committee_Id").GetText();
            if (string.IsNullOrWhiteSpace(committeeId) == false)
            {
                string positionCommand = @"
                    construct {
                        ?position a parl:FormalBodyChair.
                    }
                    where {
                        ?position parl:formalBodyChairHasFormalBody ?committee.
                        ?committee parl:formalBodyMnisId @formalBodyMnisId.
                    }";
                SparqlParameterizedString positionSparql = new SparqlParameterizedString(positionCommand);
                positionSparql.Namespaces.AddNamespace("parl", new Uri(schemaNamespace));
                positionSparql.SetLiteral("formalBodyMnisId", committeeId);
                Uri positionUri = IdRetrieval.GetSubject(positionSparql.ToString(), false, logger);
                if (positionUri != null)
                    mnisIncumbency.IncumbencyHasPosition = new Position()
                    {
                        Id = positionUri
                    };
            }
        }
    }
}
