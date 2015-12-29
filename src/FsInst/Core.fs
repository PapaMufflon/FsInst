namespace FsInst

module Core =
    open System

    type InstallationPackage = {
        Manufacturer : string
    }

    let copyright manufacturer installationPackage =
        { installationPackage with Manufacturer = manufacturer }

    let InstallationPackage = {
        Manufacturer = String.Empty
    }