using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.Bot.Builder.Middleware.MiddlewareSet;

namespace Microsoft.Bot.Builder.Middleware
{
    /// <summary>
    /// Deploying new versions of your bot more often then not should have little
    /// to no impact on the current conversations you're having with a user. Sometimes, however, a change 
    /// to your bots conversation logic can result in the user getting into a stuck state that can only be
    /// fixed by their conversation state being deleted.
    ///
    /// This middleware lets you track a version number for the conversations your bot is having so that
    /// you can automatically delete the conversation state anytime a major version number difference is
    /// detected.Example:
    ///
    ///
    /// bot.Use(new ConversationVersionMiddleware(Assembly.GetAssembly(typeof(MessagesController)), (context, version, next) =>
    /// {
    ///     context.Reply("I'm sorry, my service was upgraded let's start over.");
    ///     context.State.Conversation = new ConversationState();
    ///     return Task.CompletedTask;
    ///  }));
    /// </summary>
    public class ConversationVersionMiddleware : IReceiveActivity
    {

        public const string CONVERSATION_VERSION = "$CONVERSATION_VERSION";

        private readonly int _majorVersion;
        private readonly Func<IBotContext, int, NextDelegate, Task> _handler;

        /// <summary>
        /// This constructor will get the Entry Assembly and set the major version from that.
        /// </summary>
        /// <param name="handler">handler on what to do if an error comes in</param>
        public ConversationVersionMiddleware(Func<IBotContext, int, NextDelegate, Task> handler)
        {
            _majorVersion = Assembly.GetEntryAssembly().GetName().Version.Major;
            _handler = handler;
        }

        /// <summary>
        /// This constructor will set the major version given as an integer.
        /// </summary>
        /// <param name="majorVersion">major version to compare against</param>
        /// <param name="handler">handler on what to do if an error comes in</param>
        public ConversationVersionMiddleware(int majorVersion, Func<IBotContext, int, NextDelegate, Task> handler)
        {
            _majorVersion = majorVersion;
            _handler = handler;
        }

        /// <summary>
        /// Allows an assembly to pull the major version
        /// </summary>
        /// <param name="assembly">assembly to get the major version from</param>
        /// <param name="handler">handler on what to do if an error comes in</param>
        public ConversationVersionMiddleware(Assembly assembly, Func<IBotContext, int, NextDelegate, Task> handler)
        {
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            _majorVersion = fileVersionInfo.FileMajorPart;
            _handler = handler;
        }

        /// <summary>
        /// This method will check if the major version has been set and if it has compare to make sure the current version is not greater. 
        /// If it is it will call the handler set in the constructor.
        /// </summary>
        /// <param name="context">bot context</param>
        /// <param name="next">next delegate of middleware</param>
        /// <returns>a task upon completion</returns>
        public async Task ReceiveActivity(IBotContext context, NextDelegate next)
        {
            int? conversationVersion = (int?) context.State.Conversation[CONVERSATION_VERSION];
            if(!conversationVersion.HasValue)
            {
                context.State.Conversation[CONVERSATION_VERSION] = _majorVersion;
            }
            else if(conversationVersion.Value != _majorVersion)
            {
                await _handler.Invoke(context, conversationVersion.Value, () => {
                    context.State.Conversation[CONVERSATION_VERSION] = _majorVersion;
                    return next();
                });
            }

            await next();
        }
    }
}
