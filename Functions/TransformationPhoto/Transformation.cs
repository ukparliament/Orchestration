using Newtonsoft.Json.Linq;
using Parliament.Model;
using Parliament.Rdf.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Functions.TransformationPhoto
{
    public class Transformation : BaseTransformationJson<Settings, JObject>
    {
        public override BaseResource[] TransformSource(JObject jsonResponse)
        {
            MemberImage memberImage = new MemberImage();
            string imageResourceUri = ((JValue)jsonResponse.SelectToken("ImageResourceUri")).GetText();
            if (string.IsNullOrWhiteSpace(imageResourceUri))
            {
                logger.Warning("No image resource uri info found");
                return null;
            }
            memberImage.Id = new Uri($"{idNamespace}{imageResourceUri}");

            bool? photoTakenDown = ((JValue)jsonResponse.SelectToken("Photo_x0020_Taken_x0020_Down")).GetBoolean();
            if (photoTakenDown == true)
            {
                logger.Verbose("Image information marked as removed");
                return new BaseResource[] { memberImage };
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
                memberImage.MemberImageHasMember = new List<Member>
                    {
                        new Member()
                        {
                            Id=personUri
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

            return new BaseResource[] { memberImage };
        }

        public override Uri GetSubjectFromSource(BaseResource[] deserializedSource)
        {
            return deserializedSource.OfType<MemberImage>()
                .SingleOrDefault()
                .Id;
        }

        public override BaseResource[] SynchronizeIds(BaseResource[] source, Uri subjectUri, BaseResource[] target)
        {
            return source;
        }

    }
}
