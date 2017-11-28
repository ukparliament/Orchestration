﻿using System;

namespace Functions.TransformationPhoto
{
    public class Settings : ITransformationSettings
    {
        public string AcceptHeader
        {
            get
            {
                return string.Empty;
            }
        }

        public string SubjectRetrievalSparqlCommand
        {
            get
            {
                return @"
            construct{
                ?s a parl:MemberImage.
            }
            where{
                bind(@subjectUri as ?s)
                ?s parl:memberImageHasMember ?person.
            }";
            }
        }

        public string ExistingGraphSparqlCommand
        {
            get
            {
                return @"
        construct {
            ?memberImage a parl:MemberImage;
        	             parl:memberImageHasMember ?member;
                         parl:personImageFaceCentrePoint ?point.
        }
        where {
            bind(@subject as ?memberImage)
            ?memberImage parl:memberImageHasMember ?member.
            optional {?memberImage parl:personImageFaceCentrePoint ?point.}
        }";
            }
        }

        public string FullDataUrlParameterizedString(string dataUrl)
        {
            return System.Environment.GetEnvironmentVariable("CUSTOMCONNSTR_SharepointItem", EnvironmentVariableTarget.Process).Replace("{listId}", "2460c6bf-f26e-4049-94d0-c6096e036f3a").Replace("{id}", dataUrl);
        }
    }
}
