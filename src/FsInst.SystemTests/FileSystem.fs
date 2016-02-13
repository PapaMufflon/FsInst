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

    [<Fact>]
    let ``creates a folder if there is an item in it`` () =
        let Test = InstallationDrive/"Test"

        let installationPackage =
            InstallationPackage
            |> copyright "Acme Inc."
            |> createFolder Test
            |> installFile "FsInst.dll" into Test

        installAndTest
            installationPackage
            "createFolder.msi"
            (fun () -> File.Exists(@"C:\Test\FsInst.dll") |> should be True)
            (fun () -> File.Exists(@"C:\Test\FsInst.dll") |> should be False)
            
    [<Fact>]
    let ``can have multiple files in one folder`` () =
        let Test = InstallationDrive/"Test"

        let installationPackage =
            InstallationPackage
            |> copyright "Acme Inc."
            |> createFolder Test
            |> installFiles ["FsInst.dll"; "FsInst.Facts.dll"] into Test

        installAndTest
            installationPackage
            "multipleFiles.msi"
            (fun () ->
                File.Exists(@"C:\Test\FsInst.dll") |> should be True
                File.Exists(@"C:\Test\FsInst.Facts.dll") |> should be True)
            (fun () ->
                File.Exists(@"C:\Test\FsInst.dll") |> should be False
                File.Exists(@"C:\Test\FsInst.Facts.dll") |> should be False)

    [<Fact>]
    let ``can have hierarchical folders`` () =
        let folder = InstallationDrive/"Test"
        let subFolder = folder/"SubFolder"

        let installationPackage =
            InstallationPackage
            |> copyright "Acme Inc."
            |> createFolder subFolder
            |> installFile "FsInst.dll" into folder
            |> installFile "FsInst.Facts.dll" into subFolder

        installAndTest
            installationPackage
            "hierarchicalfolders.msi"
            (fun () ->
                File.Exists(@"C:\Test\FsInst.dll") |> should be True
                File.Exists(@"C:\Test\SubFolder\FsInst.Facts.dll") |> should be True)
            (fun () ->
                File.Exists(@"C:\Test\FsInst.dll") |> should be False
                File.Exists(@"C:\Test\SubFolder\FsInst.Facts.dll") |> should be False)

    [<Fact>]
    let ``defines a variable for the program files folder`` () =
        let installationPackage =
            InstallationPackage
            |> copyright "Acme Inc."
            |> installFile "FsInst.dll" into ProgramFiles
        
        let programFilesFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles)
        let installedFile = Path.Combine(programFilesFolder, "FsInst.dll")

        installAndTest
            installationPackage
            "programFilesFolder.msi"
            (fun () -> File.Exists(installedFile) |> should be True)
            (fun () -> File.Exists(installedFile) |> should be False)