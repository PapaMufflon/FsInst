namespace FsInst

module Core =
    open System
    open System.Globalization

    type Version = {
        Major : int
        Minor : int
        Build : int } with
        override x.ToString() = sprintf "%d.%d.%d" x.Major x.Minor x.Build

    type ProductSpecification = {
        Name : string
        Language : CultureInfo
        Version : Version
    }

    let ``en-US`` = CultureInfo("en-US")

    let V major minor build =
        { Major = major; Minor = minor; Build = build}

    let Product = {
        Name = String.Empty
        Language = ``en-US``
        Version = V 1 0 0
    }

    type InstallerSpecification =
        { Description : string
          Comments : string
          Keywords : string
          MinimumVersion : Version }

    let Installer =
        { Description = String.Empty
          Comments = String.Empty
          Keywords = String.Empty
          MinimumVersion = V 2 0 0 }

    type File(fileName:string) =
        let id = "f" +  Guid.NewGuid().ToString("N").ToUpper()

        member x.FileName = fileName
        member x.Id = id

    type Component(files:File list) =
        let name = "c" + Guid.NewGuid().ToString("N").ToUpper()
        let id = sprintf "{%s}" (Guid.NewGuid().ToString().ToUpper())

        member x.Files = files
        member x.Name = name
        member x.Id = id
    
    let newGuid prefix =
        prefix + Guid.NewGuid().ToString("N").ToUpper()

    type Folder = {
        Id : string
        Name : string
        Parent : Folder option
        Components : Component list } with

        override x.ToString() = x.Name

        static member (/) (x:Folder, y:obj) =
            match y with
            | :? Folder as f ->
                { f with
                    Id = newGuid "d"
                    Parent = (if x.Name = "TARGETDIR" then None else Some x) }
            | :? String as s ->
                { Id = newGuid "d"
                  Name = s
                  Parent = (if x.Name = "TARGETDIR" then None else Some x); Components = [] }
            | _ -> failwith "not supported"

    let InstallationDrive = { Id = newGuid "d"; Name = "TARGETDIR"; Parent = None; Components = [] }

    let ProgramFiles = { Id = "ProgramFiles64Folder"; Name = "."; Parent = None; Components = [] }

    type InstallationPackage = {
        Manufacturer : string
        Product : ProductSpecification
        Installer : InstallerSpecification
        Folders: Folder list }

    let InstallationPackage = {
        Manufacturer = String.Empty
        Product = Product
        Installer = Installer
        Folders = [] }

    let copyright manufacturer installationPackage =
        { installationPackage with Manufacturer = manufacturer }

    let using = ()

    let installing product using installer installationPackage =
        { installationPackage with
            Product = product
            Installer = installer }

    let folderWithName name =
        { Id = newGuid "d"
          Name = name
          Parent = None
          Components = [] }

    let rec createFolder (folder:Object) installationPackage =
        match folder with
        | :? Folder as f ->
            let folders =
                if installationPackage.Folders |> List.exists (fun x -> x.Id = f.Id) ||
                   f = InstallationDrive then
                    installationPackage.Folders
                else       
                    f :: installationPackage.Folders

            match f.Parent with
            | Some parentFolder -> createFolder parentFolder { installationPackage with Folders = folders }
            | None -> { installationPackage with Folders = folders }
        | _ ->
            { installationPackage with
                Folders = (folderWithName (folder.ToString())) :: installationPackage.Folders }

    let into = ()

    let installFiles (fileNames:string list) into (folder:obj) (installationPackage:InstallationPackage) =
        let destinationFolder =
            match folder with
            | :? Folder as f -> f
            | _ -> folderWithName (folder.ToString())

        let installationPackageWithFolder = createFolder destinationFolder installationPackage
        let otherFolders = List.except [ destinationFolder ] installationPackageWithFolder.Folders

        let files = List.map (fun fileName -> File(fileName)) fileNames
        let components = List.map (fun file -> Component([file])) files
        let folderWithFile =
            { destinationFolder with
                Components = destinationFolder.Components |> List.append components }

        { installationPackage with Folders = folderWithFile :: otherFolders }

    let installFile fileName into (folder:obj) (installationPackage:InstallationPackage) =
        installFiles [fileName] into folder installationPackage