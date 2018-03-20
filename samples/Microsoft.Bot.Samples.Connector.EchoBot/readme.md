# EchoBot Connector Sample
This is a simple sample showing how to create a bare bones bot hosted in an ASP.NET Core Controller using only the lower level bot Connector APIs from these NuGet packages:
 * Microsoft.Bot.Connector
 * Microsoft.Bot.Schema

The `MessagesController` exposes a single `HttpPost` based action method which accepts an `Activity` in the body, validates the caller has the correct security token and then utilizes a `ConnectorClient` to send an echo response.
