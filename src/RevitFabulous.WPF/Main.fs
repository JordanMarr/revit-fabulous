namespace RevitFabulous.WPF

/// Used for testing UI without Revit as a standalone app.
module Main = 
    open System

    [<EntryPoint>]
    [<STAThread>]
    let main(_args) =

        Controller.showCounterDialog()
        0
