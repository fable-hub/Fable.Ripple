var e=`module Demo.Examples.Forms.DynamicForm

open Fable.Ripple
open Fable.Ripple.Dom
open Fable.Ripple.Form
open Fable.Ripple.Form.Plain
open Demo.Examples.Components // demo-hide-line

// \`andThen\` builds the rest of the form from the output of the first part. The
// child form is created when the selection changes and disposed when it
// changes again. Its values live in Vars owned here, so switching back and
// forth keeps what was typed.

[<RequireQualifiedAccess>]
type UserType =
    | Student
    | Teacher

    interface SelectField.OptionItem with
        member this.Key =
            match this with
            | Student -> "student"
            | Teacher -> "teacher"

        member this.Text =
            match this with
            | Student -> "Student"
            | Teacher -> "Teacher"

type Result =
    | NewStudent of name: string
    | NewTeacher of name: string * subject: string

let render () =
    let userType: Var<SelectField.OptionItem option> = Var.create None
    let name = Var.create ""
    let subject = Var.create ""

    let nameField =
        TextField.create "name"
        |> TextField.withLabel "Name"
        |> Field.create name Ok
        |> Form.textField

    let student =
        Form.succeed NewStudent |> Form.append nameField |> Form.section "Student"

    let teacher =
        Form.succeed (fun name subject -> NewTeacher(name, subject))
        |> Form.append nameField
        |> Form.append (
            TextField.create "subject"
            |> TextField.withLabel "Subject"
            |> Field.create subject Ok
            |> Form.textField
        )
        |> Form.section "Teacher"

    let form =
        SelectField.create "user-type"
        |> SelectField.withLabel "Type of user"
        |> SelectField.withPlaceholder "Choose"
        |> SelectField.withOptions
            [
                UserType.Student
                UserType.Teacher
            ]
        |> Field.create
            userType
            (fun value ->
                match value with
                | Some item -> Ok(item :?> UserType)
                | None -> Error "Choose a type"
            )
        |> Form.selectField
        |> Form.andThen (
            function
            | UserType.Student -> student
            | UserType.Teacher -> teacher
        )

    let state = Var.create View.Idle

    Stack.stack
        [
            // demo-hide
            Try.observe
                "Pick a type, fill the name, switch to the other type and back: the name is still there. Only the section below the select is rebuilt."

            // demo-show
            Form.View.asHtml
                {
                    OnSubmit = fun result -> state.Value <- View.Success $"%A{result}"
                    State = state
                    ErrorVisibility = View.errorVisibility ()
                    Action = View.Action.SubmitOnly "Create"
                    Validation = ValidateOnSubmit
                }
                form
        ]
`;export{e as default};