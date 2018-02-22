# Connector EchoBot Sample
This is a simple sample showing how to create a POST WebAPI using just 
* Microsoft.Bot.Connector nuget package
* Microsoft.Bot.Schema nuget package

In the controller it has a POST  handler which accepts an Activity as the body.  It then:
* Validates the caller has the correct security token
* Uses the connector client library to send an echo response.
