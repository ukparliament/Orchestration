using Parliament.Ontology.Base;
using Parliament.Ontology.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using VDS.RDF;

namespace Functions.TransformationMemberMnis
{
    public class Transformation : BaseTransformation<Settings>
    {
        private XNamespace atom = "http://www.w3.org/2005/Atom";
        private XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";
        private XNamespace m = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";

        public override IOntologyInstance[] TransformSource(string response)
        {
            IMember member = new Member();
            XDocument doc = XDocument.Parse(response);
            XElement personElement = doc.Element(atom + "entry")
                .Element(atom + "content")
                .Element(m + "properties");

            member.PersonDateOfBirth = personElement.Element(d + "DateOfBirth").GetDate();
            member.PersonGivenName = personElement.Element(d + "Forename").GetText();
            member.PersonOtherNames = personElement.Element(d + "MiddleNames").GetText();
            member.PersonFamilyName = personElement.Element(d + "Surname").GetText();
            IMnisMember mnisMember = new MnisMember();
            mnisMember.MemberMnisId = personElement.Element(d + "Member_Id").GetText();
            IDodsPerson dodsPerson = new DodsPerson();
            dodsPerson.PersonDodsId = personElement.Element(d + "Dods_Id").GetText();
            IPimsPerson pimsPerson = new PimsPerson();
            pimsPerson.PersonPimsId = personElement.Element(d + "Pims_Id").GetText();
            IDeceasedPerson deceasedPerson = new DeceasedPerson();
            deceasedPerson.PersonDateOfDeath = personElement.Element(d + "DateOfDeath").GetDate();
            
            string currentGenderText = personElement.Element(d + "Gender").GetText();
            if (string.IsNullOrWhiteSpace(currentGenderText) == false)
            {
                IGenderIdentity genderIdentity = generateGenderIdentity(currentGenderText);
                member.PersonHasGenderIdentity = new IGenderIdentity[] { genderIdentity };
            }

            return new IOntologyInstance[] { member, mnisMember, dodsPerson, pimsPerson, deceasedPerson};
        }

        public override Dictionary<string, object> GetKeysFromSource(IOntologyInstance[] deserializedSource)
        {
            string memberMnisId = deserializedSource.OfType<IMnisMember>()
                .SingleOrDefault()
                .MemberMnisId;
            return new Dictionary<string, object>()
            {
                { "memberMnisId", memberMnisId }
            };
        }

        public override Dictionary<string, object> GetKeysForTarget(IOntologyInstance[] deserializedSource)
        {
            Uri genderUri = deserializedSource.OfType<IMember>()
                .SingleOrDefault()
                .PersonHasGenderIdentity
                .SingleOrDefault()
                .GenderIdentityHasGender
                .SubjectUri;
            return new Dictionary<string, object>()
            {
                { "gender", genderUri }
            };
        }

        public override IOntologyInstance[] SynchronizeIds(IOntologyInstance[] source, Uri subjectUri, IOntologyInstance[] target)
        {
            IMember member = source.OfType<IMember>().SingleOrDefault();
            if ((member.PersonHasGenderIdentity != null) && (member.PersonHasGenderIdentity.Any()))
            {
                IGenderIdentity genderIdentity = target.OfType<IGenderIdentity>().SingleOrDefault();
                if (genderIdentity != null)
                    member.PersonHasGenderIdentity.SingleOrDefault().SubjectUri = genderIdentity.SubjectUri;
            }
            
            foreach (IPerson person in source.OfType<IPerson>())
                person.SubjectUri = subjectUri; 
            
            IOntologyInstance[] people = source.OfType<IPerson>().ToArray();
            return people.ToArray();
        }

        public override IGraph AlterNewGraph(IGraph newGraph, Uri subjectUri, string response)
        {
            XDocument doc = XDocument.Parse(response);
            XElement personElement = doc.Element(atom + "entry")
                .Element(atom + "content")
                .Element(m + "properties");

            string value = personElement.Element(d + "NameDisplayAs").GetText();
            if (string.IsNullOrWhiteSpace(value) == false)
                newGraph.Assert(newGraph.CreateUriNode(subjectUri), newGraph.CreateUriNode(new Uri("http://example.com/F31CBD81AD8343898B49DC65743F0BDF")), newGraph.CreateLiteralNode(value));
            value = personElement.Element(d + "NameListAs").GetText();
            if (string.IsNullOrWhiteSpace(value) == false)
                newGraph.Assert(newGraph.CreateUriNode(subjectUri), newGraph.CreateUriNode(new Uri("http://example.com/A5EE13ABE03C4D3A8F1A274F57097B6C")), newGraph.CreateLiteralNode(value));
            value = personElement.Element(d + "NameFullTitle").GetText();
            if (string.IsNullOrWhiteSpace(value) == false)
                newGraph.Assert(newGraph.CreateUriNode(subjectUri), newGraph.CreateUriNode(new Uri("http://example.com/D79B0BAC513C4A9A87C9D5AFF1FC632F")), newGraph.CreateLiteralNode(value));

            return newGraph;
        }

        private IGenderIdentity generateGenderIdentity(string currentGenderText)
        {
            IGenderIdentity genderIdentity = new GenderIdentity();
            IGender gender = new Gender();
            genderIdentity.SubjectUri = GenerateNewId();
            gender.SubjectUri = IdRetrieval.GetSubject("genderMnisId", currentGenderText, false, logger);
            genderIdentity.GenderIdentityHasGender = gender;
            return genderIdentity;
        }

    }
}