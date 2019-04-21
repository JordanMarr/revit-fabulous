namespace RevitFabulous.Revit
open Autodesk.Revit.UI
open Autodesk.Revit.DB
open Autodesk.Revit.Attributes
open RevitFabulous.WPF
open RevitFabulous.Domain

[<Transaction(TransactionMode.Manual)>]
type ModelQueryCommand() =

    let getRevitElements doc =
        (new FilteredElementCollector(doc)).OfClass(typedefof<Element>)
        |> Seq.cast<Element>

    // Converts a Revit element to domain representation
    let toElement (revitEl: Element) = 
        { ModelQuery.Element.Category = revitEl.Category.Name
          ModelQuery.Element.Name = revitEl.Name }

    interface IExternalCommand with
        member this.Execute(commandData: ExternalCommandData, message: byref<string>, elements: ElementSet): Result = 
            try
                
                let doc = commandData.Application.ActiveUIDocument.Document

                let getElements() =
                    getRevitElements doc
                    |> Seq.map toElement
                    |> Seq.sortBy (fun e -> e.Category)
                
                ModelQueryPage.program getElements |> Controller.showDialog

                Result.Succeeded

            with ex ->
                Result.Failed
