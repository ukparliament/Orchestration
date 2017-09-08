namespace Functions.TransformationEPetition
{
    public class Settings : ITransformationSettings
    {

        public string AcceptHeader
        {
            get
            {
                return "application/json";
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

        public string ExistingGraphSparqlCommand
        {
            get
            {
                return @"
            construct {
                ?ePetition a parl:EPetition;
		            parl:ePetitionUkgapId ?ePetitionUkgapId;
	                parl:background ?background;
	                parl:additionalDetails ?additionalDetails;
	                parl:closedAt ?closedAt;
                    parl:createdAt ?createdAt;
	                parl:action ?action;
	                parl:updatedAt ?updatedAt;
	                parl:ePetitionHasGovernmentResponse ?governmentResponse;
                    parl:ePetitionHasDebate ?debate;
                    parl:ePetitionHasLocatedSignatureCount ?locatedSignatureCount;
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
                ?locatedSignatureCount a parl:LocatedSignatureCount;
                    parl:signatureCount ?signatureCount;
                    parl:signatureCountRetrievedAt ?signatureCountRetrievedAt;
                    parl:locatedSignatureCountHasPlace ?locatedSignatureCountHasPlace.
                ?locatedSignatureCountHasPlace a parl:Place.
                ?rejection a parl:Rejection; 
                    parl:rejectionHasRejectionCode ?rejectionCode ;
                    parl:rejectionDetails ?rejectionDetails ;
                    parl:rejectedAt ?rejectedAt.
                ?approval a parl:Approval; 
                    parl:approvedAt ?approvedAt.
                ?thresholdAttainment a parl:ThresholdAttainment;
                    parl:thresholdAttainmentAt ?thresholdAttainmentAt;
                    parl:thresholdAttainmentHasThreshold ?threshold.
            }
            where {
	            bind(@subject as ?ePetition)
	            ?ePetition parl:ePetitionUkgapId ?ePetitionUkgapId.
	            optional {?ePetition parl:background ?background}
	            optional {?ePetition parl:additionalDetails ?additionalDetails}
                optional {?ePetition parl:createdAt ?createdAt}
	            optional {?ePetition parl:closedAt ?closedAt}
	            optional {?ePetition parl:action ?action}
                optional {?ePetition parl:updatedAt ?updatedAt}
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
                OPTIONAL {
                    ?locatedSignatureCount
                        ^parl:ePetitionHasLocatedSignatureCount ?ePetition ;
                        parl:signatureCount ?signatureCount ;
                        parl:locatedSignatureCountHasPlace ?locatedSignatureCountHasPlace ;
                        parl:signatureCountRetrievedAt ?signatureCountRetrievedAt .
                    {
                        SELECT ?ePetition ?locatedSignatureCountHasPlace (MAX(?signatureCountRetrievedAt) AS ?signatureCountRetrievedAt)
                        WHERE {
                            ?locatedSignatureCount 
                                a parl:LocatedSignatureCount ;
                                ^parl:ePetitionHasLocatedSignatureCount ?ePetition ;
                                parl:signatureCountRetrievedAt ?signatureCountRetrievedAt ;
                                parl:locatedSignatureCountHasPlace ?locatedSignatureCountHasPlace .
                        }
                        GROUP BY ?ePetition ?locatedSignatureCountHasPlace
                    }
                }
                optional {
                    ?ePetition parl:ePetitionHasModeration ?moderation.
                    optional {
                        bind(?moderation as ?rejection)
                        ?rejection parl:rejectedAt ?rejectedAt.
                        optional {?rejection parl:rejectionHasRejectionCode ?rejectionCode}
                        optional {?rejection parl:rejectionDetails ?rejectionDetails}
                    }
                    optional {
                        bind(?moderation as ?approval)
                        ?approval parl:approvedAt ?approvedAt.
                    }
                }
                optional {
                    ?ePetition parl:ePetitionHasThresholdAttainment ?thresholdAttainment
                    optional {?thresholdAttainment parl:thresholdAttainmentAt ?thresholdAttainmentAt}
                    optional {?thresholdAttainment parl:thresholdAttainmentHasThreshold ?threshold}
                }
            }";
            }
        }

        public string FullDataUrlParameterizedString(string dataUrl)
        {
            return dataUrl;
        }
    }
}
