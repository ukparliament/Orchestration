using Parliament.Ontology.Base;
using Parliament.Ontology.Code;
using System;
using System.Xml.Linq;
using VDS.RDF.Query;

namespace Functions.TransformationContactPointElectoralMnis
{
    public class Transformation : BaseTransformationContactPoint<Settings>
    {
        public override IBaseOntology[] TransformSource(string response)
        {
            IMnisContactPoint contactPoint = new MnisContactPoint();
            XDocument doc = XDocument.Parse(response);
            XElement contactPointElement = doc.Element(atom + "entry")
                .Element(atom + "content")
                .Element(m + "properties");

            contactPoint.ContactPointMnisId = contactPointElement.Element(d + "MemberAddress_Id").GetText();
            contactPoint.Email = contactPointElement.Element(d + "Email").GetText();
            contactPoint.FaxNumber = contactPointElement.Element(d + "Fax").GetText();
            contactPoint.PhoneNumber = contactPointElement.Element(d + "Phone").GetText();
            contactPoint.ContactPointHasPostalAddress = GeneratePostalAddress(contactPointElement);
            IElectoralIncumbency incumbency = generateIncumbency(contactPointElement);
            if (incumbency != null)
                contactPoint.ContactPointHasElectoralIncumbency = new IElectoralIncumbency[] { incumbency };

            return new IBaseOntology[] { contactPoint };
        }

        private IElectoralIncumbency generateIncumbency(XElement contactPointElement)
        {
            IElectoralIncumbency incumbency = null;
            string incumbencyCommand = @"
        construct {
            ?id a parl:ParliamentaryIncumbency.
        }
        where {
            ?id parl:parliamentaryIncumbencyHasMember ?parliamentaryIncumbencyHasMember;
                parl:parliamentaryIncumbencyStartDate ?parliamentaryIncumbencyStartDate.
            ?parliamentaryIncumbencyHasMember parl:personMnisId @personMnisId.
        } order by desc(?parliamentaryIncumbencyStartDate) limit 1";
            Uri incumbencyUri;

            string mnisId = contactPointElement.Element(d + "Member_Id").GetText();
            if (string.IsNullOrWhiteSpace(mnisId))
            {
                logger.Warning("No member info found");
                return null;
            }
            SparqlParameterizedString incumbencySparql = new SparqlParameterizedString(incumbencyCommand);
            incumbencySparql.Namespaces.AddNamespace("parl", new Uri(schemaNamespace));
            incumbencySparql.SetLiteral("personMnisId", mnisId);
            incumbencyUri = IdRetrieval.GetSubject(incumbencySparql.ToString(), false, logger);
            if (incumbencyUri != null)
                incumbency = new ElectoralIncumbency()
                {
                    SubjectUri = incumbencyUri
                };
            else
                logger.Verbose("No contact found");

            return incumbency;
        }
    }
}
