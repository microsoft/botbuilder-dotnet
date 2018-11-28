# Description

Basic NetCore C# project to do basic validations previous the build/publish event. It consists in two project, `TaskBuilder` project, which implements a way to validate if a specific folder contains `.bot` files, and the  `TaskTester` project, which implements the `TaskBuilder` project in a way that, if it validation fails, the `TaskTester` project build will fail.

# How to run it

1) Open command line in project's root folder `botbuilder-dotnet\libraries\Microsoft.Bot.PublishValidation`
2) Run the next command to build the `TaskBuilder` project
  ```
  dotnet build TaskBuilder\TaskBuilder.csproj
  ```
3) Run the next command to build the `TaskTester` project
  ```
  dotnet build TaskTester\taskTester.csproj
  ```
  If the project `TaskTester` has a `.bot` file in its directory, its build will succeed, but if it hasn't this file, its build will fail.
