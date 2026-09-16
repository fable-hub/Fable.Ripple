namespace Fable.Ripple.Form.Plain

open Fable.Ripple
open Fable.Ripple.Dom
open Fable.Ripple.Form

module RadioField =

    type OptionItem = SelectField.OptionItem

    [<NoComparison>]
    type Attributes =
        {
            FieldId: string
            Label: string
            Options: OptionItem list
        }

        interface IAttributes with
            member this.GetFieldId() = this.FieldId

    let render (config: FieldRenderConfig<OptionItem option, Attributes>) =
        Html.div
            [
                attr.id config.Id
                attr.className "rf-radios"
                attr.role "radiogroup"
                attr.custom ("aria-describedby", View.helpId config)
                attr.custom (
                    "aria-invalid",
                    (fun () ->
                        if (View.shownError config ()).IsSome then
                            "true"
                        else
                            "false"
                    )
                )

                for optionItem in config.Attributes.Options do
                    Html.label
                        [
                            attr.className "rf-radio"
                            Html.input
                                [
                                    attr.type' "radio"
                                    attr.name config.Id
                                    attr.checked' (fun () -> Some optionItem = config.Value.Value)
                                    attr.disabled config.Disabled
                                    on.checkedChange (fun _ ->
                                        if not (config.ReadOnly()) then
                                            config.Value.Value <- Some optionItem
                                    )
                                    on.blur (fun _ -> config.OnBlur())
                                ]
                            Html.text optionItem.Text
                        ]
            ]
        |> PlainView.withLabelAndError config.Attributes.Label config

type RadioField =

    static member create(fieldId: string) : RadioField.Attributes =
        {
            FieldId = fieldId
            Label = ""
            Options = []
        }

    static member withLabel (label: string) (attributes: RadioField.Attributes) =
        { attributes with
            Label = label
        }

    static member withOptions
        (options: RadioField.OptionItem list)
        (attributes: RadioField.Attributes)
        =
        { attributes with
            Options = options
        }

    static member withBasicOptions
        (options: (string * string) list)
        (attributes: RadioField.Attributes)
        =
        { attributes with
            Options =
                options
                |> List.map (fun (key, text) ->
                    { new RadioField.OptionItem with
                        member _.Key = key
                        member _.Text = text
                    }
                )
        }
