module Fable.Ripple.Form.Tests.Components

open Fable.Ripple
open Fable.Ripple.Dom
open Fable.Ripple.Form
open Fable.Ripple.Form.Plain
open type Fable.Ripple.Dom.Test.RippleRegistry

let private resultText (form: Form<'Output>) =
    Html.output
        [
            attr.id "result"
            Html.text (fun () ->
                match form.Result.Value with
                | Ok output -> $"ok:{output}"
                | Error(first, others) -> $"error:{List.length (first :: others)}"
            )
        ]

let private host (validation: Validation) (form: Form<'Output>) (extra: DomItem list) =
    let state = Var.create View.Idle
    let errors = View.errorVisibility ()
    let submitted = Var.create ""

    Html.div
        [
            Form.View.asHtml
                {
                    OnSubmit = fun output -> submitted.Value <- $"{output}"
                    State = state
                    ErrorVisibility = errors
                    Action = View.Action.SubmitOnly "Submit"
                    Validation = validation
                }
                form
            resultText form
            Html.output
                [
                    attr.id "submitted"
                    Html.text submitted
                ]
            Html.button
                [
                    attr.id "reset-errors"
                    on.click (fun _ -> errors.Reset())
                    Html.text "Reset errors"
                ]
            Html.button
                [
                    attr.id "load"
                    on.click (fun _ -> state.Value <- View.Loading)
                    Html.text "Loading"
                ]
            yield! extra
        ]

/// Reads the observer count on click, once every flush triggered by the previous action is over.
let private observerCounter (source: Var<'T>) =
    let count = Var.create ""

    Html.fragment
        [
            Html.button
                [
                    attr.id "count"
                    on.click (fun _ -> count.Value <- string (Signal.observerCount source.Signal))
                    Html.text "Count observers"
                ]
            Html.output
                [
                    attr.id "observers"
                    Html.text count
                ]
        ]

let private nameField (name: Var<string>) =
    Form.textField
        {
            Parser =
                fun value ->
                    if value.Length < 2 then
                        Error "Too short"
                    else
                        Ok value
            Value = name
            Error = fun () -> None
            Attributes = TextField.create "name" |> TextField.withLabel "Name"
        }

let private simple (validation: Validation) () : DomItem =
    let name = Var.create ""

    host
        validation
        (nameField name)
        [
            Html.button
                [
                    attr.id "set"
                    on.click (fun _ -> name.Value <- "Ada")
                    Html.text "Set"
                ]
        ]

let private repeatPassword () : DomItem =
    let password = Var.create ""
    let repeat = Var.create ""

    let form =
        Form.succeed (fun password _ -> password)
        |> Form.append (
            Form.passwordField
                {
                    Parser = Ok
                    Value = password
                    Error = fun () -> None
                    Attributes =
                        PasswordField.create "password" |> PasswordField.withLabel "Password"
                }
        )
        |> Form.append (
            Form.passwordField
                {
                    Parser =
                        fun value ->
                            if value = password.Value then
                                Ok()
                            else
                                Error "The passwords do not match"
                    Value = repeat
                    Error = fun () -> None
                    Attributes = PasswordField.create "repeat" |> PasswordField.withLabel "Repeat"
                }
        )

    host ValidateOnSubmit form []

[<RequireQualifiedAccess>]
type private UserType =
    | Student
    | Teacher

    interface SelectField.OptionItem with
        member this.Key =
            match this with
            | Student -> "student"
            | Teacher -> "teacher"

        member this.Text =
            match this with
            | Student -> "student"
            | Teacher -> "teacher"

let private branch () : DomItem =
    let userType: Var<SelectField.OptionItem option> = Var.create None
    let name = Var.create ""
    let subject = Var.create ""

    let student =
        Form.succeed (fun name -> $"student {name}")
        |> Form.append (nameField name)
        |> Form.section "Student"

    let teacher =
        Form.succeed (fun name subject -> $"teacher {name} {subject}")
        |> Form.append (nameField name)
        |> Form.append (
            Form.textField
                {
                    Parser = Ok
                    Value = subject
                    Error = fun () -> None
                    Attributes = TextField.create "subject" |> TextField.withLabel "Subject"
                }
        )
        |> Form.section "Teacher"

    let form =
        Form.selectField
            {
                Parser =
                    fun value ->
                        match value with
                        | None -> Error "Required"
                        | Some value -> Ok(value :?> UserType)
                Value = userType
                Error = fun () -> None
                Attributes =
                    SelectField.create "user-type"
                    |> SelectField.withLabel "Type"
                    |> SelectField.withPlaceholder "Choose"
                    |> SelectField.withOptions
                        [
                            UserType.Student
                            UserType.Teacher
                        ]
            }
        |> Form.andThen (
            function
            | UserType.Student -> student
            | UserType.Teacher -> teacher
        )

    host
        ValidateOnBlur
        form
        [
            Html.button
                [
                    attr.id "switch-many"
                    on.click (fun _ ->
                        for _ in 1..50 do
                            userType.Value <- Some UserType.Teacher
                            userType.Value <- Some UserType.Student
                    )
                    Html.text "Switch 50 times"
                ]
            observerCounter name
        ]

[<NoComparison; NoEquality>]
type private Item =
    {
        Title: Var<string>
    }

let private list () : DomItem =
    let items =
        Var.create
            [
                {
                    Title = Var.create "first"
                }
            ]

    let form =
        FormList.create "items"
        |> FormList.withLabel "Items"
        |> FormList.withAdd "Add"
        |> FormList.withDelete "Remove"
        |> Form.list
            {
                Items = items
                Default =
                    fun () ->
                        {
                            Title = Var.create ""
                        }
            }
            (fun context item ->
                Form.textField
                    {
                        Parser = Ok
                        Value = item.Title
                        Error = fun () -> None
                        Attributes = TextField.create $"{context.FieldIdPrefix}-title"
                    }
                |> Form.withLabel (fun () -> $"Item #{context.Index.Value + 1}")
            )

    host
        ValidateOnSubmit
        form
        [
            Html.button
                [
                    attr.id "cycle"
                    on.click (fun _ ->
                        for _ in 1..50 do
                            let item =
                                {
                                    Title = Var.create "x"
                                }

                            items.Value <- items.Peek() @ [ item ]

                            items.Value <-
                                items.Peek()
                                |> List.filter (fun other ->
                                    not (obj.ReferenceEquals(other, item))
                                )
                    )
                    Html.text "Add and remove 50 times"
                ]
            observerCounter items
        ]

let private optional () : DomItem =
    let name = Var.create ""

    let form =
        nameField name
        |> Form.optional
        |> Form.map (fun value -> defaultArg value "nothing")

    host ValidateOnBlur form []

let private disable () : DomItem =
    let locked = Var.create false
    let name = Var.create ""

    let form =
        Form.succeed (fun _ name -> name)
        |> Form.append (
            Form.checkboxField
                {
                    Parser = Ok
                    Value = locked
                    Error = fun () -> None
                    Attributes = CheckboxField.create "locked" |> CheckboxField.withText "Locked"
                }
        )
        |> Form.append (nameField name |> Form.disableIf (fun () -> locked.Value))

    host ValidateOnSubmit form []

let private external () : DomItem =
    let name = Var.create ""
    let serverError: Var<(string * string) option> = Var.create None

    let form =
        Form.textField
            {
                Parser = Ok
                Value = name
                Error =
                    fun () ->
                        match serverError.Value with
                        | Some(rejected, message) when rejected = name.Value -> Some message
                        | _ -> None
                Attributes = TextField.create "name" |> TextField.withLabel "Name"
            }

    host
        ValidateOnSubmit
        form
        [
            Html.button
                [
                    attr.id "fail"
                    on.click (fun _ -> serverError.Value <- Some(name.Peek(), "Taken"))
                    Html.text "Fail"
                ]
        ]

let private asyncCheck () : DomItem =
    let name = Var.create ""

    let form =
        Form.textField
            {
                Parser = Ok
                Value = name
                Error = fun () -> None
                Attributes = TextField.create "name" |> TextField.withLabel "Name"
            }
        |> Form.validateAsync
            50
            (fun value ->
                promise {
                    do! Promise.sleep 100

                    return
                        if value = "taken" then
                            Error "Taken"
                        else
                            Ok()
                }
            )

    host ValidateOnSubmit form []

let private dirty () : DomItem =
    let name = Var.create "Ada"
    let form = nameField name

    let values: FormValues<string> =
        {
            Snapshot = fun () -> name.Value
            Load = fun value -> name.Value <- value
        }

    host
        ValidateOnSubmit
        form
        [
            Html.button
                [
                    attr.id "load-draft"
                    on.click (fun _ -> Form.load values form "Grace")
                    Html.text "Load"
                ]
            Html.output
                [
                    attr.id "snapshot"
                    Html.text (fun () -> Form.snapshot values)
                ]
            Html.output
                [
                    attr.id "dirty"
                    Html.text (fun () ->
                        if Form.isDirty form () then
                            "dirty"
                        else
                            "clean"
                    )
                ]
            Html.button
                [
                    attr.id "reset"
                    on.click (fun _ -> Form.reset form)
                    Html.text "Reset"
                ]
            Html.button
                [
                    attr.id "commit"
                    on.click (fun _ -> Form.commit form)
                    Html.text "Commit"
                ]
        ]

let private validateOnChange () : DomItem =
    let name = Var.create ""
    host ValidateOnSubmit (nameField name |> Form.validateOn ValidateOnChange) []

let private externalCombinator () : DomItem =
    let name = Var.create ""
    let rejected: Var<string option> = Var.create None

    let form =
        Form.textField (
            Field.create name Ok (TextField.create "name" |> TextField.withLabel "Name")
        )
        |> Form.withExternalError (fun () ->
            match rejected.Value with
            | Some value when value = name.Value -> Some "Taken"
            | _ -> None
        )

    host
        ValidateOnSubmit
        form
        [
            Html.button
                [
                    attr.id "fail"
                    on.click (fun _ -> rejected.Value <- Some(name.Peek()))
                    Html.text "Fail"
                ]
        ]

let private wizard () : DomItem =
    let email = Var.create ""
    let userType: Var<SelectField.OptionItem option> = Var.create None
    let name = Var.create ""
    let subject = Var.create ""
    let state = Var.create View.Idle
    let submitted = Var.create ""

    let account =
        TextField.create "email"
        |> TextField.withLabel "Email"
        |> Field.create
            email
            (fun value ->
                if value.Contains "@" then
                    Ok value
                else
                    Error "Not an email"
            )
        |> Form.textField

    let userTypeStep =
        SelectField.create "user-type"
        |> SelectField.withLabel "Type"
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
                | None -> Error "Required"
                | Some value -> Ok(value :?> UserType)
            )
        |> Form.selectField

    let details =
        Wizard.single "Type" userTypeStep
        |> Wizard.andThen (fun userType ->
            match userType with
            | UserType.Student ->
                Wizard.single "Student" (Form.map (fun name -> $"student {name}") (nameField name))
            | UserType.Teacher ->
                Wizard.single
                    "Teacher"
                    (Form.succeed (fun name subject -> $"teacher {name} {subject}")
                     |> Form.append (nameField name)
                     |> Form.append (
                         TextField.create "subject"
                         |> TextField.withLabel "Subject"
                         |> Field.create subject Ok
                         |> Form.textField
                     ))
        )

    let wizard =
        Wizard.succeed (fun email details -> $"{email} {details}")
        |> Wizard.step "Account" account
        |> Wizard.append details

    Html.div
        [
            WizardView.asHtml
                {
                    OnSubmit = fun output -> submitted.Value <- output
                    State = state
                    Validation = ValidateOnSubmit
                    Back = "Back"
                    Next = "Next"
                    Submit = "Register"
                }
                wizard
            Html.output
                [
                    attr.id "submitted"
                    Html.text submitted
                ]
        ]

let private kinds () : DomItem =
    let email = Var.create "a@b.c"
    let number = Var.create "1"
    let summary = Var.create ""
    let secret = Var.create "pw"
    let agreed = Var.create false

    let form =
        Form.succeed (fun email number summary secret agreed ->
            $"{email} {number} {summary} {secret} {agreed}"
        )
        |> Form.append (
            EmailField.create "email"
            |> EmailField.withLabel "Email"
            |> Field.create email Ok
            |> Form.emailField
        )
        |> Form.append (
            NumberField.create "number"
            |> NumberField.withLabel "Number"
            |> Field.create
                number
                (fun value ->
                    match System.Int32.TryParse value with
                    | true, n -> Ok n
                    | _ -> Error "Not a number"
                )
            |> Form.numberField
        )
        |> Form.append (
            TextareaField.create "summary"
            |> TextareaField.withLabel "Summary"
            |> Field.create summary Ok
            |> Form.textareaField
        )
        |> Form.append (
            PasswordField.create "secret"
            |> PasswordField.withLabel "Secret"
            |> Field.create secret Ok
            |> Form.passwordField
        )
        |> Form.append (
            CheckboxField.create "agreed"
            |> CheckboxField.withText "Agreed"
            |> Field.create agreed Ok
            |> Form.checkboxField
        )

    host ValidateOnSubmit form []

let private conditional () : DomItem =
    let hasPet = Var.create false
    let petName = Var.create ""

    let form =
        Form.succeed (fun _ petName -> defaultArg petName "no pet")
        |> Form.append (
            CheckboxField.create "has-pet"
            |> CheckboxField.withText "I have a pet"
            |> Field.create hasPet Ok
            |> Form.checkboxField
        )
        |> Form.append (
            TextField.create "pet-name"
            |> TextField.withLabel "Pet name"
            |> Field.create petName Ok
            |> Form.textField
            |> Form.showIf (fun () -> hasPet.Value)
        )

    host ValidateOnSubmit form []

let private conditionalWizard () : DomItem =
    let wantsDetails = Var.create false
    let details = Var.create ""
    let state = Var.create View.Idle
    let submitted = Var.create ""

    let wizard =
        Wizard.succeed (fun _ details -> defaultArg details "none")
        |> Wizard.step
            "Start"
            (CheckboxField.create "wants"
             |> CheckboxField.withText "Add details"
             |> Field.create wantsDetails Ok
             |> Form.checkboxField)
        |> Wizard.stepIf
            (fun () -> wantsDetails.Value)
            "Details"
            (TextField.create "details"
             |> TextField.withLabel "Details"
             |> Field.create details Ok
             |> Form.textField)

    Html.div
        [
            WizardView.asHtml
                {
                    OnSubmit = fun output -> submitted.Value <- output
                    State = state
                    Validation = ValidateOnSubmit
                    Back = "Back"
                    Next = "Next"
                    Submit = "Finish"
                }
                wizard
            Html.output
                [
                    attr.id "submitted"
                    Html.text submitted
                ]
        ]

register ("SubmitForm", simple ValidateOnSubmit)
register ("Conditional", conditional)
register ("ConditionalWizard", conditionalWizard)
register ("Kinds", kinds)
register ("Wizard", wizard)
register ("Dirty", dirty)
register ("ValidateOnChange", validateOnChange)
register ("ExternalCombinator", externalCombinator)
register ("AsyncCheck", asyncCheck)
register ("BlurForm", simple ValidateOnBlur)
register ("RepeatPassword", repeatPassword)
register ("Branch", branch)
register ("List", list)
register ("Optional", optional)
register ("Disable", disable)
register ("External", external)
