// Copyright 2018 Fabulous contributors. See LICENSE.md for license.
namespace RevitFabulous.WPF
open Fabulous.Core
open Fabulous.DynamicViews
open Xamarin.Forms
open RevitFabulous.Domain

/// A page that will display all elements in the Revit model, grouped by category.
/// Also has a "Refresh" button that will refresh the elements.
module ModelQueryPage = 
    type Model = 
      { Elements: ModelQuery.Element seq
        GetElements: unit -> ModelQuery.Element seq }

    type Msg = 
        | Refresh

    /// Initializes Elements to empty, and injects a function that gets elements from the Revit model.
    /// Also returns a Cmd that will trigger the "Refresh" message.
    let init (getElements: unit -> ModelQuery.Element seq) = 
        { Elements = Seq.empty
          GetElements = getElements }, Cmd.ofMsg Refresh

    let update msg model =
        match msg with
        | Refresh -> { model with Elements = model.GetElements() }, Cmd.none

    let view (model: Model) dispatch =

        let elementsByCategory = 
            model.Elements
            |> Seq.groupBy (fun e -> e.Category)

        let listElements (elements: ModelQuery.Element seq) =
            View.ScrollView(
                View.StackLayout([
                    for e in elements do
                        yield View.Label(e.Name)
                ])
            )

        View.ContentPage(
            View.ScrollView(
                View.StackLayout(
                    padding = 20.0, 
                    verticalOptions = LayoutOptions.Center,
                    children = [ 
                        for (catName, elements) in elementsByCategory do
                            yield View.Label(catName, fontAttributes = FontAttributes.Bold)
                            yield listElements elements

                        yield View.Button(text = "Refresh", horizontalOptions = LayoutOptions.Center, command = (fun () -> dispatch Refresh))
                    ]
                )
            )
        )

    /// Boostraps the page
    let program (getElements: unit -> ModelQuery.Element seq) = 
        let i = fun () -> init getElements
        Program.mkProgram i update view

    /// Provides a parameterless function with stubbed data to be used by LiveUpdate
    let programLiveUpdate =
        let getElementsStub = fun () -> seq {
            yield { ModelQuery.Element.Category = "Things"; ModelQuery.Element.Name = "Thing 1" }
            yield { ModelQuery.Element.Category = "Things"; ModelQuery.Element.Name = "Thing 2" }
            yield { ModelQuery.Element.Category = "Stuff"; ModelQuery.Element.Name = "Element 1" }
            yield { ModelQuery.Element.Category = "Stuff"; ModelQuery.Element.Name = "Element 2" }
        }
        program getElementsStub
                
        
