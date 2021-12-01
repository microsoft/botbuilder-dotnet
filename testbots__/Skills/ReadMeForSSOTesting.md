This guide documents how to configure and test SSO by using the parent and child bot projects.
## SetUp
- Go to [App registrations page on Azure Portal](https://ms.portal.azure.com/#blade/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/RegisteredApps)
- You need to create 2 AAD apps (one for the parent bot and one for the skill)
### Parent bot AAD app
- Click "New Registration"
- Enter name, set "supported account types" as Single Tenant, Redirect URI as https://token.botframework.com/.auth/web/redirect
- Go to "Expose an API". Click "Add a Scope". Enter a scope name (like "scope1"), set "who can consent" to Admins and users, display name, description and click "Add Scope" . Copy the value of the scope that you just added (should be like "api://{clientId}/scopename")
- Go to "Manifest" tab and set `accessTokenAcceptedVersion` to 2
- Go to "Certificates and secrets" , click "new client secret" and store the generated secret.

### Configuring the Parent Bot Channel Registration
- Create a new Bot Channel Registration. You can leave the messaging endpoint empty and later fill an ngrok endpoint for it.
- Go to settings tab, click "Add Setting" and enter a name, set Service Provider to "Azure Active Directory v2".
- Fill in ClientId, TenantId from the parent bot AAD app you created (look at the overview tab for these values)
- Fill in the secret from the parent bot AAD app.
- Fill in the scope that you copied earlier ("api://{clientId}/scopename") and enter it for "Scopes" on the OAuth connection. Click Save.

### Child bot AAD app and BCR
- Follow the steps in the [documentation](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-authentication?view=azure-bot-service-4.0&tabs=csharp) for creating an Azure AD v2 app and filling those values in a Bot Channel Registration.
- Go to the Azure AD app that you created in the step above.
- Go to "Manifest" tab and set `accessTokenAcceptedVersion` to 2
- Go to "Expose an API". Click "Add a client application". Enter the clientId of the parent bot AAD app.
- Go to "Expose an API". Click "Add a Scope". Enter a scope name (like "scope1"), set "who can consent" to Admins and users, display name, description and click "Add Scope" . Copy the value of the scope that you just added (should be like "api://{clientId}/scopename")
- Go back to your BCR that you created for the child bot. Go to Auth Connections in the settings blade and click on the auth connection that you created earlier. For the "Token Exchange Uri" , set the scope value that you copied in the step above.

### Running and Testing
- Configure appid, passoword and connection names in the appsettings.json files for both parent and child bots. Run both the projects.
- Set up ngrok to expose the url for the parent bot. (Child bot can run just locally, as long as it's on the same machine as the parent bot.)
- Configure the messaging endpoint for the parent bot channel registration with the ngrok url and go to "test in webchat" tab.
- Run the following commands and look at the outputs
    - login - shows an oauth card. Click the oauth card to login into the parent bot.
    - type "login" again - shows your JWT token.
    - skill login - should do nothing (no oauth card shown).
    - type "skill login" again - should show you a message from the skill with the token.
    - logout - should give a message that you have been logged out from the parent bot.
    - skill logout - should give a message that you have been logged out from the child bot.
