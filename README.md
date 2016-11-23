# Persimmon Test Runner for .NET Core

[![NuGet Status](http://img.shields.io/nuget/v/dotnet-test-persimmon.svg)](https://www.nuget.org/packages/dotnet-test-persimmon/)
[![Build status](https://ci.appveyor.com/api/projects/status/fqufmt8jc07ax9hc/branch/master?svg=true)](https://ci.appveyor.com/project/pocketberserker/dotnet-test-persimmon/branch/master)
[![Build Status](https://travis-ci.org/persimmon-projects/dotnet-test-persimmon.svg?branch=master)](https://travis-ci.org/persimmon-projects/dotnet-test-persimmon)

dotnet-test-persimmon is the unit test runner for .NET Core for running unit tests with Persimmon 2.

## Usage

Add dependency `dotnet-test-persimmon` and `"testRunner": "persimmon"`.

```json
{
  "dependencies": {
    "Persimmon": "2.0.1-beta5",
    "dotnet-test-persimmon": "1.0.0-beta1"
  },
  "testRunner": "nunit",
  "frameworks": {
    "netcoreapp1.0": {
      "imports": "portable-net45+win8",
      "dependencies": {
        "Microsoft.NETCore.App": {
          "version": "1.0.0-*",
          "type": "platform"
        }
      }
    }
  }
}
```

And run your tests using the Visual Studio Test Explorer, or by running dotnet test from the command line.

```sh
dotnet restore

# Run the tests in the current directory
dotnet test

# Run the tests in a different directory
dotnet test test/PersimmonWithDotNetCore.Test
```

