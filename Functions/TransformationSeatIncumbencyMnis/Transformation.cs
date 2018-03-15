﻿using Parliament.Rdf;
using Parliament.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using VDS.RDF;
using VDS.RDF.Query;

namespace Functions.TransformationSeatIncumbencyMnis
{
    public class Transformation : BaseTransformation<Settings>
    {
        private XNamespace atom = "http://www.w3.org/2005/Atom";
        private XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";
        private XNamespace m = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";

        public override IResource[] TransformSource(string response)
        {
            IMember member = new Member();
            XDocument doc = XDocument.Parse(response);
            XElement seatIncumbencyElement = doc.Element(atom + "entry")
                .Element(atom + "content")
                .Element(m + "properties");

            IPastParliamentaryIncumbency incumbency = new PastParliamentaryIncumbency();
            incumbency.ParliamentaryIncumbencyStartDate = seatIncumbencyElement.Element(d + "StartDate").GetDate();
            incumbency.ParliamentaryIncumbencyEndDate = seatIncumbencyElement.Element(d + "EndDate").GetDate();
            string memberId = seatIncumbencyElement.Element(d + "Member_Id").GetText();
            Uri memberUri = IdRetrieval.GetSubject("memberMnisId", memberId, false, logger);
            if (memberUri == null)
            {
                logger.Warning($"No member found for {memberId}");
                return null;
            }
            incumbency.ParliamentaryIncumbencyHasMember = new Member()
            {
                Id = memberUri
            };
            IMnisSeatIncumbency seatIncumbency = new MnisSeatIncumbency();
            seatIncumbency.CommonsSeatIncumbencyMnisId = seatIncumbencyElement.Element(d + "MemberConstituency_Id").GetText();
            IParliamentPeriod parliamentPeriod = generateSeatIncumbencyParliamentPeriod(incumbency.ParliamentaryIncumbencyStartDate.Value, incumbency.ParliamentaryIncumbencyEndDate);
            if (parliamentPeriod != null)
                seatIncumbency.SeatIncumbencyHasParliamentPeriod = new IParliamentPeriod[] { parliamentPeriod };

            string constituencyId = seatIncumbencyElement.Element(d + "Constituency_Id").GetText();
            string houseSeatCommand = @"
        construct {
        	?id a parl:HouseSeat.
        }
        where {
        	?id parl:houseSeatHasConstituencyGroup ?houseSeatHasConstituencyGroup.
        	?houseSeatHasConstituencyGroup parl:constituencyGroupMnisId @constituencyGroupMnisId.
        }";
            SparqlParameterizedString seatSparql = new SparqlParameterizedString(houseSeatCommand);
            seatSparql.Namespaces.AddNamespace("parl", new Uri(schemaNamespace));
            seatSparql.SetLiteral("constituencyGroupMnisId", constituencyId);
            Uri houseSeatUri = IdRetrieval.GetSubject(seatSparql.ToString(), false, logger);
            if (houseSeatUri != null)
                seatIncumbency.SeatIncumbencyHasHouseSeat = new HouseSeat()
                {
                    Id = houseSeatUri
                };

            return new IResource[] { incumbency, seatIncumbency };
        }

        public override Dictionary<string, object> GetKeysFromSource(IResource[] deserializedSource)
        {
            string commonsSeatIncumbencyMnisId = deserializedSource.OfType<IMnisSeatIncumbency>()
                .SingleOrDefault()
                .CommonsSeatIncumbencyMnisId;
            return new Dictionary<string, object>()
            {
                { "commonsSeatIncumbencyMnisId", commonsSeatIncumbencyMnisId }
            };
        }

        public override IResource[] SynchronizeIds(IResource[] source, Uri subjectUri, IResource[] target)
        {
            foreach (IParliamentaryIncumbency incumbency in source.OfType<IParliamentaryIncumbency>())
                incumbency.Id = subjectUri;
            return source.OfType<IParliamentaryIncumbency>().ToArray();
        }

        private IParliamentPeriod generateSeatIncumbencyParliamentPeriod(DateTimeOffset startDate, DateTimeOffset? endDate)
        {
            string sparqlCommand = @"
        construct {
            ?parliamentPeriod a parl:ParliamentPeriod.
        }where { 
            ?parliamentPeriod parl:parliamentPeriodStartDate ?parliamentPeriodStartDate.
            optional {?parliamentPeriod parl:parliamentPeriodEndDate ?parliamentPeriodEndDate}
            bind(coalesce(?parliamentPeriodEndDate,@today) as ?endDate)
            filter ((?parliamentPeriodStartDate <= @startDate && ?endDate >= @startDate) ||
    	            (?parliamentPeriodStartDate <= @endDate && ?endDate >= @endDate) ||
                    (?parliamentPeriodStartDate > @startDate && ?endDate < @endDate))
        }";
            SparqlParameterizedString sparql = new SparqlParameterizedString(sparqlCommand);
            sparql.Namespaces.AddNamespace("parl", new Uri(schemaNamespace));
            sparql.SetLiteral("startDate", startDate.ToString("yyyy-MM-dd+00:00"), new Uri("http://www.w3.org/2001/XMLSchema#date"));
            sparql.SetLiteral("endDate", endDate.HasValue ? endDate.Value.ToString("yyyy-MM-dd+00:00") : string.Empty, new Uri("http://www.w3.org/2001/XMLSchema#date"));
            sparql.SetLiteral("today", DateTime.UtcNow.ToString("yyyy-MM-dd+00:00"), new Uri("http://www.w3.org/2001/XMLSchema#date"));
            Uri parliamentPeriodUri = null;
            try
            {
                parliamentPeriodUri = IdRetrieval.GetSubject(sparql.ToString(), false, logger);
            }
            catch (InvalidOperationException e)
            {
                if (e.Message == "Sequence contains more than one element")
                    logger.Warning($"More than one parliamentary period found for incumbency {startDate}-{endDate}");
                else
                    logger.Exception(e);
            }
            if (parliamentPeriodUri != null)
            {
                return new ParliamentPeriod()
                {
                    Id = parliamentPeriodUri
                };
            }
            else
                return null;
        }

    }
}