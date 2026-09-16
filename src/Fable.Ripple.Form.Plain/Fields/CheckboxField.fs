namespace Fable.Ripple.Form.Plain

open Fable.Ripple
open Fable.Ripple.Dom
open Fable.Ripple.Form

module CheckboxField =

    [<NoComparison>]
    type Attributes =
        {
            FieldId: string
            Text: string
        }

        interface IAttributes with
            member this.GetFieldId() = this.FieldId

    let render (config: FieldRenderConfig<bool, Attributes>) =
        Html.div
            [
                attr.className "rf-field"
                Html.label
                    [
                        attr.className "rf-checkbox"
                        Html.input
                            [
                                attr.id config.Id
                                attr.type' "checkbox"
                                attr.checked' config.Value
                                on.checkedChange (fun isChecked ->
                                    if not (config.ReadOnly()) then
                                        config.Value.Value <- isChecked
                                )
                                on.blur (fun _ -> config.OnBlur())
                                attr.disabled config.Disabled
                            ]
                        Html.text config.Attributes.Text
                    ]
            ]

type CheckboxField =

    static member create(fieldId: string) : CheckboxField.Attributes =
        {
            FieldId = fieldId
            Text = ""
        }

    static member withText (text: string) (attributes: CheckboxField.Attributes) =
        { attributes with
            Text = text
        }
