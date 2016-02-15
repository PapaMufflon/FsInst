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

    [<Fact>]
    let ``can define properties of the product`` () =
        let installationPackage =
            InstallationPackage
            |> copyright "Acme Ltd."
            |> installing
                { Product with
                    Name = "Foobar 1.0";
                    Language = ``en-US``
                    Version = V 1 2 3 }

        let simulation =
            installationPackage
            |> simulate

        simulation.ControlPanel.ProgramsAndFeatures.Name |> should equal "Foobar 1.0"
        simulation.ControlPanel.ProgramsAndFeatures.Language |> should equal ``en-US``
        simulation.ControlPanel.ProgramsAndFeatures.Version |> should equal (V 1 2 3)

        let simulationOfMsi =
            installationPackage
            |> msi "productProperties.msi"
            |> FsInst.Simulation.Msi.simulate

        simulationOfMsi.ControlPanel.ProgramsAndFeatures.Name |> should equal "Foobar 1.0"
        simulationOfMsi.ControlPanel.ProgramsAndFeatures.Language |> should equal ``en-US``
        simulationOfMsi.ControlPanel.ProgramsAndFeatures.Version |> should equal (V 1 2 3)

        File.Delete("productProperties.msi")