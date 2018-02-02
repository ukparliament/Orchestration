using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parliament.Ontology.Base;
using Parliament.Ontology.Code;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Functions.TransformationParliamentPeriod
{
    public class Transformation : BaseTransformation<Settings>
    {
        
        public override IOntologyInstance[] TransformSource(string response)
        {
            IPastParliamentPeriod pastParliamentPeriod = new PastParliamentPeriod();
            JObject jsonResponse = (JObject)JsonConvert.DeserializeObject(response);

            Uri id = giveMeUri(jsonResponse, "TripleStoreId");
            if (id != null)
                pastParliamentPeriod.SubjectUri = id;
            else
                return null;
            pastParliamentPeriod.ParliamentPeriodNumber = ((JValue)jsonResponse.SelectToken("parliamentPeriodNumber")).GetFloat();

            string startDate = ((JValue)jsonResponse.SelectToken("parliamentPeriodStartDate")).GetText();
            if (DateTimeOffset.TryParse(startDate, null as IFormatProvider, System.Globalization.DateTimeStyles.AssumeUniversal, out DateTimeOffset startD))
            {
                pastParliamentPeriod.ParliamentPeriodStartDate = startD;
            }
            else
            {
                logger.Warning($"Startdate with value {startDate} has wrong value.");
                return null;
            }
            string endDate = ((JValue)jsonResponse.SelectToken("parliamentPeriodEndDate")).GetText();
            if ((string.IsNullOrWhiteSpace(endDate)==false) && (DateTimeOffset.TryParse(endDate, null as IFormatProvider, System.Globalization.DateTimeStyles.AssumeUniversal, out DateTimeOffset endD)))
                pastParliamentPeriod.ParliamentPeriodEndDate = endD;

            Uri previousId = giveMeUri(jsonResponse, "ImmediatelyPreviousParliamentPer");
            if (previousId != null)
                pastParliamentPeriod.ParliamentPeriodHasImmediatelyPreviousParliamentPeriod = new ParliamentPeriod()
                {
                    SubjectUri = previousId
                };
            Uri followingId = giveMeUri(jsonResponse, "ImmediatelyFollowingParliamentPe");
            if (followingId != null)
                pastParliamentPeriod.ParliamentPeriodHasImmediatelyFollowingParliamentPeriod = new IParliamentPeriod[]
                {
                    new ParliamentPeriod()
                    {
                        SubjectUri = followingId
                    }
                };

            IWikidataParliamentPeriod wikidataParliamentPeriod = new WikidataParliamentPeriod();
            wikidataParliamentPeriod.ParliamentPeriodWikidataId = ((JValue)jsonResponse.SelectToken("parliamentPeriodWikidataId")).GetText();

            return new IOntologyInstance[] { pastParliamentPeriod, wikidataParliamentPeriod };
        }

        public override Dictionary<string, object> GetKeysFromSource(IOntologyInstance[] deserializedSource)
        {
            Uri subjectUri = deserializedSource.OfType<IPastParliamentPeriod>()
                .SingleOrDefault()
                .SubjectUri;
            return new Dictionary<string, object>()
            {
                { "subjectUri", subjectUri }
            };
        }

        public override IOntologyInstance[] SynchronizeIds(IOntologyInstance[] source, Uri subjectUri, IOntologyInstance[] target)
        {
            IParliamentPeriod[] parliamentPeriods = source.OfType<IParliamentPeriod>().ToArray();
            foreach (IParliamentPeriod period in parliamentPeriods)
            {
                period.SubjectUri = subjectUri;
            };

            return parliamentPeriods as IOntologyInstance[];
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
