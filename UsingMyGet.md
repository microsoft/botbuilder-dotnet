# Using MyGet to consume daily builds
The BotBuilder SDK daily build feed is found on [MyGet](https://botbuilder.myget.org). For .Net development, use the [dotnet daily build feed](https://botbuilder.myget.org/gallery/botbuilder-v4-dotnet-daily). 

To consume the latest daily builds of the Bot Framework, you'll need to register MyGet as a package source in Visual Studio. 

# Register Myget in Visual Studio Steps
You can register a MyGet feed the same way you register any NuGet package source by using the **Package Manager Settings** dialog.

1. In Visual Studio go to Tools > Library Package Manager > Package Manager Settings in the Visual Studio menu.

![image](https://user-images.githubusercontent.com/11055362/45443270-066e7380-b679-11e8-9279-725fa9690d35.png)

2. In the Options dialog, choose **Package Sources** then click the green + button.  This will create a new package source entry.  Fill in the **Name** field, and add the v4 feed url as the **Source**: 

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`https://botbuilder.myget.org/F/botbuilder-v4-dotnet-daily/api/v3/index.json`
![image](https://user-images.githubusercontent.com/11055362/45443422-78df5380-b679-11e8-947b-4e9530379ca8.png)

3. Lastly, click **Update** then **OK**

This package source will now be available when managing packages for a solution or project:
![image](https://user-images.githubusercontent.com/11055362/45443611-0ae75c00-b67a-11e8-8767-29a1a6ac3485.png)

# Troubleshooting
If you are experiencing 404 errors when trying to install the MyGet packages check that you set the URL correctly in step 2. The correct URL is:

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`https://botbuilder.myget.org/F/botbuilder-v4-dotnet-daily/api/v3/index.json`

.

