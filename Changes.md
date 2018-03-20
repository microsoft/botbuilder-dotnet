## v.Next
* Changed BotContext to no longer be dynamic, and add IBotContext.Get<>() IBotContext.Set() for middleware to register 
* renamed UserStateManagerMiddleware, ConversationManagerMiddleware to UserState<> and ConversationState<> 
consolidating using generics with POCO objects and shared base class
* Added BotContextWrapper so developer can easily create custom BotContext with their own helpers/methods


## 4.0.0-alpha201802027a
* Milan release 


