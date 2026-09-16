namespace Fable.Ripple.Form.Plain

open Browser.Types
open Fable.Ripple
open Fable.Ripple.Dom
open Fable.Ripple.Form

/// The view state, the ids and ARIA wiring, error texts and the submit handling.
module View =

    type State =
        | Idle
        | Loading
        | ReadOnly
        | Error of string
        | Success of string

    [<RequireQualifiedAccess; NoComparison; NoEquality>]
    type Action =
        | SubmitOnly of string
        | Custom of (Signal<State> -> DomItem)

    let errorVisibility () : ErrorVisibility = ErrorVisibility.create ()

    [<NoComparison; NoEquality>]
    type ViewConfig<'Output> =
        {
            OnSubmit: 'Output -> unit
            State: Var<State>
            ErrorVisibility: ErrorVisibility
            Action: Action
            Validation: Validation
        }

    let errorToString (error: Error.Error) =
        match error with
        | Error.RequiredFieldIsEmpty -> "This field is required"
        | Error.ValidationFailed error -> error
        | Error.External error -> error

    /// External errors are shown as soon as they exist, the others once the view asks for them.
    let shownError (config: FieldRenderConfig<'Input, 'Attributes>) () : string option =
        match config.Error.Value with
        | Some(Error.External error) -> Some error
        | Some error when config.ShowError() -> Some(errorToString error)
        | _ -> None

    let labelText (label: string) (config: FieldRenderConfig<'Input, 'Attributes>) : DomItem =
        match config.Label with
        | Some label -> Html.text label
        | None -> Html.text label

    let helpId (config: FieldRenderConfig<'Input, 'Attributes>) = config.Id + "-help"

    /// `id`, `aria-invalid` and `aria-describedby` for the element the field's label points to.
    let controlAttributes (config: FieldRenderConfig<'Input, 'Attributes>) : DomItem =
        Html.fragment
            [
                attr.id config.Id
                attr.custom ("aria-describedby", helpId config)
                attr.custom (
                    "aria-invalid",
                    (fun () ->
                        if (shownError config ()).IsSome then
                            "true"
                        else
                            "false"
                    )
                )
            ]

    let renderContext
        (renderItems: RenderContext -> Item list -> DomItem list)
        (config: ViewConfig<'Output>)
        : RenderContext
        =
        Base.renderContext
            renderItems
            config.ErrorVisibility
            config.Validation
            (fun () -> config.State.Value = Loading)
            (fun () -> config.State.Value = ReadOnly)

    let whenSettled (validating: Signal<bool>) (action: unit -> unit) : unit =
        Base.whenSettled validating action

    // Event handlers run inside a batch: `aria-invalid` is only written once they return.
    let focusFirstInvalid (formElement: HTMLElement option) : unit =
        Browser.Dom.window.setTimeout (
            (fun () ->
                formElement
                |> Option.bind (fun element ->
                    element.querySelector "[aria-invalid=true]" |> Option.ofObj
                )
                |> Option.iter (fun element -> (element :?> HTMLElement).focus ())
            ),
            0
        )
        |> ignore

    /// The `submit` handler of a renderer's `<form>`: submits, or shows every error and focuses the first.
    let onSubmit
        (config: ViewConfig<'Output>)
        (form: Form<'Output>)
        (formElement: unit -> HTMLElement option)
        =
        on.submit (fun ev ->
            ev.stopPropagation ()
            ev.preventDefault ()

            if config.State.Peek() <> Loading then
                Base.whenSettled
                    form.Validating
                    (fun () ->
                        match form.Result.Peek() with
                        | Result.Ok output -> config.OnSubmit output
                        | Result.Error _ ->
                            config.ErrorVisibility.ShowAllErrors.Value <- true
                            focusFirstInvalid (formElement ())
                    )
        )
