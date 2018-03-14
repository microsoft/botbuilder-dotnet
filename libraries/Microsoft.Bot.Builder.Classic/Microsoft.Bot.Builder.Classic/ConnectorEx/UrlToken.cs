//// 
//// Copyright (c) Microsoft. All rights reserved.
//// Licensed under the MIT license.
//// 
//// Microsoft Bot Framework: http://botframework.com
//// 
//// Bot Builder SDK GitHub:
//// https://github.com/Microsoft/BotBuilder
//// 
//// Copyright (c) Microsoft Corporation
//// All rights reserved.
//// 
//// MIT License:
//// Permission is hereby granted, free of charge, to any person obtaining
//// a copy of this software and associated documentation files (the
//// "Software"), to deal in the Software without restriction, including
//// without limitation the rights to use, copy, modify, merge, publish,
//// distribute, sublicense, and/or sell copies of the Software, and to
//// permit persons to whom the Software is furnished to do so, subject to
//// the following conditions:
//// 
//// The above copyright notice and this permission notice shall be
//// included in all copies or substantial portions of the Software.
//// 
//// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
//// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////

//using Newtonsoft.Json;
//using Newtonsoft.Json.Bson;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.IO.Compression;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Web;

//namespace Microsoft.Bot.Builder.Classic.Dialogs
//{
//    /// <summary>
//    /// Allow object instances to serialized to URLs.  Base64 can not be stored in URLs due to special characters.
//    /// </summary>
//    /// <remarks>
//    /// We use Bson and Gzip to make it small enough to fit within the maximum character limit of URLs.
//    /// http://stackoverflow.com/a/32999062 suggests HttpServerUtility's UrlTokenEncode and UrlTokenDecode
//    /// is not standards-compliant, but they seem to do the job.
//    /// </remarks>
//    public static class UrlToken
//    {
//        /// <summary>
//        /// Encode an item to be stored in a url.
//        /// </summary>
//        /// <typeparam name="T">The item type.</typeparam>
//        /// <param name="item">The item instance.</param>
//        /// <returns>The encoded token.</returns>
//        public static string Encode<T>(T item)
//        {
//            using (var memory = new MemoryStream())
//            {
//                using (var gzip = new GZipStream(memory, CompressionMode.Compress))
//                using (var writer = new BsonWriter(gzip))
//                {
//                    var serializer = JsonSerializer.CreateDefault();
//                    serializer.Serialize(writer, item);
//                }
//                var token = HttpServerUtility.UrlTokenEncode(memory.ToArray());
//                return token;
//            }
//        }

//        /// <summary>
//        /// Decode an item from a url token.
//        /// </summary>
//        /// <typeparam name="T">The item type.</typeparam>
//        /// <param name="token">The item token.</param>
//        /// <returns>The item instance.</returns>
//        public static T Decode<T>(string token)
//        {
//            var buffer = HttpServerUtility.UrlTokenDecode(token);
//            using (var memory = new MemoryStream(buffer))
//            using (var gzip = new GZipStream(memory, CompressionMode.Decompress))
//            using (var reader = new BsonReader(gzip))
//            {
//                var serializer = JsonSerializer.CreateDefault();
//                var item = serializer.Deserialize<T>(reader);
//                return item;
//            }
//        }
//    }
//}
