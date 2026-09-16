module Demo.SevenGUIs.Crud

open Fable.Ripple
open Fable.Ripple.Dom

// 7GUIs #5 - CRUD: filterable list of people with create / update / delete.
type private Person =
    {
        Id: int
        Name: string
        Surname: string
    }

let render () =
    let people =
        Var.create
            [|
                {
                    Id = 1
                    Name = "Hans"
                    Surname = "Emil"
                }
                {
                    Id = 2
                    Name = "Max"
                    Surname = "Mustermann"
                }
                {
                    Id = 3
                    Name = "Roman"
                    Surname = "Tisch"
                }
            |]

    let filter = Var.create ""
    let name = Var.create ""
    let surname = Var.create ""
    let selected = Var.create (None: int option)
    let nextId = ref 4

    let filtered () =
        let f = filter.Value.ToLower()
        people.Value |> Array.filter (fun p -> p.Surname.ToLower().StartsWith f)

    let create () =
        if name.Value <> "" || surname.Value <> "" then
            people.Value <-
                Array.append
                    people.Value
                    [|
                        {
                            Id = nextId.Value
                            Name = name.Value
                            Surname = surname.Value
                        }
                    |]

            nextId.Value <- nextId.Value + 1

    let update () =
        match selected.Value with
        | Some id ->
            people.Value <-
                people.Value
                |> Array.map (fun p ->
                    if p.Id = id then
                        { p with
                            Name = name.Value
                            Surname = surname.Value
                        }
                    else
                        p
                )
        | None -> ()

    let delete () =
        match selected.Value with
        | Some id ->
            people.Value <- people.Value |> Array.filter (fun p -> p.Id <> id)
            selected.Value <- None
        | None -> ()

    let selectRow (p: Person) =
        selected.Value <- Some p.Id
        name.Value <- p.Name
        surname.Value <- p.Surname

    // Row text reads the current person from `people`, so edits show live.
    let rowText (p: Person) () =
        match people.Value |> Array.tryFind (fun x -> x.Id = p.Id) with
        | Some x -> x.Surname + ", " + x.Name
        | None -> p.Surname + ", " + p.Name

    Html.div
        [
            attr.className "stack"
            Html.div
                [
                    attr.className "row is-top"

                    Html.div
                        [
                            attr.className "stack"

                            Html.label
                                [
                                    Html.text "Filter surname"
                                    Html.input [ attr.bindValue filter ]
                                ]

                            Html.ul
                                [
                                    attr.className "listbox"
                                    attr.role "listbox"

                                    Html.each
                                        (fun () -> filtered ())
                                        (fun p -> p.Id)
                                        (fun p ->
                                            Html.li
                                                [
                                                    attr.role "option"
                                                    attr.classList (fun () ->
                                                        [
                                                            "is-selected",
                                                            selected.Value = Some p.Id
                                                        ]
                                                    )
                                                    on.click (fun _ -> selectRow p)
                                                    Html.text (rowText p)
                                                ]
                                        )
                                ]
                        ]

                    Html.div
                        [
                            attr.className "stack"

                            Html.label
                                [
                                    Html.text "Name"
                                    Html.input [ attr.bindValue name ]
                                ]

                            Html.label
                                [
                                    Html.text "Surname"
                                    Html.input [ attr.bindValue surname ]
                                ]
                        ]
                ]

            Html.div
                [
                    attr.className "row"
                    Html.button
                        [
                            on.click (fun _ -> create ())
                            Html.text "Create"
                        ]
                    Html.button
                        [
                            on.click (fun _ -> update ())
                            Html.text "Update"
                        ]
                    Html.button
                        [
                            on.click (fun _ -> delete ())
                            Html.text "Delete"
                        ]
                ]
        ]
