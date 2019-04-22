namespace RevitFabulous.Revit
open Autodesk.Revit.UI
open Autodesk.Revit.DB
open Autodesk.Revit.Attributes
open RevitFabulous.WPF
open RevitFabulous.Domain

[<Transaction(TransactionMode.Manual)>]
type ViewManagerCommand() =

    let getRevitViews doc =
        (new FilteredElementCollector(doc)).OfClass(typedefof<View>)
        |> Seq.cast<View>
        |> Seq.filter (fun v -> not v.IsTemplate)
        |> Seq.filter (fun v -> 
            match v with
            | :? ViewPlan -> true
            | :? ViewSchedule -> true
            | :? View3D -> true
            | :? ViewSection -> true
            | _ -> false
        )
        |> Seq.sortBy (fun v -> v.Name)

    interface IExternalCommand with
        member this.Execute(commandData: ExternalCommandData, message: byref<string>, elements: ElementSet): Result = 
            try
                let uiDoc = commandData.Application.ActiveUIDocument
                let doc = uiDoc.Document

                let getViews() = 
                    getRevitViews doc
                    |> Seq.map (fun v -> 
                        { ViewManager.View.Id = v.Id.IntegerValue
                          ViewManager.View.Name = v.Name
                          ViewManager.View.IsActive = uiDoc.ActiveView.Id.IntegerValue = v.Id.IntegerValue }
                    )

                let setActiveView (view: ViewManager.View) =
                    match getRevitViews doc |> Seq.tryFind (fun rv -> rv.Id.IntegerValue = view.Id) with
                    | Some revitView -> uiDoc.ActiveView <- revitView
                    | None -> ()
                
                // Init page with dependencies and show dialog
                ViewManagerPage.program getViews setActiveView 
                |> Controller.showDialog

                Result.Succeeded

            with ex ->
                Result.Failed
