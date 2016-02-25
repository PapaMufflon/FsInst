namespace FsInst.Simulation

[<AutoOpen>]
module Types =
    open System
    open System.Globalization
    open FsInst.Core

    type ProgramsAndFeatures =
        { Publisher : string
          Name : string
          Language : CultureInfo
          Version : FsInst.Core.Version }
    
    type ControlPanel = 
        { ProgramsAndFeatures : ProgramsAndFeatures }
    
    type Folder =
        { Id : string
          Name : string
          Parent : Folder option
          Children : Folder list
          Files : string list } with

        static member (/)(folder:Folder, subfolder:obj) =
            let subFolderName =
                match subfolder with
                | :? Folder as sub -> sub.Name
                | :? FsInst.Core.Folder as sub -> sub.Name
                | :? String as s -> s
                | _ -> failwith "not supported"

            if folder.Children |> List.exists (fun f -> f.Name = subFolderName) then 
                InstalledFolder(List.exactlyOne (folder.Children |> List.where (fun (f : Folder) -> f.Name = subFolderName)))
            else
                InstalledFile(List.exactlyOne (folder.Files |> List.where (fun s -> s = subFolderName)))

    and Browsable = 
        | InstalledFolder of Folder
        | InstalledFile of string with
        
        static member (/)(browsable, name) = 
            match browsable with
            | InstalledFolder folder -> folder/(name :> obj)
            | _ -> failwith "a file cannot have children."

    type FileSystem = 
        { InstallationDrive : Folder }
    
    let toFileSystem folders =
        let rootFolders folders =
            let isNoParent folder folders =
                not (folders
                     |> List.exists (fun (parentId, _) -> parentId = folder.Id))

            let isNoParentButChild (parentId, folder) folders =
                (not (String.IsNullOrEmpty(parentId))) && (isNoParent folder folders)

            let rec buildUpTree folders =
                if not (folders |> List.exists (fun f -> isNoParentButChild f folders)) then
                    folders
                else
                    let noParentButChild =
                        folders
                        |> List.find (fun f -> isNoParentButChild f folders)

                    let parentId, noParentButChildFolder = noParentButChild

                    let parent =
                        folders
                        |> List.find (fun (_, folder) -> folder.Id = parentId)

                    let parentsParentId, parentFolder = parent

                    let newParentFolder =
                        { parentFolder with
                            Children = noParentButChildFolder :: parentFolder.Children }

                    buildUpTree ((parentsParentId, newParentFolder) :: (List.except [parent; noParentButChild] folders))

            buildUpTree folders
            |> List.map (fun (parentId, folder) -> folder)

        { Id = String.Empty
          Name = "root"
          Parent = None
          Children = rootFolders folders
          Files = [] }

    type Simulation =
        { Installer : InstallerSpecification
          ControlPanel : ControlPanel
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

            let formatName f =
                match f.Id with
                | "ManufacturerFolder" -> installationPackage.Manufacturer
                | "ProductFolder" -> installationPackage.Product.Name
                | _ -> f.Name

            installationPackage.Folders
            |> List.map (fun f -> 
                (parentToId f.Parent,
                 { Id = f.Id
                   Name = formatName f
                   Parent = None
                   Children = []
                   Files = getFiles f }))

        let programsAndFeatures =
            { Publisher = installationPackage.Manufacturer
              Name = installationPackage.Product.Name
              Language = installationPackage.Product.Language
              Version = installationPackage.Product.Version }
        
        { Installer = installationPackage.Installer
          ControlPanel = { ProgramsAndFeatures = programsAndFeatures }
          FileSystem = { InstallationDrive = toFileSystem convertedFolders } }

module Msi =
    open System
    open System.IO
    open System.Linq
    open System.Text.RegularExpressions
    open Microsoft.Deployment.WindowsInstaller
    open Microsoft.Deployment.WindowsInstaller.Linq
    open FsInst.Core
    
    let simulate (msi : FileInfo) = 
        let readFilesAndFolders (database : QDatabase) = 
            let directories = List.ofSeq (database.Directories.Select(fun d -> (d.DefaultDir, d.Directory, d.Directory_Parent)))

            let gatherFiles directoryId (database : QDatabase) = 
                let components = List.ofSeq (database.Components.Where(fun c -> c.Directory_ = directoryId))
            
                components 
                |> List.map (fun c -> (List.ofSeq (database.Files.Where(fun f -> f.Component_ = c.Component)) |> List.exactlyOne).FileName)
            
            let convertedFolders = 
                directories
                |> List.filter (fun (name, id, parent) -> name <> "TARGETDIR")
                |> List.map (fun (name, id, parent) -> 
                    ((if parent = "TARGETDIR" then String.Empty else parent),
                     { Id = id
                       Name = name
                       Parent = None
                       Children = []
                       Files = gatherFiles id database }))

            { InstallationDrive = toFileSystem convertedFolders }
        
        use database = new QDatabase(msi.FullName, DatabaseOpenMode.ReadOnly)
        let manufacturer = database.ExecutePropertyQuery("Manufacturer")
        let name = database.ExecutePropertyQuery("ProductName")

        let convertToVersion versionString =
            let m = Regex.Match(versionString, "([0-9]+).([0-9]+).([0-9]+)")
            let parse (index:int) (regexMatch:Match) = Int32.Parse(regexMatch.Groups.[index].Value)

            if m.Success then
                V (m |> parse 1) (m |> parse 2) (m |> parse 3)
            else
                V 0 0 0

        let version = convertToVersion (database.ExecutePropertyQuery("ProductVersion"))

//        let (|Int|_|) str =
//            match Int32.TryParse(str) with
//            | (true, int) -> Some(int)
//            | _ -> None

        let language = ``en-US`` // default language
//            match (database.ExecutePropertyQuery("ProductLanguage")) with
//            | Int i -> if i = 1033 then ``en-US`` else failwith (sprintf "The language with ID %d is not supported." i)
//            | s -> failwith (sprintf "The string %s for a language identifier is not supported." s)

        let description = database.SummaryInfo.Subject
        let comments = database.SummaryInfo.Comments
        let keywords = database.SummaryInfo.Keywords

        let pageCountToVersion pageCount =
            let major = pageCount / 100
            let minor = pageCount - major*100
            V major minor 0

        let installerVersion = pageCountToVersion database.SummaryInfo.PageCount

        { Installer =
            { Description = description
              Comments = comments
              Keywords = keywords
              MinimumVersion = installerVersion }
          ControlPanel =
              { ProgramsAndFeatures =
                  { Publisher = manufacturer;
                    Name = name;
                    Language = language;
                    Version = version } }
          FileSystem = readFilesAndFolders database }
