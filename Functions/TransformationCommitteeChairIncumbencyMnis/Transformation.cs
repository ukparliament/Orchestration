using Parliament.Model;
using Parliament.Rdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using VDS.RDF.Query;

namespace Functions.TransformationCommitteeChairIncumbencyMnis
{
    public class Transformation : BaseTransformation<Settings>
    {
        XNamespace atom = "http://www.w3.org/2005/Atom";
        XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";
        XNamespace m = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";

        public override IResource[] TransformSource(string response)
        {
            IMnisFormalBodyChairIncumbency mnisIncumbency = new MnisFormalBodyChairIncumbency();
            XDocument doc = XDocument.Parse(response);
            XElement element = doc.Descendants(d + "MemberCommitteeChair_Id").SingleOrDefault();
            if ((element == null) || (element.Parent == null))
                return null;

            XElement elementIncumbency = element.Parent;

            mnisIncumbency.FormalBodyChairIncumbencyMnisId = elementIncumbency.Element(d + "MemberCommitteeChair_Id").GetText();
            mnisIncumbency.IncumbencyStartDate = elementIncumbency.Element(d + "StartDate").GetDate();

            XElement elementMemberCommittee = doc.Descendants(d + "Member_Id").SingleOrDefault();
            if (elementMemberCommittee != null)
                generateIncumbencyMemberAndCommittee(mnisIncumbency, elementMemberCommittee);

            IPastIncumbency pastIncumbency = new PastIncumbency();
            pastIncumbency.IncumbencyEndDate = elementIncumbency.Element(d + "EndDate").GetDate();

            return new IResource[] { mnisIncumbency, pastIncumbency };
        }

        public override Dictionary<string, object> GetKeysFromSource(IResource[] deserializedSource)
        {
            string formalBodyChairIncumbencyMnisId = deserializedSource.OfType<IMnisFormalBodyChairIncumbency>()
                .SingleOrDefault()
                .FormalBodyChairIncumbencyMnisId;
            return new Dictionary<string, object>()
            {
                { "formalBodyChairIncumbencyMnisId", formalBodyChairIncumbencyMnisId }
            };
        }

        public override IResource[] SynchronizeIds(IResource[] source, Uri subjectUri, IResource[] target)
        {
            foreach (IIncumbency incumbency in source.OfType<IIncumbency>())
                incumbency.Id = subjectUri;

            return source.OfType<IIncumbency>().ToArray();
        }

        private void generateIncumbencyMemberAndCommittee(IMnisFormalBodyChairIncumbency mnisIncumbency, XElement elementMemberCommittee)
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
