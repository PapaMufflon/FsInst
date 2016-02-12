namespace FsInst

module FileSystem =
    open System.Diagnostics
    open System.IO
    open FsUnit.Xunit
    open FsInst.Core
    open Xunit

    [<Fact>]
    let ``creates a folder if there is an item in it`` () =
        let Test = InstallationDrive/"Test"

        InstallationPackage
        |> copyright "Acme Inc."
        |> createFolder Test
        |> installFile "FsInst.dll" into Test
        |> msi "createFolder.msi"
        |> ignore
            
        use installProcess = Process.Start(@"C:\Windows\System32\msiexec.exe", " /i createFolder.msi /quiet")
        installProcess.WaitForExit()
        
        File.Exists(@"C:\Test\FsInst.dll") |> should be True

        use uninstallProcess = Process.Start(@"C:\Windows\System32\msiexec.exe", " /x createFolder.msi /quiet")
        uninstallProcess.WaitForExit()

        File.Exists(@"C:\Test\FsInst.dll") |> should be False

        File.Delete("createFolder.msi")

    [<Fact>]
    let ``can have multiple files in one folder`` () =
        let Test = InstallationDrive/"Test"

        InstallationPackage
        |> copyright "Acme Inc."
        |> createFolder Test
        |> installFiles ["FsInst.dll"; "FsInst.Facts.dll"] into Test
        |> msi "multipleFiles.msi"
        |> ignore

        use installProcess = Process.Start(@"C:\Windows\System32\msiexec.exe", " /i multipleFiles.msi /quiet")
        installProcess.WaitForExit()
        
        File.Exists(@"C:\Test\FsInst.dll") |> should be True
        File.Exists(@"C:\Test\FsInst.Facts.dll") |> should be True

        use uninstallProcess = Process.Start(@"C:\Windows\System32\msiexec.exe", " /x multipleFiles.msi /quiet")
        uninstallProcess.WaitForExit()

        File.Exists(@"C:\Test\FsInst.dll") |> should be False
        File.Exists(@"C:\Test\FsInst.Facts.dll") |> should be False

        File.Delete("multipleFiles.msi")

    [<Fact>]
    let ``can have hierarchical folders`` () =
        let folder = InstallationDrive/"Test"
        let subFolder = folder/"SubFolder"

        InstallationPackage
        |> copyright "Acme Inc."
        |> createFolder subFolder
        |> installFile "FsInst.dll" into folder
        |> installFile "FsInst.Facts.dll" into subFolder
        |> msi "hierarchicalFolders.msi"
        |> ignore

        use installProcess = Process.Start(@"C:\Windows\System32\msiexec.exe", " /i hierarchicalFolders.msi /quiet")
        installProcess.WaitForExit()
        
        File.Exists(@"C:\Test\FsInst.dll") |> should be True
        File.Exists(@"C:\Test\SubFolder\FsInst.Facts.dll") |> should be True

        use uninstallProcess = Process.Start(@"C:\Windows\System32\msiexec.exe", " /x hierarchicalFolders.msi /quiet")
        uninstallProcess.WaitForExit()

        File.Exists(@"C:\Test\FsInst.dll") |> should be False
        File.Exists(@"C:\Test\SubFolder\FsInst.Facts.dll") |> should be False

        File.Delete("hierarchicalFolders.msi")