# Alexa Adapter for Bot Builder .NET

This adapter can be used to allow your bot to act as an endpoint for an Amazon Alexa Skill.  Incoming Alexa Skill requests are transformed, by the adapter, into Bot Builder Activties and then when your bot responds, the adapter transforms the outgoing Activity into an Alexa response.

## Adding the adapter and skills endpoint to your bot

Currently there are integration libraries available for WebApi and .NET Core available for the adapter.

### WebApi

When implementing your bot using WebApi, the integration layer for Alexa works the same as the default for Bot Framework.  The only difference being in your BotConfig file under the App_Start folder you call MapAlexaBotFramework instead;

```cs
    public class BotConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapAlexaBotFramework(botConfig => { });
        }
    }
``` 

### .NET Core

TO BE COMPLETED

## Default Alexa Request to Activity mapping

When an incoming request is receieved, the activity sent to your bot is comprised of the following values;

* **Channel ID** : "alexa"
* **Recipient Channel Account** : Id = Application Id from the Alexa request, Name = "skill"
* **From Channel Account** : Id = User Id from the Alexa request, Name = "user"
* **Conversation Account** : Id = "{Alexa request Application Id}:{Alexa Request User Id}"
* **Type** : Request Type from the Alexa request. e.g. IntentRequest, LaunchRequest or SessionEndedRequest
* **Id** : Request Id from the Alexa request
* **Timestamp** : Timestamp from the Alexa request
* **Locale** : Locale from the Alexa request

For incoming requests of type IntentRequest we also set the following properties on the Activity

* **Value** : DialogState value from the Alexa request

For incoming requests of type SessionEndedRequest we also set the following properties on the Activity

* **Code** : Reason value from the Alexa request
* **Value** : Error value from the Alexa request

The entire body of the Alexa request is placed into the Activity as Channel Data, of type AlexaRequestBody.

## Default Activity to Alexa Response mapping

The Alexa adapter will send a response to the Alexa skill request if the outgoing activity is of type MessageActivity or EndOfConversation activity.

If the actvity you send from your bot is of type EndOfConversation then a response is sent indicating that the session should be ended, by setting the the ShouldEndSession flag on the ALexa response to true.

If the activity type you send from your bot is of type MessageActivity the following values are mapped to an Alexa response object;

* **OutputSpeech Type** : Set to 'SSML' if activity.Speak is not null. Set to 'PlainText' if the activity.Text property is populated but the activity.Speak property is not.
* **OutputSpeech SSML** : Populated using the value of activity.Speak if it is not null.
* **OutputSpeech Text** : Populated using the value of activity.Text if it is not null.

* **ShouldEndSession** : Defaults to false. However, setting the InputHint property on the activity to InputHint.IgnoringInput will set this value to true and end the session.

### Cards

The Alexa Adapter supports sending Bot Framework cards of type HeroCard, ThumbnailCard and SigninCard as part of your replies to the Alexa skill request.

* **HeroCard and ThumbnailCard** : 

 * Alexa Card Small Image URL = The first image in the Images collection on the Hero / Thumbnail card
 * Alexa Card Large Image URL = If a second image exists in the Images collection on the Hero / Thumbnail card this will be used. If no second image exists then this is null.
 * Alexa Card Title = Title property of the Hero / Thumbnail card
 * Alexa Card Content = Text property on the Hero / Thumbnail card

***Note: You should ensure that the images you use on your HeroCard / Thumbnail cards are the correct expected size for Alexa Skills responses.***

* **SigninCard** : If a SignInCard is attached to your outgoing activity, this will be mapped as a LinkAccount card in the Alexa response.

## Extension Methods

### Session Attributes

Alexa Skills use Session Attributes on the request / response objects to allow for values to be persisted accross turns of a conversation.  When an incoming Alexa request is receieved we place the Session Attributes on the request into the Services collection on the TurnContext.  We then provide an extension method on the context to allow you to add / update / remove items on the Session Attributes list. Calling the extension method AlexaSessionAttributes returns an object of type Dictionary<string, string>. If you wanted to add an item to the Session Attributes collection you could do the following;

```cs 
    context.AlexaSessionAttributes.Add("NewItemKey","New Item Value");
```

### Progressive Responses

Alexa Skills allow only a single primary response for each request.  However, if your bot will be running some form of long running activity (such as a lookup to a 3rd party API) you are able to send the user a holding response using the Alexa Progressive Responses API, before sending your final response.

To send a Progressive Response we have provided an extension method on the TurnContext called AlexaSendProgressiveResponse, which takes a string parameter which is the text you wish to be spoken back to the user. e.g.

```cs
    context.AlexaSendProgressiveResponse("Hold on, I will just check that for you.");
```

The extension method will get the right values from the incoming request to determine the correct API endpoint / access token and send your Progressive response for you.  The extension method will also return a HttpResponseMessage which will provide information as to if the Progressive Response was send successfully or if there was any kind of error.

***Note: Alexa Skills allow you to send up to 5 progressive responses on each turn.  You should manage and check the number of Progressive Responses you are sending as the Bot Builder SDK does not check this.*** 


### Get entire Alexa Request Body

We have provided an extension method to allow you to get the original Alexa request body, which we store on the ChannelData property of the Activity sent to your bot, as a strongly typed object of type AlexaRequestBody.  To get the request just call the extension method as below;

```cs
    AlexaRequestBody request = context.GetAlexaRequestBody();
```

***Note: If you call this extension method when the incoming Activity is not from an Alexa skill then the extension method will simply return null.*** 


## Alexa Middleware

TO BE COMPLETED