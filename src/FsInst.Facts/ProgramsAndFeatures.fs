namespace FsInst

module ``Programs and Features`` =
    open System.IO
    open Xunit
    open FsUnit.Xunit
    open FsInst.Core
    open FsInst.Simulation
    open FsInst.Simulation.InstallationPackage

    [<Fact>]
    let ``get the publisher from the manufacturer property`` () =
        let installationPackage =
            InstallationPackage
            |> copyright "Acme Ltd."

        let simulation =
            installationPackage
            |> simulate

        simulation.ControlPanel.ProgramsAndFeatures.Publisher |> should equal "Acme Ltd."

        let simulationOfMsi =
            installationPackage
            |> msi "publisher.msi"
            |> FsInst.Simulation.Msi.simulate

        simulationOfMsi.ControlPanel.ProgramsAndFeatures.Publisher |> should equal "Acme Ltd."

        File.Delete("publisher.msi")