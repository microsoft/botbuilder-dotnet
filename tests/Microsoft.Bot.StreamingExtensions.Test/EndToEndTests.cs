using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.StreamingExtensions;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Microsoft.Bot.StreamingExtensions.UnitTests.Utilities;

namespace Microsoft.Bot.StreamingExtensions.UnitTests
{
    [TestClass]
    public class EndToEndTests
    {
        [TestMethod]
        public void SendToServer()
        {
            var flow = new NamedPipeFlow();
            flow.SendToServer(
                CreateGet("/hello"),
                CreateOK(),
                async (req, exp, res) =>
                {
                    Assert.AreEqual(200, res.StatusCode);
                });
            flow.Run();
        }

        [TestMethod]
        public void SendToClient()
        {
            var flow = new NamedPipeFlow();
            flow.SendToClient(
                CreateGet("/hello"),
                CreateOK(),
                async (req, exp, res) =>
                {
                    Assert.AreEqual(200, res.StatusCode);
                });
            flow.Run();
        }

        [TestMethod]
        public void SendBody()
        {
            var response = CreateOK();
            response.SetBody("TestBody");

            var flow = new NamedPipeFlow();
            flow.SendToServer(
                CreateGet("/hello"),
                response,
                async (req, exp, res) =>
                {
                    Assert.AreEqual(200, res.StatusCode);
                    var body = res.ReadBodyAsString();
                    Assert.AreEqual("TestBody", body);
                });
            flow.Run();
        }

        [TestMethod]
        public void SendMultipleToServer()
        {
            int count = 20;
            bool[] results = new bool[count];
            SetAll(results, false);

            var flow = new NamedPipeFlow();
            for (int i = 0; i < count; i++)
            {
                flow.SendToServer(
                    CreateGet($"/hello/{i}"),
                    CreateOK(i.ToString()),
                    async (req, exp, res) =>
                    {
                        Assert.AreEqual(200, res.StatusCode);
                        var body = res.ReadBodyAsString();
                        int idx = Int32.Parse(body);
                        results[idx] = true;
                    });
            }
            flow.Run();

            // received all messages
            Assert.IsTrue(results.All(x => x));
        }


        [TestMethod]
        public void SendMultipleToClient()
        {
            int count = 20;
            bool[] results = new bool[count];
            SetAll(results, false);

            var flow = new NamedPipeFlow();
            for (int i = 0; i < count; i++)
            {
                flow.SendToClient(
                    CreateGet($"/hello/{i}"),
                    CreateOK(i.ToString()),
                    async (req, exp, res) =>
                    {
                        Assert.AreEqual(200, res.StatusCode);
                        var body = res.ReadBodyAsString();
                        int idx = Int32.Parse(body);
                        results[idx] = true;
                    });
            }
            flow.Run();

            // received all messages
            Assert.IsTrue(results.All(x => x));
        }

        [TestMethod]
        public void SendMultipleToBoth()
        {
            int count = 500;
            bool[] toClientResults = new bool[count];
            SetAll(toClientResults, false);
            bool[] toServerResults = new bool[count];
            SetAll(toServerResults, false);

            var flow = new NamedPipeFlow();
            for (int i = 0; i < count; i++)
            {
                flow.SendToClient(
                    CreateGet($"/client/hello{i}"),
                    CreateOK(i.ToString()),
                    async (req, exp, res) =>
                    {
                        Assert.AreEqual(200, res.StatusCode);
                        var body = res.ReadBodyAsString();
                        int idx = Int32.Parse(body);
                        toClientResults[idx] = true;
                    });

                flow.SendToServer(
                    CreateGet($"/server/hello{i}"),
                    CreateOK(i.ToString()),
                    async (req, exp, res) =>
                    {
                        Assert.AreEqual(200, res.StatusCode);
                        var body = res.ReadBodyAsString();
                        int idx = Int32.Parse(body);
                        toServerResults[idx] = true;
                    });
            }
            flow.Run();

            // received all messages
            Assert.IsTrue(toClientResults.All(x => x));
            Assert.IsTrue(toServerResults.All(x => x));
        }

        [TestMethod]
        public void SendBigLoad()
        {
            int count = 20000;
            var sb = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                sb.Append('a');
            }

            var flow = new NamedPipeFlow();
            flow.SendToServer(
                CreatePost("/hello", sb.ToString()),
                CreateOK(sb.ToString()),
                async (req, exp, res) =>
                {
                    Assert.AreEqual(200, res.StatusCode);
                    var body = res.ReadBodyAsString();
                    Assert.AreEqual(count, body.Length);
                });
            flow.Run();
        }

