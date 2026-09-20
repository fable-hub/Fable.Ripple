namespace Fable.Ripple.Dom

open System
open Browser
open Browser.Types
open Fable.Ripple
open Base

/// HTML elements, text, keyed lists, conditionals, fragments and mounting. Each
/// element takes one `DomItem list` where attributes (`attr.*`), events (`ev.*`) and
/// child elements live together.
type Html =

    /// Generic element by tag name.
    static member elem (tag: string) (items: DomItem list) : DomItem = createElement tag items

    (*
        Text (overloaded: static / reactive). FS1114: must precede the inline
        element overloads that expand `Html.text`.
    *)

    static member text(s: string) : DomItem =
        Child(document.createTextNode s :> Node)

    static member text(f: unit -> string) : DomItem =
        let t = document.createTextNode ""
        Signal.effect (fun () -> t.nodeValue <- f ()) |> ignore
        Child(t :> Node)

    /// Reactive text from any signal source - a `Var` or a derived `Signal`, of any
    /// type (stringified). One SRTP overload covers them all, so a `Var` flows in
    /// directly with no `.Signal`. Forwards to the thunk overload above, keeping the
    /// internal `DomItem` construction out of the inlined body (so it inlines anywhere).
    static member inline text(s: ^s when ^s: (member get_Value: unit -> ^a)) : DomItem =
        Html.text (fun () -> string (^s: (member get_Value: unit -> ^a) s))

    (*
        Sections
    *)
    static member header(items: DomItem list) : DomItem = createElement "header" items
    static member header(s: string) : DomItem = createElement "header" [ Html.text s ]

    static member inline header(s: ^s when ^s: (member get_Value: unit -> ^a)) : DomItem =
        createElement "header" [ Html.text s ]

    static member footer(items: DomItem list) : DomItem = createElement "footer" items
    static member footer(s: string) : DomItem = createElement "footer" [ Html.text s ]

    static member inline footer(s: ^s when ^s: (member get_Value: unit -> ^a)) : DomItem =
        createElement "footer" [ Html.text s ]

    static member main(items: DomItem list) : DomItem = createElement "main" items
    static member section(items: DomItem list) : DomItem = createElement "section" items
    static member article(items: DomItem list) : DomItem = createElement "article" items
    static member aside(items: DomItem list) : DomItem = createElement "aside" items
    static member nav(items: DomItem list) : DomItem = createElement "nav" items
    static member h1(items: DomItem list) : DomItem = createElement "h1" items
    static member h1(s: string) : DomItem = createElement "h1" [ Html.text s ]

    static member inline h1(s: ^s when ^s: (member get_Value: unit -> ^a)) : DomItem =
        createElement "h1" [ Html.text s ]

    static member h2(items: DomItem list) : DomItem = createElement "h2" items
    static member h2(s: string) : DomItem = createElement "h2" [ Html.text s ]

    static member inline h2(s: ^s when ^s: (member get_Value: unit -> ^a)) : DomItem =
        createElement "h2" [ Html.text s ]

    static member h3(items: DomItem list) : DomItem = createElement "h3" items
    static member h3(s: string) : DomItem = createElement "h3" [ Html.text s ]

    static member inline h3(s: ^s when ^s: (member get_Value: unit -> ^a)) : DomItem =
        createElement "h3" [ Html.text s ]

    static member h4(items: DomItem list) : DomItem = createElement "h4" items
    static member h4(s: string) : DomItem = createElement "h4" [ Html.text s ]

    static member inline h4(s: ^s when ^s: (member get_Value: unit -> ^a)) : DomItem =
        createElement "h4" [ Html.text s ]

    static member h5(items: DomItem list) : DomItem = createElement "h5" items
    static member h5(s: string) : DomItem = createElement "h5" [ Html.text s ]

    static member inline h5(s: ^s when ^s: (member get_Value: unit -> ^a)) : DomItem =
        createElement "h5" [ Html.text s ]

    static member h6(items: DomItem list) : DomItem = createElement "h6" items
    static member h6(s: string) : DomItem = createElement "h6" [ Html.text s ]

    static member inline h6(s: ^s when ^s: (member get_Value: unit -> ^a)) : DomItem =
        createElement "h6" [ Html.text s ]

    static member hgroup(items: DomItem list) : DomItem = createElement "hgroup" items
    static member address(items: DomItem list) : DomItem = createElement "address" items
    static member search(items: DomItem list) : DomItem = createElement "search" items

    (*
        Grouping
    *)
    static member div(items: DomItem list) : DomItem = createElement "div" items
    static member p(items: DomItem list) : DomItem = createElement "p" items
    static member p(s: string) : DomItem = createElement "p" [ Html.text s ]

    static member inline p(s: ^s when ^s: (member get_Value: unit -> ^a)) : DomItem =
        createElement "p" [ Html.text s ]

    static member hr(items: DomItem list) : DomItem = createElement "hr" items
    static member pre(items: DomItem list) : DomItem = createElement "pre" items
    static member blockquote(items: DomItem list) : DomItem = createElement "blockquote" items

    static member blockquote(s: string) : DomItem =
        createElement "blockquote" [ Html.text s ]

    static member inline blockquote(s: ^s when ^s: (member get_Value: unit -> ^a)) : DomItem =
        createElement "blockquote" [ Html.text s ]

    static member ol(items: DomItem list) : DomItem = createElement "ol" items
    static member ul(items: DomItem list) : DomItem = createElement "ul" items
    static member li(items: DomItem list) : DomItem = createElement "li" items
    static member li(s: string) : DomItem = createElement "li" [ Html.text s ]

    static member inline li(s: ^s when ^s: (member get_Value: unit -> ^a)) : DomItem =
        createElement "li" [ Html.text s ]

    static member menu(items: DomItem list) : DomItem = createElement "menu" items
    static member dl(items: DomItem list) : DomItem = createElement "dl" items
    static member dt(items: DomItem list) : DomItem = createElement "dt" items
    static member dt(s: string) : DomItem = createElement "dt" [ Html.text s ]

    static member inline dt(s: ^s when ^s: (member get_Value: unit -> ^a)) : DomItem =
        createElement "dt" [ Html.text s ]

    static member dd(items: DomItem list) : DomItem = createElement "dd" items
    static member dd(s: string) : DomItem = createElement "dd" [ Html.text s ]

    static member inline dd(s: ^s when ^s: (member get_Value: unit -> ^a)) : DomItem =
        createElement "dd" [ Html.text s ]

    static member figure(items: DomItem list) : DomItem = createElement "figure" items
    static member figcaption(items: DomItem list) : DomItem = createElement "figcaption" items

    static member figcaption(s: string) : DomItem =
        createElement "figcaption" [ Html.text s ]

    static member inline figcaption(s: ^s when ^s: (member get_Value: unit -> ^a)) : DomItem =
        createElement "figcaption" [ Html.text s ]

    (*
        Text semantics
    *)
    static member a(items: DomItem list) : DomItem = createElement "a" items
    static member em(items: DomItem list) : DomItem = createElement "em" items
    static member em(s: string) : DomItem = createElement "em" [ Html.text s ]

    static member inline em(s: ^s when ^s: (member get_Value: unit -> ^a)) : DomItem =
        createElement "em" [ Html.text s ]

    static member strong(items: DomItem list) : DomItem = createElement "strong" items
    static member strong(s: string) : DomItem = createElement "strong" [ Html.text s ]

    static member inline strong(s: ^s when ^s: (member get_Value: unit -> ^a)) : DomItem =
        createElement "strong" [ Html.text s ]

    static member small(items: DomItem list) : DomItem = createElement "small" items
    static member small(s: string) : DomItem = createElement "small" [ Html.text s ]

    static member inline small(s: ^s when ^s: (member get_Value: unit -> ^a)) : DomItem =
        createElement "small" [ Html.text s ]

    static member s(items: DomItem list) : DomItem = createElement "s" items
    static member cite(items: DomItem list) : DomItem = createElement "cite" items
    static member cite(s: string) : DomItem = createElement "cite" [ Html.text s ]

    static member inline cite(s: ^s when ^s: (member get_Value: unit -> ^a)) : DomItem =
        createElement "cite" [ Html.text s ]

    static member q(items: DomItem list) : DomItem = createElement "q" items
    static member q(s: string) : DomItem = createElement "q" [ Html.text s ]

    static member inline q(s: ^s when ^s: (member get_Value: unit -> ^a)) : DomItem =
        createElement "q" [ Html.text s ]

    static member dfn(items: DomItem list) : DomItem = createElement "dfn" items
    static member abbr(items: DomItem list) : DomItem = createElement "abbr" items
    static member abbr(s: string) : DomItem = createElement "abbr" [ Html.text s ]

    static member inline abbr(s: ^s when ^s: (member get_Value: unit -> ^a)) : DomItem =
        createElement "abbr" [ Html.text s ]

    static member ruby(items: DomItem list) : DomItem = createElement "ruby" items
    static member rt(items: DomItem list) : DomItem = createElement "rt" items
    static member rp(items: DomItem list) : DomItem = createElement "rp" items
    static member data(items: DomItem list) : DomItem = createElement "data" items
    static member time(items: DomItem list) : DomItem = createElement "time" items
    static member code(items: DomItem list) : DomItem = createElement "code" items
    static member code(s: string) : DomItem = createElement "code" [ Html.text s ]

    static member inline code(s: ^s when ^s: (member get_Value: unit -> ^a)) : DomItem =
        createElement "code" [ Html.text s ]

    static member var(items: DomItem list) : DomItem = createElement "var" items
    static member var(s: string) : DomItem = createElement "var" [ Html.text s ]

    static member inline var(s: ^s when ^s: (member get_Value: unit -> ^a)) : DomItem =
        createElement "var" [ Html.text s ]

    static member samp(items: DomItem list) : DomItem = createElement "samp" items
    static member samp(s: string) : DomItem = createElement "samp" [ Html.text s ]

    static member inline samp(s: ^s when ^s: (member get_Value: unit -> ^a)) : DomItem =
        createElement "samp" [ Html.text s ]

    static member kbd(items: DomItem list) : DomItem = createElement "kbd" items
    static member kbd(s: string) : DomItem = createElement "kbd" [ Html.text s ]

    static member inline kbd(s: ^s when ^s: (member get_Value: unit -> ^a)) : DomItem =
        createElement "kbd" [ Html.text s ]

    static member sub(items: DomItem list) : DomItem = createElement "sub" items
    static member sup(items: DomItem list) : DomItem = createElement "sup" items
    static member i(items: DomItem list) : DomItem = createElement "i" items
    static member b(items: DomItem list) : DomItem = createElement "b" items
    static member u(items: DomItem list) : DomItem = createElement "u" items
    static member mark(items: DomItem list) : DomItem = createElement "mark" items
    static member mark(s: string) : DomItem = createElement "mark" [ Html.text s ]

    static member inline mark(s: ^s when ^s: (member get_Value: unit -> ^a)) : DomItem =
        createElement "mark" [ Html.text s ]

    static member bdi(items: DomItem list) : DomItem = createElement "bdi" items
    static member bdo(items: DomItem list) : DomItem = createElement "bdo" items
    static member span(items: DomItem list) : DomItem = createElement "span" items
    static member span(s: string) : DomItem = createElement "span" [ Html.text s ]

    static member inline span(s: ^s when ^s: (member get_Value: unit -> ^a)) : DomItem =
        createElement "span" [ Html.text s ]

    static member br(items: DomItem list) : DomItem = createElement "br" items
    static member wbr(items: DomItem list) : DomItem = createElement "wbr" items
    static member ins(items: DomItem list) : DomItem = createElement "ins" items
    static member del(items: DomItem list) : DomItem = createElement "del" items

    (*
        Embedded
    *)
    static member picture(items: DomItem list) : DomItem = createElement "picture" items
    static member source(items: DomItem list) : DomItem = createElement "source" items
    static member img(items: DomItem list) : DomItem = createElement "img" items
    static member iframe(items: DomItem list) : DomItem = createElement "iframe" items
    static member embed(items: DomItem list) : DomItem = createElement "embed" items
    static member object(items: DomItem list) : DomItem = createElement "object" items
    static member param(items: DomItem list) : DomItem = createElement "param" items
    static member video(items: DomItem list) : DomItem = createElement "video" items
    static member audio(items: DomItem list) : DomItem = createElement "audio" items
    static member track(items: DomItem list) : DomItem = createElement "track" items
    static member map(items: DomItem list) : DomItem = createElement "map" items
    static member area(items: DomItem list) : DomItem = createElement "area" items
    static member canvas(items: DomItem list) : DomItem = createElement "canvas" items

    (*
        Tables
    *)
    static member table(items: DomItem list) : DomItem = createElement "table" items
    static member caption(items: DomItem list) : DomItem = createElement "caption" items
    static member caption(s: string) : DomItem = createElement "caption" [ Html.text s ]

    static member inline caption(s: ^s when ^s: (member get_Value: unit -> ^a)) : DomItem =
        createElement "caption" [ Html.text s ]

    static member colgroup(items: DomItem list) : DomItem = createElement "colgroup" items
    static member col(items: DomItem list) : DomItem = createElement "col" items
    static member thead(items: DomItem list) : DomItem = createElement "thead" items
    static member tbody(items: DomItem list) : DomItem = createElement "tbody" items
    static member tfoot(items: DomItem list) : DomItem = createElement "tfoot" items
    static member tr(items: DomItem list) : DomItem = createElement "tr" items
    static member td(items: DomItem list) : DomItem = createElement "td" items
    static member td(s: string) : DomItem = createElement "td" [ Html.text s ]

    static member inline td(s: ^s when ^s: (member get_Value: unit -> ^a)) : DomItem =
        createElement "td" [ Html.text s ]

    static member th(items: DomItem list) : DomItem = createElement "th" items
    static member th(s: string) : DomItem = createElement "th" [ Html.text s ]

    static member inline th(s: ^s when ^s: (member get_Value: unit -> ^a)) : DomItem =
        createElement "th" [ Html.text s ]

    (*
        Forms
    *)
    static member form(items: DomItem list) : DomItem = createElement "form" items
    static member label(items: DomItem list) : DomItem = createElement "label" items
    static member input(items: DomItem list) : DomItem = createElement "input" items
    static member button(items: DomItem list) : DomItem = createElement "button" items
    static member select(items: DomItem list) : DomItem = createElement "select" items
    static member datalist(items: DomItem list) : DomItem = createElement "datalist" items
    static member optgroup(items: DomItem list) : DomItem = createElement "optgroup" items
    static member option(items: DomItem list) : DomItem = createElement "option" items
    static member option(s: string) : DomItem = createElement "option" [ Html.text s ]

    static member inline option(s: ^s when ^s: (member get_Value: unit -> ^a)) : DomItem =
        createElement "option" [ Html.text s ]

    static member textarea(items: DomItem list) : DomItem = createElement "textarea" items
    static member output(items: DomItem list) : DomItem = createElement "output" items
    static member output(s: string) : DomItem = createElement "output" [ Html.text s ]

    static member inline output(s: ^s when ^s: (member get_Value: unit -> ^a)) : DomItem =
        createElement "output" [ Html.text s ]

    static member progress(items: DomItem list) : DomItem = createElement "progress" items
    static member meter(items: DomItem list) : DomItem = createElement "meter" items
    static member fieldset(items: DomItem list) : DomItem = createElement "fieldset" items
    static member legend(items: DomItem list) : DomItem = createElement "legend" items
    static member legend(s: string) : DomItem = createElement "legend" [ Html.text s ]

    static member inline legend(s: ^s when ^s: (member get_Value: unit -> ^a)) : DomItem =
        createElement "legend" [ Html.text s ]

    (*
        Interactive
    *)
    static member details(items: DomItem list) : DomItem = createElement "details" items
    static member summary(items: DomItem list) : DomItem = createElement "summary" items
    static member summary(s: string) : DomItem = createElement "summary" [ Html.text s ]

    static member inline summary(s: ^s when ^s: (member get_Value: unit -> ^a)) : DomItem =
        createElement "summary" [ Html.text s ]

    static member dialog(items: DomItem list) : DomItem = createElement "dialog" items
    static member slot(items: DomItem list) : DomItem = createElement "slot" items

    /// Empty item, renders nothing.
    static member none: DomItem = Empty emptyMarker

    /// Splice a `Node` built elsewhere - SVG, a server-rendered fragment, or the
    /// output of a non-Fable library - as a child. Standard DOM rules apply: a node
    /// already mounted is *moved*, not copied.
    static member node(n: Node) : DomItem = Child n

    /// Group children with no wrapper element - splices `items` into the parent.
    static member fragment(items: DomItem list) : DomItem = Apply(fun e -> applyItems e items)

    /// Reactive keyed list: one element per item, reconciled by key.
    static member each
        (getItems: unit -> 'a[])
        (keyOf: 'a -> 'k)
        (render: 'a -> DomItem)
        : DomItem
        =
        Apply(fun parent ->
            Dom.keyedEach parent null getItems keyOf (fun x -> toElement (render x))
            |> ignore
        )

    /// Reactive subtree: rebuilds `f ()` in place whenever the signals it reads
    /// change; each rebuild runs in its own `Signal.root`, disposed on rebuild/
    /// teardown. Use it with a native `match`/`if` to switch views on a signal.
    static member dynamic(f: unit -> DomItem) : DomItem =
        Apply(fun parent ->
            let anchor = document.createComment "dynamic" :> Node
            parent.appendChild anchor |> ignore

            // `Node option`, not `Node`: a branch may legitimately render nothing
            // (`Html.none`), leaving a scope to dispose but no node to remove.
            let mutable current: (Node option * IDisposable) option = None

            let clear () =
                current
                |> Option.iter (fun (node, dispose) ->
                    dispose.Dispose()
                    node |> Option.iter (fun n -> parent.removeChild n |> ignore)
                )

                current <- None

            Signal.effect (fun () ->
                clear ()

                let node, dispose =
                    Signal.root (fun () ->
                        match f () with
                        | Apply _ -> failwith "Html.dynamic expects an element or Html.none"
                        | Child node -> Some node
                        | Empty _ -> None
                    )

                node |> Option.iter (fun n -> parent.insertBefore (n, anchor) |> ignore)
                current <- Some(node, dispose)
            )
            |> ignore

            Signal.onCleanup clear
        )

    /// `switch` over a value computed by `read`, for a shape that depends on more
    /// than one signal without naming a derived signal for it.
    static member switchWith(read: unit -> 'T, f: 'T -> DomItem) : DomItem =
        let value = Signal.computed read

        Html.dynamic (fun () ->
            let current = value.Value
            Signal.untracked (fun () -> f current)
        )

    /// A subtree rebuilt from one signal source: `f` receives the value, and only
    /// that source can rebuild it.
    ///
    /// The branch runs untracked, so a signal read directly inside it is an
    /// ordinary read - the value at build time - rather than a second rebuild
    /// trigger. Bindings nested in the branch (`Html.text` with a function,
    /// reactive attributes) keep their own tracking and update in place.
    // The inlined body calls only into this file. Reaching across to another
    // assembly from here (`Signal.untracked`) makes the in-browser compiler fetch
    // that assembly's source by absolute path, which 404s.
    static member inline switch
        (s: ^s when ^s: (member get_Value: unit -> ^a))
        (f: ^a -> DomItem)
        : DomItem
        =
        Html.switchWith ((fun () -> (^s: (member get_Value: unit -> ^a) s)), f)

    /// Reactive conditional: renders `whenTrue ()` while `cond` holds, otherwise
    /// nothing. The active branch is built fresh when it appears and disposed when
    /// it leaves; `cond` is auto-tracked, so it re-evaluates on signal changes.
    static member show(cond: unit -> bool, whenTrue: unit -> DomItem) : DomItem =
        Html.dynamic (fun () ->
            if cond () then
                whenTrue ()
            else
                Html.none
        )

    /// Reactive conditional with a fallback: `whenTrue ()` while `cond` holds,
    /// else `whenFalse ()`.
    static member show
        (cond: unit -> bool, whenTrue: unit -> DomItem, whenFalse: unit -> DomItem)
        : DomItem
        =
        Html.dynamic (fun () ->
            if cond () then
                whenTrue ()
            else
                whenFalse ()
        )

    /// `show` driven directly by a signal source (a `Var`/`Signal` of `bool`).
    static member inline show
        (cond: ^s when ^s: (member get_Value: unit -> bool), whenTrue: unit -> DomItem)
        : DomItem
        =
        Html.show ((fun () -> (^s: (member get_Value: unit -> bool) cond)), whenTrue)

    static member inline show
        (
            cond: ^s when ^s: (member get_Value: unit -> bool),
            whenTrue: unit -> DomItem,
            whenFalse: unit -> DomItem
        )
        : DomItem
        =
        Html.show ((fun () -> (^s: (member get_Value: unit -> bool) cond)), whenTrue, whenFalse)

    /// Realise a root item to its element.
    static member render(item: DomItem) : HTMLElement = toElement item

    /// Mount `view` into the element with the given id, inside its own
    /// `Signal.root`. Disposing the result tears that scope down and removes the
    /// mounted element; disposing twice is a no-op.
    ///
    /// `view` is a function, not a `DomItem`: a `DomItem` argument would be built -
    /// effects and all - before `mount` opened the scope that is meant to own it.
    static member mount (id: string) (view: unit -> DomItem) : IDisposable =
        let container = document.getElementById id
        let node, scope = Signal.root (fun () -> toElement (view ()))

        container.appendChild node |> ignore

        let mutable live = true

        { new IDisposable with
            member _.Dispose() =
                if live then
                    live <- false
                    scope.Dispose()
                    container.removeChild node |> ignore
        }
