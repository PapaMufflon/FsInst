#r @"packages/FAKE/tools/FakeLib.dll"
open System
open System.IO
open Fake
open Fake.Git
open Fake.ReportGeneratorHelper
open Fake.OpenCoverHelper
open Fake.Testing

let buildDir = "./build"
let solutionFile = "./src/FsInst.sln"

Target "Clean" (fun _ ->
    CleanDir buildDir
)

Target "Build" (fun _ ->
    !! solutionFile
    |> MSBuildRelease buildDir "Rebuild"
    |> ignore
)

Target "Test" (fun _ ->
    !! (buildDir + @"\*Facts.dll") 
      |> xUnit (fun p -> { p with ToolPath = "./packages/xunit.runner.console/tools/xunit.console.exe" })
)

Target "Default" (fun _ ->
    trace "Have fun using FsInst!!!"
)

"Clean"
  ==> "Build"
  ==> "Test"
  ==> "Default"

RunTargetOrDefault "Default"