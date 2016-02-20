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
                using Installer

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

    [<Fact>]
    let ``can define installer properties`` () =
        let installationPackage =
            InstallationPackage
            |> copyright "Acme Ltd."
            |> installing Product using
                { Installer with
                    Description = "Acme's Foobar 1.0 Installer";
                    Comments = "Foobar is a registered trademark of Acme Ltd."
                    Keywords = "Installer"
                    MinimumVersion = V 2 0 0 }

        let simulation =
            installationPackage
            |> simulate

        simulation.Installer.Description |> should equal "Acme's Foobar 1.0 Installer"
        simulation.Installer.Comments |> should equal "Foobar is a registered trademark of Acme Ltd."
        simulation.Installer.Keywords |> should equal "Installer"
        simulation.Installer.MinimumVersion |> should equal (V 2 0 0)

        let simulationOfMsi =
            installationPackage
            |> msi "installerProperties.msi"
            |> FsInst.Simulation.Msi.simulate

        simulationOfMsi.Installer.Description |> should equal "Acme's Foobar 1.0 Installer"
        simulationOfMsi.Installer.Comments |> should equal "Foobar is a registered trademark of Acme Ltd."
        simulationOfMsi.Installer.Keywords |> should equal "Installer"
        simulationOfMsi.Installer.MinimumVersion |> should equal (V 2 0 0)

        File.Delete("installerProperties.msi")