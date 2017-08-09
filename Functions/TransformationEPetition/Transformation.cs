using Newtonsoft.Json;
using Parliament.Ontology.Base;
using Parliament.Ontology.Code;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Functions.TransformationEPetition
{
    public class Transformation : BaseTransformation<Settings>
    {

        public override IBaseOntology[] TransformSource(string response)
        {
            Rootobject sourceEPetition = JsonConvert.DeserializeObject<Rootobject>(response, new JsonSerializerSettings() { DateTimeZoneHandling = DateTimeZoneHandling.Utc });
            DateTime petitionRetrievalTimestamp = DateTime.UtcNow;
            IUkgapEPetition ukgapEPetition = new UkgapEPetition();
            if ((sourceEPetition != null) && (sourceEPetition.data != null) && (sourceEPetition.data.attributes != null))
            {
                ukgapEPetition.EPetitionUkgapId = sourceEPetition.data.id.ToString();
                ukgapEPetition.CreatedAt = DeserializerHelper.GiveMeSingleDateValue(sourceEPetition.data.attributes.created_at);
                ukgapEPetition.Background = DeserializerHelper.GiveMeSingleTextValue(sourceEPetition.data.attributes.background);
                ukgapEPetition.AdditionalDetails = DeserializerHelper.GiveMeSingleTextValue(sourceEPetition.data.attributes.additional_details);
                ukgapEPetition.ClosedAt = DeserializerHelper.GiveMeSingleDateValue(sourceEPetition.data.attributes.closed_at);
                ukgapEPetition.UpdatedAt = DeserializerHelper.GiveMeSingleDateValue(sourceEPetition.data.attributes.updated_at);
                ukgapEPetition.Action = DeserializerHelper.GiveMeSingleTextValue(sourceEPetition.data.attributes.action);
                if (sourceEPetition.data.attributes.government_response != null)
                {
                    IGovernmentResponse governmentResponse = generateGovernmentResponse(sourceEPetition.data.attributes.government_response);
                    ukgapEPetition.EPetitionHasGovernmentResponse = new IGovernmentResponse[] { governmentResponse };
                }
                if (sourceEPetition.data.attributes.debate != null)
                {
                    IDebate debate = generateDebate(sourceEPetition.data.attributes.debate, sourceEPetition.data.attributes.scheduled_debate_date);
                    ukgapEPetition.EPetitionHasDebate = new IDebate[] { debate };
                }
                ukgapEPetition.EPetitionHasModeration = generateModerations(sourceEPetition.data.attributes.open_at, sourceEPetition.data.attributes.rejected_at, sourceEPetition.data.attributes.rejection);
                ukgapEPetition.EPetitionHasThresholdAttainment = generateThresholdAttainments(sourceEPetition.data.attributes.moderation_threshold_reached_at, sourceEPetition.data.attributes.response_threshold_reached_at, sourceEPetition.data.attributes.debate_threshold_reached_at);
                List<ILocatedSignatureCount> signatures = new List<ILocatedSignatureCount>();
                signatures.AddRange(generateInternationalAreasSignatures(sourceEPetition.data.attributes.signatures_by_country, petitionRetrievalTimestamp));
                signatures.AddRange(generateConstituencySignatures(sourceEPetition.data.attributes.signatures_by_constituency, petitionRetrievalTimestamp));
                ukgapEPetition.EPetitionHasLocatedSignatureCount = signatures;
            }

            return new IBaseOntology[] { ukgapEPetition };
        }

        public override Dictionary<string, object> GetKeysFromSource(IBaseOntology[] deserializedSource)
        {
            string ePetitionUkgapId = deserializedSource.OfType<IUkgapEPetition>()
                .SingleOrDefault()
                .EPetitionUkgapId;
            return new Dictionary<string, object>()
            {
                { "ePetitionUkgapId", ePetitionUkgapId }
            };
        }

        public override IBaseOntology[] SynchronizeIds(IBaseOntology[] source, Uri subjectUri, IBaseOntology[] target)
        {
            IUkgapEPetition ePetition = source.OfType<IUkgapEPetition>().SingleOrDefault();
            ePetition.SubjectUri = subjectUri;

            IGovernmentResponse governmentResponse = target.OfType<IGovernmentResponse>().SingleOrDefault();
            if (governmentResponse != null)
                ePetition.EPetitionHasGovernmentResponse.SingleOrDefault().SubjectUri = governmentResponse.SubjectUri;
            IDebate debate = target.OfType<IDebate>().SingleOrDefault();
            if (debate != null)
                ePetition.EPetitionHasDebate.SingleOrDefault().SubjectUri = debate.SubjectUri;
            IEnumerable<IModeration> moderations = target.OfType<IModeration>();
            foreach (IModeration moderation in moderations)
            {
                IModeration foundModeration = null;
                if (moderation is IApproval)
                    foundModeration = ePetition.EPetitionHasModeration
                        .SingleOrDefault(m => ((IApproval)m).ApprovedAt == ((IApproval)moderation).ApprovedAt);
                if (moderation is IRejection)
                    foundModeration = ePetition.EPetitionHasModeration
                        .SingleOrDefault(m => ((IRejection)m).RejectedAt == ((IRejection)moderation).RejectedAt);
                if (foundModeration != null)
                    foundModeration.SubjectUri = moderation.SubjectUri;
                else
                    ePetition.EPetitionHasModeration = ePetition.EPetitionHasModeration.Concat(new IModeration[] { moderation });
            }
            IEnumerable<IThresholdAttainment> thresholdAttainments = target.OfType<IThresholdAttainment>();
            foreach (IThresholdAttainment thresholdAttainment in thresholdAttainments)
            {
                IThresholdAttainment foundThresholdAttainment = ePetition.EPetitionHasThresholdAttainment.SingleOrDefault(t => t.ThresholdAttainmentAt == thresholdAttainment.ThresholdAttainmentAt);
                if (foundThresholdAttainment != null)
                    foundThresholdAttainment.SubjectUri = thresholdAttainment.SubjectUri;
            }
            IEnumerable<ILocatedSignatureCount> signatures = target.OfType<ILocatedSignatureCount>();
            foreach (ILocatedSignatureCount signature in signatures)
            {
                ILocatedSignatureCount foundLocatedSignatureCount = ePetition.EPetitionHasLocatedSignatureCount
                    .SingleOrDefault(s =>
                        (s.LocatedSignatureCountHasPlace.SubjectUri == signature.LocatedSignatureCountHasPlace.SubjectUri) &&
                        (s.SignatureCount.SingleOrDefault() == signature.SignatureCount.SingleOrDefault()));
                if (foundLocatedSignatureCount != null)
                {
                    foundLocatedSignatureCount.SubjectUri = signature.SubjectUri;
                    foundLocatedSignatureCount.SignatureCountRetrievedAt = signature.SignatureCountRetrievedAt;
                }
            }

            return new IBaseOntology[] { ePetition };
        }

        private IGovernmentResponse generateGovernmentResponse(Government_Response sourceGovernmentResponse)
        {
            IGovernmentResponse governmentResponse = new GovernmentResponse();
            governmentResponse.SubjectUri = GenerateNewId();
            governmentResponse.GovernmentResponseCreatedAt = DeserializerHelper.GiveMeSingleDateValue(sourceGovernmentResponse.created_at);
            governmentResponse.GovernmentResponseUpdatedAt = DeserializerHelper.GiveMeSingleDateValue(sourceGovernmentResponse.updated_at);
            governmentResponse.GovernmentResponseDetails = DeserializerHelper.GiveMeSingleTextValue(sourceGovernmentResponse.details);
            governmentResponse.GovernmentResponseSummary = DeserializerHelper.GiveMeSingleTextValue(sourceGovernmentResponse.summary);

            return governmentResponse;
        }

        private IDebate generateDebate(Debate sourceDebate, DateTime? scheduledDebateDate)
        {
            IDebate debate = new Parliament.Ontology.Code.Debate();
            debate.SubjectUri = GenerateNewId();
            debate.DebateProposedDate = DeserializerHelper.GiveMeSingleDateValue(scheduledDebateDate);
            debate.DebateDate = DeserializerHelper.GiveMeSingleDateValue(sourceDebate.debated_on);
            debate.DebateVideoUrl = DeserializerHelper.GiveMeSingleTextValue(sourceDebate.video_url);
            debate.DebateTranscriptUrl = DeserializerHelper.GiveMeSingleTextValue(sourceDebate.transcript_url);
            debate.DebateOverview = DeserializerHelper.GiveMeSingleTextValue(sourceDebate.overview);

            return debate;
        }

        private IModeration[] generateModerations(DateTime? openedAt, DateTime? rejectedAt, Rejection sourceRejection)
        {
            List<IModeration> moderations = new List<IModeration>();
            if (openedAt.HasValue)
            {
                IApproval approval = new Approval();
                approval.SubjectUri = GenerateNewId();
                approval.ApprovedAt = openedAt;
                moderations.Add(approval);
            }
            if (rejectedAt.HasValue)
            {
                IRejection rejection = new Parliament.Ontology.Code.Rejection();
                rejection.SubjectUri = GenerateNewId();
                rejection.RejectedAt = rejectedAt;
                rejection.RejectionDetails = DeserializerHelper.GiveMeSingleTextValue(sourceRejection.details);
                if (string.IsNullOrWhiteSpace(sourceRejection.code) == false)
                {
                    Uri rejectionCodeUri = IdRetrieval.GetSubject("rejectionCodeName", sourceRejection.code, false, logger);
                    if (rejectionCodeUri != null)
                        rejection.RejectionHasRejectionCode = new RejectionCode()
                        {
                            SubjectUri = rejectionCodeUri
                        };
                    else
                        logger.Warning($"Found rejected petition with a rejection code not currently supported - {sourceRejection.code}");
                }
                else
                    logger.Warning("Found rejected petition with no rejection code");
                moderations.Add(rejection);
            }
            return moderations.ToArray();
        }

        private IThresholdAttainment[] generateThresholdAttainments(DateTime? moderatedAt, DateTime? respondedAt, DateTime? debatedAt)
        {
            List<IThresholdAttainment> thresholdAttainments = new List<IThresholdAttainment>();
            Dictionary<string, string> thresholds = IdRetrieval.GetSubjectsDictionary("thresholdName", logger);
            foreach (KeyValuePair<string, string> threshold in thresholds)
            {
                DateTime? timestamp = null;
                switch (threshold.Key.ToLower())
                {
                    case "moderation":
                        timestamp = moderatedAt;
                        break;
                    case "response":
                        timestamp = respondedAt;
                        break;
                    case "debate":
                        timestamp = debatedAt;
                        break;
                }
                if (timestamp.HasValue)
                {
                    IThresholdAttainment thresholdAttainment = new ThresholdAttainment();
                    thresholdAttainment.SubjectUri = GenerateNewId();
                    thresholdAttainment.ThresholdAttainmentAt = timestamp;
                    thresholdAttainment.ThresholdAttainmentHasThreshold = new Threshold()
                    {
                        SubjectUri = new Uri(threshold.Value)
                    };
                    thresholdAttainments.Add(thresholdAttainment);
                }
            }
            return thresholdAttainments.ToArray();
        }

        private ILocatedSignatureCount[] generateInternationalAreasSignatures(Signatures_By_Country[] signaturesByCountry, DateTime petitionRetrievalTimestamp)
        {
            List<ILocatedSignatureCount> signatures = new List<ILocatedSignatureCount>();
            Dictionary<string, string> countries = IdRetrieval.GetSubjectsDictionary("countryGovRegisterId", logger);
            Dictionary<string, string> territories = IdRetrieval.GetSubjectsDictionary("territoryGovRegisterId", logger);
            foreach (Signatures_By_Country country in signaturesByCountry)
            {
                ILocatedSignatureCount locatedSignatureCount = new LocatedSignatureCount();
                locatedSignatureCount.SubjectUri = GenerateNewId();
                locatedSignatureCount.SignatureCount = DeserializerHelper.GiveMeSingleIntegerValue(country.signature_count);
                locatedSignatureCount.SignatureCountRetrievedAt = DeserializerHelper.GiveMeSingleDateValue(petitionRetrievalTimestamp);
                IPlace place = null;
                if (territories.ContainsKey(country.code))
                    place = new Territory()
                    {
                        SubjectUri = new Uri(territories[country.code])
                    };
                else
                    if (countries.ContainsKey(country.code))
                    place = new Country()
                    {
                        SubjectUri = new Uri(countries[country.code])
                    };
                if (place != null)
                {
                    locatedSignatureCount.LocatedSignatureCountHasPlace = place;
                    signatures.Add(locatedSignatureCount);
                }
                else
                    logger.Warning($"Found signature on petition without matching international area code - {country.code}");
            }
            return signatures.ToArray();
        }

        private ILocatedSignatureCount[] generateConstituencySignatures(Signatures_By_Constituency[] signaturesByConstituency, DateTime petitionRetrievalTimestamp)
        {
            List<ILocatedSignatureCount> signatures = new List<ILocatedSignatureCount>();
            Dictionary<string, string> constituencies = IdRetrieval.GetSubjectsDictionary("constituencyGroupOnsCode", logger);
            foreach (Signatures_By_Constituency constituency in signaturesByConstituency)
            {
                ILocatedSignatureCount locatedSignatureCount = new LocatedSignatureCount();
                locatedSignatureCount.SubjectUri = GenerateNewId();
                locatedSignatureCount.SignatureCount = DeserializerHelper.GiveMeSingleIntegerValue(constituency.signature_count);
                locatedSignatureCount.SignatureCountRetrievedAt = DeserializerHelper.GiveMeSingleDateValue(petitionRetrievalTimestamp);
                if (constituencies.ContainsKey(constituency.ons_code))
                {
                    locatedSignatureCount.LocatedSignatureCountHasPlace = new ConstituencyArea()
                    {
                        SubjectUri = new Uri(constituencies[constituency.ons_code])
                    };
                    signatures.Add(locatedSignatureCount);
                }
                else
                    logger.Warning($"Found signature on petition without matching constituency code - {constituency.ons_code}");
            }
            return signatures.ToArray();
        }

    }
}