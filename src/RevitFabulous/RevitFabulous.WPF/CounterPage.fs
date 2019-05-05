// Copyright 2018 Fabulous contributors. See LICENSE.md for license.
namespace RevitFabulous.WPF
open Fabulous.Core
open Fabulous.DynamicViews
open Xamarin.Forms

module CounterPage = 
    type Model = 
      { Count : int
        Step : int
        TimerOn: bool }

    type Msg = 
        | Increment 
        | Decrement 
        | Reset
        | SetStep of int
        | TimerToggled of bool
        | TimedTick
        | Refresh

    let initModel = { Count = 0; Step = 1; TimerOn=false }

    let init () = 
        let m = initModel
        m, Cmd.none

    let timerCmd = 
        async { do! Async.Sleep 200
                return TimedTick }
        |> Cmd.ofAsyncMsg

    let update msg model =
        let (m, cmd) =
            match msg with
            | Increment -> { model with Count = model.Count + model.Step }, Cmd.none
            | Decrement -> { model with Count = model.Count - model.Step }, Cmd.none
            | Reset -> init ()
            | SetStep n -> { model with Step = n }, Cmd.none
            | TimerToggled on -> { model with TimerOn = on }, (if on then timerCmd else Cmd.none)
            | TimedTick -> 
                if model.TimerOn then 
                    { model with Count = model.Count + model.Step }, timerCmd
                else 
                    model, Cmd.none
            | Refresh ->
                let m = ModelStorage.readModel() |> Option.defaultValue model
                m, Cmd.none
        
        m, cmd

    let view (model: Model) dispatch =
        View.ContentPage(
            View.StackLayout(
                padding = 20.0, 
                verticalOptions = LayoutOptions.Center,
                children = [ 
                    View.Label(text = sprintf "%d" model.Count, horizontalOptions = LayoutOptions.Center, widthRequest=200.0, horizontalTextAlignment=TextAlignment.Center)
                    View.Button(text = "Increment (+)", command = (fun () -> dispatch Increment), horizontalOptions = LayoutOptions.Center, widthRequest = 100.)
                    View.Button(text = "Decrement (-)", command = (fun () -> dispatch Decrement), horizontalOptions = LayoutOptions.Center)
                    View.Label(text = "Timer", horizontalOptions = LayoutOptions.Center)
                    View.Switch(isToggled = model.TimerOn, toggled = (fun on -> dispatch (TimerToggled on.Value)), horizontalOptions = LayoutOptions.Center)
                    View.Slider(minimumMaximum = (0.0, 10.0), value = double model.Step, valueChanged = (fun args -> dispatch (SetStep (int (args.NewValue + 0.5)))), horizontalOptions = LayoutOptions.FillAndExpand)
                    View.Label(text = sprintf "Step size: %d" model.Step, horizontalOptions = LayoutOptions.Center) 
                    View.Button(text = "Reset", horizontalOptions = LayoutOptions.Center, command = (fun () -> dispatch Reset), canExecute = (model <> initModel))
                    View.Button(text = "Refresh", horizontalOptions = LayoutOptions.Center, command = (fun () -> dispatch Refresh))
                ]))
                    
    let program = 
#if DEBUG
        // Keep track of Model state so that it can be reloaded during live update
        Program.mkProgram 
            (fun () ->
                let m,c = init()
                let model = ModelStorage.readModel() |> Option.defaultValue m
                model,c
            )
            (fun msg model -> 
                let (m,c) = update msg model
                ModelStorage.saveModel m
                m,c
            )
            view
#else
        Program.mkProgram init update view
#endif