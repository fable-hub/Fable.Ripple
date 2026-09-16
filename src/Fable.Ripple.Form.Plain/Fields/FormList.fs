namespace Fable.Ripple.Form.Plain

open System
open Fable.Core
open Fable.Ripple
open Fable.Ripple.Dom
open Fable.Ripple.Form

/// A list of sub-forms: one form per item of a `Var<'Item list>`, cached by item reference and disposed
/// when the item leaves. `form` is the combinator; `render` draws it as `div.rf-list`.
module FormList =

    type Attributes =
        {
            FieldId: string
            Label: string
            Add: string option
            Delete: string option
        }

    [<NoComparison; NoEquality>]
    type Config<'Item> =
        {
            Items: Var<'Item list>
            Default: unit -> 'Item
        }

    [<NoComparison; NoEquality>]
    type ElementContext =
        {
            FieldIdPrefix: string
            Index: Signal<int>
        }

    [<NoComparison; NoEquality>]
    type Element =
        {
            Key: int
            Items: Item list
            Delete: unit -> unit
        }

    [<NoComparison; NoEquality>]
    type RenderConfig =
        {
            Elements: unit -> Element[]
            Add: unit -> unit
            Disabled: unit -> bool
        }

    [<NoComparison; NoEquality>]
    type private Entry<'Item, 'Output> =
        {
            Key: int
            Item: 'Item
            Index: Var<int>
            Form: Form<'Output>
            Dispose: IDisposable
        }

    // Cutoff of the Result signals: outputs may lack equality, so every recompute notifies.
    let private never (_: 'a) (_: 'a) = false

    /// The list as a form. `idPrefix` starts the ids of the items' fields; `render` draws the list.
    let form
        (idPrefix: string)
        (render: RenderContext -> RenderConfig -> DomItem)
        (config: Config<'Item>)
        (elementFor: ElementContext -> 'Item -> Form<'Output>)
        : Form<'Output list>
        =
        let cache = JS.Constructors.Map.Create<'Item, Entry<'Item, 'Output>>()
        let mutable nextKey = 0
        let entries: Var<Entry<'Item, 'Output> list> = Var.createWith never []

        Signal.effect (fun () ->
            let items = config.Items.Value

            Signal.untracked (fun () ->
                let live = JS.Constructors.Set.Create<'Item>(items)

                cache.forEach (fun entry item _ ->
                    if not (live.has item) then
                        entry.Dispose.Dispose()
                        cache.delete item |> ignore
                )

                entries.Value <-
                    items
                    |> List.mapi (fun index item ->
                        if cache.has item then
                            let entry = cache.get item
                            entry.Index.Value <- index
                            entry
                        else
                            let key = nextKey
                            nextKey <- nextKey + 1
                            let indexVar = Var.create index

                            let form, dispose =
                                Signal.root (fun () ->
                                    elementFor
                                        {
                                            FieldIdPrefix = $"{idPrefix}-{key}"
                                            Index = indexVar.Signal
                                        }
                                        item
                                )

                            let entry =
                                {
                                    Key = key
                                    Item = item
                                    Index = indexVar
                                    Form = form
                                    Dispose = dispose
                                }

                            cache.set (item, entry) |> ignore
                            entry
                    )
            )
        )
        |> ignore

        Signal.onCleanup (fun () -> cache.forEach (fun entry _ _ -> entry.Dispose.Dispose()))

        let remove (item: 'Item) =
            config.Items.Value <-
                config.Items.Peek()
                |> List.filter (fun other -> not (obj.ReferenceEquals(other, item)))

        {
            Items =
                [
                    Dynamic(fun context ->
                        render
                            context
                            {
                                Disabled = context.Disabled
                                Add =
                                    fun () ->
                                        config.Items.Value <-
                                            config.Items.Peek() @ [ config.Default() ]
                                Elements =
                                    fun () ->
                                        entries.Value
                                        |> List.map (fun entry ->
                                            ({
                                                Key = entry.Key
                                                Items = entry.Form.Items
                                                Delete = fun () -> remove entry.Item
                                            }
                                            : Element)
                                        )
                                        |> Array.ofList
                            }
                    )
                ]
            Result =
                Signal.computedWith
                    never
                    (fun () ->
                        List.foldBack
                            (fun
                                (entry: Entry<'Item, 'Output>)
                                (current: Result<'Output list, Error.Error * Error.Error list>) ->
                                match entry.Form.Result.Value, current with
                                | Ok output, _ ->
                                    Result.map (fun outputs -> output :: outputs) current
                                | Error errors, Ok _ -> Error errors
                                | Error(first, others), Error(currentFirst, currentOthers) ->
                                    Error(first, others @ (currentFirst :: currentOthers))
                            )
                            entries.Value
                            (Ok [])
                    )
            IsEmpty =
                Signal.computed (fun () ->
                    entries.Value |> List.forall (fun entry -> entry.Form.IsEmpty.Value)
                )
            Validating =
                Signal.computed (fun () ->
                    entries.Value |> List.exists (fun entry -> entry.Form.Validating.Value)
                )
            Fields = fun () -> entries.Value |> List.collect (fun entry -> entry.Form.Fields())
        }

    /// The list as `div.rf-list`: a label, one `div.rf-list__item` per element, the delete and add buttons.
    let render (attributes: Attributes) (context: RenderContext) (config: RenderConfig) =
        let enabled () = not (config.Disabled())

        Html.div
            [
                attr.className "rf-list"
                Html.p
                    [
                        attr.className "rf-label"
                        Html.text attributes.Label
                    ]
                Html.div
                    [
                        attr.className "rf-list__items"
                        Html.each
                            config.Elements
                            (fun element -> element.Key)
                            (fun element ->
                                Html.div
                                    [
                                        attr.className "rf-list__item"
                                        yield! context.RenderItems context element.Items
                                        match attributes.Delete with
                                        | Some label ->
                                            Html.show (
                                                enabled,
                                                fun () ->
                                                    Html.button
                                                        [
                                                            attr.type' "button"
                                                            attr.className
                                                                "rf-button rf-button--small"
                                                            on.click (fun _ -> element.Delete())
                                                            Html.text label
                                                        ]
                                            )
                                        | None -> Html.none
                                    ]
                            )
                    ]
                match attributes.Add with
                | Some label ->
                    Html.show (
                        enabled,
                        fun () ->
                            Html.button
                                [
                                    attr.type' "button"
                                    attr.className "rf-button rf-button--small"
                                    on.click (fun _ -> config.Add())
                                    Html.text label
                                ]
                    )
                | None -> Html.none
            ]

type FormList =

    static member create(fieldId: string) : FormList.Attributes =
        {
            FieldId = fieldId
            Label = ""
            Add = None
            Delete = None
        }

    static member withLabel (label: string) (attributes: FormList.Attributes) =
        { attributes with
            Label = label
        }

    static member withAdd (add: string) (attributes: FormList.Attributes) =
        { attributes with
            Add = Some add
        }

    static member withDelete (delete: string) (attributes: FormList.Attributes) =
        { attributes with
            Delete = Some delete
        }
