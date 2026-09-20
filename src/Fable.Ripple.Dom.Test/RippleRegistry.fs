namespace Fable.Ripple.Dom.Test

open Fable.Ripple.Dom
open Browser

(*
    RippleRegistry - browser-side component registration.

    Owns the window.__signalsMount bridge. Call register once per component in
    your Components.fs browser bundle; Playwright tests load them by name.

    A Signals.Dom `DomItem` is *eager* (it builds real DOM when constructed), so a
    component is registered as a `unit -> DomItem` factory and built fresh for each
    mount - never a pre-built `DomItem`, which could only be mounted once.

    Usage in Components.fs (browser bundle):

      open type Fable.Ripple.Dom.Test.RippleRegistry

      register ("Counter", Counter.render)
*)

module private Registry =

    open Fable.Core.JsInterop

    let private store = System.Collections.Generic.Dictionary<string, string -> unit>()

    do
        window?__signalsMount <-
            fun (name: string) (containerId: string) ->
                match store.TryGetValue name with
                | true, mount -> mount containerId
                | false, _ -> failwithf "Component '%s' not registered" name

    let add (name: string) (mount: string -> unit) = store.[name] <- mount

type RippleRegistry =

    /// Register a component by name so Playwright tests can load it. `view` is a
    /// `unit -> DomItem` factory, evaluated fresh (new DOM + reactive scope) per mount.
    static member register(name: string, view: unit -> DomItem) =
        Registry.add name (fun containerId -> Html.mount containerId view |> ignore)
