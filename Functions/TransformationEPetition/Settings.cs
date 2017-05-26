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

        public string ExisitngGraphSparqlCommand
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
	                parl:ePetitionHasGovernmentResponse ?governmentResponse.
		        ?governmentResponse parl:governmentResponseCreatedAt ?governmentResponseCreatedAt;
		            parl:governmentResponseUpdatedAt ?governmentResponseUpdatedAt;
		            parl:governmentResponseDetails ?governmentResponseDetails;
		            parl:governmentResponseSummary ?governmentResponseSummary.
		        ?ePetition parl:ePetitionHasDebate ?debate
		            parl:debateProposedDate ?debateProposedDate;
                    parl:debateDate ?debateDate;
                    parl:debateVideoUrl ?debateVideoUrl;
                    parl:debateTranscriptUrl ?debateTranscriptUrl;
                    parl:debateOverview ?debateOverview.
	            ?ePetition parl:ePetitionHasLocatedSignatureCount ?locatedSignature;
                    parl:signatureCount ?signatureCount;
                    parl:signatureCountRetrievedAt ?signatureCountRetrievedAt;
                    parl:locatedSignatureCountHasPlace ?locatedSignatureCountHasPlace.
                ?ePetition parl:ePetitionHasModerationState ?moderationState.
                ?moderationState parl:moderationStateChange ?moderationStateChange;
                    parl:moderationStateHasModerationOption ?moderationOption.
                ?moderationOption parl:moderationOptionDetails ?moderationOptionDetails;
                    parl:rejectionDetails ?rejectionDetails;
                    parl:rejectionHasRejectionCode ?rejectionCode.
                ?ePetition parl:ePetitionHasThresholdAttainment ?thresholdAttainment;
                    parl:dateOfThresholdAttainment ?dateOfThresholdAttainment;
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
                optional {
                    ?ePetition parl:ePetitionHasLocatedSignatureCount ?locatedSignature
                    optional {?locatedSignature parl:signatureCount ?signatureCount}
                    optional {?locatedSignature parl:signatureCountRetrievedAt ?signatureCountRetrievedAt}
                    optional {?locatedSignature parl:locatedSignatureCountHasPlace ?locatedSignatureCountHasPlace}
                }
                optional {
                    ?ePetition parl:ePetitionHasModerationState ?moderationState
                    optional {?moderationState parl:moderationStateChange ?moderationStateChange}
                    optional {
                        ?moderationState parl:moderationStateHasModerationOption ?moderationOption
                        optional {?moderationOption parl:moderationOptionDetails ?moderationOptionDetails}
                        optional {?moderationOption parl:rejectionDetails ?rejectionDetails}
                        optional {?moderationOption parl:rejectionHasRejectionCode ?rejectionCode}
                    }
                }
                optional {
                    ?ePetition parl:ePetitionHasThresholdAttainment ?thresholdAttainment
                    optional {?thresholdAttainment parl:dateOfThresholdAttainment ?dateOfThresholdAttainment}
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
            return dataUrl;
        }
    }
}
