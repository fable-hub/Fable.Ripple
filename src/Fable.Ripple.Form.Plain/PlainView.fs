namespace Fable.Ripple.Form.Plain

open Browser.Types
open Fable.Ripple
open Fable.Ripple.Dom
open Fable.Core.JsInterop
open Fable.Ripple.Form

/// Class-based markup with no CSS framework: `rf-*` classes, styled by the host page.
module PlainView =

    let errorMessage (message: string) =
        Html.p
            [
                attr.className "rf-help rf-help--error"
                Html.text message
            ]

    let errorHelp (config: FieldRenderConfig<'Input, 'Attributes>) =
        let shown = View.shownError config

        Html.p
            [
                attr.id (View.helpId config)
                attr.className "rf-help"
                attr.toggleClass ("rf-help--error", (fun () -> (shown ()).IsSome))
                Html.text (fun () -> shown () |> Option.defaultValue "")
            ]

    let withLabelAndError
        (label: string)
        (config: FieldRenderConfig<'Input, 'Attributes>)
        (control: DomItem)
        : DomItem
        =
        Html.div
            [
                attr.className "rf-field"
                attr.toggleClass ("rf-field--validating", config.Validating)
                Html.label
                    [
                        attr.className "rf-label"
                        attr.htmlFor config.Id
                        View.labelText label config
                    ]
                control
                errorHelp config
            ]

    /// The `autofocus` attribute, and a focus call once the control is in the document, for controls
    /// created after the page loaded.
    let autoFocus (enabled: bool) : DomItem =
        Html.fragment
            [
                attr.autoFocus enabled
                if enabled then
                    attr.ref (fun element ->
                        emitJsStatement element "globalThis.queueMicrotask(() => $0.focus())"
                    )
            ]

    /// `rf-input--invalid` while the field shows an error.
    let isInvalid (config: FieldRenderConfig<'Input, 'Attributes>) =
        attr.toggleClass ("rf-input--invalid", (fun () -> (View.shownError config ()).IsSome))

    /// The `placeholder` attribute, or nothing.
    let placeholder (value: string option) =
        match value with
        | Some placeholder -> attr.placeholder placeholder
        | None -> Html.none

    /// An `input.rf-input` of `inputType` bound to the value, wrapped with its label and help text.
    let textLike
        (inputType: string)
        (label: string)
        (extra: DomItem list)
        (config: FieldRenderConfig<string, 'Attributes>)
        =
        Html.input
            [
                View.controlAttributes config
                attr.className "rf-input"
                attr.type' inputType
                attr.bindValue config.Value
                on.blur (fun _ -> config.OnBlur())
                attr.disabled config.Disabled
                attr.readOnly config.ReadOnly
                isInvalid config
                yield! extra
            ]
        |> withLabelAndError label config

    let renderItems (context: RenderContext) (items: Item list) : DomItem list =
        items
        |> List.map (fun item ->
            match item with
            | FieldItem(_, render) -> render context
            | Dynamic render -> render context
        )

    /// `fieldset.rf-section` with a `legend`, for `Form.section`.
    let section (title: string) (context: RenderContext) (items: Item list) : DomItem =
        Html.fieldset
            [
                attr.className "rf-section"
                Html.legend
                    [
                        attr.className "rf-section__title"
                        Html.text title
                    ]
                yield! context.RenderItems context items
            ]

    /// `div.rf-group`, for `Form.group`.
    let group (context: RenderContext) (items: Item list) : DomItem =
        Html.div
            [
                attr.className "rf-group"
                yield! context.RenderItems context items
            ]

    let asHtmlWith
        (renderItems: RenderContext -> Item list -> DomItem list)
        (config: View.ViewConfig<'Output>)
        (form: Form<'Output>)
        : DomItem
        =
        let mutable formElement: HTMLElement option = None
        let context = View.renderContext renderItems config

        Html.form
            [
                attr.className "rf-form"
                attr.ref (fun element -> formElement <- Some element)
                View.onSubmit config form (fun () -> formElement)

                yield! renderItems context form.Items

                Html.dynamic (fun () ->
                    match config.State.Value with
                    | View.Error error -> errorMessage error
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

                match config.Action with
                | View.Action.SubmitOnly submitLabel ->
                    Html.div
                        [
                            attr.className "rf-actions"
                            Html.button
                                [
                                    attr.type' "submit"
                                    attr.className "rf-button rf-button--primary"
                                    attr.toggleClass (
                                        "rf-button--loading",
                                        (fun () -> config.State.Value = View.Loading)
                                    )
                                    Html.text submitLabel
                                ]
                        ]
                | View.Action.Custom render -> render config.State.Signal
            ]

    let asHtml (config: View.ViewConfig<'Output>) (form: Form<'Output>) : DomItem =
        asHtmlWith renderItems config form
