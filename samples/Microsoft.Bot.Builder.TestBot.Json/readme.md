# Declarative Sample Bot

This bot demonstrates simple scenarios for declarative bots that slowly build complexity as they introduce concepts. It is recommended to follow the numeric order of the smaples to better grasp the concepts introduced.

# Run instructions

We'll introduce a smoother way to transitoin between samples, but for now here is how you test samples:

- Under Samples, there are a number of folders. Each folder contains a collection of files and subdirectories that represent a bot sample.
- Select the sample you would like to run and copy the folder name
- Open TestBot.cs, and identify the constructor
- In the constructor, replace the line below with the name / path to your selected sample. The line below illustrates how to set up for sample 8:

```
rootDialog = CognitiveLoader.Load<IDialog>(File.ReadAllText(@"Samples\Planning 8 - ExternalLanguage\main.dialog"));
```