using Newtonsoft.Json.Linq;
using Parliament.Model;
using Parliament.Rdf.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query;

namespace Functions.TransformationLordsSeatIncumbency
{
    public class Transformation : BaseTransformationJson<Settings, JObject>
    {

        public override BaseResource[] TransformSource(JObject jsonResponse)
        {
            ParliamentaryIncumbency parliamentaryIncumbency = new ParliamentaryIncumbency();

            string id = ((JValue)jsonResponse.SelectToken("OData__x0049_D0")).GetText();
            Uri uri = null;
            if (string.IsNullOrWhiteSpace(id))
            {
                logger.Warning("No Id info found");
                return null;
            }
            else
            {
                if (Uri.TryCreate($"{idNamespace}{id}", UriKind.Absolute, out uri))
                    parliamentaryIncumbency.Id = uri;
                else
                {
                    logger.Warning($"Invalid url '{id}' found");
                    return null;
                }
            }
            Uri houseSeatUri = giveMeUri(jsonResponse, "Seat_x0020_ID_x0020__x0028_for_x");
            if (houseSeatUri == null)
                return null;
            else
                parliamentaryIncumbency.SeatIncumbencyHasHouseSeat = new HouseSeat()
                {
                    Id = houseSeatUri
                };

            Uri personUri = giveMeUri(jsonResponse, "Person_x0020_ID_x0020__x0028_for");
            if (personUri == null)
                return null;
            else
                parliamentaryIncumbency.ParliamentaryIncumbencyHasMember = new Member()
                {
                    Id = personUri
                };

            float? mnisId = ((JValue)jsonResponse.SelectToken("MNIS_x0020_ID")).GetFloat();
            if (mnisId.HasValue)
                parliamentaryIncumbency.LordsSeatIncumbencyMnisId = Convert.ToInt32(mnisId.Value).ToString();
            parliamentaryIncumbency.ParliamentaryIncumbencyStartDate = ((JValue)jsonResponse.SelectToken("Start_x0020_Date")).GetDate();
            parliamentaryIncumbency.ParliamentaryIncumbencyEndDate = ((JValue)jsonResponse.SelectToken("End_x0020_Date")).GetDate();
            parliamentaryIncumbency.SeatIncumbencyHasParliamentPeriod = giveMeParliamentPeriods(parliamentaryIncumbency.ParliamentaryIncumbencyStartDate.Value, parliamentaryIncumbency.ParliamentaryIncumbencyEndDate);
            if (parliamentaryIncumbency.SeatIncumbencyHasParliamentPeriod == null)
                return null;

            return new BaseResource[] { parliamentaryIncumbency };
        }

        public override Uri GetSubjectFromSource(BaseResource[] deserializedSource)
        {
            return deserializedSource.OfType<ParliamentaryIncumbency>()
                .SingleOrDefault()
                .Id;
        }

        public override BaseResource[] SynchronizeIds(BaseResource[] source, Uri subjectUri, BaseResource[] target)
        {
            return source;
        }

        private Uri giveMeUri(JObject jsonResponse, string tokenName)
        {
            object id = ((JValue)jsonResponse.SelectToken($"{tokenName}.Value"))?.Value;
            if (id == null)
            {
                logger.Warning($"No {tokenName} Id info found");
                return null;
            }
            else
            {
                if (Uri.TryCreate($"{idNamespace}{id}", UriKind.Absolute, out Uri uri))
                    return uri;
                else
                {
                    logger.Warning($"Invalid url '{id}' found");
                    return null;
                }
            }
        }

        private IEnumerable<ParliamentPeriod> giveMeParliamentPeriods(DateTimeOffset startDate, DateTimeOffset? endDate)
        {
            string sparqlCommand = @"
        construct {
            ?parliamentPeriod a parl:ParliamentPeriod.
        }where { 
            ?parliamentPeriod parl:parliamentPeriodStartDate ?parliamentPeriodStartDate.
            optional {?parliamentPeriod parl:parliamentPeriodEndDate ?parliamentPeriodEndDate}
            filter (
                ((?parliamentPeriodStartDate <= @startDate) && ((?parliamentPeriodEndDate >= @startDate) || ((bound(?parliamentPeriodEndDate)=false) && ((@hasEndDate=false) || (?parliamentPeriodStartDate <= @endDate))))) ||
                ((?parliamentPeriodStartDate >= @startDate) && ((?parliamentPeriodEndDate <= @endDate) || (@hasEndDate=false))) ||
                ((?parliamentPeriodStartDate >= @startDate) && (((bound(?parliamentPeriodEndDate)=false) && (@hasEndDate=false)) || (?parliamentPeriodStartDate <= @endDate)))
            )
        }";
            SparqlParameterizedString sparql = new SparqlParameterizedString(sparqlCommand);
            sparql.Namespaces.AddNamespace("parl", new Uri(schemaNamespace));
            sparql.SetLiteral("startDate", startDate.ToString("yyyy-MM-dd+00:00"), new Uri("http://www.w3.org/2001/XMLSchema#date"));
            sparql.SetLiteral("endDate", endDate.HasValue ? endDate.Value.ToString("yyyy-MM-dd+00:00") : string.Empty, new Uri("http://www.w3.org/2001/XMLSchema#date"));
            sparql.SetLiteral("hasEndDate", endDate.HasValue);
            List<Uri> parliamentPeriodUris = null;
            parliamentPeriodUris = GraphRetrieval.GetSubjects(sparql.ToString(), logger);
            if (parliamentPeriodUris != null)
                return parliamentPeriodUris.Select(p => new ParliamentPeriod()
                {
                    Id = p
                });
            else
                return null;
        }
    }
}
