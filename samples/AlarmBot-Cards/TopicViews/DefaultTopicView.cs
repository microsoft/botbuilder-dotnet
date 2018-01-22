using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Builder.Templates;

namespace AlarmBot.TopicViews
{
    public class DefaultTopicView : TemplateRendererMiddleware
    {
        public DefaultTopicView() : base( new DictionaryRenderer(ReplyTemplates))
        {

        }

        // template ids
        public const string GREETING = "DefaultTopic.StartTopic";
        public const string RESUMETOPIC = "DefaultTopic.ResumeTopic";
        public const string HELP = "DefaultTopic.Help";
        public const string CONFUSED = "DefaultTopic.Confusion";

        // template functions for rendeing responses in different a languages
        public static TemplateDictionary ReplyTemplates = new TemplateDictionary
        {
            ["default"] = new TemplateIdMap
                {
                    { GREETING, (context, data) => $"Hello, I'm the alarmbot." },
                    { HELP, (context, data) => $"I can add an alarm, show alarms or delete an alarm. " },
                    { RESUMETOPIC, (context, data) => $"What can I do for you?" },
                    { CONFUSED, (context, data) => $"I am sorry, I didn't understand that." },
                },
            ["en"] = new TemplateIdMap { },
            ["fr"] = new TemplateIdMap { }
        };


    }
}
