#r @"packages/build/FAKE/tools/FakeLib.dll"
open Fake
open Fake.Git
open Fake.ReleaseNotesHelper
open System
open System.IO

let project = "dotnet-test-persimmon"

let outDir = "bin"

let gitOwner = "persimmon-projects"
let gitHome = "https://github.com/" + gitOwner

let gitName = "dotnet-test-persimmon"

let gitRaw = environVarOrDefault "gitRaw" "https://raw.github.com/persimmon-projects"

let release = LoadReleaseNotes "RELEASE_NOTES.md"

Target "Clean" (fun _ ->
  CleanDirs ["bin"]
)

Target "Build" (fun _ ->
  DotNetCli.Restore id

  DotNetCli.Build (fun p -> { p with Project = "./src/dotnet-test-persimmon" })
  DotNetCli.Build (fun p -> { p with Project = "./test/dotnet-test-persimmon.test" })
  DotNetCli.Build (fun p -> { p with Project = "./test/dotnet-test-persimmon.test.runner" })
)

Target "RunTests" (fun _ ->
  DotNetCli.RunCommand id "run -p ./test/dotnet-test-persimmon.test.runner"
)

Target "SetVersionInProjectJSON" (fun _ ->
  !! "./src/**/project.json"
  |> Seq.iter (DotNetCli.SetVersionInProjectJson release.NugetVersion)
)

Target "NuGet" (fun _ ->
  DotNetCli.Pack (fun p ->
    {
      p with
        OutputPath = outDir
        Project = "./src/dotnet-test-persimmon"
    }
  )
)

Target "PublishNuget" (fun _ ->
  Paket.Push(fun p ->
    { p with
        WorkingDir = outDir })
)

#load "paket-files/build/fsharp/FAKE/modules/Octokit/Octokit.fsx"
open Octokit

Target "Release" (fun _ ->
  StageAll ""
  Git.Commit.Commit "" (sprintf "Bump version to %s" release.NugetVersion)
  Branches.push ""

  Branches.tag "" release.NugetVersion
  Branches.pushTag "" "origin" release.NugetVersion

  // release on github
  createClient (getBuildParamOrDefault "github-user" "") (getBuildParamOrDefault "github-pw" "")
  |> createDraft gitOwner gitName release.NugetVersion (release.SemVer.PreRelease <> None) release.Notes
  |> releaseDraft
  |> Async.RunSynchronously
)

Target "BuildPackage" DoNothing

Target "All" DoNothing

"Clean"
  ==> "SetVersionInProjectJSON"
  ==> "Build"
  ==> "RunTests"
  ==> "All"

"All"
  ==> "NuGet"
  ==> "BuildPackage"

"BuildPackage"
  ==> "PublishNuget"
  ==> "Release"

RunTargetOrDefault "All"
