namespace Functions.TransformationMemberMnis
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
                ?s parl:memberMnisId @memberMnisId.
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
                parl:personDateOfBirth ?personDateOfBirth;
        		parl:personGivenName ?personGivenName;
        		parl:personOtherNames ?personOtherNames;
        		parl:personFamilyName ?personFamilyName;
        		parl:personDateOfDeath ?personDateOfDeath;
        		parl:memberMnisId ?memberMnisId;
                parl:personPimsId ?personPimsId;
                parl:personDodsId ?personDodsId;
                <http://example.com/F31CBD81AD8343898B49DC65743F0BDF> ?personNameDisplayAs;
                <http://example.com/A5EE13ABE03C4D3A8F1A274F57097B6C> ?personNameListAs;
                <http://example.com/D79B0BAC513C4A9A87C9D5AFF1FC632F> ?personNameFullTitle;
                parl:personHasGenderIdentity ?genderIdentity.                
            ?genderIdentity a parl:GenderIdentity;
                parl:genderIdentityHasGender ?gender.            
        }
        where {
            bind(@subject as ?person)
            ?person parl:memberMnisId ?memberMnisId.
            optional {?person parl:personPimsId ?personPimsId}
            optional {?person parl:personDodsId ?personDodsId}
            optional {?person parl:personDateOfBirth ?personDateOfBirth}
            optional {?person parl:personGivenName ?personGivenName}
            optional {?person parl:personOtherNames ?personOtherNames}
            optional {?person parl:personFamilyName ?personFamilyName}
            optional {?person parl:personDateOfDeath ?personDateOfDeath}
            optional {?person <http://example.com/F31CBD81AD8343898B49DC65743F0BDF> ?personNameDisplayAs}
            optional {?person <http://example.com/A5EE13ABE03C4D3A8F1A274F57097B6C> ?personNameListAs}
            optional {?person <http://example.com/D79B0BAC513C4A9A87C9D5AFF1FC632F> ?personNameFullTitle}
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

        public string ParameterizedString(string dataUrl)
        {
            return $"{dataUrl}?$select=Surname,MiddleNames,Forename,DateOfBirth,DateOfDeath,Gender,Member_Id,House,StartDate,EndDate,Dods_Id,Pims_Id,NameDisplayAs,NameListAs,NameFullTitle";
        }
    }
}
