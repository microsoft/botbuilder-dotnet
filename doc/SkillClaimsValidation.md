# HowTo: Block all Skill Claims

Add a class inheriting from `ClaimsValidator` which overrides `ValidateClaimsAsync` and throws an exception if the claims are skill claims:
```cs
    public class NoSkillsClaimsValidator : ClaimsValidator
    {
        public override Task ValidateClaimsAsync(IList<Claim> claims)
        {
            if (SkillValidation.IsSkillClaim(claims))
            {
                throw new UnauthorizedAccessException("Invalid call from a skill.");
            }
            return Task.CompletedTask;
        }
    }
```

Update `BotFrameworkHttpAdapter` implementation, to pass the base constructor an `AuthenticationConfiguration` using the `NoSkillsClaimsValidator`:

```cs
    public class AdapterWithErrorHandler : BotFrameworkHttpAdapter
    {
        public AdapterWithErrorHandler(IConfiguration configuration, ILogger<BotFrameworkHttpAdapter> logger)
            : base(configuration: configuration, credentialProvider: new ConfigurationCredentialProvider(configuration),
                  authConfig: new AuthenticationConfiguration() { ClaimsValidator = new NoSkillsClaimsValidator() }, logger: logger)
        {
            OnTurnError = async (turnContext, exception) =>
            {
                // Log any leaked exception from the application.
                logger.LogError(exception, $"[OnTurnError] unhandled error : {exception.Message}");
                // Send a message to the user
                await turnContext.SendActivityAsync("The bot encountered an error or bug.");
                await turnContext.SendActivityAsync("To continue to run this bot, please fix the bot source code.");
                // Send a trace activity, which will be displayed in the Bot Framework Emulator
                await turnContext.TraceActivityAsync("OnTurnError Trace", exception.Message, "https://www.botframework.com/schemas/error", "TurnError");
            };
        }
    }
```