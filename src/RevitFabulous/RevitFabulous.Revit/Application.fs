namespace RevitFabulous.Revit
open Autodesk.Revit.UI
open Autodesk.Revit.DB
open Autodesk.Revit.Attributes
open System

type Application() =
    interface IExternalApplication with

        member this.OnStartup(app: UIControlledApplication) =

            let dir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)

            // bindingRedirect for FSharp.Core (fixes issue when using FsPickler)
            AppDomain.CurrentDomain.add_AssemblyResolve(ResolveEventHandler(fun snd e ->
                if e.Name.Contains("FSharp.Core.resources") then
                    let filename = System.IO.Path.Combine(dir, "FSharp.Core.resources.dll")
                    if System.IO.File.Exists(filename)
                    then System.Reflection.Assembly.LoadFrom(filename)
                    else null
                elif e.Name.Contains("FSharp.Core") then
                    let filename = System.IO.Path.Combine(dir, "FSharp.Core.dll")
                    if System.IO.File.Exists(filename)
                    then System.Reflection.Assembly.LoadFrom(filename)
                    else null
                else
                    null
                ))
            Result.Succeeded

        member this.OnShutdown(application: UIControlledApplication) = 
            Result.Succeeded
