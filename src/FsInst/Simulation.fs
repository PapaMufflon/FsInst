namespace FsInst.Simulation

[<AutoOpen>]
module Types =
    type ProgramsAndFeatures = 
        { Publisher : string }
    
    type ControlPanel = 
        { ProgramsAndFeatures : ProgramsAndFeatures }
    
    type Folder = 
        { Name : string
          Parent : Folder option
          Children : Folder list
          Files : string list }
    
    type Browsable = 
        | BrowsableFolder of Folder
        | BrowsableFile of string
    
    let (/) (x : obj) (name : string) = 
        let sub folder = 
            if folder.Children |> List.exists (fun f -> f.Name = name) then 
                BrowsableFolder(List.exactlyOne (folder.Children |> List.where (fun (f : Folder) -> f.Name = name)))
            else
                BrowsableFile(List.exactlyOne (folder.Files |> List.where (fun s -> s = name)))

        match x with
        | :? Browsable as browsable -> 
            match browsable with
            | BrowsableFolder folder -> sub folder
            | _ -> failwith "a file cannot have children."
        | :? Folder as folder -> sub folder
        | _ -> failwith "this object is cannot have children."
    
    type FileSystem = 
        { InstallationDrive : Folder }
    
    let toFileSystem folders = 
        let rootFolders folders =
            let parentsWithChildren = 
                folders
                |> List.groupBy (fun (parentId, folder) -> parentId)
                |> List.map (fun (key, children) -> (key, List.map (fun (sameKey, child) -> child) children))

            folders
            |> List.map (fun (parentId, folder) -> 
                let suitableChildren =
                    parentsWithChildren
                    |> List.tryFind (fun (pId, children) -> parentId = pId)

                match suitableChildren with
                | Some(parentId, children) -> { folder with Children = children }
                | None -> folder)
            
        { Name = "root"
          Parent = None
          Children = rootFolders folders
          Files = [] }

    type Simulation = 
        { ControlPanel : ControlPanel
          FileSystem : FileSystem }

module InstallationPackage =
    open System
    open FsInst.Core
    
    let simulate installationPackage =
        let convertedFolders =
            let getFiles (f : FsInst.Core.Folder) = 
                f.Components
                |> List.collect (fun c -> c.Files)
                |> List.map (fun file -> file.FileName)

            let parentToId = function
            | Some (p : FsInst.Core.Folder) -> p.Id
            | None -> String.Empty

            installationPackage.Folders
            |> List.map (fun f -> 
                (parentToId f.Parent,
                 { Name = f.Name
                   Parent = None
                   Children = []
                   Files = getFiles f }))

        { ControlPanel = { ProgramsAndFeatures = { Publisher = installationPackage.Manufacturer } }
          FileSystem = { InstallationDrive = toFileSystem convertedFolders } }

module Msi = 
    open System.IO
    open System.Linq
    open Microsoft.Deployment.WindowsInstaller
    open Microsoft.Deployment.WindowsInstaller.Linq
    
    let simulate (msi : FileInfo) = 
        let readFilesAndFolders (database : QDatabase) = 
            let directories = List.ofSeq (database.Directories.Select(fun d -> (d.DefaultDir, d.Directory, d.Directory_Parent)))

            let gatherFiles directoryId (database : QDatabase) = 
                let components = List.ofSeq (database.Components.Where(fun c -> c.Directory_ = directoryId))
            
                components 
                |> List.map (fun c -> (List.ofSeq (database.Files.Where(fun f -> f.Component_ = c.Component)) |> List.exactlyOne).FileName)
            
            let convertedFolders = 
                directories |> List.map (fun (name, id, parent) -> 
                                   (parent, 
                                    { Name = name
                                      Parent = None
                                      Children = []
                                      Files = gatherFiles id database }))

            { InstallationDrive = toFileSystem convertedFolders }
        
        use database = new QDatabase(msi.FullName, DatabaseOpenMode.ReadOnly)
        let manufacturer = (database.ExecutePropertyQuery("Manufacturer"))

        { ControlPanel = { ProgramsAndFeatures = { Publisher = manufacturer } }
          FileSystem = readFilesAndFolders database }