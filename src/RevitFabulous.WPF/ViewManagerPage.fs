// Copyright 2018 Fabulous contributors. See LICENSE.md for license.
namespace RevitFabulous.WPF
open Fabulous.Core
open Fabulous.DynamicViews
open Xamarin.Forms
open RevitFabulous.Domain
open RevitFabulous.Domain.ViewManager

/// A page that will display all views in the Revit model, and allows user to set one as active.
/// Also has a "Refresh" button that will refresh the elements.
module ViewManagerPage = 
    type Model = 
      { Views: View seq
        GetViews: unit -> View seq
        SetActiveView: View -> unit }

    type Msg = 
        | Refresh
        | SetActive of View

    /// Initializes Views to empty, and injects a functions for getting views and setting the active view.
    /// Finally, returns a Cmd that will trigger the "Refresh" message to load the Views.
    let init (getViews: unit -> View seq) (setActiveView: View -> unit) = 
        { Views = Seq.empty
          GetViews = getViews
          SetActiveView = setActiveView }, Cmd.ofMsg Refresh

    let update msg model =
        match msg with
        | Refresh -> 
            { model with Views = model.GetViews() }, Cmd.none
        | SetActive view -> 
            model.SetActiveView view
            { model with Views = model.GetViews() }, Cmd.none

    let view (model: Model) (dispatch: Msg -> unit) =
        View.ContentPage(
            title = "View Manager",
            content = View.ScrollView(
                View.StackLayout(
                    padding = 20.0, 
                    horizontalOptions = LayoutOptions.Center,
                    verticalOptions = LayoutOptions.Center,
                    children = [ 
                        for view in model.Views do

                            // Using "Fabulous Simple Elements" library:
                            yield Grid.grid [
                                Grid.Columns ([ GridLength 100.; GridLength.Star ])

                                Grid.Children [
                                    Button.button [
                                        Button.Text (if view.IsActive then "Active" else "Set Active")
                                        Button.GridColumn 0
                                        Button.OnClick (fun () -> dispatch (SetActive view))
                                        Button.CanExecute (not view.IsActive)
                                    ]

                                    Label.label [
                                        Label.Text view.Name
                                        Label.GridColumn 1
                                        Label.FontSize FontSize.Micro
                                        Label.FontAttributes (if view.IsActive then FontAttributes.Bold else FontAttributes.None)
                                    ]   
                                ]
                            ]
                    ]
                )
            )
        )

    /// Boostraps the page
    let program (getViews: unit -> View seq) (setActiveView: View -> unit) = 
        let i = fun () -> init getViews setActiveView
        Program.mkProgram i update view

    /// Provides a parameterless function with stubbed data to be used by LiveUpdate
    let programLiveUpdate =

        let getViewsStub = fun () -> seq {
            yield { View.Id = 1; Name = "View 1"; IsActive = false }
            yield { View.Id = 2; Name = "View 2"; IsActive = true }
            yield { View.Id = 3; Name = "View 3"; IsActive = false }
            yield { View.Id = 4; Name = "View 4"; IsActive = false }
        }

        let setActiveViewStub view =
            ()

        program getViewsStub setActiveViewStub
                
        
