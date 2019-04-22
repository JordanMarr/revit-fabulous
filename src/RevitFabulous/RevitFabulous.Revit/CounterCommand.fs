namespace RevitFabulous.Revit
open Autodesk.Revit.UI
open Autodesk.Revit.DB
open Autodesk.Revit.Attributes
open RevitFabulous.WPF

[<Transaction(TransactionMode.Manual)>]
type CounterCommand() =
    interface IExternalCommand with
        member this.Execute(commandData: ExternalCommandData, message: byref<string>, elements: ElementSet): Result = 
            try
                CounterPage.program |> Controller.showDialog
                
                Result.Succeeded

            with ex ->
                Result.Failed
