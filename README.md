# FsInst
> A functional way to tackle the Windows Installer.

The goal of this project should be to create a terse, easy to learn and testable wrapper for the Windows Installer API.

## Terse
```fsharp
let Test = Folder("Test", None, []) // this will go away...

// configure installer content
let installationPackage =
    InstallationPackage
    |> copyright "Acme Inc."
    |> createFolder Test
    |> installFile "FsInst.dll" into Test

// create msi file
let msi = installationPackage |> msi "setup.msi"
```

## Testable
```fsharp
// test in-memory content of installer data
let simulation = simulate installationPackage

(simulation.FileSystem.InstallationDrive/"Test").Name |> should equal Test.Name

// load and test msi file
let simulationOfMsi = FsInst.Simulation.Msi.simulate msi

(simulationOfMsi.FileSystem.InstallationDrive/"Test").Name |> should equal Test.Name
```

## First Goal
Being able to reproduce the [getting started example of the WiX Toolset tutorial](https://www.firegiant.com/wix/tutorial/getting-started/) approximately like this:

```fsharp
InstallationPackage
|> copyright "Acme Ltd."
|> installing
    { Product with
        Name = "Foobar 1.0";
        Language = ``en-US``
        Version = V 1 0 0 }
    using
    { Installer with
        Description = "Acme's Foobar 1.0 Installer";
        Comments = "Foobar is a registered trademark of Acme Ltd."
        Keywords = "Installer"
        Language = ``en-US``
        MinimumVersion = V 2 0 0
        Compressed = Yes }
|> createFolder ProgramFiles64/Manufacturer/Product
|> createFolder ProgramMenuFolder/Product
|> installFiles [
    Solution.FoobarAppl10.Executable
    Solution.FoobarAppl10.References
    Solution.FoobarAppl10.``Manual.pdf``] into ProgramFiles64/Manufacturer/Product
|> installFiles [
    { Shortcut with
        File = ProgramFiles64/Manufacturer/Product/Solution.FoobarAppl10.Executable.Filename
        Name = Property.Product
        Icon = takeIcon Solution.FoobarAppl10.Executable 0
        WorkingDirectory = ProgramFiles64/Manufacturer/Product }
    { Shortcut with
        File = ProgramFiles64/Manufacturer/Product/``Manual.pdf``
        Name = "Instruction Manual" } ] into ProgramMenuFolder/Product
|> installFiles
    { Shortcut with
        File = Solution.FoobarAppl10.Executable
        Name = Property.``Foobar 1.0``
        Icon = takeIcon Solution.FoobarAppl10.Executable 0
        WorkingDirectory = ProgramFiles64/Manufacturer/Product } into DesktopFolder
|> msi "setup.msi"
```