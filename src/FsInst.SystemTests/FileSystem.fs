namespace FsInst

module FileSystem =
    open System.Diagnostics
    open System.IO
    open FsUnit.Xunit
    open FsInst.Core
    open Xunit

    let installAndTest installationPackage msiFileName installationAssertion uninstallationAssertion =
        installationPackage
        |> msi msiFileName
        |> ignore

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
            |> createFolder Test
            |> installFile "FsInst.dll" into Test

        let targetFile = sprintf @"%sTest\FsInst.dll" targetDir
        
        installAndTest
            installationPackage
            "createFolder.msi"
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
            |> createFolder Test
            |> installFiles ["FsInst.dll"; "FsInst.Facts.dll"] into Test

        let targetFile1 = sprintf @"%sTest\FsInst.dll" targetDir
        let targetFile2 = sprintf @"%sTest\FsInst.Facts.dll" targetDir

        installAndTest
            installationPackage
            "multipleFiles.msi"
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
            |> installFiles ["FsInst.dll"; "FsInst.Facts.dll"] into (InstallationDrive/"Test")

        let targetFile1 = sprintf @"%sTest\FsInst.dll" targetDir
        let targetFile2 = sprintf @"%sTest\FsInst.Facts.dll" targetDir

        installAndTest
            installationPackage
            "multipleFiles.msi"
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
            |> createFolder subFolder
            |> installFile "FsInst.dll" into folder
            |> installFile "FsInst.Facts.dll" into subFolder

        let targetFile1 = sprintf @"%sTest\FsInst.dll" targetDir
        let targetFile2 = sprintf @"%sTest\SubFolder\FsInst.Facts.dll" targetDir

        installAndTest
            installationPackage
            "hierarchicalfolders.msi"
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
            |> installFile "FsInst.dll" into ProgramFiles
        
        let programFilesFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles)
        let installedFile = Path.Combine(programFilesFolder, "FsInst.dll")
        
        installAndTest
            installationPackage
            "programFilesFolder.msi"
            (fun () -> File.Exists(installedFile) |> should be True)
            (fun () -> File.Exists(installedFile) |> should be False)