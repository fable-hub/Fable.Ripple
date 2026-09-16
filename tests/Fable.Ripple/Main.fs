module Fable.Ripple.Tests.Main

open type Scriptorium.Quill.Runner
open type Scriptorium.Quill.Test

[<EntryPoint>]
let main _ =

    let tests = testList ("Fable.Ripple", [ Signal.tests ])

    runTests tests
