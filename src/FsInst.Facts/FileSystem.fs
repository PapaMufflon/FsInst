namespace FsInst

module FileSystem =
    open System.Diagnostics
    open System.IO
    open Xunit
    open FsUnit.Xunit
    open FsInst.Core
    open FsInst.Simulation
    open FsInst.Simulation.InstallationPackage

    [<Fact>]
    let ``creates a folder if there will be an item in it`` () =
        let Test = Folder("Test", None, [])

        let installationPackage =
            InstallationPackage
            |> copyright "Acme Inc."
            |> createFolder Test
            |> installFile "FsInst.dll" into Test

        let simulation = simulate installationPackage

        (simulation.FileSystem.InstallationDrive/"Test").Name |> should equal Test.Name

        let simulationOfMsi = 
            installationPackage
            |> msi "createFolder.msi"
            |> FsInst.Simulation.Msi.simulate
            
        (simulationOfMsi.FileSystem.InstallationDrive/"Test").Name |> should equal Test.Name

        use installProcess = Process.Start(@"C:\Windows\System32\msiexec.exe", " /i createFolder.msi /quiet")
        installProcess.WaitForExit()
        
        File.Exists(@"C:\Test\FsInst.dll") |> should be True

        use uninstallProcess = Process.Start(@"C:\Windows\System32\msiexec.exe", " /x createFolder.msi /quiet")
        uninstallProcess.WaitForExit()

        File.Exists(@"C:\Test\FsInst.dll") |> should be False

        File.Delete("createFolder.msi")