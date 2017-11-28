﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parliament.Ontology.Base;
using Parliament.Ontology.Code;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Functions.TransformationLordsSeat
{
    public class Transformation : BaseTransformation<Settings>
    {
        private readonly string idNamespace = Environment.GetEnvironmentVariable("IdNamespace", EnvironmentVariableTarget.Process);

        public override IOntologyInstance[] TransformSource(string response)
        {
            IHouseSeat houseSeat = new HouseSeat();
            JObject jsonResponse = (JObject)JsonConvert.DeserializeObject(response);

            string id = ((JValue)jsonResponse.SelectToken("ID0")).GetText();
            Uri uri = null;
            if (string.IsNullOrWhiteSpace(id))
            {
                logger.Warning("No Id info found");
                return null;
            }
            else
            {
                if (Uri.TryCreate($"{idNamespace}{id}", UriKind.Absolute, out uri))
                    houseSeat.SubjectUri = uri;
                else
                {
                    logger.Warning($"Invalid url '{id}' found");
                    return null;
                }
            }
            houseSeat.HouseSeatName = ((JValue)jsonResponse.SelectToken("Name")).GetText();

            Uri houseSeatTypeUri = giveMeUri(jsonResponse, "Type_x0020_ID_x0020__x0028_for_x");
            if (houseSeatTypeUri == null)
                return null;
            else
                houseSeat.HouseSeatHasHouseSeatType = new HouseSeatType()
                {
                    SubjectUri = houseSeatTypeUri
                };


            Uri houseOfLordsUri = IdRetrieval.GetSubject("houseName", "House of Lords", false, logger);
            houseSeat.HouseSeatHasHouse = new House()
            {
                SubjectUri = houseOfLordsUri
            };

            return new IOntologyInstance[] { houseSeat };
        }

        public override Dictionary<string, object> GetKeysFromSource(IOntologyInstance[] deserializedSource)
        {
            Uri subjectUri = deserializedSource.OfType<IHouseSeat>()
                .SingleOrDefault()
                .SubjectUri;
            return new Dictionary<string, object>()
            {
                { "subjectUri", subjectUri }
            };
        }

        public override IOntologyInstance[] SynchronizeIds(IOntologyInstance[] source, Uri subjectUri, IOntologyInstance[] target)
        {
            IHouseSeat houseSeatTypes = source.OfType<IHouseSeat>().SingleOrDefault();

            return new IOntologyInstance[] { houseSeatTypes };
        }

        private Uri giveMeUri(JObject jsonResponse, string tokenName)
        {
            object id = ((JValue)jsonResponse.SelectToken($"{tokenName}.Value"))?.Value;
            if (id == null)
            {
                logger.Warning($"No {tokenName} Id info found");
                return null;
            }
            else
            {
                if (Uri.TryCreate($"{idNamespace}{id}", UriKind.Absolute, out Uri uri))
                    return uri;
                else
                {
                    logger.Warning($"Invalid url '{id}' found");
                    return null;
                }
            }
        }
    }
}