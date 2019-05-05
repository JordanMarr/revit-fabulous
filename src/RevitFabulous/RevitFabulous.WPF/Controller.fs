namespace RevitFabulous.WPF

open Xamarin.Forms
open Xamarin.Forms.Platform.WPF
open Fabulous.Core
open Fabulous.DynamicViews

type private MainWindow() = 
    inherit FormsApplicationPage()

type private GenericApp<'model, 'msg> (program : Program<'model, 'msg, ('model -> ('msg -> unit)-> ViewElement)>) as app =
    inherit Application ()
    let runner = 
        program
#if DEBUG
        |> Program.withConsoleTrace
#endif
        |> Program.runWithDynamicView app

#if DEBUG
    do runner.EnableLiveUpdate()
#endif    

/// Load UI pages here.
module Controller =

    let mutable private initialized = false

    let private init() =
        if not initialized then
            let app = if isNull System.Windows.Application.Current then System.Windows.Application() else System.Windows.Application.Current
            app.ShutdownMode <- System.Windows.ShutdownMode.OnExplicitShutdown // This is key to allow reuse of MainWindow.
            if not Forms.IsInitialized then Forms.Init()
            initialized <- true
    
    let showDialog program =
        init()
        let win = MainWindow()
        program |> GenericApp |> win.LoadApplication 
        win.ShowDialog() |> ignore

    let getWindow program =
        init()
        let win = MainWindow()
        program |> GenericApp |> win.LoadApplication
        win :> System.Windows.Window