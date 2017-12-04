using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Templates;
using System.Text;
using AlarmBot.Models;

namespace AlarmBot.Topics
{
    /// <summary>
    /// Class around topic of adding an alarm
    /// </summary>
    public class AddAlarmTopic : ITopic
    {
        /// <summary>
        /// enumeration of states of the converation
        /// </summary>
        public enum TopicStates
        {
            // initial state
            Started,

            // we asked for title
            TitlePrompt,
            
            // we asked for time
            TimePrompt,
            
            // we asked for confirmation to cancel
            CancelConfirmation,
            
            // we asked for confirmation to add
            AddConfirmation,
            
            // we asked for confirmation to show help instead of allowing help as the answer
            HelpConfirmation
        };

        public const string TopicName = "AddAlarmTopic";

        // template ids
        public const string STARTTOPIC = "AddAlarmTopic.StartTopic";
        public const string RESUMETOPIC = "AddAlarmTopic.ResumeTopic";
        public const string HELP = "AddAlarmTopic.Help";
        public const string CONFUSED = "AddAlarmTopic.Confusion";
        public const string CANCELPROMPT = "AddAlarmTopic.Cancelation";
        public const string TOPICCANCELED = "AddAlarmTopic.TopicCanceled";
        public const string CANCELCANCELED = "AddAlarmTopic.CancelCanceled";
        public const string CANCELREPROMPT = "AddAlarmTopic.CancelReprompt";
        public const string TITLEPROMPT = "AddAlarmTopic.TitlePrompt";
        public const string TITLEVALIDATIONPROMPT = "AddAlarmTopic.TitleValidationPrompt";
        public const string TIMEPROMPT = "AddAlarmTopic.TimePrompt";
        public const string TIMEVALIDATIONPROMPT = "AddAlarmTopic.TimeValidationPrompt";
        public const string ADDEDALARM = "AddAlarmTopic.AddedAlarm";
        public const string ADDCONFIRMATION = "AddAlarmTopic.AddConfirmation";

        /// <summary>
        /// Standard english language alarm description
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string AlarmDescription(Alarm alarm)
        {
            StringBuilder sb = new StringBuilder();
            if (!String.IsNullOrWhiteSpace(alarm.Title))
                sb.AppendLine($"* Title: {alarm.Title}");
            if (!String.IsNullOrWhiteSpace(alarm.Time))
                sb.AppendLine($"* Time: {alarm.Time}");
            return sb.ToString();
        }

        /// <summary>
        /// table of language functions which render output in various languages
        /// </summary>
        public static TemplateDictionary ReplyTemplates = new TemplateDictionary
        {
            // Default templates
            ["default"] = new TemplateIdMap
                {
                    { STARTTOPIC, (context, data) => $"Ok, let's add an alarm." },
                    { HELP, (context, data) => $"I am working with you to create an alarm.  To do that I need to know the title and time.\n\n{AlarmDescription(data)}"},
                    { CONFUSED, (context, data) => $"I am sorry, I didn't understand: {context.Request.Text}." },
                    { CANCELPROMPT, (context, data) => $"# Cancel alarm?\n\nDid you want to cancel the alarm?\n\n{AlarmDescription(data)}\n\n(Yes or No)" },
                    { CANCELREPROMPT, (context, data) => $"# Cancel alarm?\n\nPlease answer the question with a \"yes\" or \"no\" reply. Did you want to cancel the alarm?\n\n{AlarmDescription(data)}\n\n" },
                    { TOPICCANCELED, (context, data) => $"OK, I have canceled this alarm." },
                    { TIMEPROMPT, (context, data) => $"# Adding alarm\n\n{AlarmDescription(data)}\n\nWhat time would you like to set the alarm for?" },
                    { TITLEPROMPT, (context, data)=> $"# Adding alarm\n\n{AlarmDescription(data)}\n\nWhat would you like to call your alarm ?" },
                    { ADDCONFIRMATION, (context, data)=> $"# Adding Alarm\n\n{AlarmDescription(data)}\n\nDo you want to save this alarm?" },
                    { ADDEDALARM, (context, data)=> $"# Alarm Added\n\n{AlarmDescription(data)}." }
                }
        };

        public AddAlarmTopic()
        {
        }

        /// <summary>
        /// Alarm object representing the information being gathered by the conversation before it is committed
        /// </summary>
        public Alarm Alarm { get; set; }

        /// <summary>
        /// Current state of the topic conversation
        /// </summary>
        public TopicStates TopicState { get; set; } = TopicStates.Started;

