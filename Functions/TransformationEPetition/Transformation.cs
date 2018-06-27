using Newtonsoft.Json;
using Parliament.Rdf.Serialization;
using Parliament.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF;

namespace Functions.TransformationEPetition
{
    public class Transformation : BaseTransformation<Settings>
    {

        public override BaseResource[] TransformSource(string response)
        {
            Rootobject sourceEPetition = JsonConvert.DeserializeObject<Rootobject>(response, new JsonSerializerSettings() { DateTimeZoneHandling = DateTimeZoneHandling.Utc });
            DateTime petitionRetrievalTimestamp = DateTime.UtcNow;
            UkgapEPetition ukgapEPetition = new UkgapEPetition();
            BaseResource[] moderations = null;
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
                    GovernmentResponse governmentResponse = generateGovernmentResponse(sourceEPetition.data.attributes.government_response);
                    ukgapEPetition.EPetitionHasGovernmentResponse = new GovernmentResponse[] { governmentResponse };
                }
                if (sourceEPetition.data.attributes.debate != null)
                {
                    Parliament.Model.Debate debate = generateDebate(sourceEPetition.data.attributes.debate, sourceEPetition.data.attributes.scheduled_debate_date);
                    ukgapEPetition.EPetitionHasDebate = new Parliament.Model.Debate[] { debate };
                }
                moderations = generateModerations(sourceEPetition.data.attributes.open_at, sourceEPetition.data.attributes.rejected_at, sourceEPetition.data.attributes.rejection);
                ukgapEPetition.EPetitionHasThresholdAttainment = generateThresholdAttainments(sourceEPetition.data.attributes.moderation_threshold_reached_at, sourceEPetition.data.attributes.response_threshold_reached_at, sourceEPetition.data.attributes.debate_threshold_reached_at);
                List<LocatedSignatureCount> signatures = new List<LocatedSignatureCount>();
                signatures.AddRange(generateInternationalAreasSignatures(sourceEPetition.data.attributes.signatures_by_country, petitionRetrievalTimestamp));
                signatures.AddRange(generateConstituencySignatures(sourceEPetition.data.attributes.signatures_by_constituency, petitionRetrievalTimestamp));
                ukgapEPetition.EPetitionHasLocatedSignatureCount = signatures;
            }

