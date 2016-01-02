namespace FsInst

module FileSystem =
    open Xunit
    open FsUnit.Xunit
    open FsInst.Core
    open FsInst.Simulation
    open FsInst.Simulation.InstallationPackage

    [<Fact>]
    let ``creates a folder if there will be an item in it`` () =
        let Test = Folder("Test", None, [])

        let installationPackage =
            InstallationPackage
            |> createFolder Test
            |> installFile "FileSystem.fs" into Test

        let simulation =
            installationPackage
            |> simulate

        (simulation.FileSystem.InstallationDrive/"Test").Name |> should equal Test.Name

        let simulationOfMsi =
            installationPackage
            |> msi
            |> FsInst.Simulation.Msi.simulate
            
        (simulationOfMsi.FileSystem.InstallationDrive/"Test").Name |> should equal Test.Name