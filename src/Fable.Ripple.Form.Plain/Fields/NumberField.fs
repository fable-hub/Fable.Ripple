namespace Fable.Ripple.Form.Plain

open Fable.Ripple
open Fable.Ripple.Dom
open Fable.Ripple.Form

module NumberField =

    [<NoComparison>]
    type Attributes =
        {
            FieldId: string
            Label: string
            Placeholder: string option
            Max: float option
            Min: float option
            Step: float option
            AutoFocus: bool
        }

        interface IAttributes with
            member this.GetFieldId() = this.FieldId

    let render (config: FieldRenderConfig<string, Attributes>) =
        PlainView.textLike
            "number"
            config.Attributes.Label
            [
                PlainView.autoFocus config.Attributes.AutoFocus
                PlainView.placeholder config.Attributes.Placeholder
                match config.Attributes.Max with
                | Some value -> attr.max (string value)
                | None -> ()
                match config.Attributes.Min with
                | Some value -> attr.min (string value)
                | None -> ()
                match config.Attributes.Step with
                | Some value -> attr.step (string value)
                | None -> ()
            ]
            config

type NumberField =

    static member create(fieldId: string) : NumberField.Attributes =
        {
            FieldId = fieldId
            Label = ""
            Placeholder = None
            Max = None
            Min = None
            Step = None
            AutoFocus = false
        }

    static member withLabel (label: string) (attributes: NumberField.Attributes) =
        { attributes with
            Label = label
        }

    static member withPlaceholder (placeholder: string) (attributes: NumberField.Attributes) =
        { attributes with
            Placeholder = Some placeholder
        }

    static member withMax (max: float) (attributes: NumberField.Attributes) =
        { attributes with
            Max = Some max
        }

    static member withMin (min: float) (attributes: NumberField.Attributes) =
        { attributes with
            Min = Some min
        }

    static member withStep (step: float) (attributes: NumberField.Attributes) =
        { attributes with
            Step = Some step
        }

    static member withAutoFocus(attributes: NumberField.Attributes) =
        { attributes with
            AutoFocus = true
        }
