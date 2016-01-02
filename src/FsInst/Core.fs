namespace FsInst

module Core =
    open System

    type File(fileName:string) =
        member x.FileName = fileName
        member x.Id = Guid.NewGuid().ToString()

    type Component(files:File list) =
        member x.Files = files
        member x.Name = Guid.NewGuid().ToString()
        member x.Id = Guid.NewGuid().ToString()
    
    type Folder(name:string, parent:Folder option, components:Component list) =
        member x.Name = name
        member x.Parent = parent
        member x.Components = components
        member x.Id = Guid.NewGuid().ToString()

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

    let installFiles (files:File list) into folder (installationPackage:InstallationPackage) =
        let otherFolders = List.except [ folder ] installationPackage.Folders
        let folderWithFile = Folder(folder.Name, folder.Parent, Component(files) :: folder.Components)

        { installationPackage with Folders = folderWithFile :: otherFolders }