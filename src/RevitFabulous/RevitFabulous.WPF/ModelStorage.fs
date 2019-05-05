namespace RevitFabulous.WPF

open Fabulous.Core

module ModelStorage =
    open System
    open MBrace.FsPickler.Json
    open Microsoft.FSharp.Reflection
    open System.Reflection

    module private FileStore =
        let private path = @"C:\Windows\Temp\fabulous-model.json"
    
        let saveJson json =
            IO.File.WriteAllText(path, json)

        let loadJson() =
            if System.IO.File.Exists(path) then
                let json = System.IO.File.ReadAllText(path)
                if not (json |> String.IsNullOrWhiteSpace)
                then Some json
                else None
            else
                None

    // TODO: this does not work correctly because LiveUpdate creates a new Application.Current.
    module private AppPropertyStore = 
        let saveJson (json: string) =
            System.Windows.Application.Current.Properties.["model"] <- json

        let loadJson() =
            if System.Windows.Application.Current.Properties.Contains("model") then 
                let json = System.Windows.Application.Current.Properties.["model"]
                Some (json :?> string)
            else
                None

    let private saveJson = FileStore.saveJson
    let private loadJson = FileStore.loadJson

    let private serializer = JsonSerializer(indent = true)
    let private utf8 = Text.UTF8Encoding(false)

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

    /// Converts the current model to a RecordValue and then serializes 
    /// it us FsPickler to ensure that any functions are preserved.
    let saveModel model =
        let json = 
            try
                let recordValue =
                    match (model :> obj) with
                    | :? FSharp.Compiler.PortaCode.Interpreter.RecordValue as rv -> rv
                    | _ -> model |> Reflection.convertToRecordValue

            
                use stream = new IO.MemoryStream()
            
                serializer.Serialize(stream, recordValue)
                stream.ToArray() |> utf8.GetString

            with ex ->
                System.Windows.MessageBox.Show(ex.ToString()) |> ignore
                ""
            
        saveJson(json)

    /// Reads the given model (stored as a serialized RecordValue),
    /// and then returns it to the caller as either a simple record (as defined by the user),
    /// or as a RecordValue (as defined by PortaCode / LiveUpdate.
    let readModel<'model>() : 'model option =
        match loadJson() with
        | Some json ->
            try
                use reader = new IO.StringReader(json)
                                
                let recordValue = serializer.Deserialize<FSharp.Compiler.PortaCode.Interpreter.RecordValue>(reader)                
                let (FSharp.Compiler.PortaCode.Interpreter.RecordValue values) = recordValue
                if typeof<'model> = typeof<obj>
                then (box recordValue) :?> 'model |> Some
                else FSharp.Reflection.FSharpValue.MakeRecord(typeof<'model>, values) :?> 'model |> Some
            with ex ->
                None
        | None ->
            None
            
    let persistModelDuringLiveUpdate (program: Program<'model, 'msg, _>) =
            let msInit () =
                let initModel,cmd = program.init ()
                let model = readModel() |> Option.defaultValue initModel
                model,cmd

            let msUpdate msg model =
                let newModel,cmd = program.update msg model
                saveModel newModel
                newModel,cmd
                
            let msView model dispatch =
                program.view model dispatch
                    
            { program with
                init = msInit 
                update = msUpdate
                view = msView }
