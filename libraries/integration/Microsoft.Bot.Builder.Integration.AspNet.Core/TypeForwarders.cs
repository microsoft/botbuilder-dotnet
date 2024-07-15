// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Runtime.CompilerServices;
using Microsoft.Bot.Connector.Authentication;

// These classes were moved from Microsoft.Bot.Builder.Integration.AspNet.Core to
// Microsoft.Bot.Connector. To avoid breaking changes in any bot that could be referencing
// the classes, we are adding these TypeForwardedTo attributes.

[assembly: TypeForwardedTo(typeof(ConfigurationBotFrameworkAuthentication))]
[assembly: TypeForwardedTo(typeof(ConfigurationServiceClientCredentialFactory))]
