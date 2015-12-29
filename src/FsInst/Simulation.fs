namespace FsInst.Simulation

[<AutoOpen>]
module Types =
    type ProgramsAndFeatures = 
        { Publisher : string }

    type ControlPanel = 
        { ProgramsAndFeatures : ProgramsAndFeatures }

    type Simulation = 
        { ControlPanel : ControlPanel }

module InstallationPackage =
    open FsInst.Core

    let simulate installationPackage =
        { ControlPanel = { ProgramsAndFeatures = { Publisher = installationPackage.Manufacturer } } }

module Msi =
    open System.IO
    open Microsoft.Deployment.WindowsInstaller

    let simulate (msi : FileInfo) =
        use database = new Database("setup.msi", DatabaseOpenMode.ReadOnly)
        let manufacturer = (database.ExecutePropertyQuery("Manufacturer"))

        { ControlPanel = { ProgramsAndFeatures = { Publisher = manufacturer } } }