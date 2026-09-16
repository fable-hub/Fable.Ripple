namespace Fable.Ripple.Form.Plain

open Fable.Ripple.Dom
open Fable.Ripple.Form

[<RequireQualifiedAccess>]
module Form =

    module View =

        let asHtml (config: View.ViewConfig<'Output>) (form: Form<'Output>) : DomItem =
            PlainView.asHtml config form

        let asHtmlWith
            (renderItems: RenderContext -> Item list -> DomItem list)
            (config: View.ViewConfig<'Output>)
            (form: Form<'Output>)
            : DomItem
            =
            PlainView.asHtmlWith renderItems config form

    let succeed (output: 'Output) : Form<'Output> = Base.succeed output

    let append (newForm: Form<'A>) (currentForm: Form<'A -> 'B>) : Form<'B> =
        Base.append newForm currentForm

    let map (fn: 'A -> 'B) (form: Form<'A>) : Form<'B> = Base.map fn form
    let andThen (child: 'A -> Form<'B>) (parent: Form<'A>) : Form<'B> = Base.andThen child parent
    let optional (form: Form<'A>) : Form<'A option> = Base.optional form
    let disable (form: Form<'A>) : Form<'A> = Base.disable form

    let disableIf (condition: unit -> bool) (form: Form<'A>) : Form<'A> =
        Base.disableIf condition form

    let readOnly (form: Form<'A>) : Form<'A> = Base.readOnly form

    let readOnlyIf (condition: unit -> bool) (form: Form<'A>) : Form<'A> =
        Base.readOnlyIf condition form

    let withLabel (label: unit -> string) (form: Form<'A>) : Form<'A> = Base.withLabel label form

    let withExternalError (error: unit -> string option) (form: Form<'A>) : Form<'A> =
        Base.withExternalError error form

    let validateOn (validation: Validation) (form: Form<'A>) : Form<'A> =
        Base.validateOn validation form

    let reset (form: Form<'A>) : unit = Base.reset form
    let commit (form: Form<'A>) : unit = Base.commit form

    let load (values: FormValues<'Values>) (form: Form<'A>) (data: 'Values) : unit =
        Base.load values form data

    let snapshot (values: FormValues<'Values>) : 'Values = Base.snapshot values
    let isDirty (form: Form<'A>) : unit -> bool = Base.isDirty form

    /// Wraps the fields in a `fieldset` with `title` as its legend.
    let section (title: string) (form: Form<'Output>) : Form<'Output> =
        Base.wrap (PlainView.section title) form

    /// Puts the fields side by side.
    let group (form: Form<'Output>) : Form<'Output> = Base.wrap PlainView.group form

    /// Replaces the items of `form` with one drawn by `render`, which places them in its own markup.
    let wrap (render: RenderContext -> Item list -> DomItem) (form: Form<'Output>) : Form<'Output> =
        Base.wrap render form

    let validateAsync
        (debounceMs: int)
        (check: 'A -> Fable.Core.JS.Promise<Result<unit, string>>)
        (form: Form<'A>)
        : Form<'A>
        =
        Base.validateAsync debounceMs check form

    let textField (config: FieldConfig<TextField.Attributes, string, 'Output>) : Form<'Output> =
        Base.field System.String.IsNullOrEmpty TextField.render config

    let emailField (config: FieldConfig<EmailField.Attributes, string, 'Output>) : Form<'Output> =
        Base.field System.String.IsNullOrEmpty EmailField.render config

    let passwordField
        (config: FieldConfig<PasswordField.Attributes, string, 'Output>)
        : Form<'Output>
        =
        Base.field System.String.IsNullOrEmpty PasswordField.render config

    let numberField (config: FieldConfig<NumberField.Attributes, string, 'Output>) : Form<'Output> =
        Base.field System.String.IsNullOrEmpty NumberField.render config

    let textareaField
        (config: FieldConfig<TextareaField.Attributes, string, 'Output>)
        : Form<'Output>
        =
        Base.field System.String.IsNullOrEmpty TextareaField.render config

    let checkboxField
        (config: FieldConfig<CheckboxField.Attributes, bool, 'Output>)
        : Form<'Output>
        =
        Base.field (fun _ -> false) CheckboxField.render config

    let selectField
        (config: FieldConfig<SelectField.Attributes, SelectField.OptionItem option, 'Output>)
        : Form<'Output>
        =
        Base.field
            (fun (value: SelectField.OptionItem option) -> value.IsNone)
            SelectField.render
            config

    let radioField
        (config: FieldConfig<RadioField.Attributes, RadioField.OptionItem option, 'Output>)
        : Form<'Output>
        =
        Base.field
            (fun (value: RadioField.OptionItem option) -> value.IsNone)
            RadioField.render
            config

    /// `selectField` whose options come from a tracked read instead of the attributes.
    let selectFieldWith
        (options: unit -> SelectField.OptionItem list)
        (config: FieldConfig<SelectField.Attributes, SelectField.OptionItem option, 'Output>)
        : Form<'Output>
        =
        Base.field
            (fun (value: SelectField.OptionItem option) -> value.IsNone)
            (SelectField.renderDynamic options)
            config

    /// Shows the form while `condition` holds; `Ok None` and no errors otherwise.
    let showIf (condition: unit -> bool) (form: Form<'A>) : Form<'A option> =
        Base.showIf condition form

    /// One form per item, with the list's label and buttons from `attributes`.
    let list
        (config: FormList.Config<'Item>)
        (elementFor: FormList.ElementContext -> 'Item -> Form<'Output>)
        (attributes: FormList.Attributes)
        : Form<'Output list>
        =
        FormList.form attributes.FieldId (FormList.render attributes) config elementFor

    /// `list` with another renderer for the items, for example one with drag and drop.
    let listWith
        (render: RenderContext -> FormList.RenderConfig -> DomItem)
        (config: FormList.Config<'Item>)
        (elementFor: FormList.ElementContext -> 'Item -> Form<'Output>)
        (attributes: FormList.Attributes)
        : Form<'Output list>
        =
        FormList.form attributes.FieldId render config elementFor
