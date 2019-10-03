Sample: Root without dialogs and child with dialogs

The Child bot has several dialogs.

The root bot doesn't have any dialogs but it does implement an intercept handler delegate that echoes the acivities it receives from the child.

Once it starts a dialog on the child, if keeps forwarding messages until it receives an end of conversation.