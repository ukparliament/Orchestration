using Microsoft.ApplicationInsights;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace Functions.Transformation
{
    public static class TransformationMemberMnis
    {
        private static readonly TelemetryClient telemetryClient = new TelemetryClient()
        {
            InstrumentationKey = Environment.GetEnvironmentVariable("ApplicationInsightsInstrumentationKey", EnvironmentVariableTarget.Process)
        };
        private static Stopwatch timer = null;
        private static readonly string schemaNamespace = Environment.GetEnvironmentVariable("SchemaNamespace", EnvironmentVariableTarget.Process);
        private static readonly string houseName = "House of Lords";
        private static readonly string graphSparql = @"
        construct {
        	?person a parl:Person;
        		parl:personDateOfBirth ?personDateOfBirth;
        		parl:personGivenName ?personGivenName;
        		parl:personOtherNames ?personOtherNames;
        		parl:personFamilyName ?personFamilyName;
        		parl:personDateOfDeath ?personDateOfDeath;
                <http://example.com/F31CBD81AD8343898B49DC65743F0BDF> ?personNameDisplayAs;
                <http://example.com/A5EE13ABE03C4D3A8F1A274F57097B6C> ?personNameListAs;
                <http://example.com/D79B0BAC513C4A9A87C9D5AFF1FC632F> ?personNameFullTitle;
        		parl:personMnisId ?personMnisId;
                parl:personPimsId ?personPimsId;
                parl:personDodsId ?personDodsId.
            ?genderIdentity a parl:GenderIdentity;
                parl:genderIdentityHasGender ?gender;
                parl:genderIdentityHasPerson ?person.
            ?incumbency a parl:Incumbency;
        		parl:incumbencyHasMember ?person;
                parl:seatIncumbencyHasHouseSeat ?seatIncumbencyHasHouseSeat;
                parl:seatIncumbencyHasParliamentPeriod ?seatIncumbencyHasParliamentPeriod;
        		parl:incumbencyStartDate ?incumbencyStartDate;
        		parl:incumbencyEndDate ?incumbencyEndDate;
                parl:houseIncumbencyHasHouse ?houseIncumbencyHasHouse;
                parl:houseIncumbencyHasHouseIncumbencyType ?houseIncumbencyHasHouseIncumbencyType.
            ?partyMembership a parl:PartyMembership;
        		parl:partyMembershipStartDate ?partyMembershipStartDate;
                parl:partyMembershipEndDate ?partyMembershipEndDate;
        		parl:partyMembershipHasParty ?partyMembershipHasParty;
        		parl:partyMembershipHasPartyMember ?person.
        }
        where {
            bind(@personUri as ?person)
            ?person a parl:Person;
                parl:personMnisId ?personMnisId.
            optional {?person parl:personPimsId ?personPimsId}
            optional {?person parl:personDodsId ?personDodsId}
            optional {?person parl:personDateOfBirth ?personDateOfBirth}
            optional {?person parl:personGivenName ?personGivenName}
            optional {?person parl:personOtherNames ?personOtherNames}
            optional {?person parl:personFamilyName ?personFamilyName}
            optional {?person parl:personDateOfDeath ?personDateOfDeath}
            optional {?person <http://example.com/F31CBD81AD8343898B49DC65743F0BDF> ?personNameDisplayAs}
            optional {?person <http://example.com/A5EE13ABE03C4D3A8F1A274F57097B6C> ?personNameListAs}
            optional {?person <http://example.com/D79B0BAC513C4A9A87C9D5AFF1FC632F> ?personNameFullTitle}
            optional {
                ?genderIdentity a parl:GenderIdentity;
                    parl:genderIdentityHasGender ?gender;
                    parl:genderIdentityHasPerson ?person.
                ?gender parl:genderMnisId @genderMnisId.
            }
            optional {
                ?incumbency a parl:Incumbency;
                    parl:incumbencyHasMember ?person.
                optional {?incumbency parl:incumbencyStartDate ?incumbencyStartDate}
                optional {?incumbency parl:incumbencyEndDate ?incumbencyEndDate}
                optional {?incumbency parl:seatIncumbencyHasHouseSeat ?seatIncumbencyHasHouseSeat}
                optional {?incumbency parl:seatIncumbencyHasParliamentPeriod ?seatIncumbencyHasParliamentPeriod}
                optional {
                    ?incumbency parl:houseIncumbencyHasHouse ?houseIncumbencyHasHouse;
                        parl:houseIncumbencyHasHouseIncumbencyType ?houseIncumbencyHasHouseIncumbencyType.
                    ?houseIncumbencyHasHouse parl:houseName @houseName.
                }
            }
            optional {
                ?partyMembership a parl:PartyMembership;
                    parl:partyMembershipStartDate ?partyMembershipStartDate;
                    parl:partyMembershipHasParty ?partyMembershipHasParty;
                    parl:partyMembershipHasPartyMember ?person.
                optional {?partyMembership parl:partyMembershipEndDate ?partyMembershipEndDate}
            }
        }";

        [FunctionName("TransformationMemberMnis")]
        public static async Task<object> Run([HttpTrigger(WebHookType = "genericJson")]HttpRequestMessage req, TraceWriter log)
        {
            telemetryClient.Context.Operation.Name = "TransformationMemberMnis";
            telemetryClient.Context.Operation.Id = Guid.NewGuid().ToString();

            telemetryClient.TrackEvent("Triggered");
            timer = Stopwatch.StartNew();
            string jsonContent = await req.Content.ReadAsStringAsync();
            dynamic data = JsonConvert.DeserializeObject(jsonContent);

            if ((data.url == null) || (data.callbackUrl == null))
            {
                timer.Stop();
                telemetryClient.TrackTrace("Missing some value(s)", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Error);
                telemetryClient.TrackEvent("Finished", new Dictionary<string, string>(), new Dictionary<string, double>() { { "ProcessTime", timer.Elapsed.TotalMilliseconds } });
                return req.CreateResponse(HttpStatusCode.BadRequest, "Missing some value(s)");
            }
            telemetryClient.Context.Properties["DataUrl"] = data.url.ToString();
            if (data.batchId != null)
                telemetryClient.Context.Properties["BatchId"] = data.batchId.ToString();
            new Thread(() => startProcess(data.url.ToString(), data.callbackUrl.ToString())).Start();

            return req.CreateResponse();
        }

        private static async Task<HttpResponseMessage> communicateBack(string callbackUrl, string errorMessage = null)
        {
            using (HttpClient client = new HttpClient())
            {
                timer.Stop();
                if (string.IsNullOrWhiteSpace(errorMessage))
                    telemetryClient.TrackEvent("Finished", new Dictionary<string, string>(), new Dictionary<string, double>() { { "ProcessTime", timer.Elapsed.TotalMilliseconds } });
                else
                    telemetryClient.TrackEvent(errorMessage, new Dictionary<string, string>(), new Dictionary<string, double>() { { "ProcessTime", timer.Elapsed.TotalMilliseconds } });
                return await client.PostAsync(callbackUrl, new StringContent(errorMessage ?? "OK"));
            }
        }

        private static XmlNamespaceManager getNamespaceManager()
        {
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(new NameTable());
            namespaceManager.AddNamespace("atom", "http://www.w3.org/2005/Atom");
            namespaceManager.AddNamespace("m", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
            namespaceManager.AddNamespace("d", "http://schemas.microsoft.com/ado/2007/08/dataservices");

            return namespaceManager;
        }

        private static async Task<HttpResponseMessage> startProcess(string dataUrl, string callbackUrl)
        {
            string fullDataUrl = $"{dataUrl}?$select=Surname,MiddleNames,Forename,NameDisplayAs,NameListAs,NameFullTitle,DateOfBirth,DateOfDeath,Gender,Member_Id,House,StartDate,EndDate,Dods_Id,Pims_Id,MemberConstituencies/StartDate,MemberConstituencies/EndDate,MemberConstituencies/EndDate,MemberConstituencies/Constituency/Constituency_Id,MemberConstituencies/Constituency/StartDate,MemberConstituencies/Constituency/EndDate,MemberParties/Party_Id,MemberParties/StartDate,MemberParties/EndDate,MemberLordsMembershipTypes/StartDate,MemberLordsMembershipTypes/EndDate,MemberLordsMembershipTypes/LordsMembershipType_Id&$expand=MemberConstituencies,MemberConstituencies/Constituency,MemberParties,MemberLordsMembershipTypes";
            string xml = null;
            Stopwatch externalTimer = Stopwatch.StartNew();
            DateTime externalStartTime = DateTime.UtcNow;
            bool externalCallOk = true;

            try
            {
                telemetryClient.TrackTrace("Contacting source", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                xml = await XmlDataRetrieval.GetXmlDataFromUrl(fullDataUrl, "application/atom+xml");
            }
            catch (Exception e)
            {
                externalCallOk = false;
                telemetryClient.TrackException(e);
                return await communicateBack(callbackUrl, $"Problem while retrieving data from {dataUrl}");
            }
            finally
            {
                externalTimer.Stop();
                telemetryClient.TrackDependency("XmlDataRetrieval", fullDataUrl, externalStartTime, externalTimer.Elapsed, externalCallOk);
            }
            if (string.IsNullOrWhiteSpace(xml))
            {
                telemetryClient.TrackTrace("Empty response", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Error);
                return await communicateBack(callbackUrl, $"Empty response for {dataUrl}");
            }
            XDocument doc = null;
            try
            {
                telemetryClient.TrackTrace("Parsing response", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                doc = XDocument.Parse(xml);
            }
            catch (Exception e)
            {
                telemetryClient.TrackException(e);
                return await communicateBack(callbackUrl, $"Problem while reading data as xml for {dataUrl}");
            }
            XmlNamespaceManager xmlNamespaceManager = getNamespaceManager();
            Uri subjectUri = null;
            try
            {
                telemetryClient.TrackTrace("Getting key", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                XElement key = doc.XPathSelectElement("atom:entry/atom:content/m:properties/d:Member_Id", xmlNamespaceManager);
                telemetryClient.TrackTrace($"Key: {key.Value}", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                subjectUri = IdRetrieval.GetSubject("Person", "personMnisId", key.Value, true, telemetryClient);
                telemetryClient.TrackTrace($"Subject: {subjectUri}", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
            }
            catch (Exception e)
            {
                telemetryClient.TrackException(e);
                return await communicateBack(callbackUrl, $"Problem while obtaining key for {dataUrl}");
            }
            IGraph oldGraph = null;
            string genderMnisId = "";
            string houseIncumbencyTypeName = "";
            try
            {
                telemetryClient.TrackTrace($"Check gender", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                XElement genderElement = doc.XPathSelectElement("atom:entry/atom:content/m:properties/d:Gender", xmlNamespaceManager);
                if ((genderElement != null) && (string.IsNullOrWhiteSpace(genderElement.Value) == false))
                    genderMnisId = genderElement.Value;
                telemetryClient.TrackTrace("Trying to get existing graph", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                oldGraph = generateExistingGraph(subjectUri, genderMnisId);
                if (oldGraph == null)
                {
                    telemetryClient.TrackTrace($"Cannot retrieve old graph for {subjectUri}", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Error);
                    return await communicateBack(callbackUrl, $"Cannot retrieve old graph for {subjectUri}");
                }
            }
            catch (Exception e)
            {
                telemetryClient.TrackException(e);
                return await communicateBack(callbackUrl, $"Problem with retrieving old graph for {subjectUri}");
            }
            IGraph newGraph = null;
            try
            {
                telemetryClient.TrackTrace("Generating new graph", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                newGraph = generateNewGraph(doc, oldGraph, subjectUri, genderMnisId, houseIncumbencyTypeName, xmlNamespaceManager);
            }
            catch (Exception e)
            {
                telemetryClient.TrackException(e);
                return await communicateBack(callbackUrl, $"Problem with retrieving new graph for {subjectUri}");
            }
            GraphDiffReport difference = oldGraph.Difference(newGraph);
            telemetryClient.TrackTrace($"Found triples: {difference.AddedTriples.Count()} added and {difference.RemovedTriples.Count()} removed", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
            //foreach(Triple t in difference.AddedTriples)
            //  log.Info($"{t}");
            //foreach(Triple t in difference.RemovedTriples)
            //  log.Info($"{t}");
            if ((difference.AddedTriples.Any() == false) && (difference.RemovedTriples.Any() == false))
            {
                telemetryClient.TrackTrace("No update required", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                return await communicateBack(callbackUrl);
            }
            externalTimer = Stopwatch.StartNew();
            externalStartTime = DateTime.UtcNow;
            externalCallOk = true;
            try
            {
                telemetryClient.TrackTrace("Updating graph", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                GraphUpdate.UpdateDifference(difference);
            }
            catch (Exception e)
            {
                externalCallOk = false;
                telemetryClient.TrackException(e);
                return await communicateBack(callbackUrl, $"Problem when updating graph for {subjectUri}");
            }
            finally
            {
                externalTimer.Stop();
                telemetryClient.TrackDependency("GraphUpdate", $"+{difference.AddedTriples.Count()}/-{difference.RemovedTriples.Count()}", externalStartTime, externalTimer.Elapsed, externalCallOk);
            }
            return await communicateBack(callbackUrl);
        }

        private static IGraph generateExistingGraph(Uri subject, string genderMnisId)
        {
            SparqlParameterizedString sparql = new SparqlParameterizedString(graphSparql);
            sparql.SetUri("personUri", subject);
            sparql.SetLiteral("genderMnisId", genderMnisId);
            sparql.SetLiteral("houseName", houseName);
            sparql.Namespaces.AddNamespace("parl", new Uri(schemaNamespace));

            return GraphRetrieval.GetGraph(sparql.ToString(), telemetryClient);
        }

        private static IGraph generateNewGraph(XDocument doc, IGraph oldGraph, Uri subjectUri, string genderMnisId, string houseIncumbencyTypeName, XmlNamespaceManager xmlNamespaceManager)
        {
            Graph result = new Graph();
            result.NamespaceMap.AddNamespace("parl", new Uri(schemaNamespace));
            result.NamespaceMap.AddNamespace("ex", new Uri("http://example.com/"));

            telemetryClient.TrackTrace("Generate triples", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
            IUriNode subject = result.CreateUriNode(subjectUri);
            IUriNode rdfTypeNode = result.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            //Person
            result.Assert(subject, rdfTypeNode, result.CreateUriNode("parl:Person"));
            TripleGenerator.GenerateTriple(result, subject, "parl:personDateOfBirth", doc, "atom:entry/atom:content/m:properties/d:DateOfBirth", xmlNamespaceManager, "xsd:date");
            TripleGenerator.GenerateTriple(result, subject, "parl:personGivenName", doc, "atom:entry/atom:content/m:properties/d:Forename", xmlNamespaceManager);
            TripleGenerator.GenerateTriple(result, subject, "parl:personOtherNames", doc, "atom:entry/atom:content/m:properties/d:MiddleNames", xmlNamespaceManager);
            TripleGenerator.GenerateTriple(result, subject, "parl:personFamilyName", doc, "atom:entry/atom:content/m:properties/d:Surname", xmlNamespaceManager);
            TripleGenerator.GenerateTriple(result, subject, "ex:F31CBD81AD8343898B49DC65743F0BDF", doc, "atom:entry/atom:content/m:properties/d:NameDisplayAs", xmlNamespaceManager);
            TripleGenerator.GenerateTriple(result, subject, "ex:A5EE13ABE03C4D3A8F1A274F57097B6C", doc, "atom:entry/atom:content/m:properties/d:NameListAs", xmlNamespaceManager);
            TripleGenerator.GenerateTriple(result, subject, "ex:D79B0BAC513C4A9A87C9D5AFF1FC632F", doc, "atom:entry/atom:content/m:properties/d:NameFullTitle", xmlNamespaceManager);
            TripleGenerator.GenerateTriple(result, subject, "parl:personPimsId", doc, "atom:entry/atom:content/m:properties/d:Pims_Id", xmlNamespaceManager);
            TripleGenerator.GenerateTriple(result, subject, "parl:personDodsId", doc, "atom:entry/atom:content/m:properties/d:Dods_Id", xmlNamespaceManager);

            //DeceasedPerson
            TripleGenerator.GenerateTriple(result, subject, "parl:personDateOfDeath", doc, "atom:entry/atom:content/m:properties/d:DateOfDeath", xmlNamespaceManager, "xsd:date");
            //MnisPerson
            TripleGenerator.GenerateTriple(result, subject, "parl:personMnisId", doc, "atom:entry/atom:content/m:properties/d:Member_Id", xmlNamespaceManager);
            //GenderIdentity
            generateTripleGenderIdentity(result, oldGraph, subject, genderMnisId);
            //Incumbency
            generateTripleIncumbency(result, oldGraph, subject, doc, houseIncumbencyTypeName, xmlNamespaceManager);
            //Party membership
            generateTriplePartyMembership(result, oldGraph, subject, doc, xmlNamespaceManager);

            return result;
        }

        private static void generateTripleGenderIdentity(IGraph graph, IGraph oldGraph, IUriNode subject, string genderMnisId)
        {
            Uri genderIdentityUri;
            IUriNode genderIdenitytTypeNode = graph.CreateUriNode("parl:GenderIdentity");
            IUriNode rdfTypeNode = graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));

            telemetryClient.TrackTrace("Check gender identity", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
            IEnumerable<Triple> existingGenderIdentity = oldGraph.GetTriplesWithPredicateObject(rdfTypeNode, genderIdenitytTypeNode);
            if ((existingGenderIdentity != null) && (existingGenderIdentity.Any()))
            {
                genderIdentityUri = ((IUriNode)existingGenderIdentity.SingleOrDefault().Subject).Uri;
                telemetryClient.TrackTrace($"Found gender identity {genderIdentityUri}", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
            }
            else
            {
                string genderIdentityId = new IdGenerator.IdMaker().MakeId();
                genderIdentityUri = new Uri(genderIdentityId);
                telemetryClient.TrackTrace($"New gender identity {genderIdentityUri}", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
            }
            IUriNode genderIdentityNode = graph.CreateUriNode(genderIdentityUri);

            graph.Assert(genderIdentityNode, rdfTypeNode, genderIdenitytTypeNode);
            graph.Assert(genderIdentityNode, graph.CreateUriNode("parl:genderIdentityHasPerson"), subject);
            generateTripleGender(graph, oldGraph, genderIdentityNode, genderMnisId);
        }

        private static void generateTripleGender(IGraph graph, IGraph oldGraph, IUriNode subject, string genderMnisId)
        {
            Uri genderUri;
            IUriNode genderPredicateNode = graph.CreateUriNode("parl:genderIdentityHasGender");

            telemetryClient.TrackTrace($"Check gender", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
            IEnumerable<Triple> existingGender = oldGraph.GetTriplesWithSubjectPredicate(subject, genderPredicateNode);
            if ((existingGender != null) && (existingGender.Any()))
            {
                genderUri = ((IUriNode)existingGender.SingleOrDefault().Object).Uri;
                telemetryClient.TrackTrace($"Found gender {genderUri}", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
            }
            else
            {
                genderUri = IdRetrieval.GetSubject("Gender", "genderMnisId", genderMnisId, false, telemetryClient);
                if (genderUri == null)
                {
                    throw new Exception($"{subject.Uri}: Could not found gender for {genderMnisId}");
                }
            }
            graph.Assert(subject, genderPredicateNode, graph.CreateUriNode(genderUri));
        }

        private static void generateTripleIncumbency(IGraph graph, IGraph oldGraph, IUriNode subject, XDocument doc, string houseIncumbencyTypeName, XmlNamespaceManager xmlNamespaceManager)
        {
            XElement lordsType;
            XElement startDateElement;
            XElement endDateElement;
            IUriNode incumbencyNode;
            HashSet<string> dates = new HashSet<string>();
            string seatCommand = @"
        construct {
        	?id a parl:HouseSeat.
        }
        where {
        	?id a parl:HouseSeat;
        		parl:houseSeatHasConstituencyGroup ?houseSeatHasConstituencyGroup.
        	?houseSeatHasConstituencyGroup parl:constituencyGroupMnisId @constituencyGroupMnisId.
        }";
            IUriNode rdfTypeNode = graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            IUriNode dataTypeUri = graph.CreateUriNode("xsd:date");
            IUriNode housePredicateNode = graph.CreateUriNode("parl:houseIncumbencyHasHouse");
            IUriNode incumbencyTypePredicateNode = graph.CreateUriNode("parl:houseIncumbencyHasHouseIncumbencyType");

            XElement house = doc.XPathSelectElement("atom:entry/atom:content/m:properties/d:House", xmlNamespaceManager);
            if (house.Value == "Lords")
            {
                foreach (XElement element in doc.XPathSelectElements($"atom:entry/atom:link[@title='MemberLordsMembershipTypes']/m:inline/atom:feed/atom:entry", xmlNamespaceManager))
                {
                    lordsType = element.XPathSelectElement("atom:content/m:properties/d:LordsMembershipType_Id", xmlNamespaceManager);
                    startDateElement = element.XPathSelectElement("atom:content/m:properties/d:StartDate", xmlNamespaceManager);
                    endDateElement = element.XPathSelectElement("atom:content/m:properties/d:EndDate", xmlNamespaceManager);
                    if (dates.Contains(startDateElement.Value) == true)
                    {
                        telemetryClient.TrackTrace($"Duplicate incumbency ({startDateElement.Value})", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                        continue;
                    }
                    dates.Add(startDateElement.Value);
                    if ((string.IsNullOrWhiteSpace(endDateElement.Value)) ||
                        (Convert.ToDateTime(startDateElement.Value) <= Convert.ToDateTime(endDateElement.Value)))
                    {
                        telemetryClient.TrackTrace($"House incumbency ({startDateElement.Value})", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                        incumbencyNode = generateIncumbency(graph, oldGraph, subject, startDateElement, endDateElement);
                        IEnumerable<Triple> existingHouses = oldGraph.GetTriplesWithSubjectPredicate(incumbencyNode, housePredicateNode);
                        Uri houseUri = null;
                        if ((existingHouses != null) && (existingHouses.Any()))
                        {
                            houseUri = ((IUriNode)existingHouses.SingleOrDefault().Object).Uri;
                        }
                        else
                        {
                            houseUri = IdRetrieval.GetSubject("House", "houseName", houseName, false, telemetryClient);
                        }
                        IEnumerable<Triple> existingTypes = oldGraph.GetTriplesWithSubjectPredicate(incumbencyNode, incumbencyTypePredicateNode);
                        Uri incumbencyTypeUri = null;
                        if ((existingTypes != null) && (existingTypes.Any()))
                        {
                            incumbencyTypeUri = ((IUriNode)existingTypes.SingleOrDefault().Object).Uri;
                        }
                        else
                        {
                            incumbencyTypeUri = IdRetrieval.GetSubject("HouseIncumbencyType", "houseIncumbencyTypeMnisId", lordsType.Value, false, telemetryClient);
                        }
                        graph.Assert(incumbencyNode, housePredicateNode, graph.CreateUriNode(houseUri));
                        graph.Assert(incumbencyNode, incumbencyTypePredicateNode, graph.CreateUriNode(incumbencyTypeUri));
                    }
                }
            }

            foreach (XElement element in doc.XPathSelectElements("atom:entry/atom:link[@title='MemberConstituencies']/m:inline/atom:feed/atom:entry", xmlNamespaceManager))
            {
                startDateElement = element.XPathSelectElement("atom:content/m:properties/d:StartDate", xmlNamespaceManager);
                telemetryClient.TrackTrace($"Seat incumbency ({startDateElement.Value})", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                endDateElement = element.XPathSelectElement("atom:content/m:properties/d:EndDate", xmlNamespaceManager);
                incumbencyNode = generateIncumbency(graph, oldGraph, subject, startDateElement, endDateElement);
                generateSeatIncumbencyParliamentPeriod(graph, incumbencyNode, startDateElement);
                telemetryClient.TrackTrace($"Check seat ({startDateElement.Value})", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                Uri houseSeatUri;
                XElement constituencyElement = element.XPathSelectElement("atom:link[@title='Constituency']/m:inline/atom:entry/atom:content/m:properties", xmlNamespaceManager);
                if (constituencyElement != null)
                {
                    string constituencyId = constituencyElement.XPathSelectElement("d:Constituency_Id", xmlNamespaceManager).Value;
                    SparqlParameterizedString seatSparql = new SparqlParameterizedString(seatCommand);
                    seatSparql.Namespaces.AddNamespace("parl", new Uri(schemaNamespace));
                    seatSparql.SetLiteral("constituencyGroupMnisId", constituencyId);
                    houseSeatUri = IdRetrieval.GetSubject(seatSparql.ToString(), false, telemetryClient);
                    if (houseSeatUri != null)
                    {
                        telemetryClient.TrackTrace($"House seat ({startDateElement.Value})", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                        graph.Assert(incumbencyNode, graph.CreateUriNode("parl:seatIncumbencyHasHouseSeat"), graph.CreateUriNode(houseSeatUri));
                    }
                }
            }
        }

        private static IUriNode generateIncumbency(IGraph graph, IGraph oldGraph, IUriNode subject, XElement startDateElement, XElement endDateElement)
        {
            IUriNode dataTypeNode = graph.CreateUriNode("xsd:date");
            IUriNode rdfTypeNode = graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            string startDate = giveMeDatePart(startDateElement);
            string endDate = giveMeDatePart(endDateElement);
            INode startDateNode = graph.CreateLiteralNode(startDate, dataTypeNode.Uri);
            IUriNode startDatePredicate = graph.CreateUriNode("parl:incumbencyStartDate");

            IEnumerable<Triple> existingIncumbencies = oldGraph.GetTriplesWithPredicateObject(startDatePredicate, startDateNode);
            Uri incumbencyUri = null;
            if ((existingIncumbencies != null) && (existingIncumbencies.Any()))
            {
                incumbencyUri = ((IUriNode)existingIncumbencies.SingleOrDefault().Subject).Uri;
                telemetryClient.TrackTrace($"Found incumbency {incumbencyUri} ({startDateElement.Value})", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
            }
            else
            {
                string incumbencyId = new IdGenerator.IdMaker().MakeId();
                incumbencyUri = new Uri(incumbencyId);
                telemetryClient.TrackTrace($"New incumbency {incumbencyUri} ({startDateElement.Value})", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
            }
            IUriNode incumbencyNode = graph.CreateUriNode(incumbencyUri);
            graph.Assert(incumbencyNode, rdfTypeNode, graph.CreateUriNode("parl:Incumbency"));
            graph.Assert(incumbencyNode, graph.CreateUriNode("parl:incumbencyHasMember"), subject);
            graph.Assert(incumbencyNode, startDatePredicate, startDateNode);
            if (string.IsNullOrWhiteSpace(endDate) == false)
                graph.Assert(incumbencyNode, graph.CreateUriNode("parl:incumbencyEndDate"), graph.CreateLiteralNode(endDate, dataTypeNode.Uri));

            return incumbencyNode;
        }

        private static void generateSeatIncumbencyParliamentPeriod(IGraph graph, IUriNode seatIncumbency, XElement startDateElement)
        {
            IUriNode dataTypeNode = graph.CreateUriNode("xsd:date");
            string sparqlCommand = @"
        construct {
            ?s a parl:ParliamentPeriod.
        }where { 
            ?s a parl:ParliamentPeriod;
            parl:parliamentPeriodStartDate ?parliamentPeriodStartDate.
            filter (?parliamentPeriodStartDate >= @startDate) 
        } order by ?parliamentPeriodStartDate limit 1";
            SparqlParameterizedString sparql = new SparqlParameterizedString(sparqlCommand);
            sparql.Namespaces.AddNamespace("parl", new Uri(schemaNamespace));
            string startDate = giveMeDatePart(startDateElement);
            sparql.SetLiteral("startDate", startDate, dataTypeNode.Uri);
            Uri parliamentPeriodUri = IdRetrieval.GetSubject(sparql.ToString(), false, telemetryClient);
            if (parliamentPeriodUri != null)
            {
                telemetryClient.TrackTrace($"Parliament period seat ({startDate})", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                graph.Assert(seatIncumbency, graph.CreateUriNode("parl:seatIncumbencyHasParliamentPeriod"), graph.CreateUriNode(parliamentPeriodUri));
            }
        }

        private static string giveMeDatePart(XElement element)
        {
            if ((element == null) || (string.IsNullOrWhiteSpace(element.Value)))
                return null;
            string date = element.Value;
            if (date.IndexOf("T") > 0)
                date = date.Substring(0, date.IndexOf("T"));
            return date;
        }

        private static void generateTriplePartyMembership(IGraph graph, IGraph oldGraph, IUriNode subject, XDocument doc, XmlNamespaceManager namespaceManager)
        {
            Uri partyUri = null;
            IUriNode partyMembershipNode = null;
            IUriNode rdfTypeNode = graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            IUriNode startDatePredicate = graph.CreateUriNode("parl:partyMembershipStartDate");
            IUriNode endDatePredicate = graph.CreateUriNode("parl:partyMembershipEndDate");
            IUriNode hasPartyPredicate = graph.CreateUriNode("parl:partyMembershipHasParty");

            foreach (XElement element in doc.XPathSelectElements("atom:entry/atom:link[@title='MemberParties']/m:inline/atom:feed/atom:entry", namespaceManager))
            {
                XElement partyElement = element.XPathSelectElement("atom:content/m:properties/d:Party_Id", namespaceManager);
                telemetryClient.TrackTrace($"Party membership ({partyElement.Value})", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                XElement startDateElement = element.XPathSelectElement("atom:content/m:properties/d:StartDate", namespaceManager);
                XElement endDateElement = element.XPathSelectElement("atom:content/m:properties/d:EndDate", namespaceManager);


                string startDate = giveMeDatePart(startDateElement);
                string endDate = giveMeDatePart(endDateElement);
                IUriNode dataTypeUri = graph.CreateUriNode("xsd:date");

                partyUri = IdRetrieval.GetSubject("Party", "partyMnisId", partyElement.Value, false, telemetryClient);
                if (partyUri == null)
                {
                    telemetryClient.TrackTrace($"Party not found ({partyElement.Value})", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                    continue;
                }
                INode startDateNode = graph.CreateLiteralNode(startDate, dataTypeUri.Uri);
                IUriNode hasPartyNode = graph.CreateUriNode(partyUri);
                IEnumerable<Triple> existingMembershipByDate = oldGraph.GetTriplesWithPredicateObject(startDatePredicate, startDateNode);
                partyMembershipNode = null;
                if ((existingMembershipByDate != null) && (existingMembershipByDate.Any()))
                {
                    IEnumerable<Triple> existingMembershipByParty = oldGraph.GetTriplesWithPredicateObject(hasPartyPredicate, hasPartyNode);
                    if ((existingMembershipByParty != null) && (existingMembershipByParty.Any()))
                    {
                        IEnumerable<IUriNode> subjectsByDate = existingMembershipByDate.Select(t => (IUriNode)t.Subject);
                        IEnumerable<IUriNode> subjectsByParty = existingMembershipByParty.Select(t => (IUriNode)t.Subject);
                        partyMembershipNode = subjectsByDate.Intersect(subjectsByParty).SingleOrDefault();
                    }
                }
                if (partyMembershipNode == null)
                {
                    string partyMembershipId = new IdGenerator.IdMaker().MakeId();
                    partyMembershipNode = graph.CreateUriNode(new Uri(partyMembershipId));
                    telemetryClient.TrackTrace($"New party membership {partyMembershipNode.Uri} ({partyElement.Value})", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                }
                else
                    telemetryClient.TrackTrace($"Found existing party membership {partyMembershipNode.Uri} ({partyElement.Value})", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                graph.Assert(partyMembershipNode, rdfTypeNode, graph.CreateUriNode("parl:PartyMembership"));
                graph.Assert(partyMembershipNode, graph.CreateUriNode("parl:partyMembershipHasPartyMember"), subject);
                graph.Assert(partyMembershipNode, graph.CreateUriNode("parl:partyMembershipHasParty"), graph.CreateUriNode(partyUri));
                graph.Assert(partyMembershipNode, startDatePredicate, startDateNode);
                if (string.IsNullOrWhiteSpace(endDate) == false)
                    graph.Assert(partyMembershipNode, endDatePredicate, graph.CreateLiteralNode(endDate, dataTypeUri.Uri));
            }
        }
    }
}