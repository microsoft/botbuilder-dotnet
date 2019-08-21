using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Middleware for injecting a class into the turn context.
    /// </summary>
    /// <typeparam name="T">The type to register.</typeparam>
    public class RegisterClassMiddleware<T> : IMiddleware
        where T : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterClassMiddleware{T}"/> class.
        /// </summary>
        /// <param name="service">The service instance to register.</param>
        public RegisterClassMiddleware(T service)
        {
            this.Service = service;
        }

        /// <summary>
        /// Gets or sets the Service to be registered into turn context.
        /// </summary>
        /// <value>
        /// The Service to be registered into turn context.
        /// </value>
        public T Service { get; set; }

        /// <summary>
        /// registers into the turncontext.
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
            turnContext.TurnState.Add(this.Service);
            await nextTurn(cancellationToken).ConfigureAwait(false);
        }
    }
}
