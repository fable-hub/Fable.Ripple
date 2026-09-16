var e=`module Demo.Examples.Components

open Browser.Types
open Fable.Ripple.Dom

// The demo's own styling, behind a typed API.
//
// \`Html.div [ attr.className "stack" … ]\` says nothing about the lesson, and the
// class name is a bare string - undiscoverable, unrenameable, wrong only at
// runtime. Three classes account for 70% of that noise across the examples, so
// three definitions remove most of it.
//
// A component takes a \`DomItem list\` and hands it back with something added,
// and that is the whole mechanism. Because an element takes ONE list holding
// attributes, handlers and children alike, there is no props record to design
// and no props/children split to get inconsistent about. A component does not
// hide the one-list model - it is that model with something concatenated.
//
//     Row.row [
//         Button.primary ("Go", submit)
//         Button.action ("Cancel", cancel, [ attr.disabled busy ])
//     ]
//
// \`Button\` shows the shape at its most useful: the two items every button has
// are parameters, and everything else is still just the list, passed through.
//
// One module per component, each owning its constructor and its variants. The
// \`Stack.stack\` stutter is deliberate: it is what gives each component a
// namespace of its own, so adding a variant later is a local change rather than
// another entry in a flat list of everything.
//
// Variants are \`DomItem\` values, so they need no mechanism at all - they go in
// the same list as everything else. A variant that depends on a signal needs no
// special form either: \`attr.classList (fun () -> [ "primary", cond.Value ])\` is
// already reactive and already diffs tokens, and \`Html.none\` is the no-op for a
// plain conditional.

/// Apply a component's class.
///
/// \`attr.classList\`, and APPENDED - both matter. \`attr.className\` owns the whole
/// class attribute, so a caller passing one would silently drop the component's
/// own class; \`classList\` goes through the DOM API and adds a token instead.
/// Appending means the class survives even a caller that sets \`className\`
/// wholesale, which is the point: the class is the one thing a component
/// promises, and a promise a later attribute can quietly void is not one.
///
/// The cost is that an \`attr.ref\` in the caller's list runs before the class is
/// applied. Nothing here reads its own class from a ref - the example that does
/// is Rendering / Classes that compose, which stays hand-written for exactly
/// that reason.
let private styled (name: string) (element: DomItem list -> DomItem) (items: DomItem list) =
    element (items @ [ attr.classList [ name, true ] ])

/// A vertical group: the default container for an example's contents.
module Stack =

    let stack (items: DomItem list) : DomItem = styled "stack" Html.div items

    /// Caps the width, for a form that should not span the whole panel.
    let narrow: DomItem = attr.classList [ "is-narrow", true ]

/// A horizontal group, wrapping, centred on its children.
module Row =

    let row (items: DomItem list) : DomItem = styled "row" Html.div items

    /// Align children at the top rather than the centre - for columns of
    /// differing height, where centring reads as misalignment.
    let top: DomItem = attr.classList [ "is-top", true ]

/// Buttons: a label, a click, and whatever else the list needs.
///
/// \`extra\` is CONCATENATED with the list the component builds, exactly as
/// \`styled\` above concatenates its class. That is what keeps this a component
/// and not a props record: there is no parameter per attribute, and anything
/// that can go in \`Html.button [ ... ]\` goes in \`extra\` unchanged - including
/// all three shapes of \`attr.disabled\`, because each is only a \`DomItem\`.
///
/// A type rather than a module, for two things a module function cannot do: an
/// optional trailing argument, and an overload on the label so a reactive one
/// costs no second name.
type Button =

    /// A literal label.
    static member action(label: string, onClick: MouseEvent -> unit, ?extra: DomItem list) =
        Html.button (
            [
                on.click onClick
                Html.text label
            ]
            @ defaultArg extra []
        )

    /// A label that follows a signal.
    static member action(label: unit -> string, onClick: MouseEvent -> unit, ?extra: DomItem list) =
        Html.button (
            [
                on.click onClick
                Html.text label
            ]
            @ defaultArg extra []
        )

    /// The main action. At most one per row - emphasis that is everywhere is not
    /// emphasis.
    ///
    /// The class lands AFTER \`extra\`, for the reason \`styled\` gives: a caller's
    /// \`attr.className\` owns the whole attribute and would drop it, whereas
    /// \`classList\` appends a token and survives.
    static member primary(label: string, onClick: MouseEvent -> unit, ?extra: DomItem list) =
        Html.button (
            [
                on.click onClick
                Html.text label
            ]
            @ defaultArg extra []
            @ [ attr.classList [ "primary", true ] ]
        )

    static member primary
        (label: unit -> string, onClick: MouseEvent -> unit, ?extra: DomItem list)
        =
        Html.button (
            [
                on.click onClick
                Html.text label
            ]
            @ defaultArg extra []
            @ [ attr.classList [ "primary", true ] ]
        )

/// Split a string on backticks, so prose can name an identifier without the
/// call site building spans by hand. Odd-numbered pieces are the code.
let private prose (text: string) : DomItem list =
    text.Split '\`'
    |> Array.toList
    |> List.mapi (fun i piece ->
        if i % 2 = 1 then
            Html.code piece
        else
            Html.text piece
    )

/// What the reader should do, and what it proves.
///
/// An example used to explain itself in two places the reader is not looking: a
/// header comment inside the source listing, which is collapsed, and a note
/// UNDER the controls, arriving after the confusion rather than before it.
/// \`Try.observe\` leads instead; \`Try.outcome\` closes, on the few examples with
/// something left to say that the lead cannot carry.
///
/// Both take prose with backticks for code, so a line can name a button or an
/// identifier without the call site assembling spans.
module Try =

    /// What to do and what to watch for, in one or two sentences, above the demo.
    ///
    /// Stating the point up front means the reader knows what they are looking
    /// at before they look at it, rather than being told afterwards what they
    /// should have noticed. Every example turned out to fit in one or two
    /// sentences; the numbered list this replaced only ever made "press this,
    /// watch that" look like a procedure.
    let observe (text: string) : DomItem = Html.p [ Html.fragment (prose text) ]

    /// What the reader should now be seeing, and why it matters.
    let outcome (text: string) : DomItem =
        Html.p
            [
                attr.classList [ "outcome", true ]
                Html.fragment (prose text)
            ]

/// An F# specimen inside a panel: the construction that panel is about, shown
/// rather than described, and highlighted the same way the code panel is.
///
/// The source is a literal rather than a slice of this file, because the point
/// is the shape of the construction - not where it happens to sit here.
module Code =

    let block (source: string) : DomItem =
        Html.div
            [
                attr.className "specimen"
                Demo.Highlight.fsharp source
            ]

/// Two or three things placed side by side, each stating its own claim.
///
/// A label alone ("plain function") tells the reader what a column IS but not
/// what to look for in it, so the comparison only lands for someone who already
/// knows the answer. Every side therefore carries a one-line claim, and the
/// numbers underneath are that claim's evidence.
module Compare =

    /// One column: what it is, what it claims, and the live proof.
    let side (label: string) (claim: string) (body: DomItem) : DomItem =
        Html.article
            [
                Html.header label
                Html.p [ Html.fragment (prose claim) ]
                body
            ]

    let compare (sides: DomItem list) : DomItem = Html.div sides
`;export{e as default};