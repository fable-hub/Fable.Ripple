module Fable.UrlParser.Tests.Main

open type Scriptorium.Quill.Runner
open type Scriptorium.Quill.Test

[<EntryPoint>]
let main _ =

    let tests =
        testList (
            "Fable.UrlParser",
            [
                Parser.tests
                UrlCodec.tests
            ]
        )

    runTests tests
