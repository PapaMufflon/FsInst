#r @"packages/FAKE/tools/FakeLib.dll"
open System
open System.IO
open Fake
open Fake.Git
open Fake.ReportGeneratorHelper
open Fake.OpenCoverHelper

let buildDir = "./build"
let solutionFile = "./src/FsInst.sln"

Target "Clean" (fun _ ->
    CleanDir buildDir
)

Target "Build" (fun _ ->
    !! solutionFile
    |> MSBuildRelease "" "Rebuild"
    |> ignore
)

Target "Default" (fun _ ->
    trace "Have fun building FsInst!!!"
)

"Clean"
  ==> "Build"
  ==> "Default"

RunTargetOrDefault "Default"