            return moderations.Concat(ukgapEPetition.AsEnumerable()).ToArray();
        }

        public override Dictionary<string, INode> GetKeysFromSource(BaseResource[] deserializedSource)
        {
            string ePetitionUkgapId = deserializedSource.OfType<UkgapEPetition>()
                .SingleOrDefault()
                .EPetitionUkgapId;
            return new Dictionary<string, INode>()
            {
                { "ePetitionUkgapId", SparqlConstructor.GetNode(ePetitionUkgapId) }
            };
        }

        public override BaseResource[] SynchronizeIds(BaseResource[] source, Uri subjectUri, BaseResource[] target)
        {
            UkgapEPetition ePetition = source.OfType<UkgapEPetition>().SingleOrDefault();
            ePetition.Id = subjectUri;

            GovernmentResponse governmentResponse = target.OfType<GovernmentResponse>().SingleOrDefault();
            if (governmentResponse != null)
                ePetition.EPetitionHasGovernmentResponse.SingleOrDefault().Id = governmentResponse.Id;
            Parliament.Model.Debate debate = target.OfType<Parliament.Model.Debate>().SingleOrDefault();
            if (debate != null)
                ePetition.EPetitionHasDebate.SingleOrDefault().Id = debate.Id;
            IEnumerable<Approval> approvals = target.OfType<Approval>();
            List<Approval> ePetitionApprovals = source.OfType<Approval>().ToList();
            foreach (Approval approval in approvals)
            {
                Approval foundModeration = ePetitionApprovals
                        .SingleOrDefault(m => m.ApprovedAt == approval.ApprovedAt);
                if (foundModeration != null)
                    foundModeration.Id = approval.Id;
                else
                    ePetitionApprovals.Add(approval);
            }
            foreach (Approval approval in ePetitionApprovals)
                approval.ApprovalHasApprovedEPetition = new ApprovedEPetition[]
                    {
                        new ApprovedEPetition()
                        {
                            Id=subjectUri
                        }
                    };

            IEnumerable<Parliament.Model.Rejection> rejects = target.OfType<Parliament.Model.Rejection>();
            List<Parliament.Model.Rejection> ePetitionRejections = source.OfType<Parliament.Model.Rejection>().ToList();
            foreach (Parliament.Model.Rejection rejection in rejects)
            {
                Parliament.Model.Rejection foundModeration = ePetitionRejections
                        .SingleOrDefault(m => m.RejectedAt == rejection.RejectedAt);
                if (foundModeration != null)
                    foundModeration.Id = rejection.Id;
                else
                    ePetitionRejections.Add(rejection);
            }
            foreach (Parliament.Model.Rejection rejection in ePetitionRejections)
                rejection.RejectionHasRejectedEPetition = new RejectedEPetition[]
                    {
                        new RejectedEPetition()
                        {
                            Id=subjectUri
                        }
                    };

            IEnumerable<ThresholdAttainment> thresholdAttainments = target.OfType<ThresholdAttainment>();
            foreach (ThresholdAttainment thresholdAttainment in thresholdAttainments)
            {
                ThresholdAttainment foundThresholdAttainment = ePetition.EPetitionHasThresholdAttainment.SingleOrDefault(t => t.ThresholdAttainmentAt == thresholdAttainment.ThresholdAttainmentAt);
                if (foundThresholdAttainment != null)
                    foundThresholdAttainment.Id = thresholdAttainment.Id;
            }
            IEnumerable<LocatedSignatureCount> signatures = target.OfType<LocatedSignatureCount>();
            foreach (LocatedSignatureCount signature in signatures)
            {
                LocatedSignatureCount foundLocatedSignatureCount = ePetition.EPetitionHasLocatedSignatureCount
                    .SingleOrDefault(s => s.LocatedSignatureCountHasPlace.Id == signature.LocatedSignatureCountHasPlace.Id);
                if (foundLocatedSignatureCount != null)
                {
                    foundLocatedSignatureCount.Id = signature.Id;
                    if (foundLocatedSignatureCount.SignatureCount.SingleOrDefault() == signature.SignatureCount.SingleOrDefault())
                        foundLocatedSignatureCount.SignatureCountRetrievedAt = signature.SignatureCountRetrievedAt;
                }
                else
                    foundLocatedSignatureCount.Id = GenerateNewId();
            }

            return ePetitionApprovals.AsEnumerable<BaseResource>().Concat(ePetitionRejections).Concat(ePetition.AsEnumerable()).ToArray();
        }

        private GovernmentResponse generateGovernmentResponse(Government_Response sourceGovernmentResponse)
        {
            GovernmentResponse governmentResponse = new GovernmentResponse();
            governmentResponse.Id = GenerateNewId();
            governmentResponse.GovernmentResponseCreatedAt = DeserializerHelper.GiveMeSingleDateValue(sourceGovernmentResponse.created_at);
            governmentResponse.GovernmentResponseUpdatedAt = DeserializerHelper.GiveMeSingleDateValue(sourceGovernmentResponse.updated_at);
            governmentResponse.GovernmentResponseDetails = DeserializerHelper.GiveMeSingleTextValue(sourceGovernmentResponse.details);
            governmentResponse.GovernmentResponseSummary = DeserializerHelper.GiveMeSingleTextValue(sourceGovernmentResponse.summary);

            return governmentResponse;
        }

        private Parliament.Model.Debate generateDebate(Debate sourceDebate, DateTime? scheduledDebateDate)
        {
            Parliament.Model.Debate debate = new Parliament.Model.Debate();
            debate.Id = GenerateNewId();
            debate.DebateProposedDate = DeserializerHelper.GiveMeSingleDateValue(scheduledDebateDate);
            debate.DebateDate = DeserializerHelper.GiveMeSingleDateValue(sourceDebate.debated_on);
            debate.DebateVideoUrl = DeserializerHelper.GiveMeSingleTextValue(sourceDebate.video_url);
            debate.DebateTranscriptUrl = DeserializerHelper.GiveMeSingleTextValue(sourceDebate.transcript_url);
            debate.DebateOverview = DeserializerHelper.GiveMeSingleTextValue(sourceDebate.overview);

            return debate;
        }

        private BaseResource[] generateModerations(DateTime? openedAt, DateTime? rejectedAt, Rejection sourceRejection)
        {
            List<BaseResource> moderations = new List<BaseResource>();
            if (openedAt.HasValue)
            {
                Approval approval = new Approval();
                approval.Id = GenerateNewId();
                approval.ApprovedAt = openedAt;
                moderations.Add(approval);
            }
            if (rejectedAt.HasValue)
            {
                Parliament.Model.Rejection rejection = new Parliament.Model.Rejection();
                rejection.Id = GenerateNewId();
                rejection.RejectedAt = rejectedAt;
                rejection.RejectionDetails = DeserializerHelper.GiveMeSingleTextValue(sourceRejection.details);
                if (string.IsNullOrWhiteSpace(sourceRejection.code) == false)
                {
                    Uri rejectionCodeUri = IdRetrieval.GetSubject("rejectionCodeName", sourceRejection.code, false, logger);
                    if (rejectionCodeUri != null)
                        rejection.RejectionHasRejectionCode = new RejectionCode()
                        {
                            Id = rejectionCodeUri
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

        private ThresholdAttainment[] generateThresholdAttainments(DateTime? moderatedAt, DateTime? respondedAt, DateTime? debatedAt)
        {
            List<ThresholdAttainment> thresholdAttainments = new List<ThresholdAttainment>();
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
                    ThresholdAttainment thresholdAttainment = new ThresholdAttainment();
                    thresholdAttainment.Id = GenerateNewId();
                    thresholdAttainment.ThresholdAttainmentAt = timestamp;
                    thresholdAttainment.ThresholdAttainmentHasThreshold = new Threshold()
                    {
                        Id = new Uri(threshold.Value)
                    };
                    thresholdAttainments.Add(thresholdAttainment);
                }
            }
            return thresholdAttainments.ToArray();
        }

        private LocatedSignatureCount[] generateInternationalAreasSignatures(Signatures_By_Country[] signaturesByCountry, DateTime petitionRetrievalTimestamp)
        {
            List<LocatedSignatureCount> signatures = new List<LocatedSignatureCount>();
            Dictionary<string, string> countries = IdRetrieval.GetSubjectsDictionary("countryGovRegisterId", logger);
            Dictionary<string, string> territories = IdRetrieval.GetSubjectsDictionary("territoryGovRegisterId", logger);
            foreach (Signatures_By_Country country in signaturesByCountry)
            {
                LocatedSignatureCount locatedSignatureCount = new LocatedSignatureCount();
                locatedSignatureCount.SignatureCount = DeserializerHelper.GiveMeSingleIntegerValue(country.signature_count);
                locatedSignatureCount.SignatureCountRetrievedAt = DeserializerHelper.GiveMeSingleDateValue(petitionRetrievalTimestamp);
                Place place = null;
                if (territories.ContainsKey(country.code))
                    place = new Place()
                    {
                        Id = new Uri(territories[country.code])
                    };
                else
                    if (countries.ContainsKey(country.code))
                    place = new Place()
                    {
                        Id = new Uri(countries[country.code])
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

        private LocatedSignatureCount[] generateConstituencySignatures(Signatures_By_Constituency[] signaturesByConstituency, DateTime petitionRetrievalTimestamp)
        {
            List<LocatedSignatureCount> signatures = new List<LocatedSignatureCount>();
            Dictionary<string, string> constituencies = IdRetrieval.GetSubjectsDictionary("constituencyGroupOnsCode", logger);
            foreach (Signatures_By_Constituency constituency in signaturesByConstituency)
            {
                LocatedSignatureCount locatedSignatureCount = new LocatedSignatureCount();
                locatedSignatureCount.SignatureCount = DeserializerHelper.GiveMeSingleIntegerValue(constituency.signature_count);
                locatedSignatureCount.SignatureCountRetrievedAt = DeserializerHelper.GiveMeSingleDateValue(petitionRetrievalTimestamp);
                if (constituencies.ContainsKey(constituency.ons_code))
                {
                    locatedSignatureCount.LocatedSignatureCountHasPlace = new Place()
                    {
                        Id = new Uri(constituencies[constituency.ons_code])
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