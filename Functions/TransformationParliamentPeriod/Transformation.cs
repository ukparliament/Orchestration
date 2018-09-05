using Newtonsoft.Json.Linq;
using Parliament.Model;
using Parliament.Rdf.Serialization;
using System;
using System.Linq;

namespace Functions.TransformationParliamentPeriod
{
    public class Transformation : BaseTransformationJson<Settings, JObject>
    {

        public override BaseResource[] TransformSource(JObject jsonResponse)
        {
            ParliamentPeriod parliamentPeriod = new ParliamentPeriod();
            Uri id = giveMeUri(jsonResponse, "TripleStoreId");
            if (id != null)
                parliamentPeriod.Id = id;
            else
                return null;
            parliamentPeriod.ParliamentPeriodNumber = ((JValue)jsonResponse.SelectToken("parliamentPeriodNumber")).GetFloat();

            string startDate = ((JValue)jsonResponse.SelectToken("parliamentPeriodStartDate")).GetText();
            if (DateTimeOffset.TryParse(startDate, null as IFormatProvider, System.Globalization.DateTimeStyles.AssumeUniversal, out DateTimeOffset startD))
            {
                parliamentPeriod.ParliamentPeriodStartDate = startD;
            }
            else
            {
                logger.Warning($"Startdate with value {startDate} has wrong value.");
                return null;
            }
            string endDate = ((JValue)jsonResponse.SelectToken("parliamentPeriodEndDate")).GetText();
            if ((string.IsNullOrWhiteSpace(endDate) == false) && (DateTimeOffset.TryParse(endDate, null as IFormatProvider, System.Globalization.DateTimeStyles.AssumeUniversal, out DateTimeOffset endD)))
                parliamentPeriod.ParliamentPeriodEndDate = endD;

            Uri previousId = giveMeUri(jsonResponse, "ImmediatelyPreviousParliamentPer");
            if (previousId != null)
                parliamentPeriod.ParliamentPeriodHasImmediatelyPreviousParliamentPeriod = new ParliamentPeriod()
                {
                    Id = previousId
                };
            Uri followingId = giveMeUri(jsonResponse, "ImmediatelyFollowingParliamentPe");
            if (followingId != null)
                parliamentPeriod.ParliamentPeriodHasImmediatelyFollowingParliamentPeriod = new ParliamentPeriod[]
                {
                    new ParliamentPeriod()
                    {
                        Id = followingId
                    }
                };

            parliamentPeriod.ParliamentPeriodWikidataId = ((JValue)jsonResponse.SelectToken("parliamentPeriodWikidataId")).GetText();

            return new BaseResource[] { parliamentPeriod };
        }

        public override Uri GetSubjectFromSource(BaseResource[] deserializedSource)
        {
            return deserializedSource.OfType<ParliamentPeriod>()
                .SingleOrDefault()
                .Id;
        }

        public override BaseResource[] SynchronizeIds(BaseResource[] source, Uri subjectUri, BaseResource[] target)
        {
            foreach (BaseResource period in source)
                period.Id = subjectUri;

            return source;
        }

        private Uri giveMeUri(JObject jsonResponse, string tokenName)
        {
            string id = ((JValue)jsonResponse.SelectToken(tokenName)).GetText();
            if (string.IsNullOrWhiteSpace(id))
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


    }
}
