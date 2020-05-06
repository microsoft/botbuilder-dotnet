using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Middleware for adding an object to or registering a service with the current turn context.
    /// </summary>
    /// <typeparam name="T">The type of object or service to add.</typeparam>
    public class RegisterClassMiddleware<T> : IMiddleware
        where T : class
    {
        private string key;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterClassMiddleware{T}"/> class.
        /// </summary>
        /// <param name="service">The object or service to add.</param>
        public RegisterClassMiddleware(T service)
        {
            this.Service = service;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterClassMiddleware{T}"/> class.
        /// </summary>
        /// <param name="service">The object or service to add.</param>
        /// <param name="key">optional key for service object in turn state (default is instance.GetType().FullName).</param>
        public RegisterClassMiddleware(T service, string key)
        {
            this.Service = service;
            this.key = key;
        }

        /// <summary>
        /// Gets or sets the object or service to add to the turn context.
        /// </summary>
        /// <value>
        /// The object or service to add.
        /// </value>
        public T Service { get; set; }

        /// <summary>
        /// Adds the associated object or service to the current turn context.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="nextTurn">The delegate to call to continue the bot middleware pipeline.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <seealso cref="ITurnContext"/>
        /// <seealso cref="Bot.Schema.IActivity"/>
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate nextTurn, CancellationToken cancellationToken)
        {
            // Register service
            if (this.key != null)
            {
                turnContext.TurnState.Add(this.key, this.Service);
            }
            else
            {
                turnContext.TurnState.Add(this.Service);
            }

            await nextTurn(cancellationToken).ConfigureAwait(false);
        }
    }
}
