namespace FsInst

[<AutoOpen>]
module Msi =
    open System
    open System.IO
    open Microsoft.Deployment.WindowsInstaller
    open FsInst.Core

    let private createTables (database:Database) =
        let tos = typeof<string>

        database.Tables.Add(TableInfo("Property", [| new ColumnInfo("Property", "s72"); new ColumnInfo("Value", "l0") |], [| "Property" |]))
        database.Tables.Add(TableInfo("Directory", [| new ColumnInfo("Directory", "s72"); new ColumnInfo("Directory_Parent", tos, 72, false); new ColumnInfo("DefaultDir", "l255") |], [| "Directory" |]))
        database.Tables.Add(TableInfo("Component", [| new ColumnInfo("Component", "s72"); new ColumnInfo("ComponentId", tos, 38, false); new ColumnInfo("Directory_", "s72"); new ColumnInfo("Attributes", "i2"); new ColumnInfo("Condition", tos, 255, false); new ColumnInfo("KeyPath", tos, 72, false, false, true) |], [| "Component" |]))
        database.Tables.Add(TableInfo("File", [| new ColumnInfo("File", "s72"); new ColumnInfo("Component", "s72"); new ColumnInfo("FileName", "l255"); new ColumnInfo("FileSize", "i4"); new ColumnInfo("Version", tos, 72, false); new ColumnInfo("Language", tos, 20, false); new ColumnInfo("Attributes", typeof<int16>, 2, false); new ColumnInfo("Sequence", "i4") |], [| "File" |]))

    let msi installationPackage =
        let fileName = Guid.NewGuid().ToString() + ".msi"

        if File.Exists(fileName) then
            File.Delete(fileName)

        use database = new Database(fileName, DatabaseOpenMode.Create)
        createTables database

        if installationPackage.Manufacturer <> String.Empty then
            database.Execute(sprintf "INSERT INTO Property (Property, Value) VALUES ('Manufacturer', '%s')" installationPackage.Manufacturer)

        if not (List.isEmpty installationPackage.Folders) then
            let formatParent = function
            | Some (x:Folder) -> x.Id
            | None -> "TARGETDIR"
                
            for folder in installationPackage.Folders do
                database.Execute(sprintf "INSERT INTO Directory (Directory, Directory_Parent, DefaultDir) VALUES ('%s', '%s', '%s')" folder.Id (formatParent folder.Parent) folder.Name)

                for c in folder.Components do
                    let componentQuery = sprintf "INSERT INTO Component (Component, ComponentId, Directory_, Attributes, KeyPath) VALUES ('%s', '%s', '%s',  %d, '%s')" c.Name c.Id folder.Id 0 (List.head c.Files).Id
                    database.Execute(componentQuery)

                    c.Files
                    |> List.iteri (fun i f -> database.Execute(sprintf "INSERT INTO File (File, Component, FileName, FileSize, Attributes, Sequence) VALUES ('%s', '%s', '%s', %d, %d, %d)" f.Id c.Id f.FileName 1 512 i))

        database.Commit()

        FileInfo(fileName)