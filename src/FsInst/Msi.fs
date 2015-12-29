namespace FsInst

[<AutoOpen>]
module Msi =
    open System.IO
    open Microsoft.Deployment.WindowsInstaller
    open FsInst.Core

    let msi installationPackage =
        use database = new Database("setup.msi", DatabaseOpenMode.Create)
        database.Tables.Add(new TableInfo("Property", [| new ColumnInfo("Property", "s72"); new ColumnInfo("Value", "l0") |], [| "Property" |]))
        database.Execute("INSERT INTO Property (Property, Value) VALUES ('" + "Manufacturer" + "', '" + installationPackage.Manufacturer + "')")
        database.Commit()

        FileInfo("setup.msi")