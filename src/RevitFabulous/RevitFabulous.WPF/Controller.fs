namespace RevitFabulous.WPF

open Xamarin.Forms
open Xamarin.Forms.Platform.WPF
open Fabulous.Core
open Fabulous.DynamicViews
open RevitFabulous.Domain
open System

module ModelStorage =
    open MBrace.FsPickler.Json
    let path = @"C:\Windows\Temp\model.json"
    let serializer = JsonSerializer(indent = true)
    let utf8 = Text.UTF8Encoding(false)

    let saveModel(model: 'model) =
        let json = 
            use stream = new IO.MemoryStream()
            serializer.Serialize(stream, model)
            stream.ToArray() |> utf8.GetString

        IO.File.WriteAllText(path, json)

    let readModel<'model>() : 'model option =
        if System.IO.File.Exists(path) then
            try
                let json = System.IO.File.ReadAllText(path)
                use reader = new IO.StringReader(json)
                
                try
                    serializer.Deserialize<'model>(reader) |> Some
                with ex ->
                    let recordValue = serializer.Deserialize<FSharp.Compiler.PortaCode.Interpreter.RecordValue>(reader)
                    let (FSharp.Compiler.PortaCode.Interpreter.RecordValue values) = recordValue
                    FSharp.Reflection.FSharpValue.MakeRecord(typeof<'model>, values) :?> 'model |> Some

                //match serializer.Deserialize<obj>(reader) with
                //| :? FSharp.Compiler.PortaCode.Interpreter.RecordValue as recordValue ->
                //    let (FSharp.Compiler.PortaCode.Interpreter.RecordValue values) = recordValue
                //    FSharp.Reflection.FSharpValue.MakeRecord(typeof<'model>, values) :?> 'model |> Some
                //| _ as o -> 
                //    o :?> 'model |> Some
                //let recordValue = o :?> FSharp.Compiler.PortaCode.Interpreter.RecordValue
                //let recordValue = serializer.Deserialize<FSharp.Compiler.PortaCode.Interpreter.RecordValue>(reader)
                //let (FSharp.Compiler.PortaCode.Interpreter.RecordValue values) = recordValue
                //FSharp.Reflection.FSharpValue.MakeRecord(typeof<'model>, values) :?> 'model |> Some
            with ex ->
                None
        else
            None


    //let readModel() : 'model option =
    //    if System.IO.File.Exists(path) then
    //        try
    //            let json = System.IO.File.ReadAllText(path)
    //            use reader = new IO.StringReader(json)
    //            Some (serializer.Deserialize<'model>(reader))
    //        with ex ->
    //            None
    //    else
    //        None

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