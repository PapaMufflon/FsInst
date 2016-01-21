namespace FsInst

module Core =
    open System

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
    
    type Folder(name:string, parent:Folder option, components:Component list) =
        let id = "d" + Guid.NewGuid().ToString("N").ToUpper()

        member x.Name = name
        member x.Parent = parent
        member x.Components = components
        member x.Id = id

        new(name:string, parent:Folder option) = Folder(name, parent, [])
        new(name:string) = Folder(name, None, [])

        static member (/) (x, (y:Object)) =
            match y with
            | :? Folder as f -> Folder(f.Name, Some x, f.Components)
            | :? String as s -> Folder(s, Some x, [])
            | _ -> failwith "not supported"

    let (/) (x : Folder list) (y : string) =
        List.exactlyOne (List.where (fun (f:Folder) -> f.Name = y) x)

    type InstallationPackage = {
        Manufacturer : string
        Folders: Folder list
    }

    let InstallationPackage = {
        Manufacturer = String.Empty
        Folders = []
    }

    let copyright manufacturer installationPackage =
        { installationPackage with Manufacturer = manufacturer }

    let createFolder (folder:Object) installationPackage =
        match folder with
        | :? Folder as f -> { installationPackage with Folders = f :: installationPackage.Folders }
        | _ -> { installationPackage with Folders = Folder(folder.ToString(), None, []) :: installationPackage.Folders }

    let into = ()

    let installFile fileName into folder (installationPackage:InstallationPackage) =
        let otherFolders = List.except [ folder ] installationPackage.Folders
        let folderWithFile = Folder(folder.Name, folder.Parent, Component([File(fileName)]) :: folder.Components)

        { installationPackage with Folders = folderWithFile :: otherFolders }

    let installFiles (fileNames:string list) into folder (installationPackage:InstallationPackage) =
        let otherFolders = List.except [ folder ] installationPackage.Folders
        let files = List.map (fun fileName -> File(fileName)) fileNames
        let components = List.map (fun file -> Component([file])) files
        let folderWithFile = Folder(folder.Name, folder.Parent, folder.Components |> List.append components)

        { installationPackage with Folders = folderWithFile :: otherFolders }