        /// <summary>
        /// Called when the add alarm topic is started
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task<bool> StartTopic(BotContext context)
        {
            this.Alarm = new Alarm()
            {
                // initialize from intent entities
                Title = context.TopIntent?.Entities.Where(entity => entity.GroupName == "AlarmTitle")
                    .Select(entity => entity.ValueAs<string>()).FirstOrDefault(),

                // initialize from intent entities
                Time = context.TopIntent?.Entities.Where(entity => entity.GroupName == "AlarmTime")
                    .Select(entity => entity.ValueAs<string>()).FirstOrDefault()
            };

            return PromptForMissingData(context);
        }


        /// <summary>
        /// we call for every turn while the topic is still active
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<bool> ContinueTopic(BotContext context)
        {
            // for messages
            if (context.Request.Type == ActivityTypes.Message)
            {
                switch (context.TopIntent?.Name)
                {
                    case "showAlarms":
                        // allow show alarm to interrupt, but it's one turn, so we show the data without changing the topic
                        await new ShowAlarmsTopic().StartTopic(context);
                        return await this.PromptForMissingData(context);

                    case "help":
                        // show contextual help 
                        context.ReplyWith(HELP, this.Alarm);
                        return await this.PromptForMissingData(context);

                    case "cancel":
                        // prompt to cancel
                        context.ReplyWith(CANCELPROMPT, this.Alarm);
                        this.TopicState = TopicStates.CancelConfirmation;
                        return true;

                    default:
                        string utterance = (context.Request.Text ?? "").Trim();

                        // we ar eusing TopicState to remember what we last asked
                        switch (this.TopicState)
                        {
                            case TopicStates.TitlePrompt:
                                this.Alarm.Title = utterance;
                                return await this.PromptForMissingData(context);

                            case TopicStates.TimePrompt:
                                this.Alarm.Time = utterance;
                                return await this.PromptForMissingData(context);

                            case TopicStates.CancelConfirmation:
                                if (utterance.Trim() == "y" || utterance.Contains("yes"))
                                {
                                    // End current topic
                                    context.ReplyWith(TOPICCANCELED, this.Alarm);
                                    return false;
                                }
                                else if (utterance.Trim() == "n" || utterance.Contains("no"))
                                {
                                    // Re-prompt user for current field.
                                    context.ReplyWith(CANCELCANCELED, this.Alarm);
                                    return await this.PromptForMissingData(context);
                                }
                                else
                                {
                                    // prompt again to confirm the cancelation
                                    context.ReplyWith(CANCELREPROMPT, this.Alarm);
                                    return true;
                                }

                            case TopicStates.AddConfirmation:
                                if (utterance.Trim() == "y" || utterance.Contains("yes"))
                                {
                                    // Save alarm
                                    var alarms = (List<Alarm>)context.State.User[UserProperties.ALARMS];
                                    if (alarms == null)
                                    {
                                        alarms = new List<Alarm>();
                                        context.State.User[UserProperties.ALARMS] = alarms;
                                    }
                                    alarms.Add(this.Alarm);

                                    context.ReplyWith(ADDEDALARM, this.Alarm);

                                    // end topic
                                    return false;
                                }
                                else if (utterance.Trim() == "n" || utterance.Contains("no"))
                                {
                                    // Re-prompt user for current field.
                                    //context.ReplyWith(CORRECTIONPROMPT, this.Alarm);
                                    return true;
                                }
                                else
                                {
                                    // figure out if utterance is title or 
                                    //  context.ReplyWith(CANCELREPROMPT, this.Alarm);
                                    return true;
                                }

                            default:
                                return await this.PromptForMissingData(context);
                        }
                }
            }
            return true;
        }

        /// <summary>
        /// Called when this topic is resumed after being interrupted
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task<bool> ResumeTopic(BotContext context)
        {
            // simply prompt again based on our state
            return this.PromptForMissingData(context);
        }

        /// <summary>
        /// Shared method to get missing information
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task<bool> PromptForMissingData(BotContext context)
        {
            // If we don't have a title (or if its too long), prompt the user to get it.
            if (String.IsNullOrWhiteSpace(this.Alarm.Title))
            {
                this.TopicState = TopicStates.TitlePrompt;
                context.ReplyWith(TITLEPROMPT, this.Alarm);
                return true;
            }
            // if title exists but is not valid, then provide feedback and prompt again
            else if (this.Alarm.Title.Length < 1 || this.Alarm.Title.Length > 100)
            {
                this.Alarm.Title = null;
                this.TopicState = TopicStates.TitlePrompt;
                context.ReplyWith(TITLEVALIDATIONPROMPT, this.Alarm);
                return true;
            }

            // If we don't have a time, prompt the user to get it.
            if (String.IsNullOrWhiteSpace(this.Alarm.Time))
            {
                this.TopicState = TopicStates.TimePrompt;
                context.ReplyWith(TIMEPROMPT, this.Alarm);
                return true;
            }

            // ask for confirmation that we want to add it
            context.ReplyWith(ADDCONFIRMATION, this.Alarm);
            this.TopicState = TopicStates.AddConfirmation;
            return true;
        }
    }
}
