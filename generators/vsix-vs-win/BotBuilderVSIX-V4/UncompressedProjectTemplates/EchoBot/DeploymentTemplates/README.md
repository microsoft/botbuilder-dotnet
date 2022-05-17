# ARM Template Parameters

These are the parameters needed to deploy a bot to Azure using ARM templates. They can be set in line when running `az deployment create` command or in the parameters.json file.
<br/>
<br/>

## Common


| Parameters                               | Description                                                                                                                                                                            |
| :--------------------------------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| appId                                    | Active Directory App ID or User-Assigned Managed Identity Client ID, set as MicrosoftAppId in the Web App's Application Settings                                                       |
| appSecret                                | Active Directory App Password, set as MicrosoftAppPassword in the Web App's Application Settings. Required for MultiTenant and SingleTenant app types. Defaults to \"\"                |
| appType                                  | Type of Bot Authentication. set as MicrosoftAppType in the Web App's Application Settings. Allowed values are: MultiTenant, SingleTenant, UserAssignedMSI. Defaults to \"MultiTenant\" |
| botId                                    | The globally unique and immutable bot ID. Also used to configure the displayName of the bot, which is mutable                                                                          |
| botSku                                   | The pricing tier of the Bot Service Registration. Acceptable values are F0 and S1                                                                                                      |
| newAppServicePlanName                    | The name of the new App Service Plan                                                                                                                                                   |
| newAppServicePlanSku                     | The SKU of the App Service Plan. Defaults to Standard values                                                                                                                           |
| existingAppServicePlan                   | Name of the existing App Service Plan used to create the Web App for the bot                                                                                                           |
| newWebAppName                            | The globally unique name of the Web App. Defaults to the value passed in for \"botId\"                                                                                                 |
| tenantId                                 | The Azure AD Tenant ID to use as part of the Bot's Authentication. Only used for SingleTenant and UserAssignedMSI app types. Defaults to \"Subscription Tenant ID\"                    |
| existingUserAssignedMSIName              | The User-Assigned Managed Identity Resource used for the Bot's Authentication. Defaults to \"\"                                                                                        |
| existingUserAssignedMSIResourceGroupName | The User-Assigned Managed Identity Resource Group used for the Bot's Authentication. Defaults to \"\"                                                                                  |

<br/>

## New resource group (new-rg-parameters.json)


| Parameters                | Description                                  |
| :------------------------ | :------------------------------------------- |
| groupLocation             | Specifies the location of the Resource Group |
| groupName                 | Specifies the name of the Resource Group     |
| newAppServicePlanLocation | The location of the App Service Plan         |

<br/>

## Existing resource group (preexisting-rg-parameters.json)


| Parameters             | Description                                  |
| :--------------------- | :------------------------------------------- |
| appServicePlanLocation | Specifies the location of the Resource Group |
                                                                                 