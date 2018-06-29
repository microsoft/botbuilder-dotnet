# BotBuilder Open Source Engagement

## Goals
The BotBuilder SDK is an open-source project on GitHub that aspires to:

* Build a thriving community of developers who build bots. 
* Align 1st and 3rd party developers
* Provide the tools and processes needed to support our development community. 
* Leverage existing best-practices wherever they are found. 
* Keep all platform implementations of the BotBuilder in Sync.

## Non-Goals:
* TBD

# Exemplar Projects
From an open-source community perspective, the BotBuilder is using several projects as a North Star. 
1. [Typescript](https://github.com/Microsoft/TypeScript)
2. [VS Code](https://github.com/Microsoft/vscode)
3. [.Net Core Fx](https://github.com/dotnet/corefx)
4. [.Net Core CLR](https://github.com/dotnet/coreclr)
5. [ASP.Net](https://github.com/aspnet/Home)
6. [TensorFlow](https://github.com/tensorflow/tensorflow)
7. [Bootstrap](https://github.com/twbs/bootstrap)

# Builds
Many popular project have daily builds. This includes Typescript, [TensorFlow](https://pypi.org/project/tf-nightly/), [VS Code](https://code.visualstudio.com/insiders/)and [.Net Core FX](https://dotnet.myget.org/gallery/dotnet-core). 

Nightly builds enabled features, bug fixes, and external pull requests to more rapidly reach the interested audience. The BotBuilder SDK enables nightly builds via MyGet. To facilitate nightly builds, the team has enabled [botbuilder.myget.org](https://botbuilder.myget.org) and has created a daily build gallery for each of:
1. [C# SDK v4](https://botbuilder.myget.org/gallery/botbuilder-v4-dotnet-daily)
2. [Node SDK v4](https://botbuilder.myget.org/gallery/botbuilder-v4-js-daily)
3. [Java SDK v4](https://botbuilder.myget.org/gallery/botbuilder-v4-java-daily)

Following the Python Best practices as defined by TensorFlow, the daily builds for the Python SDK will be here:
[https://pypi.org/project/botbuilder-nightly/](https://pypi.org/project/botbuilder-nightly/)

# External Pull Requests

We love external pull requests. For submitted PR's we require:
1. Follow the contribution guidelines
2. Signed the relevant CLA
3. Follow coding standards for relevant platform
4. Unit Tests all pass. 
5. New code has all necessary and relevant Unit Tests.
6. Changed code has corresponding unit tests to validate changes. 

The BotBuilder team is following the [.Net Core FX/CLR contribution guidelines](https://github.com/dotnet/coreclr/blob/master/Documentation/project-docs/contributing.md). 

Each PR will be assigned to a BotBuilder team member for review and reviewed against the relevant guidelines. 

# Multi-Platform Pull Requests
As a general rule, external 3rd party pull requests are expected to be in a particular language (C#, JS, etc). If the BotBuilder team believes there is sufficient value in the PR then the team will work with the contributor to port the PR to other platforms. In most cases, this will involve the BotBuilder team porting the PR to other platforms. 

Note: Generally, bug fixes are not expected to be multi-platform. They will be dealt with on an as-needed basis. 

# Stand-up
The ASP.Net team does a [weekly public standup](https://live.asp.net/). At this time, the BotBuilder team doesn't think this is needed. Any voice/video discussions that need to happen can be scheduled, but most discussion should take place via GitHub. 

# External Speaking Engagements
Requests for presentations at local groups should be submitted via GitHub Issues. Each request will be separately considered, and community members may choose to engage. As the community grows we hope this becomes an active part of our broad community.

# Repo Layout and Usage

In Github, we are going with 5 repos. These break down as:
1. BotBuilder. This is the "root" repo were overviews, docs, and links to other repos are found. By design, this is the "landing page" for Bot developers. There should be no source-code found in this repo. 
2. BotBuilder-dotnet. This is the C# Repo. Contained here is the V3 and V4 C# Code, along with tests, relevant samples, and issues. Links to the daily preview build, the public build, and other platform relevant materials are found here.
3. BotBuilder-js.  This is the JavaScript Repo. Contained here is the V3 and V4 Javascript Code, along with tests, relevant samples, and issues. Links to the daily preview build for Node, the public build for Node, and other platform relevant materials are found here. 
4. BotBuilder-Java. This is the Java Repo. Contained here is the V4 Java Code, along with tests, relevant samples, and issues. Links to the daily preview build, the public build, and other platform relevant materials are found here.
5. BotBuilder-Python. This is the Python Repo. Contained here is the V4 Python Code, along with tests, relevant samples, and issues. Links to the daily preview build, the public build, and other platform relevant materials are found here.

By design, most Issues and discussions should happen on the platform-agnostic "BotBuilder" repo. Issues that are incorrectly filed on a platform specific repo will be closed and re-opened on the correct repo. Likewise, platform specific issues will be moved as needed. 

# Branching Strategy
The "Master" branch in each repo represents the most current codebase. In the Master Branch:
* All "new" code must be added to a branch. Any necessary rebasing against master must happen in that branch. 
* Upon completion of a PR, a member of the BotBuilder team will merge the branch into master. 
* Each time a Release is done, a new branch will be created. 

The branching strategy should be the same as other major open-source code such as typescript:
https://github.com/Microsoft/TypeScript/branches


# Issue Tags
The following tags are used across repos:

Tag | Description | Color
--- | ----------- | -----
Bug | The issue represents a bug. | Red
Approved | The issue has been reviewed by the team and accepted as an issue to be resolved | Green
Backlog | The issue has been approved by the team and added to the backlog work queue. | Yellow
on-hold | The issue is acknowledged, but not actively being worked | Orange
Docs | The issue in some way pertains to docs. Common uses are to report docs that are  incorrect, need enhancements, or generally unclear | Green
Samples | The issue is pertains to samples. Common uses are to request new samples, changes to existing samples, or questions about a samples | Coral
DCR | A Discussion thread about a potential change or new feature. | Purple
Feature:Dialogs | Issue pertains to the dialog and prompt system | Pink
Feature:Proactive | Issue relevant to proactive messaging | Dark Blue
Feature:State-Management | Issue relevant to state management in the Bot | Light Yellow






