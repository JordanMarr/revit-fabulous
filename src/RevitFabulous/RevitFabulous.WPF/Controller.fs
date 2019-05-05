namespace RevitFabulous.WPF

open Xamarin.Forms
open Xamarin.Forms.Platform.WPF
open Fabulous.Core
open Fabulous.DynamicViews
open RevitFabulous.Domain
open System

module ModelStorage =
    open MBrace.FsPickler.Json
    open Microsoft.FSharp.Reflection
    open System.Reflection

    let path = @"C:\Windows\Temp\model.json"
    let serializer = JsonSerializer(indent = true)
    let utf8 = Text.UTF8Encoding(false)

    module private Reflection = 
        let isOption (t: System.Type) = 
            t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<Option<_>>  

        /// Gets the value of a property.  
        /// If property is an Option<> type, returns the Option<>.Value.
        let getPropertyValue (o: obj) (p: PropertyInfo) =
            let pVal = p.GetValue(o)
            if isOption(p.PropertyType) then
                    match pVal with
                    | null -> null  // x is None
                    | _ -> match pVal.GetType().GetProperty("Value") with
                            | null -> null  // x is not an option
                            | prop -> prop.GetValue(pVal, null)
            else pVal

        let convertToRecordValue<'model> (model: 'model) =
            FSharpType.GetRecordFields(typeof<'model>, BindingFlags.Public)
            |> Array.map (getPropertyValue model)
            |> FSharp.Compiler.PortaCode.Interpreter.RecordValue

    let saveModel model =
        let recordValue =
            match (model :> obj) with
            | :? FSharp.Compiler.PortaCode.Interpreter.RecordValue as rv -> rv
            | _ -> model |> Reflection.convertToRecordValue

        let json = 
            use stream = new IO.MemoryStream()
            serializer.Serialize(stream, recordValue)
            stream.ToArray() |> utf8.GetString

        IO.File.WriteAllText(path, json)

    let readModel<'model>() : 'model option =
        if System.IO.File.Exists(path) then
            try
                let json = System.IO.File.ReadAllText(path)
                use reader = new IO.StringReader(json)
                                
                let recordValue = serializer.Deserialize<FSharp.Compiler.PortaCode.Interpreter.RecordValue>(reader)                
                let (FSharp.Compiler.PortaCode.Interpreter.RecordValue values) = recordValue
                if typeof<'model> = typeof<obj>
                then (box recordValue) :?> 'model |> Some
                else FSharp.Reflection.FSharpValue.MakeRecord(typeof<'model>, values) :?> 'model |> Some
            with ex ->
                None
        else
            None

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