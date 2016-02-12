#r @"packages/FAKE/tools/FakeLib.dll"
open Fake
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

Target "SystemTest" (fun _ ->
    !! (buildDir + @"\*SystemTests.dll") 
      |> xUnit (fun p -> { p with ToolPath = "./packages/xunit.runner.console/tools/xunit.console.exe" })
)

Target "Default" (fun _ ->
    trace "Have fun using FsInst!!!"
)

"Clean"
  ==> "Build"
  ==> "Test"
  ==> "SystemTest"
  ==> "Default"

RunTargetOrDefault "Default"