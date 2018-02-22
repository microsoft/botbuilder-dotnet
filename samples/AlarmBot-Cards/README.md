# AlarmBot-Cards

This is a full sample of AlarmBot which uses recognizers, templates and organizes the conversations into classes called topics.  
Unlike AlarmBot, AlarmBot-Cards uses Adaptive Cards to build an identical experience.

> Topics are a concept in this sample, not in the SDK. The SDK allows you to organize your code in whatever way you want.

This sample explores a MVVM style of organizing your code around conversations.

* __Models__ - the objects you are editing
* __Topics__ - aka the busines logic for editing the model
* __Responses__ - class which defines output to the user

I guess you could call this model __MTTV__ pattern? :) 

## Topic

The sample defines an interface ITopic which gives it the basic ability to manage topics.  

* **ITopic.StartTopic()** -Called when a topic is created.
* **ITopic.ContinueTopic()** - Called for every activity as context.State.Conversation.ActivePrompt points to it.
* **ITopic.ResumeTopic()** - Called whenever someone has interrupted your topic so you can resume your topic cleanly. 

The UserStateMangerMiddleware() and ConversationStateManagerMiddleware() adds automatic persistence of the 
**context.state.ConversationProperties** and **context.state.UserProperties** objects.  

This samples sets **context.state.Conversation.ActiveTopic** to point to an instance of the active ITopic class,
which is automatically serialized on activity. It then uses this property to manage the active topic and switch between topics.

The topics which are used in this sample are:

* **DefaultTopic** - The default topic the bot starts out with. This class will start other topics as the user requests them.
* **AddAlarmTopic** - A topic which manages collecting the information for adding an alarm
* **ShowAlarmsTopic** - A topic which shows the alarms which have been added
* **DeleteAlarmTopic** - A Topic which finds an alarm and confirms to delete it.



