using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web;

namespace Parliament.Data.Orchestration.CoordinateTransformation
{
    public class TextPlainFormatter : MediaTypeFormatter
    {

        public TextPlainFormatter()
        {
            SupportedMediaTypes.Add(new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain"));
            MediaTypeMappings.Add(new RequestHeaderMapping("Accept", "text/plain", StringComparison.Ordinal, false, "text/plain"));
        }

        public override bool CanReadType(Type type)
        {
            return type == typeof(string);
        }

        public override bool CanWriteType(Type type)
        {
            return type == typeof(string);
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            return Task.Factory.StartNew(() => {
                using (StreamReader reader = new StreamReader(readStream))
                {
                    return (object)reader.ReadToEnd();
                }
            });
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            return Task.Factory.StartNew(() => {
                using (StreamWriter writer = new StreamWriter(writeStream))
                {
                    writer.Write(value);
                }
            });
        }

    }
}