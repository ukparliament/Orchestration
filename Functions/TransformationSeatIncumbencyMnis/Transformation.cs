﻿using Parliament.Rdf.Serialization;
using Parliament.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using VDS.RDF;
using VDS.RDF.Query;

namespace Functions.TransformationSeatIncumbencyMnis
{
    public class Transformation : BaseTransformationXml<Settings,XDocument>
    {
        public override BaseResource[] TransformSource(XDocument doc)
        {
            Member member = new Member();
            XElement seatIncumbencyElement = doc.Element(atom + "entry")
                .Element(atom + "content")
                .Element(m + "properties");

            ParliamentaryIncumbency incumbency = new ParliamentaryIncumbency();
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
            incumbency.CommonsSeatIncumbencyMnisId = seatIncumbencyElement.Element(d + "MemberConstituency_Id").GetText();
            ParliamentPeriod parliamentPeriod = generateSeatIncumbencyParliamentPeriod(incumbency.ParliamentaryIncumbencyStartDate.Value, incumbency.ParliamentaryIncumbencyEndDate);
            if (parliamentPeriod != null)
                incumbency.SeatIncumbencyHasParliamentPeriod = new ParliamentPeriod[] { parliamentPeriod };

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
                incumbency.SeatIncumbencyHasHouseSeat = new HouseSeat()
                {
                    Id = houseSeatUri
                };

            return new BaseResource[] { incumbency };
        }

        public override Dictionary<string, INode> GetKeysFromSource(BaseResource[] deserializedSource)
        {
            string commonsSeatIncumbencyMnisId = deserializedSource.OfType<ParliamentaryIncumbency>()
                .SingleOrDefault()
                .CommonsSeatIncumbencyMnisId;
            return new Dictionary<string, INode>()
            {
                { "commonsSeatIncumbencyMnisId", SparqlConstructor.GetNode(commonsSeatIncumbencyMnisId) }
            };
        }

        public override BaseResource[] SynchronizeIds(BaseResource[] source, Uri subjectUri, BaseResource[] target)
        {
            foreach (BaseResource incumbency in source)
                incumbency.Id = subjectUri;
            return source;
        }

        private ParliamentPeriod generateSeatIncumbencyParliamentPeriod(DateTimeOffset startDate, DateTimeOffset? endDate)
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
            sparql.SetLiteral("endDate", endDate.HasValue ? endDate.Value.ToString("yyyy-MM-dd+00:00") : DateTime.UtcNow.ToString("yyyy-MM-dd+00:00"), new Uri("http://www.w3.org/2001/XMLSchema#date"));
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