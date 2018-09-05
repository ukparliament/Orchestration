using Parliament.Rdf.Serialization;
using Parliament.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using VDS.RDF;

namespace Functions.TransformationMemberMnis
{
    public class Transformation : BaseTransformationXml<Settings,XDocument>
    {
        public override BaseResource[] TransformSource(XDocument doc)
        {
            Person member = new Person();
            XElement personElement = doc.Element(atom + "entry")
                .Element(atom + "content")
                .Element(m + "properties");

            member.PersonDateOfBirth = personElement.Element(d + "DateOfBirth").GetDate();
            member.PersonGivenName = personElement.Element(d + "Forename").GetText();
            member.PersonOtherNames = personElement.Element(d + "MiddleNames").GetText();
            member.PersonFamilyName = personElement.Element(d + "Surname").GetText();
            member.MemberMnisId = personElement.Element(d + "Member_Id").GetText();
            member.PersonDodsId = personElement.Element(d + "Dods_Id").GetText();
            member.PersonPimsId = personElement.Element(d + "Pims_Id").GetText();
            member.PersonDateOfDeath = personElement.Element(d + "DateOfDeath").GetDate();
            
            string currentGenderText = personElement.Element(d + "Gender").GetText();
            if (string.IsNullOrWhiteSpace(currentGenderText) == false)
            {
                GenderIdentity genderIdentity = generateGenderIdentity(currentGenderText);
                member.PersonHasGenderIdentity = new GenderIdentity[] { genderIdentity };
            }

            return new BaseResource[] { member };
        }

        public override Dictionary<string, INode> GetKeysFromSource(BaseResource[] deserializedSource)
        {
            string memberMnisId = deserializedSource.OfType<Person>()
                .SingleOrDefault()
                .MemberMnisId;
            return new Dictionary<string, INode>()
            {
                { "memberMnisId", SparqlConstructor.GetNode(memberMnisId) }
            };
        }

        public override Dictionary<string, INode> GetKeysForTarget(BaseResource[] deserializedSource)
        {
            Uri genderUri = deserializedSource.OfType<Person>()
                .SingleOrDefault()
                .PersonHasGenderIdentity
                .SingleOrDefault()
                .GenderIdentityHasGender
                .Id;
            return new Dictionary<string, INode>()
            {
                { "gender", SparqlConstructor.GetNode(genderUri) }
            };
        }

        public override BaseResource[] SynchronizeIds(BaseResource[] source, Uri subjectUri, BaseResource[] target)
        {
            Person member = source.OfType<Person>().SingleOrDefault();
            if ((member.PersonHasGenderIdentity != null) && (member.PersonHasGenderIdentity.Any()))
            {
                GenderIdentity genderIdentity = target.OfType<GenderIdentity>().SingleOrDefault();
                if (genderIdentity != null)
                    member.PersonHasGenderIdentity.SingleOrDefault().Id = genderIdentity.Id;
            }
            
            foreach (BaseResource person in source)
                person.Id = subjectUri; 
            
            return source;
        }

        public override IGraph AlterNewGraph(IGraph newGraph, Uri subjectUri, XDocument doc)
        {
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

        private GenderIdentity generateGenderIdentity(string currentGenderText)
        {
            GenderIdentity genderIdentity = new GenderIdentity();
            Gender gender = new Gender();
            genderIdentity.Id = GenerateNewId();
            gender.Id = IdRetrieval.GetSubject("genderMnisId", currentGenderText, false, logger);
            genderIdentity.GenderIdentityHasGender = gender;
            return genderIdentity;
        }

    }
}