        [TestMethod]
        public void SendAdaptiveCard()
        {
            var request = Request.CreatePost("/hello");
            request.SetBody(CreateBuggyCard());

            var response = Response.OK();

            var flow = new NamedPipeFlow();
            flow.SendToServer(
                request,
                response,
                async (req, exp, res) => { },
                async (req) =>
                {
                    var body = req.ReadBodyAsJson<Attachment>();
                    Assert.IsNotNull(body);
                });
            flow.Run();
        }

        #region Helpers

        private Request CreateGet(string path)
        {
            var r = Request.CreateGet(path);
            return r;
        }

        private Request CreatePost(string path, string body)
        {
            var r = Request.CreatePost(path);
            r.SetBody(body);
            return r;
        }

        private Response CreateOK(string body = null)
        {
            var r = new Response()
            {
                StatusCode = 200
            };
            if (body != null)
            {
                r.SetBody(body);
            }
            return r;
        }

        private static void SetAll<T>(T[] array, T value)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = value;
            }
        }

        public static Attachment CreateBuggyCard()
        {
            var adaptiveCardJson = "{\"type\":\"AdaptiveCard\",\"version\":\"1.0\",\"id\":\"ToDoCard\",\"speak\":\"\",\"backgroundImage\":\"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAKwAAACeCAYAAACvg+F+AAAABGdBTUEAALGPC/xhBQAAAAFzUkdCAK7OHOkAAAAJcEhZcwAAFiUAABYlAUlSJPAAAAAhdEVYdENyZWF0aW9uIFRpbWUAMjAxOTowMzoxMyAxOTo0Mjo0OBCBEeIAAAG8SURBVHhe7dJBDQAgEMCwA/+egQcmlrSfGdg6z0DE/oUEw5JiWFIMS4phSTEsKYYlxbCkGJYUw5JiWFIMS4phSTEsKYYlxbCkGJYUw5JiWFIMS4phSTEsKYYlxbCkGJYUw5JiWFIMS4phSTEsKYYlxbCkGJYUw5JiWFIMS4phSTEsKYYlxbCkGJYUw5JiWFIMS4phSTEsKYYlxbCkGJYUw5JiWFIMS4phSTEsKYYlxbCkGJYUw5JiWFIMS4phSTEsKYYlxbCkGJYUw5JiWFIMS4phSTEsKYYlxbCkGJYUw5JiWFIMS4phSTEsKYYlxbCkGJYUw5JiWFIMS4phSTEsKYYlxbCkGJYUw5JiWFIMS4phSTEsKYYlxbCkGJYUw5JiWFIMS4phSTEsKYYlxbCkGJYUw5JiWFIMS4phSTEsKYYlxbCkGJYUw5JiWFIMS4phSTEsKYYlxbCkGJYUw5JiWFIMS4phSTEsKYYlxbCkGJYUw5JiWFIMS4phSTEsKYYlxbCkGJYUw5JiWFIMS4phSTEsKYYlxbCkGJYUw5JiWFIMS4phSTEsKYYlxbCkGJYUw5JiWFIMS4phCZm52U4FOCAVGHQAAAAASUVORK5CYII=\",\"body\":[{\"type\":\"Container\",\"items\":[{\"type\":\"ColumnSet\",\"columns\":[{\"type\":\"Column\",\"width\":\"stretch\",\"items\":[{\"type\":\"ColumnSet\",\"columns\":[{\"type\":\"Column\",\"width\":\"auto\",\"items\":[{\"type\":\"Image\",\"id\":\"icon\",\"size\":\"small\",\"url\":\"data:image/svg+xml;base64,PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz4KPCEtLSBHZW5lcmF0b3I6IEFk%0D%0Ab2JlIElsbHVzdHJhdG9yIDIzLjAuMiwgU1ZHIEV4cG9ydCBQbHVnLUluIC4gU1ZHIFZlcnNpb246%0D%0AIDYuMDAgQnVpbGQgMCkgIC0tPgo8c3ZnIHZlcnNpb249IjEuMSIgaWQ9IkxheWVyXzEiIHhtbG5z%0D%0APSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyIgeG1sbnM6eGxpbms9Imh0dHA6Ly93d3cudzMu%0D%0Ab3JnLzE5OTkveGxpbmsiIHg9IjBweCIgeT0iMHB4IgoJIHZpZXdCb3g9IjAgMCAyNS40IDE5LjMi%0D%0AIHN0eWxlPSJlbmFibGUtYmFja2dyb3VuZDpuZXcgMCAwIDI1LjQgMTkuMzsiIHhtbDpzcGFjZT0i%0D%0AcHJlc2VydmUiPgo8c3R5bGUgdHlwZT0idGV4dC9jc3MiPgoJLnN0MHtmaWxsOiNGRkZGRkY7fQo8%0D%0AL3N0eWxlPgo8dGl0bGU+cGxhdGZvcm1zQXNzZXQgMzZxdWVzaXRvbnM8L3RpdGxlPgo8cGF0aCBj%0D%0AbGFzcz0ic3QwIiBkPSJNMi4yLDIuNkw0LjgsMGwxLjEsMS4xTDIuMiw0LjlMMCwyLjdsMS4xLTEu%0D%0AMUwyLjIsMi42eiBNMi4yLDcuNGwyLjYtMi42bDEuMSwxLjFMMi4yLDkuN0wwLDcuNWwxLjEtMS4x%0D%0ACglMMi4yLDcuNHogTTIuMiwxMi4ybDIuNi0yLjZsMS4xLDEuMWwtMy44LDMuOEwwLDEyLjNsMS4x%0D%0ALTEuMUwyLjIsMTIuMnogTTIuMiwxN2wyLjYtMi42bDEuMSwxLjFsLTMuOCwzLjhMMCwxNy4xTDEu%0D%0AMSwxNkwyLjIsMTd6CgkgTTcuOCwxLjZoMTcuNnYxLjZINy44VjEuNnogTTcuOCw4VjYuNGgxNy42%0D%0AVjhMNy44LDh6IE03LjgsMTIuOHYtMS42aDE3LjZ2MS42SDcuOHogTTcuOCwxNy41VjE2aDE3LjZ2%0D%0AMS42TDcuOCwxNy41eiIvPgo8L3N2Zz4K\",\"horizontalAlignment\":\"center\",\"width\":\"35px\",\"height\":\"35px\"}]},{\"type\":\"Column\",\"width\":\"stretch\",\"items\":[{\"type\":\"TextBlock\",\"id\":\"title\",\"size\":\"large\",\"weight\":\"bolder\",\"color\":\"light\",\"text\":\"Your To Do List\"}],\"verticalContentAlignment\":\"Center\"}]},{\"type\":\"TextBlock\",\"size\":\"medium\",\"weight\":\"bolder\",\"color\":\"light\",\"text\":\"3 items\",\"maxLines\":1}],\"backgroundImage\":\"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAmwAAADVCAIAAABlrTvmAAAACXBIWXMAABYlAAAWJQFJUiTwAAAG0mlUWHRYTUw6Y29tLmFkb2JlLnhtcAAAAAAAPD94cGFja2V0IGJlZ2luPSLvu78iIGlkPSJXNU0wTXBDZWhpSHpyZVN6TlRjemtjOWQiPz4gPHg6eG1wbWV0YSB4bWxuczp4PSJhZG9iZTpuczptZXRhLyIgeDp4bXB0az0iQWRvYmUgWE1QIENvcmUgNS42LWMxNDUgNzkuMTYzNDk5LCAyMDE4LzA4LzEzLTE2OjQwOjIyICAgICAgICAiPiA8cmRmOlJERiB4bWxuczpyZGY9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkvMDIvMjItcmRmLXN5bnRheC1ucyMiPiA8cmRmOkRlc2NyaXB0aW9uIHJkZjphYm91dD0iIiB4bWxuczp4bXA9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC8iIHhtbG5zOmRjPSJodHRwOi8vcHVybC5vcmcvZGMvZWxlbWVudHMvMS4xLyIgeG1sbnM6cGhvdG9zaG9wPSJodHRwOi8vbnMuYWRvYmUuY29tL3Bob3Rvc2hvcC8xLjAvIiB4bWxuczp4bXBNTT0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL21tLyIgeG1sbnM6c3RFdnQ9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZUV2ZW50IyIgeG1wOkNyZWF0b3JUb29sPSJBZG9iZSBQaG90b3Nob3AgQ0MgMjAxOSAoTWFjaW50b3NoKSIgeG1wOkNyZWF0ZURhdGU9IjIwMTktMDMtMjJUMTc6MDM6NDAtMDc6MDAiIHhtcDpNb2RpZnlEYXRlPSIyMDE5LTAzLTIyVDE3OjE2OjU2LTA3OjAwIiB4bXA6TWV0YWRhdGFEYXRlPSIyMDE5LTAzLTIyVDE3OjE2OjU2LTA3OjAwIiBkYzpmb3JtYXQ9ImltYWdlL3BuZyIgcGhvdG9zaG9wOkNvbG9yTW9kZT0iMyIgcGhvdG9zaG9wOklDQ1Byb2ZpbGU9InNSR0IgSUVDNjE5NjYtMi4xIiB4bXBNTTpJbnN0YW5jZUlEPSJ4bXAuaWlkOjY0ZGY4YjdiLTM2MTYtNDRkNy04MTI3LTgyNzk4NmUyODk3ZSIgeG1wTU06RG9jdW1lbnRJRD0iYWRvYmU6ZG9jaWQ6cGhvdG9zaG9wOmExNDU0NmE3LWZhODMtMjc0Mi1hNWU0LWIxMzYzNjQ2NWU5NyIgeG1wTU06T3JpZ2luYWxEb2N1bWVudElEPSJ4bXAuZGlkOjhlMDlmYmE5LTE2ZjktNGZiNC05MzdhLThkOTYzMGMxOTQyNiI+IDx4bXBNTTpIaXN0b3J5PiA8cmRmOlNlcT4gPHJkZjpsaSBzdEV2dDphY3Rpb249ImNyZWF0ZWQiIHN0RXZ0Omluc3RhbmNlSUQ9InhtcC5paWQ6OGUwOWZiYTktMTZmOS00ZmI0LTkzN2EtOGQ5NjMwYzE5NDI2IiBzdEV2dDp3aGVuPSIyMDE5LTAzLTIyVDE3OjAzOjQwLTA3OjAwIiBzdEV2dDpzb2Z0d2FyZUFnZW50PSJBZG9iZSBQaG90b3Nob3AgQ0MgMjAxOSAoTWFjaW50b3NoKSIvPiA8cmRmOmxpIHN0RXZ0OmFjdGlvbj0ic2F2ZWQiIHN0RXZ0Omluc3RhbmNlSUQ9InhtcC5paWQ6Yzc5ODg2ZTQtZTg3Zi00MmMzLWI5ZjYtN2FkMDg3YTlkOTg4IiBzdEV2dDp3aGVuPSIyMDE5LTAzLTIyVDE3OjE2OjU2LTA3OjAwIiBzdEV2dDpzb2Z0d2FyZUFnZW50PSJBZG9iZSBQaG90b3Nob3AgQ0MgMjAxOSAoTWFjaW50b3NoKSIgc3RFdnQ6Y2hhbmdlZD0iLyIvPiA8cmRmOmxpIHN0RXZ0OmFjdGlvbj0ic2F2ZWQiIHN0RXZ0Omluc3RhbmNlSUQ9InhtcC5paWQ6NjRkZjhiN2ItMzYxNi00NGQ3LTgxMjctODI3OTg2ZTI4OTdlIiBzdEV2dDp3aGVuPSIyMDE5LTAzLTIyVDE3OjE2OjU2LTA3OjAwIiBzdEV2dDpzb2Z0d2FyZUFnZW50PSJBZG9iZSBQaG90b3Nob3AgQ0MgMjAxOSAoTWFjaW50b3NoKSIgc3RFdnQ6Y2hhbmdlZD0iLyIvPiA8L3JkZjpTZXE+IDwveG1wTU06SGlzdG9yeT4gPC9yZGY6RGVzY3JpcHRpb24+IDwvcmRmOlJERj4gPC94OnhtcG1ldGE+IDw/eHBhY2tldCBl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\"}]}]},{\"type\":\"Container\",\"id\":\"items\",\"items\":[{\"type\":\"Container\",\"items\":[{\"type\":\"ColumnSet\",\"columns\":[{\"type\":\"Column\",\"width\":\"5\",\"items\":[{\"type\":\"Image\",\"size\":\"stretch\",\"url\":\"data:image/svg+xml;base64,PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz4KPCEtLSBHZW5lcmF0b3I6IEFk%0D%0Ab2JlIElsbHVzdHJhdG9yIDIzLjAuMiwgU1ZHIEV4cG9ydCBQbHVnLUluIC4gU1ZHIFZlcnNpb246%0D%0AIDYuMDAgQnVpbGQgMCkgIC0tPgo8c3ZnIHZlcnNpb249IjEuMSIgYmFzZVByb2ZpbGU9InRpbnki%0D%0AIGlkPSJMYXllcl8xIiB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHhtbG5zOnhs%0D%0AaW5rPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5L3hsaW5rIgoJIHg9IjBweCIgeT0iMHB4IiB2aWV3%0D%0AQm94PSIwIDAgMjQgMjQiIHhtbDpzcGFjZT0icHJlc2VydmUiPgo8cGF0aCBmaWxsPSIjNzY3Njc2%0D%0AIiBkPSJNMjQsMHYyNEgwVjBIMjR6IE0yMi41LDEuNWgtMjF2MjFoMjFWMS41eiIvPgo8L3N2Zz4K\",\"horizontalAlignment\":\"center\",\"height\":\"18px\"}],\"verticalContentAlignment\":\"Center\"},{\"type\":\"Column\",\"width\":\"95\",\"items\":[{\"type\":\"TextBlock\",\"size\":\"medium\",\"color\":\"dark\",\"text\":\"Yes.\"}]}]}]},{\"type\":\"Container\",\"items\":[{\"type\":\"ColumnSet\",\"columns\":[{\"type\":\"Column\",\"width\":\"5\",\"items\":[{\"type\":\"Image\",\"size\":\"stretch\",\"url\":\"data:image/svg+xml;base64,PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz4KPCEtLSBHZW5lcmF0b3I6IEFk%0D%0Ab2JlIElsbHVzdHJhdG9yIDIzLjAuMiwgU1ZHIEV4cG9ydCBQbHVnLUluIC4gU1ZHIFZlcnNpb246%0D%0AIDYuMDAgQnVpbGQgMCkgIC0tPgo8c3ZnIHZlcnNpb249IjEuMSIgYmFzZVByb2ZpbGU9InRpbnki%0D%0AIGlkPSJMYXllcl8xIiB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHhtbG5zOnhs%0D%0AaW5rPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5L3hsaW5rIgoJIHg9IjBweCIgeT0iMHB4IiB2aWV3%0D%0AQm94PSIwIDAgMjQgMjQiIHhtbDpzcGFjZT0icHJlc2VydmUiPgo8cGF0aCBmaWxsPSIjNzY3Njc2%0D%0AIiBkPSJNMjQsMHYyNEgwVjBIMjR6IE0yMi41LDEuNWgtMjF2MjFoMjFWMS41eiIvPgo8L3N2Zz4K\",\"horizontalAlignment\":\"center\",\"height\":\"18px\"}],\"verticalContentAlignment\":\"Center\"},{\"type\":\"Column\",\"width\":\"95\",\"items\":[{\"type\":\"TextBlock\",\"size\":\"medium\",\"color\":\"dark\",\"text\":\"conference\"}]}]}]},{\"type\":\"Container\",\"items\":[{\"type\":\"ColumnSet\",\"columns\":[{\"type\":\"Column\",\"width\":\"5\",\"items\":[{\"type\":\"Image\",\"size\":\"stretch\",\"url\":\"data:image/svg+xml;base64,PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz4KPCEtLSBHZW5lcmF0b3I6IEFk%0D%0Ab2JlIElsbHVzdHJhdG9yIDIzLjAuMiwgU1ZHIEV4cG9ydCBQbHVnLUluIC4gU1ZHIFZlcnNpb246%0D%0AIDYuMDAgQnVpbGQgMCkgIC0tPgo8c3ZnIHZlcnNpb249IjEuMSIgYmFzZVByb2ZpbGU9InRpbnki%0D%0AIGlkPSJMYXllcl8xIiB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHhtbG5zOnhs%0D%0AaW5rPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5L3hsaW5rIgoJIHg9IjBweCIgeT0iMHB4IiB2aWV3%0D%0AQm94PSIwIDAgMjQgMjQiIHhtbDpzcGFjZT0icHJlc2VydmUiPgo8cGF0aCBmaWxsPSIjNzY3Njc2%0D%0AIiBkPSJNMjQsMHYyNEgwVjBIMjR6IE0yMi41LDEuNWgtMjF2MjFoMjFWMS41eiIvPgo8L3N2Zz4K\",\"horizontalAlignment\":\"center\",\"height\":\"18px\"}],\"verticalContentAlignment\":\"Center\"},{\"type\":\"Column\",\"width\":\"95\",\"items\":[{\"type\":\"TextBlock\",\"size\":\"medium\",\"color\":\"dark\",\"text\":\"shoppling\"}]}]}]}]},{\"type\":\"Container\",\"items\":[{\"type\":\"TextBlock\",\"size\":\"small\",\"color\":\"dark\",\"text\":\"Powered by **Microsoft Graph**\",\"horizontalAlignment\":\"right\"}],\"separator\":true}]}";
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };

            return adaptiveCardAttachment;
        }

        #endregion
    }
}
