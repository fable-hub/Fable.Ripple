namespace Fable.Ripple.Form

open System.Collections.Generic
open Fable.Ripple
open Fable.Ripple.Dom

module Error =

    /// <summary>
    /// Why a field or a form has no output.
    /// </summary>
    type Error =
        /// <summary>
        /// The field is empty and not wrapped in <see cref="M:Fable.Ripple.Form.Base.optional"/>.
        /// </summary>
        | RequiredFieldIsEmpty
        /// <summary>
        /// The field's parser returned <c>Error</c>; the string is its message.
        /// </summary>
        | ValidationFailed of string
        /// <summary>
        /// An error computed outside the parser: <see cref="M:Fable.Ripple.Form.Base.withExternalError"/>,
        /// <see cref="M:Fable.Ripple.Form.Base.validateAsync"/> or <see cref="T:Fable.Ripple.Form.FieldConfig`3"/>'s <c>Error</c>.
        /// Renderers show it whatever the <see cref="T:Fable.Ripple.Form.Validation"/> mode.
        /// </summary>
        | External of string

/// <summary>
/// The contract of a field's attributes: the id the form knows the field by.
///
/// Any record can be a field's attributes by implementing it. Renderers put the id on the control
/// and use it for the label's <c>for</c> and the help text.
/// </summary>
type IAttributes =
    abstract GetFieldId: unit -> string

/// <summary>
/// When a field starts showing its own error.
///
/// External errors are shown as soon as they exist, whatever the mode.
/// </summary>
type Validation =
    /// <summary>
    /// After the field lost focus, and after a failed submit.
    /// </summary>
    | ValidateOnBlur
    /// <summary>
    /// After a failed submit only.
    /// </summary>
    | ValidateOnSubmit
    /// <summary>
    /// Once the field is dirty or lost focus, and after a failed submit.
    /// </summary>
    | ValidateOnChange

/// <summary>
/// Which errors a view shows: every one after a failed submit, and one field's after it lost focus.
///
/// Create one per form with <see cref="M:Fable.Ripple.Form.ErrorVisibility.create"/> and hand it to the view.
/// </summary>
[<NoComparison; NoEquality>]
type ErrorVisibility =
    {
        /// <summary>
        /// Set by the view after a failed submit. Every field shows its error while it is <c>true</c>.
        /// </summary>
        ShowAllErrors: Var<bool>
        /// <summary>
        /// One flag per field id, set when the field lost focus.
        /// </summary>
        Touched: Dictionary<string, Var<bool>>
    }

    /// <summary>
    /// A visibility with nothing shown.
    /// </summary>
    static member create() : ErrorVisibility =
        {
            ShowAllErrors = Var.create false
            Touched = Dictionary<string, Var<bool>>()
        }

    /// <summary>
    /// The touched flag of a field, created on first use.
    /// </summary>
    /// <param name="id">The field id</param>
    member this.TouchedFlag(id: string) : Var<bool> =
        match this.Touched.TryGetValue id with
        | true, flag -> flag
        | _ ->
            let flag = Var.create false
            this.Touched.[id] <- flag
            flag

    /// <summary>
    /// Hides every error again: clears <c>ShowAllErrors</c> and every touched flag.
    /// </summary>
    member this.Reset() =
        Signal.batch (fun () ->
            this.ShowAllErrors.Value <- false

            for flag in this.Touched.Values do
                flag.Value <- false
        )

