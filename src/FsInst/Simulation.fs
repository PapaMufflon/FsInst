namespace FsInst.Simulation

[<AutoOpen>]
module Types =
    open FsInst.Core

    type ProgramsAndFeatures = 
        { Publisher : string }

    type ControlPanel = 
        { ProgramsAndFeatures : ProgramsAndFeatures }

    type FileSystem =
        { InstallationDrive : Folder list }

    type Simulation = 
        { ControlPanel : ControlPanel
          FileSystem : FileSystem }

module InstallationPackage =
    open FsInst.Core

    let simulate installationPackage = 
        { ControlPanel = { ProgramsAndFeatures = { Publisher = installationPackage.Manufacturer } }
          FileSystem = { InstallationDrive = installationPackage.Folders } }

module Msi =
    open System.IO
    open System.Linq
    open Microsoft.Deployment.WindowsInstaller
    open Microsoft.Deployment.WindowsInstaller.Linq
    open FsInst.Core

    let private createComponents directoryId (database:QDatabase) =
        List.ofSeq
            (database
                .Components
                .Where(fun c -> c.Directory_ = directoryId)
                .Select(fun c -> Component([File(database.Files.Single(fun f -> f.Component_ = c.Component).FileName)])))

    let private readFilesAndFolders (database:QDatabase) =
        let rootFolders = List.ofSeq (database.Directories.Select(fun d -> Folder(d.DefaultDir, None, createComponents d.Directory database)))

        { InstallationDrive = rootFolders }

    let simulate (msi : FileInfo) =
        use database = new QDatabase(msi.FullName, DatabaseOpenMode.ReadOnly)
        let manufacturer = (database.ExecutePropertyQuery("Manufacturer"))

        { ControlPanel = { ProgramsAndFeatures = { Publisher = manufacturer } }
          FileSystem = readFilesAndFolders database }