Sample: Call a remote dialog from a root dialog.

RootBot is based on the the CoreBotSample, the main difference is that BookingDialog is in chidlbot and invoked remotelly using skillconnector.

For that, it uses a RemoteDialog that wraps the calls to the remove dialog into skill connector. 