/// <summary>
/// What the combinators tell a renderer about a subtree.
///
/// A view builds the root context with <see cref="M:Fable.Ripple.Form.Base.renderContext"/>; combinators such as
/// <see cref="M:Fable.Ripple.Form.Base.disableIf"/> or <see cref="M:Fable.Ripple.Form.Base.withLabel"/> derive a
/// new one for their subtree. Every function member is a tracked read.
/// </summary>
[<NoComparison; NoEquality>]
type RenderContext =
    {
        /// <summary>
        /// The fields are disabled.
        /// </summary>
        Disabled: unit -> bool
        /// <summary>
        /// The fields are read-only.
        /// </summary>
        ReadOnly: unit -> bool
        /// <summary>
        /// Every field shows its error, after a failed submit.
        /// </summary>
        ShowAllErrors: unit -> bool
        /// <summary>
        /// No field shows its error, whatever the other flags; set by <see cref="M:Fable.Ripple.Form.Base.optional"/> while the form is empty.
        /// </summary>
        HideErrors: unit -> bool
        /// <summary>
        /// The touched flag of a field id.
        /// </summary>
        Touched: string -> Var<bool>
        /// <summary>
        /// When the fields show their own errors.
        /// </summary>
        Validation: Validation
        /// <summary>
        /// A label replacing the fields' own, from <see cref="M:Fable.Ripple.Form.Base.withLabel"/>.
        /// </summary>
        Label: (unit -> string) option
        /// <summary>
        /// An error applied to every field of the subtree, from <see cref="M:Fable.Ripple.Form.Base.withExternalError"/>
        /// or <see cref="M:Fable.Ripple.Form.Base.validateAsync"/>.
        /// </summary>
        ExternalError: unit -> Error.Error option
        /// <summary>
        /// An async check is running on the subtree.
        /// </summary>
        Validating: unit -> bool
        /// <summary>
        /// The renderer's own item function, for nested items.
        /// </summary>
        RenderItems: RenderContext -> Item list -> DomItem list
    }

/// <summary>
/// One node of a form's render tree.
///
/// A renderer maps each case to markup; nested items come back to it through
/// <see cref="P:Fable.Ripple.Form.RenderContext.RenderItems"/>.
/// </summary>
and [<NoComparison; NoEquality>] Item =
    /// <summary>
    /// One field, from <see cref="M:Fable.Ripple.Form.Base.field"/>. The id is the one of its attributes.
    /// </summary>
    | FieldItem of id: string * render: (RenderContext -> DomItem)
    /// <summary>
    /// Anything else: a branch of <see cref="M:Fable.Ripple.Form.Base.andThen"/>, a subtree of
    /// <see cref="M:Fable.Ripple.Form.Base.showIf"/>, a layout from <see cref="M:Fable.Ripple.Form.Base.wrap"/>.
    /// </summary>
    | Dynamic of render: (RenderContext -> DomItem)

/// <summary>
/// How a form's fields map to a plain record, written by the caller for
/// <see cref="M:Fable.Ripple.Form.Base.load"/> and <see cref="M:Fable.Ripple.Form.Base.snapshot"/>.
/// </summary>
[<NoComparison; NoEquality>]
type FormValues<'Values> =
    {
        /// <summary>
        /// Reads the record from the fields' <c>Var</c>s. Built from <c>.Value</c> reads it is a tracked read;
        /// built from <c>.Peek()</c> it is not.
        /// </summary>
        Snapshot: unit -> 'Values
        /// <summary>
        /// Writes the record into the fields' <c>Var</c>s.
        /// </summary>
        Load: 'Values -> unit
    }

/// <summary>
/// One field's value as the form sees it, without its type.
/// </summary>
[<NoComparison; NoEquality>]
type FieldHandle =
    {
        Id: string
        /// <summary>
        /// Writes the initial value back, or the one at the last <c>Commit</c>.
        /// </summary>
        Reset: unit -> unit
        /// <summary>
        /// Makes the current value the one <c>Reset</c> returns to.
        /// </summary>
        Commit: unit -> unit
        /// <summary>
        /// Tracked read: the value differs from the initial or committed one.
        /// </summary>
        IsDirty: unit -> bool
    }

/// <summary>
/// A form: its render tree and its state as signals.
///
/// Built from fields with <see cref="M:Fable.Ripple.Form.Base.succeed"/> and
/// <see cref="M:Fable.Ripple.Form.Base.append"/>; every field holds its value in a <c>Var</c> and nothing
/// here is a function of a values record.
/// </summary>
[<NoComparison; NoEquality>]
type Form<'Output> =
    {
        /// <summary>
        /// The render tree, consumed by a view.
        /// </summary>
        Items: Item list
        /// <summary>
        /// The parsed output, or the first error and the others.
        /// </summary>
        Result: Signal<Result<'Output, Error.Error * Error.Error list>>
        /// <summary>
        /// Every field is empty.
        /// </summary>
        IsEmpty: Signal<bool>
        /// <summary>
        /// An async check is running.
        /// </summary>
        Validating: Signal<bool>
        /// <summary>
        /// Tracked read of every field, for <c>reset</c>, <c>commit</c> and <c>isDirty</c>.
        /// </summary>
        Fields: unit -> FieldHandle list
    }

