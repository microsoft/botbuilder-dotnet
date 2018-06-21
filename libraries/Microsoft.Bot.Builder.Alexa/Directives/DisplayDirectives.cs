using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Bot.Builder.Alexa.Directives
{
    public class DisplayDirective : IAlexaDirective
    {
        public string Type => "Display.RenderTemplate";
        public IRenderTemplate Template { get; set; }
    }

    public interface IRenderTemplate
    {
        string Type { get; }
        string Token { get; set; }
    }

    public interface IBodyTemplate : IRenderTemplate
    {
    }

    public interface IListTemplate : IRenderTemplate
    {
        List<ListItem> ListItems { get; set; }
    }

    public class DisplayRenderBodyTemplate1 : IBodyTemplate
    {
        public string Type => "BodyTemplate1";
        public string Token { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public BackButtonVisibility BackButton { get; set; }
        public Image BackgroundImage { get; set; }
        public string Title { get; set; }
        public TextContent TextContent { get; set; }
    }

    public class DisplayRenderBodyTemplate2 : IBodyTemplate
    {
        public string Type => "BodyTemplate2";
        public string Token { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public BackButtonVisibility BackButton { get; set; }
        public Image BackgroundImage { get; set; }
        public string Title { get; set; }
        public Image Image { get; set; }
        public TextContent TextContent { get; set; }
    }

    public class DisplayRenderBodyTemplate3 : IBodyTemplate
    {
        public string Type => "BodyTemplate3";
        public string Token { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public BackButtonVisibility BackButton { get; set; }
        public Image BackgroundImage { get; set; }
        public string Title { get; set; }
        public Image Image { get; set; }
        public TextContent TextContent { get; set; }
    }

    public class DisplayRenderBodyTemplate6 : IBodyTemplate
    {
        public string Type => "BodyTemplate6";
        [JsonConverter(typeof(StringEnumConverter))]
        public BackButtonVisibility BackButton { get; set; }
        public Image BackgroundImage { get; set; }
        public TextContent TextContent { get; set; }
        public string Token { get; set; }
    }

    public class DisplayRenderBodyTemplate7 : IBodyTemplate
    {
        public string Type => "BodyTemplate7";
        public string Token { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public BackButtonVisibility BackButton { get; set; }
        public string Title { get; set; }
        public Image BackgroundImage { get; set; }
        public Image Image { get; set; }
    }

    public class DisplayRenderListTemplate1 : IListTemplate
    {
        public string Type => "ListTemplate1";
        public string Token { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public BackButtonVisibility BackButton { get; set; }
        public string Title { get; set; }
        public Image BackgroundImage { get; set; }
        public List<ListItem> ListItems { get; set; }
    }

    public class DisplayRenderListTemplate2 : IListTemplate
    {
        public string Type => "ListTemplate2";
        public string Token { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public BackButtonVisibility BackButton { get; set; }
        public string Title { get; set; }
        public Image BackgroundImage { get; set; }
        public List<ListItem> ListItems { get; set; }
    }

    public class ListItem
    {
        public string Token { get; set; }
        public Image Image { get; set; }
        public TextContent TextContent { get; set; }
    }

    public enum BackButtonVisibility
    {
        VISIBLE,
        HIDDEN
    }

    public class Image
    {
        public string ContentDescription { get; set; }
        public ImageSource[] Sources { get; set; }
    }

    public class ImageSource
    {
        public string Url { get; set; }
        public int? WidthPixels { get; set; }
        public int? HeightPixels { get; set; }
        public string Size { get; set; }
    }

    public class TextContent
    {
        public InnerTextContent PrimaryText { get; set; }
        public InnerTextContent SecondaryText { get; set; }
        public InnerTextContent TertiaryText { get; set; }
    }

    public class InnerTextContent
    {
        public string Text { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public TextContentType Type { get; set; }
    }
}
