namespace FsInst.Simulation

module FileSystem =
    open System.IO
    open FsUnit.Xunit
    open FsInst.Core
    open Xunit
    open FsInst

    [<Fact>]
    let ``creates a folder if there is an item in it`` () =
        let Test = InstallationDrive/"Test"

        let simulationOfMsi =
            InstallationPackage
            |> copyright "Acme Inc."
            |> createFolder Test
            |> installFile "FsInst.dll" into Test
            |> msi "createFolderSimulation.msi"
            |> FsInst.Simulation.Msi.simulate
            
        let msiFolder = simulationOfMsi.FileSystem.InstallationDrive/"Test"
        
        match msiFolder with
        | InstalledFolder f -> f.Name |> should equal Test.Name
        | _ -> failwith "Test is not a folder."

        File.Delete("createFolderSimulation.msi")

    [<Fact>]
    let ``can have multiple files in one folder`` () =
        let Test = InstallationDrive/"Test"

        let simulationOfMsi = 
            InstallationPackage
            |> copyright "Acme Inc."
            |> createFolder Test
            |> installFiles ["FsInst.dll"; "FsInst.Facts.dll"] into Test
            |> msi "multipleFilesSimulation.msi"
            |> FsInst.Simulation.Msi.simulate

        simulationOfMsi.FileSystem.InstallationDrive/"Test"/"FsInst.dll" |> should equal (InstalledFile("FsInst.dll"))
        simulationOfMsi.FileSystem.InstallationDrive/"Test"/"FsInst.Facts.dll" |> should equal (InstalledFile("FsInst.Facts.dll"))

        File.Delete("multipleFilesSimulation.msi")

    [<Fact>]
    let ``can have hierarchical folders`` () =
        let folder = InstallationDrive/"Test"
        let subFolder = folder/"SubFolder"

        let simulationOfMsi = 
            InstallationPackage
            |> copyright "Acme Inc."
            |> createFolder subFolder
            |> installFile "FsInst.dll" into folder
            |> installFile "FsInst.Facts.dll" into subFolder
            |> msi "hierarchicalFoldersSimulation.msi"
            |> FsInst.Simulation.Msi.simulate

        simulationOfMsi.FileSystem.InstallationDrive/"Test"/"FsInst.dll" |> should equal (InstalledFile("FsInst.dll"))
        simulationOfMsi.FileSystem.InstallationDrive/"Test"/"SubFolder"/"FsInst.Facts.dll" |> should equal (InstalledFile("FsInst.Facts.dll"))

        File.Delete("hierarchicalFoldersSimulation.msi")