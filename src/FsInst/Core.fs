namespace FsInst

module Core =
    open System
    open System.Globalization

    type Version =
        { Major : int
          Minor : int
          Build : int } with
        override x.ToString() = sprintf "%d.%d.%d" x.Major x.Minor x.Build

    type ProductSpecification =
        { Name : string
          Language : CultureInfo
          Version : Version }

    let ``en-US`` = CultureInfo("en-US")

    let V major minor build =
        { Major = major; Minor = minor; Build = build}

    let Product =
        { Name = String.Empty
          Language = ``en-US``
          Version = V 1 0 0 }

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

    let newGuid prefix =
        prefix + Guid.NewGuid().ToString("N").ToUpper()

    type File =
        { Id : string
          FileName : string } with

        static member create fileName =
            { Id = newGuid "f"
              FileName = fileName }

    type Component =
        { Id : string
          Name : string
          Files : File list } with

        static member create files =
            { Id = sprintf "{%s}" (Guid.NewGuid().ToString().ToUpper())
              Name = newGuid "c"
              Files = files }

    type Folder =
        { Id : string
          Name : string
          Parent : Folder option
          Components : Component list } with

        override x.ToString() = x.Name

        static member (/) (x:Folder, y:obj) =
            let parentOrTargetDir =
                if x.Name = "TARGETDIR" then None else Some x

            match y with
            | :? Folder as f ->
                { f with
                    Parent = parentOrTargetDir }
            | :? String as s ->
                { Id = newGuid "d"
                  Name = s
                  Parent = parentOrTargetDir
                  Components = [] }
            | :? ProductSpecification as p ->
                { Id = "ProductFolder"
                  Name = "?Product?"
                  Parent = parentOrTargetDir
                  Components = [] }
            | _ -> failwith "not supported"

    let InstallationDrive =
        { Id = newGuid "d"
          Name = "TARGETDIR"
          Parent = None
          Components = [] }

    let ProgramFiles =
        { Id = "ProgramFiles64Folder"
          Name = "."
          Parent = None
          Components = [] }

    let Manufacturer =
        { Id = "ManufacturerFolder"
          Name = "?Manufacturer?"
          Parent = None
          Components = [] }

    type InstallationPackage =
        { Manufacturer : string
          Product : ProductSpecification
          Installer : InstallerSpecification
          Folders: Folder list }

    let InstallationPackage =
        { Manufacturer = String.Empty
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

        let files = List.map (fun fileName -> fileName |> File.create) fileNames
        let components = List.map (fun file -> [file] |> Component.create) files
        let folderWithFile =
            { destinationFolder with
                Components = destinationFolder.Components |> List.append components }

        { installationPackage with Folders = folderWithFile :: otherFolders }

    let installFile fileName into (folder:obj) (installationPackage:InstallationPackage) =
        installFiles [fileName] into folder installationPackage
