module Platform

open System.Runtime.InteropServices

let isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
let isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)