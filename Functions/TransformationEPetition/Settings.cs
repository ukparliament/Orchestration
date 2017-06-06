using System.Collections.Generic;
using System.Xml;

namespace Functions.TransformationEPetition
{
    public class Settings : ITransformationSettings
    {
        public string OperationName
        {
            get
            {
                return "TransformationEPetition";
            }
        }

        public string AcceptHeader
        {
            get
            {
                return "application/json";
            }
        }

        public XmlNamespaceManager SourceXmlNamespaceManager
        {
            get
            {
                XmlNamespaceManager sourceXmlNamespaceManager = new XmlNamespaceManager(new NameTable());
                return sourceXmlNamespaceManager;
            }
        }

        public string SubjectRetrievalSparqlCommand
        {
            get
            {
                return @"
            construct{
                ?s a parl:EPetition.
            }
            where{
                ?s a parl:EPetition; 
                    parl:ePetitionUkgapId @ePetitionUkgapId.
            }";
            }
        }

        public Dictionary<string, string> SubjectRetrievalParameters
        {
            get
            {
                return new Dictionary<string, string>
                {
                    {"ePetitionUkgapId","root/data/id" }
                };
            }
        }

        public string ExistingGraphSparqlCommand
        {
            get
            {
                return @"
            construct {
                ?ePetition a parl:EPetition;
		            parl:ePetitionUkgapId ?ePetitionUkgapId;
	                parl:openedAt ?openedAt;
	                parl:background ?background;
	                parl:additionalDetails ?additionalDetails;
	                parl:closedAt ?closedAt;
	                parl:action ?action;
	                parl:ePetitionHasGovernmentResponse ?governmentResponse;
                    parl:ePetitionHasDebate ?debate;
                    parl:ePetitionHasLocatedSignatureCount ?locatedSignature;
                    parl:ePetitionHasModeration ?moderation;
                    parl:ePetitionHasThresholdAttainment ?thresholdAttainment.
		        ?governmentResponse a parl:GovernmentResponse;
                    parl:governmentResponseCreatedAt ?governmentResponseCreatedAt;
		            parl:governmentResponseUpdatedAt ?governmentResponseUpdatedAt;
		            parl:governmentResponseDetails ?governmentResponseDetails;
		            parl:governmentResponseSummary ?governmentResponseSummary.
                ?debate a parl:Debate;
		            parl:debateProposedDate ?debateProposedDate;
                    parl:debateDate ?debateDate;
                    parl:debateVideoUrl ?debateVideoUrl;
                    parl:debateTranscriptUrl ?debateTranscriptUrl;
                    parl:debateOverview ?debateOverview.
             #   ?locatedSignatureCount a parl:LocatedSignatureCount;
             #       parl:signatureCount ?signatureCount;
             #       parl:signatureCountRetrievedAt ?signatureCountRetrievedAt;
             #       parl:locatedSignatureCountHasPlace ?locatedSignatureCountHasPlace.
                ?moderation a parl:Moderation; 
                    parl:moderationHasModerationOption ?moderationOption;
                    parl:moderatedAt ?moderatedAt.
                ?thresholdAttainment a parl:ThresholdAttainment;
                    parl:thresholdAttainmentAt ?thresholdAttainmentAt;
                    parl:thresholdAttainmentHasThreshold ?threshold.
            }
            where {
	            bind(@subject as ?ePetition)
	            ?ePetition a parl:EPetition;
		            parl:ePetitionUkgapId ?ePetitionUkgapId.
	            optional {?ePetition parl:openedAt ?openedAt}
	            optional {?ePetition parl:background ?background}
	            optional {?ePetition parl:additionalDetails ?additionalDetails}
	            optional {?ePetition parl:closedAt ?closedAt}
	            optional {?ePetition parl:action ?action}
	            optional {
		            ?ePetition parl:ePetitionHasGovernmentResponse ?governmentResponse
		            optional {?governmentResponse parl:governmentResponseCreatedAt ?governmentResponseCreatedAt}
		            optional {?governmentResponse parl:governmentResponseUpdatedAt ?governmentResponseUpdatedAt}
		            optional {?governmentResponse parl:governmentResponseDetails ?governmentResponseDetails}
		            optional {?governmentResponse parl:governmentResponseSummary ?governmentResponseSummary}
	            }
		        optional {
		            ?ePetition parl:ePetitionHasDebate ?debate
		            optional {?debate parl:debateProposedDate ?debateProposedDate}
                    optional {?debate parl:debateDate ?debateDate}
                    optional {?debate parl:debateVideoUrl ?debateVideoUrl}
                    optional {?debate parl:debateTranscriptUrl ?debateTranscriptUrl}
                    optional {?debate parl:debateOverview ?debateOverview}
	            }
             #   optional {
             #       ?ePetition parl:ePetitionHasLocatedSignatureCount ?locatedSignatureCount
             #       optional {?locatedSignatureCount parl:signatureCount ?signatureCount}
             #       optional {?locatedSignatureCount parl:signatureCountRetrievedAt ?signatureCountRetrievedAt}
             #       optional {?locatedSignatureCount parl:locatedSignatureCountHasPlace ?locatedSignatureCountHasPlace}
             #   }
                optional {
                    ?ePetition parl:ePetitionHasModeration ?moderation
                    optional {?moderation parl:moderationAt ?moderationAt}
                    optional {?moderation parl:moderationHasModerationOption ?moderationOption}
                }
                optional {
                    ?ePetition parl:ePetitionHasThresholdAttainment ?thresholdAttainment
                    optional {?thresholdAttainment parl:thresholdAttainmentAt ?thresholdAttainmentAt}
                    optional {?thresholdAttainment parl:thresholdAttainmentHasThreshold ?threshold}
                }
            }";
            }
        }

        public Dictionary<string, string> ExistingGraphSparqlParameters
        {
            get
            {
                return null;
            }
        }

        public string FullDataUrlParameterizedString(string dataUrl)
        {
            return $"https://petition.parliament.uk/petitions/{dataUrl}.json";
        }
    }
}
