# AlarmBot

This is a full sample of AlarmBot which uses recognizers, templates and organizes the conversations into classes called topics.  

> Topics are a concept in this sample, not in the SDK. The SDK allows you to organize your code in whatever way you want.

This sample explores a MVVM style of organizing your code around conversations.

* __Models__ - the objects you are editing
* __Topics__ - aka the busines logic for editing the model
* __TopicViews__ - class which defines output templates which are the language and data driven output to the user.

I guess you could call this model __MTTV__ pattern? :) 

## Topic

The sample defines an interface ITopic which gives it the basic ability to manage topics.  

* **ITopic.StartTopic()** -Called when a topic is created.
* **ITopic.ContinueTopic()** - Called for every activity as context.State.Conversation.ActivePrompt points to it.
* **ITopic.ResumeTopic()** - Called whenever someone has interrupted your topic so you can resume your topic cleanly. 

The ActivityStateManager() middleware adds automatic persistence of the **context.state.conversation** and **context.state.user** objects.  

This samples sets **context.state.Conversation.ActiveTopic** to point to an instance of the active ITopic class,
which is automatically serialized on activity. It then uses this property to manage the active topic and switch between topics.

The topics which are used in this sample are:

* **DefaultTopic** - The default topic the bot starts out with. This guy will start other topics as the user requests them.
* **AddAlarmTopic** - A topic which manages collecting the information for adding an alarm
* **ShowAlarmsTopic** - A topic which shows the alarms which have been added
* **DeleteAlarmTopic** - A Topic which finds an alarm and confirms to delete it.



## TopicViews

Normally samples intermix the text of responses with conversational logic.  While useful for "hello world" 
samples, we know from experience that it is really useful to split UI such as text and cards away from the 
logic that manipulates state. 

This sample does this by defining **TopicViews** which is a folder of classes which implement templates for
generating the responses for the bot.

The Topic expresses responses with context.ReplyWith() calls, which invoke the templating system that the 
bot builder manages.  Templates are literally function maps which are looked up by language -> TemplateId -> Template Function.  
The template function then takes in the context and a data object to data bind to. 

Not only does this allow you to localize and parameterize the output of your bot, it also isolates your 
bots logic from the library/tooling which is used to generate the responses. AND because the templating 
lookup mechanism is driven by the middleware pipeline, host bots which call into 3rd party can replace
the text or card created by any component, essentially giving you the ability to control the "styling"
of the bot's output in a standard way.

Each topic has a corresponding Topic View registered as middleware

The topics which are used in this sample are:

* **DefaultTopicView** - Templates for rendering the output of the DefaultTopic.
* **AddAlarmTopicView** - Templates for rendering the output of the DefaultTopic.
* **ShowAlarmsTopicView** - Templates for rendering the output of the DefaultTopic.
* **DeleteAlarmTopicView** - Templates for rendering the output of the DefaultTopic.







 