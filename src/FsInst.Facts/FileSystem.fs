namespace FsInst

module FileSystem =
    open FsUnit.Xunit
    open FsInst.Core
    open FsInst.Simulation
    open FsInst.Simulation.InstallationPackage
    open Xunit

    [<Fact>]
    let ``creates a folder if there is an item in it`` () =
        let Test = InstallationDrive/"Test"

        let installationPackage =
            InstallationPackage
            |> copyright "Acme Inc."
            |> createFolder Test
            |> installFile "FsInst.dll" into Test

        let simulation = simulate installationPackage

        let simulatedFolder = simulation.FileSystem.InstallationDrive/"Test"
        
        match simulatedFolder with
        | InstalledFolder f -> f.Name |> should equal Test.Name
        | _ -> failwith "Test is not a folder."

    [<Fact>]
    let ``creates implicitly a folder if you deploy a file in it`` () =
        let installationPackage =
            InstallationPackage
            |> copyright "Acme Inc."
            |> installFile "FsInst.dll" into "Test"

        let simulation = simulate installationPackage

        let simulatedFolder = simulation.FileSystem.InstallationDrive/"Test"

        match simulatedFolder with
        | InstalledFolder f -> f.Name |> should equal "Test"
        | _ -> failwith "Test is not a folder."

    [<Fact>]
    let ``creates implicitly a subfolder if you deploy a file in it`` () =
        let installationPackage =
            InstallationPackage
            |> copyright "Acme Inc."
            |> installFile "FsInst.dll" into (ProgramFiles/"Test")

        let simulation = simulate installationPackage

        let simulatedFolder = simulation.FileSystem.InstallationDrive/ProgramFiles/"Test"

        match simulatedFolder with
        | InstalledFolder f -> f.Name |> should equal "Test"
        | _ -> failwith "Test is not a folder."

    [<Fact>]
    let ``can have multiple files in one folder`` () =
        let Test = InstallationDrive/"Test"

        let installationPackage =
            InstallationPackage
            |> copyright "Acme Inc."
            |> createFolder Test
            |> installFiles ["FsInst.dll"; "FsInst.Facts.dll"] into Test

        let simulation = simulate installationPackage

        simulation.FileSystem.InstallationDrive/"Test"/"FsInst.dll" |> should equal (InstalledFile("FsInst.dll"))
        simulation.FileSystem.InstallationDrive/"Test"/"FsInst.Facts.dll" |> should equal (InstalledFile("FsInst.Facts.dll"))

    [<Fact>]
    let ``can have hierarchical folders`` () =
        let folder = InstallationDrive/"Test"
        let subFolder = folder/"SubFolder"

        let installationPackage =
            InstallationPackage
            |> copyright "Acme Inc."
            |> createFolder subFolder
            |> installFile "FsInst.dll" into folder
            |> installFile "FsInst.Facts.dll" into subFolder

        let simulation = simulate installationPackage

        simulation.FileSystem.InstallationDrive/"Test"/"FsInst.dll" |> should equal (InstalledFile("FsInst.dll"))
        simulation.FileSystem.InstallationDrive/"Test"/"SubFolder"/"FsInst.Facts.dll" |> should equal (InstalledFile("FsInst.Facts.dll"))

    [<Fact>]
    let ``defines a variable for the program files folder`` () =
        let installationPackage =
            InstallationPackage
            |> copyright "Acme Inc."
            |> installFile "FsInst.dll" into ProgramFiles
        
        let simulation = simulate installationPackage

        simulation.FileSystem.InstallationDrive/ProgramFiles/"FsInst.dll" |> should equal (InstalledFile("FsInst.dll"))

    [<Fact>]
    let ``the manufacturer variable can be used to create a folder`` () =
        let installationPackage =
            InstallationPackage
            |> copyright "Acme Inc"
            |> installFile "FsInst.dll" into (ProgramFiles/Manufacturer)

        let simulation = simulate installationPackage

        let simulatedFolder = simulation.FileSystem.InstallationDrive/ProgramFiles/"Acme Inc"

        match simulatedFolder with
        | InstalledFolder f -> f.Name |> should equal "Acme Inc"
        | _ -> failwith "'Acme Inc' is not a folder"

    [<Fact>]
    let ``the product variable can be used to create a folder`` () =
        let installationPackage =
            InstallationPackage
            |> copyright "Acme Inc"
            |> installing
                { Product with
                    Name = "Foobar 1.0" }
                using Installer
            |> installFile "FsInst.dll" into (ProgramFiles/Manufacturer/Product)

        let simulation = simulate installationPackage

        let simulatedFolder = simulation.FileSystem.InstallationDrive/ProgramFiles/"Acme Inc"/"Foobar 1.0"

        match simulatedFolder with
        | InstalledFolder f -> f.Name |> should equal "Foobar 1.0"
        | _ -> failwith "Foobar 1.0 is not a folder"
