namespace RevitFabulous.WPF

/// Used for testing UI without Revit as a standalone app.
module Main = 
    open System
    open Xamarin.Forms

    [<EntryPoint>]
    [<STAThread>]
    let main(_args) =

        Controller.showCounterDialog()
        0

        //let app = new System.Windows.Application()
        //Forms.Init()
        //let window = MainWindow() 
        
        //CounterPage.program 
        //|> GenericApp
        //|> window.LoadApplication

        //app.Run(window)

