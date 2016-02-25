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

    [<Fact>]
    let ``defines a variable for the program files folder`` () =
        let simulationOfMsi =
            InstallationPackage
            |> copyright "Acme Inc."
            |> installFile "FsInst.dll" into ProgramFiles
            |> msi "programFilesFolderSimulation.msi"
            |> FsInst.Simulation.Msi.simulate
        
        simulationOfMsi.FileSystem.InstallationDrive/ProgramFiles/"FsInst.dll" |> should equal (InstalledFile("FsInst.dll"))

        File.Delete("programFilesFolderSimulation.msi")

    [<Fact>]
    let ``the manufacturer variable can be used to create a folder`` () =
        let simulationOfMsi =
            InstallationPackage
            |> copyright "Acme Inc"
            |> installFile "FsInst.dll" into (ProgramFiles/Manufacturer)
            |> msi "manufacturerSimulation.msi"
            |> FsInst.Simulation.Msi.simulate

        let simulatedFolder = simulationOfMsi.FileSystem.InstallationDrive/ProgramFiles/"Acme Inc"

        match simulatedFolder with
        | InstalledFolder f -> f.Name |> should equal "Acme Inc"
        | _ -> failwith "Acme Inc is no folder"

        File.Delete("manufacturerSimulation.msi")

    [<Fact>]
    let ``the product variable can be used to create a folder`` () =
        let simulationOfMsi =
            InstallationPackage
            |> copyright "Acme Inc"
            |> installing
                { Product with
                    Name = "Foobar 1.0" }
                using Installer
            |> installFile "FsInst.dll" into (ProgramFiles/Manufacturer/Product)
            |> msi "productSimulation.msi"
            |> FsInst.Simulation.Msi.simulate

        let simulatedFolder = simulationOfMsi.FileSystem.InstallationDrive/ProgramFiles/"Acme Inc"/"Foobar 1.0"

        match simulatedFolder with
        | InstalledFolder f -> f.Name |> should equal "Foobar 1.0"
        | _ -> failwith "Foobar 1.0 is no folder"
