# Overview

Below are some useful CLI Commands

## Create a .NET 9 class library

```bash
dotnet new classlib -n Rag.SemanticKernel.RestClient -f net9.0
```

## Add the existing or new project to the solution

```bash
dotnet sln Rag.SemanticKernel.App.sln add Rag.SemanticKernel.RestClient/Rag.SemanticKernel.RestClient.csproj
```

## To add the library to your main project

```bash
dotnet sln add MyLibrary/MyLibrary.csproj
```

## Then reference it from another project

```bash
dotnet add MyMainApp reference MyLibrary
```

