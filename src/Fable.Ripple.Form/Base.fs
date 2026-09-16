module Fable.Ripple.Form.Base

open System
open Fable.Core
open Fable.Ripple
open Fable.Ripple.Dom

type private FormResult<'Output> = Result<'Output, Error.Error * Error.Error list>

// Cutoff of the Result signals: outputs may lack equality, so every recompute notifies.
let private never (_: 'a) (_: 'a) = false

// The value is a built child (a form or a wizard): a new object is the change, not its structure.
let private sameChild (a: 'a option) (b: 'a option) =
    match a, b with
    | None, None -> true
    | Some a, Some b -> obj.ReferenceEquals(a, b)
    | _ -> false

let private mapContext (f: RenderContext -> RenderContext) (item: Item) : Item =
    match item with
    | FieldItem(id, render) -> FieldItem(id, fun context -> render (f context))
    | Dynamic render -> Dynamic(fun context -> render (f context))

let private withContext (f: RenderContext -> RenderContext) (form: Form<'Output>) : Form<'Output> =
    { form with
        Items = List.map (mapContext f) form.Items
    }

/// <summary>
/// A form with no field whose output is <paramref name="output"/>.
///
/// Start a composition with the function that builds the output, then <see cref="M:Fable.Ripple.Form.Base.append"/> the fields.
/// An argument can be discarded, as for a repeat-password field:
///
/// <para>
/// <code lang="fsharp">
/// Form.succeed (fun password _ -> password)
/// |> Form.append passwordField
/// |> Form.append repeatPasswordField
/// </code>
/// </para>
/// </summary>
/// <param name="output">The value the form yields</param>
/// <returns>A form with no items, always <c>Ok output</c>, empty, and not validating</returns>
let succeed (output: 'Output) : Form<'Output> =
    {
        Items = []
        Result = Signal.constant (Ok output)
        IsEmpty = Signal.constant true
        Validating = Signal.constant false
        Fields = fun () -> []
    }

/// <summary>
/// Transforms the output of a form.
///
/// The fields, the emptiness, the async state and the handles are those of <paramref name="form"/>;
/// only <c>Result</c> changes. An <c>Error</c> passes through untouched.
/// </summary>
/// <param name="fn">Applied to the output</param>
/// <param name="form">The form whose output is transformed</param>
/// <returns>A form whose output is <c>fn</c> of the given form's output</returns>
let map (fn: 'A -> 'B) (form: Form<'A>) : Form<'B> =
    {
        Items = form.Items
        Result = Signal.computedWith never (fun () -> Result.map fn form.Result.Value)
        IsEmpty = form.IsEmpty
        Validating = form.Validating
        Fields = form.Fields
    }

/// <summary>
/// Appends a form to another one and feeds its output to the function the other one yields.
///
/// The errors of both forms are collected: the result is <c>Error</c> with the first error and the
/// others as soon as either form fails.
/// </summary>
/// <param name="newForm">The form to append</param>
/// <param name="current">The form to append to, whose output is a function</param>
/// <returns>A form with the items of both, whose output is the function applied to the new form's output</returns>
let append (newForm: Form<'A>) (current: Form<'A -> 'B>) : Form<'B> =
    {
        Items = current.Items @ newForm.Items
        Result =
            Signal.computedWith
                never
                (fun () ->
                    match current.Result.Value, newForm.Result.Value with
                    | Ok fn, next -> Result.map fn next
                    | Error errors, Ok _ -> Error errors
                    | Error(first, others), Error(newFirst, newOthers) ->
                        Error(first, others @ (newFirst :: newOthers))
                )
        IsEmpty = Signal.computed (fun () -> current.IsEmpty.Value && newForm.IsEmpty.Value)
        Validating =
            Signal.computed (fun () -> current.Validating.Value || newForm.Validating.Value)
        Fields = fun () -> current.Fields() @ newForm.Fields()
    }

/// <summary>
/// A form with one field.
///
/// The field's <c>Result</c> is computed from its <c>Var</c>: <see cref="F:Fable.Ripple.Form.Error.Error.RequiredFieldIsEmpty"/>
/// while <paramref name="isEmpty"/> holds, then the parser's result, then the config's external error.
/// The render function is called by the view with a <see cref="T:Fable.Ripple.Form.FieldRenderConfig`2"/>
/// built from the config and the <see cref="T:Fable.Ripple.Form.RenderContext"/> of the subtree.
///
/// Every field constructor of a renderer is this function with a render function:
///
/// <para>
/// <code lang="fsharp">
/// let textField config = Base.field System.String.IsNullOrEmpty Fields.text config
/// </code>
/// </para>
/// </summary>
/// <param name="isEmpty">Whether a value counts as empty, for the required check and <c>IsEmpty</c></param>
/// <param name="render">Draws the field</param>
/// <param name="config">How the field behaves</param>
/// <returns>A form with one item and one handle</returns>
let field
    (isEmpty: 'Input -> bool)
    (render: FieldRenderConfig<'Input, 'Attributes> -> DomItem)
    (config: FieldConfig<'Attributes, 'Input, 'Output>)
    : Form<'Output> when 'Attributes :> IAttributes
    =
    let id = config.Attributes.GetFieldId()

    let result: Signal<FormResult<'Output>> =
        Signal.computedWith
            never
            (fun () ->
                let value = config.Value.Value

                if isEmpty value then
                    Error(Error.RequiredFieldIsEmpty, [])
                else
                    match config.Parser value with
                    | Error error -> Error(Error.ValidationFailed error, [])
                    | Ok output ->
                        match config.Error() with
                        | Some error -> Error(Error.External error, [])
                        | None -> Ok output
            )

    let error =
        Signal.computed (fun () ->
            match result.Value with
            | Ok _ -> None
            | Error(first, _) -> Some first
        )

    let initial = Var.create (config.Value.Peek())

    let dirty () =
        not (Unchecked.equals config.Value.Value initial.Value)

    let handle =
        {
            Id = id
            Reset = fun () -> config.Value.Value <- initial.Peek()
            Commit = fun () -> initial.Value <- config.Value.Peek()
            IsDirty = dirty
        }

    {
        Items =
            [
                FieldItem(
                    id,
                    fun context ->
                        let touched = context.Touched id

                        render
                            {
                                Id = id
                                Value = config.Value
                                Error =
                                    Signal.computed (fun () ->
                                        match error.Value with
                                        | Some error -> Some error
                                        | None -> context.ExternalError()
                                    )
                                ShowError =
                                    fun () ->
                                        not (context.HideErrors())
                                        && (context.ShowAllErrors()
                                            || (
                                                match context.Validation with
                                                | ValidateOnSubmit -> false
                                                | ValidateOnBlur -> touched.Value
                                                | ValidateOnChange -> touched.Value || dirty ()
                                            ))
                                Touched = fun () -> touched.Value
                                Dirty = dirty
                                Disabled = context.Disabled
                                ReadOnly = context.ReadOnly
                                OnBlur = fun () -> touched.Value <- true
                                Label = context.Label
                                Validating = context.Validating
                                Attributes = config.Attributes
                            }
                )
            ]
        Result = result
        IsEmpty = Signal.computed (fun () -> isEmpty config.Value.Value)
        Validating = Signal.constant false
        Fields = fun () -> [ handle ]
    }

/// <summary>
/// Adds an error computed outside the form, such as one returned by a server, to every field of a form.
///
/// The form fails while <paramref name="error"/> returns <c>Some</c>; the message is shown on every field
/// without waiting for a submit.
/// </summary>
/// <param name="error">Tracked read of the error</param>
/// <param name="form">The form the error applies to</param>
/// <returns>The form, failing while the error exists</returns>
let withExternalError (error: unit -> string option) (form: Form<'Output>) : Form<'Output> =
    let external () = error () |> Option.map Error.External

    let form' =
        form
        |> withContext (fun context ->
            { context with
                ExternalError =
                    fun () ->
                        match external () with
                        | Some error -> Some error
                        | None -> context.ExternalError()
            }
        )

    { form' with
        Result =
            Signal.computedWith
                never
                (fun () ->
                    match form.Result.Value, external () with
                    | Ok _, Some error -> Error(error, [])
                    | result, _ -> result
                )
    }

/// <summary>
/// Runs an asynchronous check on the output of a form once it parses, after a debounce.
///
/// <c>Validating</c> is <c>true</c> while the check runs. A rejection is shown on every field as an
/// <see cref="F:Fable.Ripple.Form.Error.Error.External"/> error. A result that arrives after the output
/// changed again is dropped. A view waits for a pending check before submitting.
/// </summary>
/// <param name="debounceMs">Milliseconds after the last change before the check starts</param>
/// <param name="check">The check, given the parsed output</param>
/// <param name="form">The form to check</param>
/// <returns>The form, failing while the last check rejected its output</returns>
let validateAsync
    (debounceMs: int)
    (check: 'Output -> JS.Promise<Result<unit, string>>)
    (form: Form<'Output>)
    : Form<'Output>
    =
    let asyncError: Var<Error.Error option> = Var.create None
    let validating = Var.create false
    let mutable generation = 0
    let mutable timer: float option = None

    let cancel () =
        generation <- generation + 1
        timer |> Option.iter Browser.Dom.window.clearTimeout
        timer <- None

    Signal.effect (fun () ->
        let result = form.Result.Value

        Signal.untracked (fun () ->
            cancel ()

            match result with
            | Error _ ->
                Signal.batch (fun () ->
                    asyncError.Value <- None
                    validating.Value <- false
                )
            | Ok output ->
                let current = generation
                validating.Value <- true

                timer <-
                    Browser.Dom.window.setTimeout (
                        (fun () ->
                            timer <- None

                            check output
                            |> Promise.iter (fun outcome ->
                                if current = generation then
                                    Signal.batch (fun () ->
                                        asyncError.Value <-
                                            match outcome with
                                            | Ok() -> None
                                            | Error message -> Some(Error.External message)

                                        validating.Value <- false
                                    )
                            )
                        ),
                        debounceMs
                    )
                    |> Some
        )
    )
    |> ignore

    Signal.onCleanup cancel

    let form' =
        form
        |> withContext (fun context ->
            { context with
                ExternalError =
                    fun () ->
                        match asyncError.Value with
                        | Some error -> Some error
                        | None -> context.ExternalError()
                Validating = fun () -> context.Validating() || validating.Value
            }
        )

    { form' with
        Result =
            Signal.computedWith
                never
                (fun () ->
                    match form.Result.Value, asyncError.Value with
                    | Ok _, Some error -> Error(error, [])
                    | result, _ -> result
                )
        Validating = Signal.computed (fun () -> form.Validating.Value || validating.Value)
    }

/// <summary>
/// Builds a value in its own <c>Signal.root</c> each time a source yields one, disposing the previous one.
///
/// The building block of <see cref="M:Fable.Ripple.Form.Base.andThen"/> and <c>Wizard.andThen</c>.
/// </summary>
/// <param name="source">Tracked read of the input; <c>None</c> disposes the current value and yields <c>None</c></param>
/// <param name="build">Builds the value from an input</param>
/// <returns>The current value, changing when a new one is built</returns>
let switchRoot (source: unit -> 'A option) (build: 'A -> 'T) : Var<'T option> =
    let current: Var<'T option> = Var.createWith sameChild None
    let mutable owned: IDisposable option = None

    let release () =
        owned |> Option.iter (fun dispose -> dispose.Dispose())
        owned <- None

    Signal.effect (fun () ->
        let value = source ()

        Signal.untracked (fun () ->
            release ()

            current.Value <-
                value
                |> Option.map (fun value ->
                    let built, dispose = Signal.root (fun () -> build value)
                    owned <- Some dispose
                    built
                )
        )
    )
    |> ignore

    Signal.onCleanup release
    current

/// <summary>
/// <see cref="M:Fable.Ripple.Form.Base.andThen"/> over a value that carries a form, such as a wizard.
/// </summary>
/// <param name="child">Builds the value from the parent's output</param>
/// <param name="formOf">Reads the form out of the value</param>
/// <param name="parent">The form whose output the child depends on</param>
/// <returns>The combined form, and the current value for the caller to read</returns>
let andThenWith
    (child: 'A -> 'T)
    (formOf: 'T -> Form<'B>)
    (parent: Form<'A>)
    : Form<'B> * Var<'T option>
    =
    let parentOutput =
        Signal.computed (fun () ->
            match parent.Result.Value with
            | Ok output -> Some output
            | Error _ -> None
        )

    let current = switchRoot (fun () -> parentOutput.Value) child
    let currentForm () = current.Value |> Option.map formOf

    let form =
        {
            Items =
                parent.Items
                @ [
                    Dynamic(fun context ->
                        Html.dynamic (fun () ->
                            match currentForm () with
                            | Some form ->
                                Html.div [ yield! context.RenderItems context form.Items ]
                            | None -> Html.none
                        )
                    )
                ]
            Result =
                Signal.computedWith
                    never
                    (fun () ->
                        match currentForm () with
                        | Some form -> form.Result.Value
                        | None ->
                            match parent.Result.Value with
                            | Error errors -> Error errors
                            | Ok _ ->
                                failwith "andThen: parent succeeded but no child form was built"
                    )
            IsEmpty =
                Signal.computed (fun () ->
                    parent.IsEmpty.Value
                    && (
                        match currentForm () with
                        | Some form -> form.IsEmpty.Value
                        | None -> true
                    )
                )
            Validating =
                Signal.computed (fun () ->
                    parent.Validating.Value
                    || (
                        match currentForm () with
                        | Some form -> form.Validating.Value
                        | None -> false
                    )
                )
            Fields =
                fun () ->
                    parent.Fields()
                    @ (
                        match currentForm () with
                        | Some form -> form.Fields()
                        | None -> []
                    )
        }

    form, current

/// <summary>
/// Builds the rest of a form from the output of its first part.
///
/// The child form is built when the parent's output changes and disposed when it changes again, in its
/// own <c>Signal.root</c>. Values live in the <c>Var</c>s the child reads, so a child built again finds them.
/// While the parent fails, the result is the parent's errors.
/// </summary>
/// <param name="child">Builds the child form from the parent's output</param>
/// <param name="parent">The form filled first</param>
/// <returns>A form with the parent's items followed by the current child's, whose output is the child's</returns>
let andThen (child: 'A -> Form<'B>) (parent: Form<'A>) : Form<'B> =
    fst (andThenWith child id parent)

/// <summary>
/// Makes a form optional.
///
/// While every field is empty, the output is <c>Ok None</c> and no error is shown. Once a field has a
/// value, the form must parse: the output is then <c>Ok (Some value)</c>.
/// </summary>
/// <param name="form">The form to make optional</param>
/// <returns>A form whose output is an option</returns>
let optional (form: Form<'Output>) : Form<'Output option> =
    let form' =
        form
        |> withContext (fun context ->
            { context with
                HideErrors = fun () -> context.HideErrors() || form.IsEmpty.Value
            }
        )

    {
        Items = form'.Items
        Result =
            Signal.computedWith
                never
                (fun () ->
                    match form.Result.Value with
                    | Ok value -> Ok(Some value)
                    | Error errors ->
                        if form.IsEmpty.Value then
                            Ok None
                        else
                            Error errors
                )
        IsEmpty = form.IsEmpty
        Validating = form.Validating
        Fields = form.Fields
    }

/// <summary>
/// Shows a form while a condition holds.
///
/// Hidden, the form renders nothing, yields <c>Ok None</c>, reports no error and no handle. Its fields keep
/// their values, so showing it again restores them.
/// </summary>
/// <param name="condition">Tracked read of the condition</param>
/// <param name="form">The form to show</param>
/// <returns>A form whose output is <c>Some</c> while shown</returns>
let showIf (condition: unit -> bool) (form: Form<'Output>) : Form<'Output option> =
    let shown = Signal.computed condition

    {
        Items =
            [
                Dynamic(fun context ->
                    Html.show (
                        (fun () -> shown.Value),
                        fun () -> Html.div [ yield! context.RenderItems context form.Items ]
                    )
                )
            ]
        Result =
            Signal.computedWith
                never
                (fun () ->
                    if shown.Value then
                        Result.map Some form.Result.Value
                    else
                        Ok None
                )
        IsEmpty = Signal.computed (fun () -> not shown.Value || form.IsEmpty.Value)
        Validating = Signal.computed (fun () -> shown.Value && form.Validating.Value)
        Fields =
            fun () ->
                if shown.Value then
                    form.Fields()
                else
                    []
    }

/// <summary>
/// Disables every field of a form.
/// </summary>
/// <param name="form">The form to disable</param>
/// <returns>The form, its fields disabled</returns>
let disable (form: Form<'Output>) : Form<'Output> =
    form
    |> withContext (fun context ->
        { context with
            Disabled = fun () -> true
        }
    )

/// <summary>
/// Disables every field of a form while a condition holds.
/// </summary>
/// <param name="condition">Tracked read of the condition</param>
/// <param name="form">The form to disable</param>
/// <returns>The form, its fields disabled while the condition holds</returns>
let disableIf (condition: unit -> bool) (form: Form<'Output>) : Form<'Output> =
    form
    |> withContext (fun context ->
        { context with
            Disabled = fun () -> context.Disabled() || condition ()
        }
    )

/// <summary>
/// Makes every field of a form read-only.
/// </summary>
/// <param name="form">The form to make read-only</param>
/// <returns>The form, its fields read-only</returns>
let readOnly (form: Form<'Output>) : Form<'Output> =
    form
    |> withContext (fun context ->
        { context with
            ReadOnly = fun () -> true
        }
    )

/// <summary>
/// Replaces the label of every field of a form.
/// </summary>
/// <param name="label">Tracked read of the label</param>
/// <param name="form">The form whose labels are replaced</param>
/// <returns>The form, its fields labelled with the read</returns>
let withLabel (label: unit -> string) (form: Form<'Output>) : Form<'Output> =
    form
    |> withContext (fun context ->
        { context with
            Label = Some label
        }
    )

/// <summary>
/// Sets when the fields of a form start showing their own errors, overriding the view's default.
/// </summary>
/// <param name="validation">The mode</param>
/// <param name="form">The form the mode applies to</param>
/// <returns>The form, its fields validated in that mode</returns>
let validateOn (validation: Validation) (form: Form<'Output>) : Form<'Output> =
    form
    |> withContext (fun context ->
        { context with
            Validation = validation
        }
    )

/// <summary>
/// Writes every field's baseline back: its initial value, or the value at the last <see cref="M:Fable.Ripple.Form.Base.commit"/>.
/// </summary>
/// <param name="form">The form to reset</param>
let reset (form: Form<'Output>) : unit =
    Signal.batch (fun () -> Signal.untracked form.Fields |> List.iter (fun field -> field.Reset()))

/// <summary>
/// Makes the current values the baseline <see cref="M:Fable.Ripple.Form.Base.reset"/> returns to and
/// <see cref="M:Fable.Ripple.Form.Base.isDirty"/> compares with.
/// </summary>
/// <param name="form">The form to commit</param>
let commit (form: Form<'Output>) : unit =
    Signal.batch (fun () -> Signal.untracked form.Fields |> List.iter (fun field -> field.Commit()))

/// <summary>
/// Writes a record into the fields in one batch, then commits, so the form is clean afterwards.
/// </summary>
/// <param name="values">How the record maps to the fields</param>
/// <param name="form">The form to commit</param>
/// <param name="data">The record to write</param>
let load (values: FormValues<'Values>) (form: Form<'Output>) (data: 'Values) : unit =
    Signal.batch (fun () ->
        values.Load data
        commit form
    )

/// <summary>
/// Reads a record from the fields.
/// </summary>
/// <param name="values">How the record maps to the fields</param>
/// <returns>The record</returns>
let snapshot (values: FormValues<'Values>) : 'Values = values.Snapshot()

/// <summary>
/// Tracked read: whether any field differs from its initial or last committed value.
/// </summary>
/// <param name="form">The form to read</param>
let isDirty (form: Form<'Output>) () : bool =
    form.Fields() |> List.exists (fun field -> field.IsDirty())

/// <summary>
/// Makes every field of a form read-only while a condition holds.
/// </summary>
/// <param name="condition">Tracked read of the condition</param>
/// <param name="form">The form to make read-only</param>
/// <returns>The form, its fields read-only while the condition holds</returns>
let readOnlyIf (condition: unit -> bool) (form: Form<'Output>) : Form<'Output> =
    form
    |> withContext (fun context ->
        { context with
            ReadOnly = fun () -> context.ReadOnly() || condition ()
        }
    )

/// <summary>
/// Replaces the items of a form with one element drawn by a function, which receives them to place in its
/// markup with <c>context.RenderItems context items</c>.
///
/// The hook a renderer builds sections, groups and other layouts from:
///
/// <para>
/// <code lang="fsharp">
/// let section title form =
///     form
///     |> Base.wrap (fun context items ->
///         Html.fieldset [ Html.legend title; yield! context.RenderItems context items ])
/// </code>
/// </para>
/// </summary>
/// <param name="render">Draws the element around the items</param>
/// <param name="form">The form whose items are wrapped</param>
/// <returns>The form with one item</returns>
let wrap (render: RenderContext -> Item list -> DomItem) (form: Form<'Output>) : Form<'Output> =
    let items = form.Items

    { form with
        Items = [ Dynamic(fun context -> render context items) ]
    }

/// <summary>
/// The context a view hands the root items.
/// </summary>
/// <param name="renderItems">The renderer's item function</param>
/// <param name="errors">Which errors are shown</param>
/// <param name="validation">When fields show their own errors</param>
/// <param name="disabled">Tracked read: the whole form is disabled</param>
/// <param name="readOnly">Tracked read: the whole form is read-only</param>
/// <returns>A context with no label and no external error</returns>
let renderContext
    (renderItems: RenderContext -> Item list -> DomItem list)
    (errors: ErrorVisibility)
    (validation: Validation)
    (disabled: unit -> bool)
    (readOnly: unit -> bool)
    : RenderContext
    =
    {
        Disabled = disabled
        ReadOnly = readOnly
        ShowAllErrors = fun () -> errors.ShowAllErrors.Value
        HideErrors = fun () -> false
        Touched = errors.TouchedFlag
        Validation = validation
        Label = None
        ExternalError = fun () -> None
        Validating = fun () -> false
        RenderItems = renderItems
    }

/// <summary>
/// Runs an action once no async check is pending, as a check may still reject the value.
///
/// Runs it at once when nothing is pending.
/// </summary>
/// <param name="validating">The form's <c>Validating</c> signal</param>
/// <param name="action">What to run</param>
let whenSettled (validating: Signal<bool>) (action: unit -> unit) : unit =
    if validating.Peek() then
        let mutable subscription: IDisposable option = None

        subscription <-
            Some(
                Signal.effect (fun () ->
                    if not validating.Value then
                        subscription |> Option.iter (fun s -> s.Dispose())
                        Signal.untracked action
                )
            )
    else
        action ()
