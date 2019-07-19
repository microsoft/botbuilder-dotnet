# User Agents in Bot Framework

## Problem Being Solved
The Bot Framework team needs to know more about the shape of Bots that are running. The shape of bots includes data such as the host operating system, the middleware components that are being used, and the Cognitive Services that are integrated into the Bot. 

This data is needed to effectively prioritize features investments, as the process today is largely guesswork. 

Data today around usage is implied from Nuget / NPM package downloads and GitHub activity. 

## High Level Design. 
This is a solved problem in the Web + Browser space. Our solution is to copy what modern browsers do for User Agent strings so that accurate data can be extracted. 

User-Agents are defined in [RFC 2616](https://tools.ietf.org/html/rfc2616#section-14.43). The key text is extracted here:

> The User-Agent request-header field contains information about the
user agent originating the request. This is for statistical purposes,
the tracing of protocol violations, and automated recognition of user
agents for the sake of tailoring responses to avoid particular user
agent limitations. User agents SHOULD include this field with
requests. The field can contain multiple product tokens (section 3.8)
and comments identifying the agent and any subproducts which form a
significant part of the user agent. By convention, the product tokens
are listed in order of their significance for identifying the
application. 
```
    User-Agent: CERN-LineMode/2.15 libwww/2.17b3
```

Chrome User-Agent String:
```
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.79 Safari/537.
```
Edge User-Agent String:
```
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64; ServiceUI 13) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.140 Safari/537.36 Edge/17.17134
```
Internet Explorer User-Agent String:
```
User-Agent: Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko
```

Chrome Extensions modify the User-Agent string through code:
```js
chrome.webRequest.onBeforeSendHeaders.addListener(
  function(details) {
    for (var i = 0; i < details.requestHeaders.length; ++i) {
      if (details.requestHeaders[i].name === 'User-Agent') {
        details.requestHeaders[i].value = details.requestHeaders[i].value + ' OurUAToken/1.0';
      }
      break;
    }
    return { requestHeaders: details.requestHeaders };
  });
```


## Current User Agent String
V4 BotBuilder SDK does the following:
```
"user-agent": "Microsoft-BotFramework/3.1 (BotBuilder .Net/4.0.0.0)"
```

## Proposed Bot User Agent 
Add the Following:
1. SDK Version. The Bot Framework SDK Version MUST be added to the user-agent string. 
2. SDK Host. The SDK Host, .Net Core, JVM, Node version, MUST be added to the user-agent string. 
3. Operating System. The OS that hosts the Bot SHOULD be added to the user-agent string. 
4. Azure Bot Service Version (if applicable)
5. Active Middleware Components. A RFC 2616 compatible list of Middleware SHOULD be added. The format is:
    ```
    (Middleware1/version; Middleware2/version; Middleware3/version)
    ```
6. Active Storage Components. The storage providers being used SHOULD be added to the user-agent string. 
    ```
    (CosmosDBStorageProvider/version; AzureTables/version;)
    ```
7. LUIS. The usage of LUIS SHOULD be added to the user-agent string. 
    ```
    (LUIS/APIversion)
    ```
8. QnA. The usage of QnA Maker SHOULD be added to the user-agent string. 
    ```
    (QnAMaker/APIVersion)
    ```
9. Classic. Usage of the "Classic / Compatibility" layers SHOULD be added to the user-agent string.
    ```
    classic/version
    ```

A "Fully Realized" user-agent string may look like:
```
User-Agent: BotBuilder/4.0.0.0 (netcore2; abs/2.65 Windows NT 10.0; Win64; x64) Middleware(TSLogger; CatchExceptions; PII) Storage(CosmosDB) LUIS(v2.0) Qna(v3.0)
```
## Implementation
Implementation is platform specific. 

For static (build-time) strings, attributes SHOULD be put on Middleware to define the names that are used by the User Agent strings, and the BotAdapter can scan for those attributes. C#, TS, JS, Java, and Python all support this. 

For more dynamic strings (O/S, LUIS API Version, QnA Maker API Version), relevant libraries will need to look for a shared list of "Active" components and update accordingly. 



