namespace Fable.Ripple.Form.Plain

open Fable.Ripple
open Fable.Ripple.Dom
open Fable.Ripple.Form

module TextField =

    [<RequireQualifiedAccess>]
    type SpellCheck =
        | Default
        | True
        | False

    [<NoComparison>]
    type Attributes =
        {
            FieldId: string
            Label: string
            Placeholder: string option
            AutoComplete: string option
            SpellCheck: SpellCheck
            AutoFocus: bool
        }

        interface IAttributes with
            member this.GetFieldId() = this.FieldId

    let render (config: FieldRenderConfig<string, Attributes>) =
        PlainView.textLike
            "text"
            config.Attributes.Label
            [
                PlainView.autoFocus config.Attributes.AutoFocus
                PlainView.placeholder config.Attributes.Placeholder
            ]
            config

type TextField =

    static member create(fieldId: string) : TextField.Attributes =
        {
            FieldId = fieldId
            Label = ""
            Placeholder = None
            AutoComplete = None
            SpellCheck = TextField.SpellCheck.Default
            AutoFocus = false
        }

    static member withLabel (label: string) (attributes: TextField.Attributes) =
        { attributes with
            Label = label
        }

    static member withPlaceholder (placeholder: string) (attributes: TextField.Attributes) =
        { attributes with
            Placeholder = Some placeholder
        }

    static member withAutoComplete (autoComplete: string) (attributes: TextField.Attributes) =
        { attributes with
            AutoComplete = Some autoComplete
        }

    static member withSpellCheck (spellCheck: bool) (attributes: TextField.Attributes) =
        { attributes with
            SpellCheck =
                if spellCheck then
                    TextField.SpellCheck.True
                else
                    TextField.SpellCheck.False
        }

    static member withAutoFocus(attributes: TextField.Attributes) =
        { attributes with
            AutoFocus = true
        }
