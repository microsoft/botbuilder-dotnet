Sample: Send activities with Values that can be dealt with by the child.

RootBot doesn't have dialogs and has some very rough code to invoke a send activity scenario and stay on it until it gets the completed.

* Get Weather: simple single turn call to the skill, it doesn't send any entities or receives any entities in response.
* Book Flight: starts a remove flight booking flow and displays the booking info returned from the skill if the user confirms.
* Book Flight with data: send initial data to the skill which advances the booking flow to the confirmation part. it returns the booking info if the user confirms
* SendAsIs: doesn't do much now, it sends it to the skill but the skill doesn't understand it, shows and message and it ends. It should eventually try to resolve against LUIS.

