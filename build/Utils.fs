module EasyBuild.Utils

open System

/// `npx` is a `.cmd` shim on Windows, which `CreateProcess` does not resolve on its own.
let npx =
    if OperatingSystem.IsWindows() then
        "npx.cmd"
    else
        "npx"
