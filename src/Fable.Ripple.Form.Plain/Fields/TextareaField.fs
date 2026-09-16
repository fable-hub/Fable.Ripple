namespace Fable.Ripple.Form.Plain

open Fable.Ripple
open Fable.Ripple.Dom
open Fable.Ripple.Form

module TextareaField =

    [<NoComparison>]
    type Attributes =
        {
            FieldId: string
            Label: string
            Placeholder: string option
            AutoFocus: bool
            Rows: int option
            Cols: int option
        }

        interface IAttributes with
            member this.GetFieldId() = this.FieldId

    let render (config: FieldRenderConfig<string, Attributes>) =
        Html.textarea
            [
                View.controlAttributes config
                attr.className "rf-input rf-textarea"
                attr.bindValue config.Value
                on.blur (fun _ -> config.OnBlur())
                attr.disabled config.Disabled
                attr.readOnly config.ReadOnly
                PlainView.isInvalid config
                PlainView.autoFocus config.Attributes.AutoFocus
                PlainView.placeholder config.Attributes.Placeholder
                match config.Attributes.Rows with
                | Some value -> attr.rows value
                | None -> ()
            ]
        |> PlainView.withLabelAndError config.Attributes.Label config

type TextareaField =

    static member create(fieldId: string) : TextareaField.Attributes =
        {
            FieldId = fieldId
            Label = ""
            Placeholder = None
            AutoFocus = false
            Rows = None
            Cols = None
        }

    static member withLabel (label: string) (attributes: TextareaField.Attributes) =
        { attributes with
            Label = label
        }

    static member withPlaceholder (placeholder: string) (attributes: TextareaField.Attributes) =
        { attributes with
            Placeholder = Some placeholder
        }

    static member withRows (rows: int) (attributes: TextareaField.Attributes) =
        { attributes with
            Rows = Some rows
        }

    static member withCols (cols: int) (attributes: TextareaField.Attributes) =
        { attributes with
            Cols = Some cols
        }

    static member withAutoFocus(attributes: TextareaField.Attributes) =
        { attributes with
            AutoFocus = true
        }
