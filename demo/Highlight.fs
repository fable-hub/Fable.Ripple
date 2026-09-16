module Demo.Highlight

open Fable.Ripple.Dom

/// Coloured by `<nacara-highlight>`, which the docs page defines (`TreeSitter.browser` in docs/Site.fs).
/// `code` is an attribute because a property set before the element is defined is lost on upgrade.
/// The `<pre><code>` child shows the plain text until then; the element colours that same `<code>`.
let fsharp (code: string) : DomItem =
    Html.elem
        "nacara-highlight"
        [
            attr.custom ("language", "fsharp")
            attr.custom ("code", code)
            Html.pre [ Html.code code ]
        ]
