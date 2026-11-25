module Fable.Ripple.Tests.Main

open type Scriptorium.Quill.Runner
open type Scriptorium.Quill.Test

[<EntryPoint>]
let main _ =

    let tests =
        testList (
            "Fable.Ripple",
            [
                KeyMap.tests
                Signal.tests
            ]
        )

    runTests tests
