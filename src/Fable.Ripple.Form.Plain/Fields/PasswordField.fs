namespace Fable.Ripple.Form.Plain

open Fable.Ripple
open Fable.Ripple.Dom
open Fable.Ripple.Form

module PasswordField =

    [<NoComparison>]
    type Attributes =
        {
            FieldId: string
            Label: string
            Placeholder: string option
            AutoComplete: string option
            AutoFocus: bool
        }

        interface IAttributes with
            member this.GetFieldId() = this.FieldId

    let render (config: FieldRenderConfig<string, Attributes>) =
        PlainView.textLike
            "password"
            config.Attributes.Label
            [
                PlainView.autoFocus config.Attributes.AutoFocus
                PlainView.placeholder config.Attributes.Placeholder
                match config.Attributes.AutoComplete with
                | Some value -> attr.autoComplete value
                | None -> ()
            ]
            config

type PasswordField =

    static member create(fieldId: string) : PasswordField.Attributes =
        {
            FieldId = fieldId
            Label = ""
            Placeholder = None
            AutoComplete = None
            AutoFocus = false
        }

    static member withLabel (label: string) (attributes: PasswordField.Attributes) =
        { attributes with
            Label = label
        }

    static member withPlaceholder (placeholder: string) (attributes: PasswordField.Attributes) =
        { attributes with
            Placeholder = Some placeholder
        }

    static member withAutoComplete (autoComplete: string) (attributes: PasswordField.Attributes) =
        { attributes with
            AutoComplete = Some autoComplete
        }

    static member withAutoFocus(attributes: PasswordField.Attributes) =
        { attributes with
            AutoFocus = true
        }
