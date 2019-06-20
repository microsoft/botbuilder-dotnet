using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Bot.Protocol
{
    public partial class Response
    {
        public void AddStream(HttpContent content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            if (Streams == null)
            {
                Streams = new List<HttpContentStream>();
            }

            Streams.Add(
                new HttpContentStream()
                {
                    Content = content
                });
        }

        public static Response NotFound(HttpContent body = null)
        {
            return CreateResponse(HttpStatusCode.NotFound, body);
        }

        public static Response Forbidden(HttpContent body = null)
        {
            return CreateResponse(HttpStatusCode.Forbidden, body);
        }

        public static Response OK(HttpContent body = null)
        {
            return CreateResponse(HttpStatusCode.OK, body);
        }

        public static Response InternalServerError(HttpContent body = null)
        {
            return CreateResponse(HttpStatusCode.InternalServerError, body);
        }

        public static Response CreateResponse(HttpStatusCode statusCode, HttpContent body = null)
        {
            var response = new Response()
            {
                StatusCode = (int)statusCode
            };

            if (body != null)
            {
                response.AddStream(body);
            }

            return response;
        }
    }
}