/// <summary>
/// How one field behaves. Built with <see cref="M:Fable.Ripple.Form.Field.create"/>, or written by hand
/// for an external error.
/// </summary>
[<NoComparison; NoEquality>]
type FieldConfig<'Attributes, 'Input, 'Output> =
    {
        /// <summary>
        /// Turns the value into the typed output, or a message. Runs when <c>Value</c> changes and when any
        /// signal it reads changes. An empty value does not reach it.
        /// </summary>
        Parser: 'Input -> Result<'Output, string>
        /// <summary>
        /// The field's value. The control writes it; anything can read or write it.
        /// </summary>
        Value: Var<'Input>
        /// <summary>
        /// Tracked read of an error computed outside the parser, such as one returned by a server.
        /// The field fails while it returns <c>Some</c>.
        /// </summary>
        Error: unit -> string option
        /// <summary>
        /// Data specific to the field kind: label, placeholder, options.
        /// </summary>
        Attributes: 'Attributes
    }

/// <summary>
/// What a field's render function receives from <see cref="M:Fable.Ripple.Form.Base.field"/>.
/// Every function member is a tracked read.
/// </summary>
[<NoComparison; NoEquality>]
type FieldRenderConfig<'Input, 'Attributes> =
    {
        /// <summary>
        /// The field id, for the control and the label's <c>for</c>.
        /// </summary>
        Id: string
        /// <summary>
        /// The value to bind the control to.
        /// </summary>
        Value: Var<'Input>
        /// <summary>
        /// The field's error, its own or an external one from the context.
        /// </summary>
        Error: Signal<Error.Error option>
        /// <summary>
        /// The view wants the error shown, per the <see cref="T:Fable.Ripple.Form.Validation"/> mode and the error visibility.
        /// </summary>
        ShowError: unit -> bool
        /// <summary>
        /// The field lost focus once.
        /// </summary>
        Touched: unit -> bool
        /// <summary>
        /// The value differs from the initial or committed one.
        /// </summary>
        Dirty: unit -> bool
        Disabled: unit -> bool
        ReadOnly: unit -> bool
        /// <summary>
        /// To call from the control's <c>blur</c> event; sets the touched flag.
        /// </summary>
        OnBlur: unit -> unit
        /// <summary>
        /// A label replacing the attributes' own, from <see cref="M:Fable.Ripple.Form.Base.withLabel"/>.
        /// </summary>
        Label: (unit -> string) option
        /// <summary>
        /// An async check is running.
        /// </summary>
        Validating: unit -> bool
        Attributes: 'Attributes
    }

type Field =

    /// <summary>
    /// The config of a field with no external error.
    ///
    /// <para>
    /// <code lang="fsharp">
    /// TextField.create "email"
    /// |> TextField.withLabel "Email"
    /// |> Field.create email (fun value -> if value.Contains "@" then Ok value else Error "An email needs an @")
    /// |> Form.textField
    /// </code>
    /// </para>
    /// </summary>
    /// <param name="value">The field's <c>Var</c></param>
    /// <param name="parser">Turns the value into the output, or a message; <c>Ok</c> when the output is the input</param>
    /// <param name="attributes">The field kind's attributes</param>
    /// <returns>A <see cref="T:Fable.Ripple.Form.FieldConfig`3"/> to give to a renderer's field constructor</returns>
    static member create
        (value: Var<'Input>)
        (parser: 'Input -> Result<'Output, string>)
        (attributes: 'Attributes)
        : FieldConfig<'Attributes, 'Input, 'Output>
        =
        {
            Parser = parser
            Value = value
            Error = fun () -> None
            Attributes = attributes
        }
