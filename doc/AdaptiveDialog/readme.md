# Adaptive Dialog

Pre-read: [Dialogs library][1] in Bot Builder V4 SDK.

Dialogs are a central concept in the SDK, and provide a useful way to manage a conversation with the user. Bot Builder V4 SDK offers waterfall, prompts and component dialogs as the 3 core types. 

As a quick recap 
- Prompts provide an easy way to ask user for information and evaluate their response. You can use a specific type of prompt e.g. number prompt if you are looking to collect a number from user etc. 
- Waterfall dialogs help you sequentially execute a set of steps that enable you to collect information from user. Each waterfall step is an asynchronous function. Often, waterfall steps include a series of prompts to the user to collect information in a linear order. 
- Component dialogs help build re-usable dialogs each of which have their own dialog set, and helps avoid name collisions with the dialog set that consumes it. 

Current set of dialog types offered by Bot Builder V4 SDK put you in control of managing your bot's conversations however they also require me to write a bunch of boiler plate code for sophisticated conversation modelling concepts like building a dialog dispatcher, ability handle interruptions elegantly and to build a pluggable and extensible dialog system.

The new **Adaptive dialog** and the event model simplify sophisticated conversation modelling primitives, eliminate much of the boiler plate code and helps you focus on the model of the conversation rather than the mechanics of dialog management.

See [here](./memoryRulesSteps.md) to learn more about memory, adaptive dialog constructs. 

***Adaptive dialogs*** at the core comprise of 4 main concepts - 
- _Recognizers_ help understand user input. You use recognizers to extract meaningful pieces of information from user's input. All recognizers emit events - of specific interest is the 'recognizedIntent' event that fires when the recognizer picks up an intent (or extracts entities) from given user utterance. Adaptive Dialogs support the following recognizers - 
    - RegEx recognizer
    - LUIS recognizer
    - Multi-language recogizer
- _Rules_ enable you to catch and respond to events. The broadest rule is the EventRule that allows you to catch and attach a set of steps to execute when a specific event is emitted by any sub-system. Adaptive dialogs support the following Rules - 
    - EventRule - catch and respond to a specific event. 
    - IntentRule - catch and respond to 'recognizedIntent' event emitted by a recognizer. 
    - UnknownIntentRule - is used to catch and respond to a case when a 'recognizedIntent' event was not caught and handled by any of the other rules. This is especially helpful to capture and handle cases where your dialog wishes to participate in consultation.   
- _Steps_ help put together the flow of conversation when a specific event is captured via a Rule. **_Note:_** unlike Waterfall dialog where each step is a function, each step in an Adaptive dialog is in itself a dialog. This enables adaptive dialogs by design to have a much cleaner ability to handle and deal with interruptions.  Adaptive dialogs support the following steps - 
    - Sending a response
        - SendActivity
    - Tracing and logging
        - TraceActivity
        - LogStep
    - Memory manipulation
        - SaveEntity - used to extract an entity returned by recognizer into memory.
        - EditArray
        - InitProperty
        - SetProperty - used to set a property's value in memory. See [here](../CommonExpressionLanguage) to learn more about expressions.
        - DeleteProperty
    - Conversational flow and dialog management
        - IfCondition - used to evaluate an expression. See [here](../CommonExpressionLanguage) to learn more about expressions.
        - SwitchCondition
        - EndTurn
        - BeginDialog
        - EndDialog
        - CancelAllDialog
        - ReplaceDialog
        - RepeatDialog
    - Eventing
        - EmitEvent
    - Roll your own code
        - CodeStep
        - HttpRequest
- _Inputs_ are wrappers around [prompts][2] that you can use in an adaptive dialog step to ask and collect a piece of input from user, validate and accept it into memory. Inputs include these additional features - Accepts a property to bind to off the new V3 style memory scope in V4. Performs existential check before prompting. Grounds input to the specified property if the input from user matches the type of entity expected. Accepts constraints - min, max, etc. Adaptive dialogs support the following inputs - 
    - TextInput
    - ChoiceInput
    - ConfirmInput
    - NumberInput

[1]:https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-concept-dialog?view=azure-bot-service-4.0
[2]:https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-concept-dialog?view=azure-bot-service-4.0#prompts
