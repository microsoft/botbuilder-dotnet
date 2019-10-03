Sample: Root pass through and Child Echo

This is the simplest use case

A RootBot is just an IBot that forwards all the activities it receives straight to a childbot using the connector and gets an 3 echo messages back.

Rootbot has an interceptor handler that shows the messages is getting.

The childbot sends an end conversation right after it sends the echo.