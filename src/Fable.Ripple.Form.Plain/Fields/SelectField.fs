namespace Fable.Ripple.Form.Plain

open Fable.Ripple
open Fable.Ripple.Dom
open Fable.Ripple.Form

module SelectField =

    /// An option of a select or a radio group: any type with a key and a text.
    [<AllowNullLiteral>]
    type OptionItem =
        abstract Key: string
        abstract Text: string

    [<NoComparison>]
    type Attributes =
        {
            FieldId: string
            Label: string
            Placeholder: string option
            Options: OptionItem list
            AutoFocus: bool
        }

        interface IAttributes with
            member this.GetFieldId() = this.FieldId

    let render (config: FieldRenderConfig<OptionItem option, Attributes>) =
        Html.select
            [
                View.controlAttributes config
                attr.className "rf-input rf-select"
                attr.disabled config.Disabled
                PlainView.isInvalid config
                PlainView.autoFocus config.Attributes.AutoFocus

                on.change (fun (key: string) ->
                    config.Value.Value <-
                        config.Attributes.Options
                        |> List.tryFind (fun optionItem -> optionItem.Key = key)
                )

                on.blur (fun _ -> config.OnBlur())

                match config.Attributes.Placeholder with
                | Some placeholder ->
                    Html.option
                        [
                            attr.disabled true
                            attr.value ""
                            Html.text placeholder
                        ]
                | None -> Html.none

                for optionItem in config.Attributes.Options do
                    Html.option
                        [
                            attr.value optionItem.Key
                            Html.text optionItem.Text
                        ]

                // After the options: a select cannot take a value it has no option for yet.
                attr.value (fun () ->
                    match config.Value.Value with
                    | Some optionItem -> optionItem.Key
                    | None -> ""
                )
            ]
        |> PlainView.withLabelAndError config.Attributes.Label config

    /// A select whose options are a tracked read. A value that is no longer an option reads as empty.
    let renderDynamic
        (options: unit -> OptionItem list)
        (config: FieldRenderConfig<OptionItem option, Attributes>)
        =
        Html.select
            [
                View.controlAttributes config
                attr.className "rf-input rf-select"
                attr.disabled config.Disabled
                PlainView.isInvalid config
                PlainView.autoFocus config.Attributes.AutoFocus

                on.change (fun (key: string) ->
                    config.Value.Value <-
                        options () |> List.tryFind (fun optionItem -> optionItem.Key = key)
                )

                on.blur (fun _ -> config.OnBlur())

                match config.Attributes.Placeholder with
                | Some placeholder ->
                    Html.option
                        [
                            attr.disabled true
                            attr.value ""
                            Html.text placeholder
                        ]
                | None -> Html.none

                Html.each
                    (fun () -> options () |> Array.ofList)
                    (fun optionItem -> optionItem.Key)
                    (fun optionItem ->
                        Html.option
                            [
                                attr.value optionItem.Key
                                Html.text (fun () ->
                                    options ()
                                    |> List.tryFind (fun current -> current.Key = optionItem.Key)
                                    |> Option.map (fun current -> current.Text)
                                    |> Option.defaultValue optionItem.Text
                                )
                            ]
                    )

                // Read after the options so a new option can be selected in the same flush.
                attr.value (fun () ->
                    let keys = options () |> List.map (fun optionItem -> optionItem.Key)

                    match config.Value.Value with
                    | Some optionItem when List.contains optionItem.Key keys -> optionItem.Key
                    | _ -> ""
                )
            ]
        |> PlainView.withLabelAndError config.Attributes.Label config

type SelectField =

    static member create(fieldId: string) : SelectField.Attributes =
        {
            FieldId = fieldId
            Label = ""
            Placeholder = None
            Options = []
            AutoFocus = false
        }

    static member withLabel (label: string) (attributes: SelectField.Attributes) =
        { attributes with
            Label = label
        }

    static member withPlaceholder (placeholder: string) (attributes: SelectField.Attributes) =
        { attributes with
            Placeholder = Some placeholder
        }

    static member withOptions
        (options: SelectField.OptionItem list)
        (attributes: SelectField.Attributes)
        =
        { attributes with
            Options = options
        }

    /// Options from key and text pairs.
    static member withBasicOptions
        (options: (string * string) list)
        (attributes: SelectField.Attributes)
        =
        { attributes with
            Options =
                options
                |> List.map (fun (key, text) ->
                    { new SelectField.OptionItem with
                        member _.Key = key
                        member _.Text = text
                    }
                )
        }

    static member withAutoFocus(attributes: SelectField.Attributes) =
        { attributes with
            AutoFocus = true
        }
