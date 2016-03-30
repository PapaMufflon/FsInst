namespace FsInst

module FileSystem =
    open System.Diagnostics
    open System.IO
    open FsUnit.Xunit
    open FsInst.Core
    open FsInst.Simulation
    open Xunit

    let installAndTest installationPackage msiFileName simulationAssertions installationAssertion uninstallationAssertion =
        let msiFile =
            installationPackage
            |> msi msiFileName

        let simulationOfMsi = FsInst.Simulation.Msi.simulate msiFile
        simulationAssertions simulationOfMsi

        use installProcess = Process.Start(@"C:\Windows\System32\msiexec.exe", sprintf " /i %s /quiet" msiFileName)
        installProcess.WaitForExit()

        installationAssertion ()

        use uninstallProcess = Process.Start(@"C:\Windows\System32\msiexec.exe", sprintf " /x %s /quiet" msiFileName)
        uninstallProcess.WaitForExit()

        uninstallationAssertion ()

        File.Delete(msiFileName)

    let targetDir =
        let biggestDrive =
            DriveInfo.GetDrives()
            |> Array.filter (fun x -> x.IsReady && x.DriveType = DriveType.Fixed)
            |> Array.maxBy (fun x -> x.AvailableFreeSpace)

        biggestDrive.Name
        
    [<Fact>]
    let ``creates a folder if there is an item in it`` () =
        let Test = InstallationDrive/"Test"

        let installationPackage =
            InstallationPackage
            |> copyright "Acme Inc."
            |> installing
                { Product with
                    Name = "Foobar 1.0" }
                using Installer
            |> createFolder Test
            |> installFile "FsInst.dll" into Test

        let targetFile = sprintf @"%sTest\FsInst.dll" targetDir
        
        installAndTest
            installationPackage
            "createFolder.msi"
            (fun simulationOfMsi ->
                let msiFolder = simulationOfMsi.FileSystem.InstallationDrive/"Test"

                match msiFolder with
                | InstalledFolder f -> f.Name |> should equal Test.Name
                | _ -> failwith "Test is not a folder.")
            (fun () -> File.Exists(targetFile) |> should be True)
            (fun () -> File.Exists(targetFile) |> should be False)
            
    [<Fact>]
    let ``can have multiple files in one folder`` () =
        let Test = InstallationDrive/"Test"

        let installationPackage =
            InstallationPackage
            |> copyright "Acme Inc."
            |> installing
                { Product with
                    Name = "Foobar 1.0" }
                using Installer
            |> createFolder Test
            |> installFiles ["FsInst.dll"; "FsInst.Facts.dll"] into Test

        let targetFile1 = sprintf @"%sTest\FsInst.dll" targetDir
        let targetFile2 = sprintf @"%sTest\FsInst.Facts.dll" targetDir

        installAndTest
            installationPackage
            "multipleFiles.msi"
            (fun simulationOfMsi ->
                simulationOfMsi.FileSystem.InstallationDrive/"Test"/"FsInst.dll" |> should equal (InstalledFile("FsInst.dll"))
                simulationOfMsi.FileSystem.InstallationDrive/"Test"/"FsInst.Facts.dll" |> should equal (InstalledFile("FsInst.Facts.dll")))
            (fun () ->
                File.Exists(targetFile1) |> should be True
                File.Exists(targetFile2) |> should be True)
            (fun () ->
                File.Exists(targetFile1) |> should be False
                File.Exists(targetFile2) |> should be False)

    [<Fact>]
    let ``can have multiple files in one folder implicitly`` () =
        let installationPackage =
            InstallationPackage
            |> copyright "Acme Inc."
            |> installing
                { Product with
                    Name = "Foobar 1.0" }
                using Installer
            |> installFiles ["FsInst.dll"; "FsInst.Facts.dll"] into (InstallationDrive/"Test")

        let targetFile1 = sprintf @"%sTest\FsInst.dll" targetDir
        let targetFile2 = sprintf @"%sTest\FsInst.Facts.dll" targetDir

        installAndTest
            installationPackage
            "multipleFiles.msi"
            (fun simulationOfMsi ->
                simulationOfMsi.FileSystem.InstallationDrive/"Test"/"FsInst.dll" |> should equal (InstalledFile("FsInst.dll"))
                simulationOfMsi.FileSystem.InstallationDrive/"Test"/"FsInst.Facts.dll" |> should equal (InstalledFile("FsInst.Facts.dll")))
            (fun () ->
                File.Exists(targetFile1) |> should be True
                File.Exists(targetFile2) |> should be True)
            (fun () ->
                File.Exists(targetFile1) |> should be False
                File.Exists(targetFile2) |> should be False)

    [<Fact>]
    let ``can have hierarchical folders`` () =
        let folder = InstallationDrive/"Test"
        let subFolder = folder/"SubFolder"

        let installationPackage =
            InstallationPackage
            |> copyright "Acme Inc."
            |> installing
                { Product with
                    Name = "Foobar 1.0" }
                using Installer
            |> createFolder subFolder
            |> installFile "FsInst.dll" into folder
            |> installFile "FsInst.Facts.dll" into subFolder

        let targetFile1 = sprintf @"%sTest\FsInst.dll" targetDir
        let targetFile2 = sprintf @"%sTest\SubFolder\FsInst.Facts.dll" targetDir

        installAndTest
            installationPackage
            "hierarchicalfolders.msi"
            (fun simulationOfMsi ->
                simulationOfMsi.FileSystem.InstallationDrive/"Test"/"FsInst.dll" |> should equal (InstalledFile("FsInst.dll"))
                simulationOfMsi.FileSystem.InstallationDrive/"Test"/"SubFolder"/"FsInst.Facts.dll" |> should equal (InstalledFile("FsInst.Facts.dll")))
            (fun () ->
                File.Exists(targetFile1) |> should be True
                File.Exists(targetFile2) |> should be True)
            (fun () ->
                File.Exists(targetFile1) |> should be False
                File.Exists(targetFile2) |> should be False)

    [<Fact>]
    let ``defines a variable for the program files folder`` () =
        let installationPackage =
            InstallationPackage
            |> copyright "Acme Inc."
            |> installing
                { Product with
                    Name = "Foobar 1.0" }
                using Installer
            |> installFile "FsInst.dll" into ProgramFiles
        
        let programFilesFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles)
        let installedFile = Path.Combine(programFilesFolder, "FsInst.dll")
        
        installAndTest
            installationPackage
            "programFilesFolder.msi"
            (fun simulationOfMsi -> simulationOfMsi.FileSystem.InstallationDrive/ProgramFiles/"FsInst.dll" |> should equal (InstalledFile("FsInst.dll")))
            (fun () -> File.Exists(installedFile) |> should be True)
            (fun () -> File.Exists(installedFile) |> should be False)

    [<Fact>]
    let ``the manufacturer variable can be used to create a folder`` () =
        let installationPackage =
            InstallationPackage
            |> copyright "Acme Inc"
            |> installing
                { Product with
                    Name = "Foobar 1.0" }
                using Installer
            |> installFile "FsInst.dll" into (ProgramFiles/Manufacturer)

        let programFilesFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles)
        let installedFile = Path.Combine(programFilesFolder, "Acme Inc", "FsInst.dll")

        installAndTest
            installationPackage
            "manufacturer.msi"
            (fun simulationOfMsi ->
                let simulatedFolder = simulationOfMsi.FileSystem.InstallationDrive/ProgramFiles/"Acme Inc"

                match simulatedFolder with
                | InstalledFolder f -> f.Name |> should equal "Acme Inc"
                | _ -> failwith "Acme Inc is no folder")
            (fun () -> File.Exists(installedFile) |> should be True)
            (fun () -> File.Exists(installedFile) |> should be False)

    [<Fact>]
    let ``the product variable can be used to create a folder`` () =
        let installationPackage =
            InstallationPackage
            |> copyright "Acme Inc"
            |> installing
                { Product with
                    Name = "Foobar 1.0" }
                using Installer
            |> installFile "FsInst.dll" into (ProgramFiles/Manufacturer/Product)

        let programFilesFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles)
        let installedFile = Path.Combine(programFilesFolder, "Acme Inc", "Foobar 1.0", "FsInst.dll")

        installAndTest
            installationPackage
            "product.msi"
            (fun simulationOfMsi ->
                let simulatedFolder = simulationOfMsi.FileSystem.InstallationDrive/ProgramFiles/"Acme Inc"/"Foobar 1.0"

                match simulatedFolder with
                | InstalledFolder f -> f.Name |> should equal "Foobar 1.0"
                | _ -> failwith "Foobar 1.0 is no folder")
            (fun () -> File.Exists(installedFile) |> should be True)
            (fun () -> File.Exists(installedFile) |> should be False)
