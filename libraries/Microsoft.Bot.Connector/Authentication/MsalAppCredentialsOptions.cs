// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// Msal appCredentials options. <see cref="MsalAppCredentials"/>.
    /// Register this in startup.cs of your application along with <see cref="MsalServiceClientCredentialsFactory"/>>.
    /// <code>
    ///  // MSAL certificate authentication
    ///  services.AddTransient&lt;IConfidentialClientApplication&gt;(
    ///  serviceProvider => ConfidentialClientApplicationBuilder.Create(Configuration.GetSection("MicrosoftAppId").Value)
    ///      .WithCertificate(new X509Certificate2("pathToCertFile", "certPassword"))
    ///      .Build());
    ///      
    ///  // Configure msal app credential.
    ///  services.Configure&lt;MsalAppCredentialsOptions&gt;(options => options.SendX5c = true);
    ///  
    ///  // MSAL credential factory: regardless of secret, cert or custom auth, need to add the line below
    ///  services.AddTransient&lt;ServiceClientCredentialsFactory, Authentication.MsalServiceClientCredentialsFactory&gt;();
    ///  
    ///  // Create the Bot Framework Authentication to be used with the Bot Adapter.
    ///  services.AddSingleton&lt;BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication&gt;();
    /// </code>
    /// </summary>
    public class MsalAppCredentialsOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether
        /// the x5c claim (public key of the certificate) should be sent to
        /// the STS. Sending the x5c enables application developers to achieve easy certificate
        /// roll-over in Azure AD: this method will send the public certificate to Azure
        /// AD along with the token request, so that Azure AD can use it to validate the
        /// subject name based on a trusted issuer policy. This saves the application admin
        /// from the need to explicitly manage the certificate rollover (either via portal
        /// or PowerShell/CLI operation). For details see https://aka.ms/msal-net-sni.
        ///
        /// </summary>
        /// <value>
        /// true if the x5c should be sent. Otherwise false. The default is false.
        /// </value>
        public bool SendX5c { get; set; } = false;
    }
}
