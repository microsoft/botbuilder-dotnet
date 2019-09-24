### Description

As we are now consuming recognizers-text through the BotBuilder-DotNet repository, we are organizing its results in this folder to maintain the changes in one place. 

This way the user will have all the differences between versions and will let them know what to expect. 

## Changes made
The user will find the Utterance Changes divided by entity type. In every entity type subfolder the changes are organized in .json files.

##  Changes
### V1.2.6
**DateTime - Recognize**

New recognized inputs:
 - [ash wednesday](https://github.com/microsoft/botbuilder-dotnet/blob/662e13ecd4ae8835c02b15e43bf6960a0abae88b/recognizers-text/Utterance%20Changes/DateTime/recognize-differences.json#L3)
 - [halloween](https://github.com/microsoft/botbuilder-dotnet/blob/662e13ecd4ae8835c02b15e43bf6960a0abae88b/recognizers-text/Utterance%20Changes/DateTime/recognize-differences.json#L102)

Changed results:
 - [black friday](https://github.com/microsoft/botbuilder-dotnet/blob/662e13ecd4ae8835c02b15e43bf6960a0abae88b/recognizers-text/Utterance%20Changes/DateTime/recognize-differences.json#L26)
 - [easter](https://github.com/microsoft/botbuilder-dotnet/blob/662e13ecd4ae8835c02b15e43bf6960a0abae88b/recognizers-text/Utterance%20Changes/DateTime/recognize-differences.json#L67)
 - [maundy thursday](https://github.com/microsoft/botbuilder-dotnet/blob/662e13ecd4ae8835c02b15e43bf6960a0abae88b/recognizers-text/Utterance%20Changes/DateTime/recognize-differences.json#L125)
 - [palm sunday](https://github.com/microsoft/botbuilder-dotnet/blob/662e13ecd4ae8835c02b15e43bf6960a0abae88b/recognizers-text/Utterance%20Changes/DateTime/recognize-differences.json#L166)

**DateTime - Prompt**

New recognized inputs:
 - [untill friday](https://github.com/microsoft/botbuilder-dotnet/blob/662e13ecd4ae8835c02b15e43bf6960a0abae88b/recognizers-text/Utterance%20Changes/DateTime/datetime-prompt-differences.json#L3)
 - [monday untill friday](https://github.com/microsoft/botbuilder-dotnet/blob/662e13ecd4ae8835c02b15e43bf6960a0abae88b/recognizers-text/Utterance%20Changes/DateTime/datetime-prompt-differences.json#L44)
 - [this past friday](https://github.com/microsoft/botbuilder-dotnet/blob/662e13ecd4ae8835c02b15e43bf6960a0abae88b/recognizers-text/Utterance%20Changes/DateTime/datetime-prompt-differences.json#L85)
 - [past friday](https://github.com/microsoft/botbuilder-dotnet/blob/662e13ecd4ae8835c02b15e43bf6960a0abae88b/recognizers-text/Utterance%20Changes/DateTime/datetime-prompt-differences.json#L114)

**Ordinal**

Removed inputs:
 - [the second to last](https://github.com/microsoft/botbuilder-dotnet/blob/662e13ecd4ae8835c02b15e43bf6960a0abae88b/recognizers-text/Utterance%20Changes/Ordinal/choices-model-differences.json#L3)


**Number**

Changed inputs:
 - [half](https://github.com/microsoft/botbuilder-dotnet/blob/662e13ecd4ae8835c02b15e43bf6960a0abae88b/recognizers-text/Utterance%20Changes/Number/double-prompt-words-difference.json#L3)
 - [half nelson](https://github.com/microsoft/botbuilder-dotnet/blob/662e13ecd4ae8835c02b15e43bf6960a0abae88b/recognizers-text/Utterance%20Changes/Number/double-prompt-words-difference.json#L18)
 - [half seas over](https://github.com/microsoft/botbuilder-dotnet/blob/662e13ecd4ae8835c02b15e43bf6960a0abae88b/recognizers-text/Utterance%20Changes/Number/double-prompt-words-difference.json#L33)

