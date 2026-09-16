namespace Fable.Ripple.Form.Plain

open System.Collections.Generic
open Browser.Types
open Fable.Ripple
open Fable.Ripple.Dom
open Fable.Ripple.Form

module WizardView =

    [<NoComparison; NoEquality>]
    type Config<'Output> =
        {
            OnSubmit: 'Output -> unit
            State: Var<View.State>
            Validation: Validation
            Back: string
            Next: string
            Submit: string
        }

    let private stepList (steps: Step list) (current: Var<int>) =
        Html.ul
            [
                attr.className "rf-steps"

                for index, step in List.indexed steps do
                    Html.li
                        [
                            attr.className "rf-steps__segment"
                            attr.toggleClass (
                                "rf-steps__segment--active",
                                (fun () -> current.Value = index)
                            )
                            attr.toggleClass (
                                "rf-steps__segment--done",
                                (fun () -> current.Value > index)
                            )
                            Html.text step.Title
                        ]
            ]

    let asHtml (config: Config<'Output>) (wizard: Wizard<'Output>) : DomItem =
        let current = Var.create 0
        let errorsByStep = Dictionary<int, ErrorVisibility>()
        let mutable formElement: HTMLElement option = None

        let errorsOf (index: int) =
            match errorsByStep.TryGetValue index with
            | true, errors -> errors
            | _ ->
                let errors = ErrorVisibility.create ()
                errorsByStep.[index] <- errors
                errors

        let steps () = Signal.untracked wizard.Steps

        let isLast () =
            current.Peek() >= List.length (steps ()) - 1

        let showErrorsOf (index: int) =
            (errorsOf index).ShowAllErrors.Value <- true
            View.focusFirstInvalid formElement

        let advance () =
            let index = current.Peek()
            let step = List.item index (steps ())

            View.whenSettled
                step.Form.Validating
                (fun () ->
                    match step.Form.Result.Peek() with
                    | Error _ -> showErrorsOf index
                    | Ok() ->
                        if isLast () then
                            View.whenSettled
                                wizard.Form.Validating
                                (fun () ->
                                    match wizard.Form.Result.Peek() with
                                    | Ok output -> config.OnSubmit output
                                    | Error _ ->
                                        // A later step invalidated an earlier one: go back to the first step that fails.
                                        let failing =
                                            steps ()
                                            |> List.tryFindIndex (fun step ->
                                                Result.isError (step.Form.Result.Peek())
                                            )

                                        failing
                                        |> Option.iter (fun index ->
                                            current.Value <- index
                                            showErrorsOf index
                                        )
                                )
                        else
                            current.Value <- index + 1
                )

        Html.form
            [
                attr.ref (fun element -> formElement <- Some element)

                on.submit (fun ev ->
                    ev.stopPropagation ()
                    ev.preventDefault ()

                    if config.State.Peek() <> View.Loading then
                        advance ()
                )

                Html.dynamic (fun () -> stepList (wizard.Steps()) current)

                Html.dynamic (fun () ->
                    let steps = wizard.Steps()
                    let index = min current.Value (List.length steps - 1)
                    let step = List.item index steps

                    let context =
                        View.renderContext
                            PlainView.renderItems
                            {
                                OnSubmit = ignore
                                State = config.State
                                ErrorVisibility = errorsOf index
                                Action = View.Action.SubmitOnly ""
                                Validation = config.Validation
                            }

                    Html.div
                        [
                            attr.className "rf-wizard-step"
                            Html.h2
                                [
                                    attr.className "rf-wizard-step__title"
                                    Html.text step.Title
                                ]
                            yield! PlainView.renderItems context step.Form.Items
                        ]
                )

                Html.dynamic (fun () ->
                    match config.State.Value with
                    | View.Error error -> PlainView.errorMessage error
                    | View.Success success ->
                        Html.p
                            [
                                attr.className "rf-status rf-status--success"
                                Html.text success
                            ]
                    | View.Loading
                    | View.ReadOnly
                    | View.Idle -> Html.none
                )

                Html.div
                    [
                        attr.className "rf-actions"
                        Html.button
                            [
                                attr.type' "button"
                                attr.className "rf-button"
                                attr.disabled (fun () ->
                                    current.Value = 0 || config.State.Value = View.Loading
                                )
                                on.click (fun _ -> current.Value <- max 0 (current.Peek() - 1))
                                Html.text config.Back
                            ]
                        Html.button
                            [
                                attr.type' "submit"
                                attr.className "rf-button rf-button--primary"
                                attr.toggleClass (
                                    "rf-button--loading",
                                    (fun () -> config.State.Value = View.Loading)
                                )
                                Html.text (fun () ->
                                    if current.Value >= List.length (wizard.Steps()) - 1 then
                                        config.Submit
                                    else
                                        config.Next
                                )
                            ]
                    ]
            ]
