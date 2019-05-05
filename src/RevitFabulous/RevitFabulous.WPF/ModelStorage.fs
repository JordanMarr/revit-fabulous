namespace RevitFabulous.WPF

module ModelStorage =
    open System
    open MBrace.FsPickler.Json
    open Microsoft.FSharp.Reflection
    open System.Reflection

    // TODO: Store in app properties once it is working
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
