using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parliament.Ontology.Base;
using Parliament.Ontology.Code;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Functions.TransformationPhoto
{
    public class Transformation : BaseTransformation<Settings>
    {
        public override IOntologyInstance[] TransformSource(string response)
        {
            IMemberImage memberImage = new MemberImage();
            JObject jsonResponse = (JObject)JsonConvert.DeserializeObject(response);

            string imageResourceUri = ((JValue)jsonResponse.SelectToken("ImageResourceUri")).GetText();
            if (string.IsNullOrWhiteSpace(imageResourceUri))
            {
                logger.Warning("No image resource uri info found");
                return null;
            }
            memberImage.SubjectUri = new Uri($"{idNamespace}{imageResourceUri}");

            bool? photoTakenDown = ((JValue)jsonResponse.SelectToken("Photo_x0020_Taken_x0020_Down")).GetBoolean();
            if (photoTakenDown == true)
            {
                logger.Verbose("Image information marked as removed");
                return new IOntologyInstance[] { memberImage };
            }

            var mnisId = ((JValue)jsonResponse.SelectToken("Person_x0020_MnisId"))?.Value;
            if (mnisId == null)
            {
                logger.Warning("No member info found");
                return null;
            }

            string mnisIdStr = Convert.ToInt32(Convert.ToDouble(mnisId)).ToString();

            Uri personUri = IdRetrieval.GetSubject("memberMnisId", mnisIdStr, false, logger);
            if (personUri != null)
            {
                memberImage.MemberImageHasMember = new List<IMember>
                    {
                        new Member()
                        {
                            SubjectUri=personUri
                        }
                    };
            }
            else
                logger.Warning("No person found");

            var midX = ((JValue)jsonResponse.SelectToken("MidX"))?.Value;
            var midY = ((JValue)jsonResponse.SelectToken("MidY"))?.Value;
            if (midX == null || midY == null)
            {
                logger.Warning("No MidX or MidY info found");
                return null;
            }
            string midXStr = Convert.ToInt32(Convert.ToDouble(midX)).ToString();
            string midYStr = Convert.ToInt32(Convert.ToDouble(midY)).ToString();

            memberImage.PersonImageFaceCentrePoint = new List<string>
            {
                string.Format("POINT({0} {1})", midXStr, midYStr)
            };

            return new IOntologyInstance[] { memberImage };
        }

        public override Dictionary<string, object> GetKeysFromSource(IOntologyInstance[] deserializedSource)
        {
            Uri subjectUri = deserializedSource.OfType<IMemberImage>()
                .SingleOrDefault()
                .SubjectUri;
            return new Dictionary<string, object>()
            {
                { "subjectUri", subjectUri }
            };
        }

        public override IOntologyInstance[] SynchronizeIds(IOntologyInstance[] source, Uri subjectUri, IOntologyInstance[] target)
        {
            IMemberImage memberImage = source.OfType<IMemberImage>().SingleOrDefault();

            return new IOntologyInstance[] { memberImage };
        }
        
    }
}
