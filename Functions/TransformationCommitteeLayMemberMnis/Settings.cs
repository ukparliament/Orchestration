namespace Functions.TransformationCommitteeLayMemberMnis
{
    public class Settings : ITransformationSettings
    {

        public string AcceptHeader
        {
            get
            {
                return "application/atom+xml";
            }
        }

        public string SubjectRetrievalSparqlCommand
        {
            get
            {
                return @"
            construct{
                ?s a parl:Person.
            }
            where{
                ?s parl:formalBodyLayPersonMnisId @formalBodyLayPersonMnisId.
            }";
            }
        }

        public string ExistingGraphSparqlCommand
        {
            get
            {
                return @"
        construct {
        	?person a parl:Person;
                parl:formalBodyLayPersonMnisId ?formalBodyLayPersonMnisId;
                parl:personGivenName ?personGivenName;
                parl:personOtherNames ?personOtherNames;
                parl:personFamilyName ?personFamilyName;
                parl:personHasFormalBodyMembership ?personHasFormalBodyMembership;
                parl:personHasGenderIdentity ?genderIdentity.                
            ?genderIdentity a parl:GenderIdentity;
                parl:genderIdentityHasGender ?gender.
            ?personHasFormalBodyMembership a parl:FormalBodyMembership;
                parl:formalBodyMembershipStartDate ?formalBodyMembershipStartDate;
                parl:formalBodyMembershipEndDate ?formalBodyMembershipEndDate;
                parl:formalBodyMembershipHasFormalBody ?formalBodyMembershipHasFormalBody.
            ?formalBodyMembershipHasFormalBody a parl:FormalBody.
        }
        where {
            bind(@subject as ?person)
            ?person parl:formalBodyLayPersonMnisId ?formalBodyLayPersonMnisId.
            optional {?person parl:personGivenName ?personGivenName}
            optional {?person parl:personOtherNames ?personOtherNames}
            optional {?person parl:personFamilyName ?personFamilyName}
            optional {
                ?person parl:personHasFormalBodyMembership ?personHasFormalBodyMembership.
                optional {?personHasFormalBodyMembership parl:formalBodyMembershipStartDate ?formalBodyMembershipStartDate}
                optional {?personHasFormalBodyMembership parl:formalBodyMembershipEndDate ?formalBodyMembershipEndDate}
                optional {?personHasFormalBodyMembership parl:formalBodyMembershipHasFormalBody ?formalBodyMembershipHasFormalBody}
            }
            optional {                
                ?person parl:personHasGenderIdentity ?genderIdentity.
                optional {
                    bind(@gender as ?gender)
                    ?genderIdentity parl:genderIdentityHasGender ?gender.
                }
            }
        }";
            }
        }

        public string FullDataUrlParameterizedString(string dataUrl)
        {
            return $"{dataUrl}?$select=CommitteeLayMember_Id,Committee_Id,Forename,MiddleNames,Surname,StartDate,EndDate,Gender";
        }
    }
}
