namespace FsInst

[<AutoOpen>]
module Msi =
    open System
    open System.IO
    open Microsoft.Deployment.WindowsInstaller
    open FsInst.Core
    open Microsoft.Deployment.Compression.Cab
    open Microsoft.Deployment.Compression
    open System.Collections.Generic

    let msi fileName installationPackage =
        let createTables (database:Database) =
            let tos = typeof<string>
            let toi = typeof<int16>

            database.Tables.Add(TableInfo("Media", [| new ColumnInfo("DiskId", "i2"); new ColumnInfo("LastSequence", "i4"); new ColumnInfo("DiskPrompt", tos, 64, false, false, true); new ColumnInfo("Cabinet", tos, 255, false); new ColumnInfo("VolumeLabel", tos, 32, false); new ColumnInfo("Source", tos, 72, false) |], [| "DiskId" |]))
            database.Tables.Add(TableInfo("Property", [| new ColumnInfo("Property", "s72"); new ColumnInfo("Value", "l0") |], [| "Property" |]))
            database.Tables.Add(TableInfo("Directory", [| new ColumnInfo("Directory", "s72"); new ColumnInfo("Directory_Parent", tos, 72, false); new ColumnInfo("DefaultDir", "l255") |], [| "Directory" |]))
            database.Tables.Add(TableInfo("Feature", [| new ColumnInfo("Feature", "s38"); new ColumnInfo("Feature_Parent", tos, 38, false); new ColumnInfo("Title", tos, 64, false, false, true); new ColumnInfo("Description", tos, 255, false, false, true); new ColumnInfo("Display", toi, 2, false); new ColumnInfo("Level", "i2"); new ColumnInfo("Directory_", tos, 72, false); new ColumnInfo("Attributes", "i2") |], [| "Feature" |]))
            database.Tables.Add(TableInfo("FeatureComponents", [| new ColumnInfo("Feature_", "s38"); new ColumnInfo("Component_", "s72") |], [| "Feature_"; "Component_" |]))
            database.Tables.Add(TableInfo("Component", [| new ColumnInfo("Component", "s72"); new ColumnInfo("ComponentId", tos, 38, false); new ColumnInfo("Directory_", "s72"); new ColumnInfo("Attributes", "i2"); new ColumnInfo("Condition", tos, 255, false); new ColumnInfo("KeyPath", tos, 72, false) |], [| "Component" |]))
            database.Tables.Add(TableInfo("File", [| new ColumnInfo("File", "s72"); new ColumnInfo("Component_", "s72"); new ColumnInfo("FileName", "l255"); new ColumnInfo("FileSize", "i4"); new ColumnInfo("Version", tos, 72, false); new ColumnInfo("Language", tos, 20, false); new ColumnInfo("Attributes", typeof<int16>, 2, false); new ColumnInfo("Sequence", "i4") |], [| "File" |]))
            database.Tables.Add(TableInfo("MsiFileHash", [| new ColumnInfo("File_", "s72"); new ColumnInfo("Options", "i2"); new ColumnInfo("HashPart1", "i4"); new ColumnInfo("HashPart2", "i4"); new ColumnInfo("HashPart3", "i4"); new ColumnInfo("HashPart4", "i4") |], [| "File_" |]))
            database.Tables.Add(TableInfo("AdminExecuteSequence", [| new ColumnInfo("Action", "s72"); new ColumnInfo("Condition", tos, 255, false); new ColumnInfo("Sequence", toi, 2, false) |], [| "Action" |]))
            database.Tables.Add(TableInfo("AdminUISequence", [| new ColumnInfo("Action", "s72"); new ColumnInfo("Condition", tos, 255, false); new ColumnInfo("Sequence", toi, 2, false) |], [| "Action" |]))
            database.Tables.Add(TableInfo("AdvtExecuteSequence", [| new ColumnInfo("Action", "s72"); new ColumnInfo("Condition", tos, 255, false); new ColumnInfo("Sequence", toi, 2, false) |], [| "Action" |]))
            database.Tables.Add(TableInfo("InstallExecuteSequence", [| new ColumnInfo("Action", "s72"); new ColumnInfo("Condition", tos, 255, false); new ColumnInfo("Sequence", toi, 2, false) |], [| "Action" |]))
            database.Tables.Add(TableInfo("InstallUISequence", [| new ColumnInfo("Action", "s72"); new ColumnInfo("Condition", tos, 255, false); new ColumnInfo("Sequence", toi, 2, false) |], [| "Action" |]))
            database.Tables.Add(TableInfo("Error", [| new ColumnInfo("Error", "i2"); new ColumnInfo("Message", tos, 0, false, false, true) |], [| "Error" |]))

            database

        let addBasicSequences (database:Database) =
            database.Execute("INSERT INTO AdminExecuteSequence (Action, Sequence) VALUES ('CostInitialize', '800')")
            database.Execute("INSERT INTO AdminExecuteSequence (Action, Sequence) VALUES ('FileCost', '900')")
            database.Execute("INSERT INTO AdminExecuteSequence (Action, Sequence) VALUES ('CostFinalize', '1000')")
            database.Execute("INSERT INTO AdminExecuteSequence (Action, Sequence) VALUES ('InstallValidate', '1400')")
            database.Execute("INSERT INTO AdminExecuteSequence (Action, Sequence) VALUES ('InstallInitialize', '1500')")
            database.Execute("INSERT INTO AdminExecuteSequence (Action, Sequence) VALUES ('InstallAdminPackage', '3900')")
            database.Execute("INSERT INTO AdminExecuteSequence (Action, Sequence) VALUES ('InstallFiles', '4000')")
            database.Execute("INSERT INTO AdminExecuteSequence (Action, Sequence) VALUES ('InstallFinalize', '6600')")

            database.Execute("INSERT INTO AdminUISequence (Action, Sequence) VALUES ('CostInitialize', '800')")
            database.Execute("INSERT INTO AdminUISequence (Action, Sequence) VALUES ('FileCost', '900')")
            database.Execute("INSERT INTO AdminUISequence (Action, Sequence) VALUES ('CostFinalize', '1000')")
            database.Execute("INSERT INTO AdminUISequence (Action, Sequence) VALUES ('ExecuteAction', '1300')")

            database.Execute("INSERT INTO AdvtExecuteSequence (Action, Sequence) VALUES ('CostInitialize', '800')")
            database.Execute("INSERT INTO AdvtExecuteSequence (Action, Sequence) VALUES ('CostFinalize', '1000')")
            database.Execute("INSERT INTO AdvtExecuteSequence (Action, Sequence) VALUES ('InstallValidate', '1400')")
            database.Execute("INSERT INTO AdvtExecuteSequence (Action, Sequence) VALUES ('InstallInitialize', '1500')")
            database.Execute("INSERT INTO AdvtExecuteSequence (Action, Sequence) VALUES ('PublishFeatures', '6300')")
            database.Execute("INSERT INTO AdvtExecuteSequence (Action, Sequence) VALUES ('PublishProduct', '6400')")
            database.Execute("INSERT INTO AdvtExecuteSequence (Action, Sequence) VALUES ('InstallFinalize', '6600')")

            database.Execute("INSERT INTO InstallExecuteSequence (Action, Sequence) VALUES ('ValidateProductID', '700')")
            database.Execute("INSERT INTO InstallExecuteSequence (Action, Sequence) VALUES ('CostInitialize', '800')")
            database.Execute("INSERT INTO InstallExecuteSequence (Action, Sequence) VALUES ('FileCost', '900')")
            database.Execute("INSERT INTO InstallExecuteSequence (Action, Sequence) VALUES ('CostFinalize', '1000')")
            database.Execute("INSERT INTO InstallExecuteSequence (Action, Sequence) VALUES ('InstallValidate', '1400')")
            database.Execute("INSERT INTO InstallExecuteSequence (Action, Sequence) VALUES ('InstallInitialize', '1500')")
            database.Execute("INSERT INTO InstallExecuteSequence (Action, Sequence) VALUES ('ProcessComponents', '1600')")
            database.Execute("INSERT INTO InstallExecuteSequence (Action, Sequence) VALUES ('UnpublishFeatures', '1800')")
            database.Execute("INSERT INTO InstallExecuteSequence (Action, Sequence) VALUES ('RemoveFiles', '3500')")
            database.Execute("INSERT INTO InstallExecuteSequence (Action, Sequence) VALUES ('InstallFiles', '4000')")
            database.Execute("INSERT INTO InstallExecuteSequence (Action, Sequence) VALUES ('RegisterUser', '6000')")
            database.Execute("INSERT INTO InstallExecuteSequence (Action, Sequence) VALUES ('RegisterProduct', '6100')")
            database.Execute("INSERT INTO InstallExecuteSequence (Action, Sequence) VALUES ('PublishFeatures', '6300')")
            database.Execute("INSERT INTO InstallExecuteSequence (Action, Sequence) VALUES ('PublishProduct', '6400')")
            database.Execute("INSERT INTO InstallExecuteSequence (Action, Sequence) VALUES ('InstallFinalize', '6600')")

            database.Execute("INSERT INTO InstallUISequence (Action, Sequence) VALUES ('ValidateProductID', '700')")
            database.Execute("INSERT INTO InstallUISequence (Action, Sequence) VALUES ('CostInitialize', '800')")
            database.Execute("INSERT INTO InstallUISequence (Action, Sequence) VALUES ('FileCost', '900')")
            database.Execute("INSERT INTO InstallUISequence (Action, Sequence) VALUES ('CostFinalize', '1000')")
            database.Execute("INSERT INTO InstallUISequence (Action, Sequence) VALUES ('ExecuteAction', '1300')")

            database

        let writeSummaryInfo installationPackage (database:Database) =
            let productCode = Guid.NewGuid().ToString("B").ToUpper()
            let upgradeCode = Guid.NewGuid().ToString("B").ToUpper()
            let productName = installationPackage.Product.Name
            let versionToPageCount version = version.Major * 100 + version.Minor

            database.SummaryInfo.Title <- "Installation Database"
            database.SummaryInfo.Author <- installationPackage.Manufacturer
            database.SummaryInfo.Subject <- installationPackage.Installer.Description
            database.SummaryInfo.Comments <- installationPackage.Installer.Comments
            database.SummaryInfo.Keywords <- installationPackage.Installer.Keywords
            database.SummaryInfo.RevisionNumber <-  productCode
            database.SummaryInfo.Template <- "x64;1033"
            database.SummaryInfo.PageCount <- versionToPageCount installationPackage.Installer.MinimumVersion
            database.SummaryInfo.WordCount <- 2

            database.Execute("INSERT INTO Directory (Directory, DefaultDir) VALUES ('TARGETDIR', 'SourceDir')")
            database.Execute("INSERT INTO Feature (Feature, Display, Level, Attributes) VALUES ('Complete', '2', '1', '0')")

            database.Execute("INSERT INTO Property (Property, Value) VALUES ('LIMITUI', '1')")
            database.Execute(sprintf "INSERT INTO Property (Property, Value) VALUES ('ProductCode', '%s')" productCode)
            database.Execute("INSERT INTO Property (Property, Value) VALUES ('ProductLanguage', '1033')")
            
            if productName <> String.Empty then
                database.Execute(sprintf "INSERT INTO Property (Property, Value) VALUES ('ProductName', '%s')" productName)

            database.Execute(sprintf "INSERT INTO Property (Property, Value) VALUES ('ProductVersion', '%s')" (installationPackage.Product.Version.ToString()))
            database.Execute(sprintf "INSERT INTO Property (Property, Value) VALUES ('UpgradeCode', '%s')" upgradeCode)

            database

        let writeManufacturer installationPackage (database:Database) =
            if installationPackage.Manufacturer <> String.Empty then
                database.Execute(sprintf "INSERT INTO Property (Property, Value) VALUES ('Manufacturer', '%s')" installationPackage.Manufacturer)

            database

        let writeFilesAndFolders installationPackage (database:Database) =
            if not (List.isEmpty installationPackage.Folders) then
                let formatParent = function
                | Some (x:Folder) -> x.Id
                | None -> "TARGETDIR"

                let dic = new Dictionary<string, string>()
                let fileSequenceCounter = ref 1

                for folder in installationPackage.Folders do
                    database.Execute(sprintf "INSERT INTO Directory (Directory, Directory_Parent, DefaultDir) VALUES ('%s', '%s', '%s')" folder.Id (formatParent folder.Parent) (if folder.Id = "ManufacturerFolder" then installationPackage.Manufacturer else folder.Name))

                    for c in folder.Components do
                        database.Execute(sprintf "INSERT INTO Component (Component, ComponentId, Directory_, Attributes, KeyPath) VALUES ('%s', '%s', '%s',  %d, '%s')" c.Name c.Id folder.Id 0 (List.head c.Files).Id)
                        database.Execute(sprintf "INSERT INTO FeatureComponents (Feature_, Component_) VALUES ('Complete', '%s')" c.Name)

                        c.Files
                        |> List.iteri (fun i f ->
                            database.Execute(sprintf "INSERT INTO File (File, Component_, FileName, FileSize, Attributes, Sequence) VALUES ('%s', '%s', '%s', %d, %d, %d)" f.Id c.Name f.FileName 1 512 !fileSequenceCounter)
                            fileSequenceCounter := !fileSequenceCounter + 1

                            dic.Add(f.Id, f.FileName)

                            let hashes = Array.zeroCreate 4
                            Microsoft.Deployment.WindowsInstaller.Installer.GetFileHash(f.FileName, hashes)
                            database.Execute(sprintf "INSERT INTO MsiFileHash (File_, Options, HashPart1, HashPart2, HashPart3, HashPart4) VALUES ('%s', '0', '%d', '%d', '%d', '%d')" f.Id hashes.[0] hashes.[1] hashes.[2] hashes.[3]))

                use view = database.OpenView("SELECT `Name`,`Data` FROM _Streams")

                let cabinetFileName = sprintf "cabinet.%s.cab" (Guid.NewGuid().ToString("N").Substring(0, 5))
                let cabinet = CabInfo(cabinetFileName)
                cabinet.PackFileSet(".", dic, CompressionLevel.Max, null)

                let record = database.CreateRecord(2)
                record.SetString(1, cabinetFileName)
                record.SetStream(2, cabinetFileName)

                view.Insert(record)
                view.Execute()

                cabinet.Delete()

                database.Execute(sprintf "INSERT INTO Media (DiskId, LastSequence, Cabinet) VALUES ('1', '%d', '#%s')" !fileSequenceCounter cabinetFileName)

            database

        if File.Exists(fileName) then
            File.Delete(fileName)

        use database =
            new Database(fileName, DatabaseOpenMode.Create)
            |> createTables
            |> addBasicSequences
            |> writeSummaryInfo installationPackage
            |> writeManufacturer installationPackage
            |> writeFilesAndFolders installationPackage
        
        database.Commit()

        FileInfo(fileName)
