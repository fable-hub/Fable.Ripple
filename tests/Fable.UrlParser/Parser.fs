module Fable.UrlParser.Tests.Parser

open Scriptorium.Nib.Assertion

open type Scriptorium.Quill.Test

open Fable.UrlParser

(*
    Route types
*)

type Level =
    | Low
    | Medium
    | High

module Level =
    let tryParse (value: string) =
        match value with
        | "low" -> Some Low
        | "medium" -> Some Medium
        | "high" -> Some High
        | _ -> None

type Route =
    | Home
    | Counter of int
    | UserPost of userId: int * postId: int
    | Display of showOption: bool
    | Search of query: string * page: int option
    | TagList of string list
    | Archive of year: int * month: int
    | File of name: string * section: string option
    | Post of slug: string
    | Ids of int list
    | Settings of flags: string list
    | Priority of Level
    | Filter of level: Level option
    | Sort of Level
    | Double of int

(*
    Parsers
*)

let homeParser = Parser.succeed Home |> Parser.segment "home"

let counterParser = Parser.succeed Counter |> Parser.segment "counter" |> Parser.int

let userPostParser =
    Parser.succeed (fun uid pid -> UserPost(uid, pid))
    |> Parser.segment "user"
    |> Parser.int
    |> Parser.segment "post"
    |> Parser.int

let displayParser =
    Parser.succeed Display
    |> Parser.segment "display"
    |> Parser.Query.flag "showOption"

let searchParser =
    Parser.succeed (fun q page -> Search(q, page))
    |> Parser.segment "search"
    |> Parser.Query.Required.string "q"
    |> Parser.Query.Optional.int "page"

let tagsParser =
    Parser.succeed TagList |> Parser.segment "tags" |> Parser.Query.strings "tag"

let archiveParser =
    Parser.succeed (fun y m -> Archive(y, m))
    |> Parser.segment "archive"
    |> Parser.Query.Required.int "year"
    |> Parser.Query.Required.int "month"

let fileParser =
    Parser.succeed (fun section -> File("readme", section))
    |> Parser.segment "file"
    |> Parser.segment "readme"
    |> Parser.fragment

let postParser = Parser.succeed Post |> Parser.segment "post" |> Parser.string

let idsParser = Parser.succeed Ids |> Parser.segment "ids" |> Parser.Query.ints "id"

let settingsParser =
    Parser.succeed Settings |> Parser.segment "settings" |> Parser.Query.allFlags

let priorityParser =
    Parser.succeed Priority
    |> Parser.segment "priority"
    |> Parser.custom "Level" Level.tryParse

let filterParser =
    Parser.succeed Filter
    |> Parser.segment "filter"
    |> Parser.Query.Optional.custom "level" "Level" Level.tryParse

let sortParser =
    Parser.succeed Sort
    |> Parser.segment "sort"
    |> Parser.Query.Required.custom "level" "Level" Level.tryParse

let doubleParser =
    Parser.succeed id
    |> Parser.segment "double"
    |> Parser.int
    |> Parser.map (fun n -> Double(n * 2))

let allParsers =
    [
        homeParser
        counterParser
        userPostParser
        displayParser
        searchParser
        tagsParser
        archiveParser
        fileParser
        postParser
        idsParser
        priorityParser
        filterParser
        sortParser
        doubleParser
        settingsParser
    ]

let parsePath = Parser.tryParsePath allParsers
let parseHash = Parser.tryParseHash allParsers

(*
    Assertion helpers
*)

/// Assert the result is Ok and equal to the expected route.
let isRoute r = Result.okValue >> isEqualTo r

/// Assert the result is an Error whose formatted message equals the expected string.
let isErrorMsg (expectedMsg: string) =
    Result.errorValue >> isEqualTo expectedMsg

(*
    Tests
*)

let tests =
    testList (
        "Parser",
        [

            (*
                segment
            *)

            testList (
                "segment",
                [

                    test (
                        "matches a literal segment",
                        fun _ -> assertThat (parsePath "/home") (isRoute Home)
                    )

                    test (
                        "fails with SegmentMismatch when segment does not match",
                        fun _ ->
                            assertThat
                                (parsePath "/notfound")
                                (isErrorMsg "Expected segment 'settings' but got 'notfound'.")
                    )

                    test (
                        "fails with NoParsers when parser list is empty",
                        fun _ ->
                            assertThat
                                (Parser.tryParsePath [] "/home")
                                (isErrorMsg "No parsers were provided.")
                    )

                    test (
                        "fails with LeftoverSegments when URL has extra segments",
                        fun _ ->
                            assertThat
                                (parsePath "/home/extra")
                                (isErrorMsg "URL matched but had leftover segments: 'extra'.")
                    )

                    test (
                        "ignores leading slash",
                        fun _ -> assertThat (parsePath "/home") (isEqualTo (parsePath "home"))
                    )

                    test (
                        "ignores trailing slash",
                        fun _ -> assertThat (parsePath "/home/") (isRoute Home)
                    )

                ]
            )

            (*
                int (path segment)
            *)

            testList (
                "int",
                [

                    test (
                        "parses a valid integer segment",
                        fun _ -> assertThat (parsePath "/counter/42") (isRoute (Counter 42))
                    )

                    test (
                        "parses zero",
                        fun _ -> assertThat (parsePath "/counter/0") (isRoute (Counter 0))
                    )

                    test (
                        "parses a negative integer",
                        fun _ -> assertThat (parsePath "/counter/-7") (isRoute (Counter -7))
                    )

                    test (
                        "fails with BadType when segment is not an integer",
                        fun _ ->
                            assertThat
                                (parsePath "/counter/abc")
                                (isErrorMsg "Could not convert 'abc' to int.")
                    )

                    test (
                        "fails with MissingSegment when segment is absent",
                        fun _ ->
                            assertThat
                                (parsePath "/counter")
                                (isErrorMsg "Expected a path segment but the URL ended.")
                    )

                ]
            )

            (*
                multiple segments and int combinators
            *)

            testList (
                "multiple segments",
                [

                    test (
                        "parses two path int segments",
                        fun _ ->
                            assertThat (parsePath "/user/42/post/7") (isRoute (UserPost(42, 7)))
                    )

                    test (
                        "fails when intermediate literal segment mismatches",
                        fun _ ->
                            assertThat
                                (parsePath "/user/42/comment/7")
                                (isErrorMsg "Expected segment 'post' but got 'comment'.")
                    )

                ]
            )

            (*
                Query.flag
            *)

            testList (
                "Query.flag",
                [

                    test (
                        "returns true when flag is present",
                        fun _ ->
                            assertThat (parsePath "/display?showOption") (isRoute (Display true))
                    )

                    test (
                        "returns false when flag is absent",
                        fun _ -> assertThat (parsePath "/display") (isRoute (Display false))
                    )

                ]
            )

            (*
                Query.strings
            *)

            testList (
                "Query.strings",
                [

                    test (
                        "returns empty list when key is absent",
                        fun _ -> assertThat (parsePath "/tags") (isRoute (TagList []))
                    )

                    test (
                        "returns single value list",
                        fun _ ->
                            assertThat
                                (parsePath "/tags?tag=fsharp")
                                (isRoute (TagList [ "fsharp" ]))
                    )

                    test (
                        "returns multiple values",
                        fun _ ->
                            assertThat
                                (parsePath "/tags?tag=fsharp&tag=fable")
                                (isRoute (
                                    TagList
                                        [
                                            "fsharp"
                                            "fable"
                                        ]
                                ))
                    )

                ]
            )

            (*
                Query.Optional.int
            *)

            testList (
                "Query.Optional.int",
                [

                    test (
                        "returns None when key is absent",
                        fun _ ->
                            assertThat
                                (parsePath "/search?q=hello")
                                (isRoute (Search("hello", None)))
                    )

                    test (
                        "returns Some when key is present and valid",
                        fun _ ->
                            assertThat
                                (parsePath "/search?q=hello&page=2")
                                (isRoute (Search("hello", Some 2)))
                    )

                    test (
                        "fails with BadType when value is not an integer",
                        fun _ ->
                            assertThat
                                (parsePath "/search?q=hello&page=abc")
                                (isErrorMsg "Could not convert 'abc' to int.")
                    )

                    test (
                        "fails with AmbiguousQueryParameter when key appears more than once",
                        fun _ ->
                            assertThat
                                (parsePath "/search?q=hello&page=1&page=2")
                                (isErrorMsg
                                    "Query parameter 'page' appeared multiple times with values: 1, 2.")
                    )

                ]
            )

            (*
                Query.Required.string
            *)

            testList (
                "Query.Required.string",
                [

                    test (
                        "succeeds when key is present",
                        fun _ ->
                            assertThat
                                (parsePath "/search?q=fsharp")
                                (isRoute (Search("fsharp", None)))
                    )

                    test (
                        "fails with MissingQueryParameter when key is absent",
                        fun _ ->
                            assertThat
                                (parsePath "/search?page=1")
                                (isErrorMsg "Required query parameter 'q' was missing.")
                    )

                    test (
                        "fails with AmbiguousQueryParameter when key appears more than once",
                        fun _ ->
                            assertThat
                                (parsePath "/search?q=a&q=b")
                                (isErrorMsg
                                    "Query parameter 'q' appeared multiple times with values: a, b.")
                    )

                ]
            )

            (*
                Query.Required.int
            *)

            testList (
                "Query.Required.int",
                [

                    test (
                        "succeeds when key is present and valid",
                        fun _ ->
                            assertThat
                                (parsePath "/archive?year=2024&month=3")
                                (isRoute (Archive(2024, 3)))
                    )

                    test (
                        "fails with MissingQueryParameter when key is absent",
                        fun _ ->
                            assertThat
                                (parsePath "/archive?year=2024")
                                (isErrorMsg "Required query parameter 'month' was missing.")
                    )

                    test (
                        "fails with BadType when value is not an integer",
                        fun _ ->
                            assertThat
                                (parsePath "/archive?year=2024&month=abc")
                                (isErrorMsg "Could not convert 'abc' to int.")
                    )

                    test (
                        "fails with AmbiguousQueryParameter when key appears more than once",
                        fun _ ->
                            assertThat
                                (parsePath "/archive?year=2024&month=3&month=4")
                                (isErrorMsg
                                    "Query parameter 'month' appeared multiple times with values: 3, 4.")
                    )

                ]
            )

            (*
                fragment
            *)

            testList (
                "fragment",
                [

                    test (
                        "returns Some when fragment is present",
                        fun _ ->
                            assertThat
                                (parsePath "/file/readme#introduction")
                                (isRoute (File("readme", Some "introduction")))
                    )

                    test (
                        "returns None when fragment is absent",
                        fun _ ->
                            assertThat (parsePath "/file/readme") (isRoute (File("readme", None)))
                    )

                ]
            )

            (*
                parseHash
            *)

            testList (
                "parseHash",
                [

                    test (
                        "parses a hash URL with leading #/",
                        fun _ -> assertThat (parseHash "#/counter/99") (isRoute (Counter 99))
                    )

                    test (
                        "parses a hash URL with query parameters",
                        fun _ ->
                            assertThat
                                (parseHash "#/search?q=fsharp&page=1")
                                (isRoute (Search("fsharp", Some 1)))
                    )

                    test (
                        "parses a hash URL with required int query parameters",
                        fun _ ->
                            assertThat
                                (parseHash "#/archive?year=2025&month=12")
                                (isRoute (Archive(2025, 12)))
                    )

                    test (
                        "fails correctly through a hash URL",
                        fun _ ->
                            assertThat
                                (parseHash "#/search?page=1")
                                (isErrorMsg "Required query parameter 'q' was missing.")
                    )

                ]
            )

            (*
                error depth reporting
            *)

            testList (
                "error depth reporting",
                [

                    test (
                        "reports the error from the deepest-matching parser",
                        fun _ ->
                            assertThat
                                (parsePath "/user/42/comment/7")
                                (isErrorMsg "Expected segment 'post' but got 'comment'.")
                    )

                ]
            )

            (*
                string (path segment)
            *)

            testList (
                "string",
                [

                    test (
                        "parses a string segment",
                        fun _ ->
                            assertThat
                                (parsePath "/post/hello-world")
                                (isRoute (Post "hello-world"))
                    )

                    test (
                        "parses a string segment with special characters",
                        fun _ ->
                            assertThat
                                (parsePath "/post/hello world")
                                (isRoute (Post "hello world"))
                    )

                    test (
                        "fails with MissingSegment when segment is absent",
                        fun _ ->
                            assertThat
                                (parsePath "/post")
                                (isErrorMsg "Expected a path segment but the URL ended.")
                    )

                ]
            )

            (*
                Query.ints
            *)

            testList (
                "Query.ints",
                [

                    test (
                        "returns empty list when key is absent",
                        fun _ -> assertThat (parsePath "/ids") (isRoute (Ids []))
                    )

                    test (
                        "returns single int value",
                        fun _ -> assertThat (parsePath "/ids?id=1") (isRoute (Ids [ 1 ]))
                    )

                    test (
                        "returns multiple int values",
                        fun _ ->
                            assertThat
                                (parsePath "/ids?id=1&id=2&id=3")
                                (isRoute (
                                    Ids
                                        [
                                            1
                                            2
                                            3
                                        ]
                                ))
                    )

                    test (
                        "fails when any value is not an integer",
                        fun _ ->
                            assertThat
                                (parsePath "/ids?id=1&id=abc")
                                (isErrorMsg
                                    "Not all values for query parameter 'id' could be converted to int: 1, abc.")
                    )

                ]
            )

            (*
                Query.allFlags
            *)

            testList (
                "Query.allFlags",
                [

                    test (
                        "returns empty list when no flags are present",
                        fun _ -> assertThat (parsePath "/settings") (isRoute (Settings []))
                    )

                    test (
                        "returns single flag",
                        fun _ ->
                            assertThat
                                (parsePath "/settings?admin")
                                (isRoute (Settings [ "admin" ]))
                    )

                    test (
                        "returns multiple flags sorted",
                        fun _ ->
                            assertThat
                                (parsePath "/settings?admin&beta&readonly")
                                (isRoute (
                                    Settings
                                        [
                                            "admin"
                                            "beta"
                                            "readonly"
                                        ]
                                ))
                    )

                ]
            )

            (*
                custom (path segment)
            *)

            testList (
                "custom",
                [

                    test (
                        "parses a segment that converts successfully",
                        fun _ -> assertThat (parsePath "/priority/high") (isRoute (Priority High))
                    )

                    test (
                        "fails with BadType when the conversion function returns None",
                        fun _ ->
                            assertThat
                                (parsePath "/priority/urgent")
                                (isErrorMsg "Could not convert 'urgent' to Level.")
                    )

                    test (
                        "fails with MissingSegment when segment is absent",
                        fun _ ->
                            assertThat
                                (parsePath "/priority")
                                (isErrorMsg "Expected a path segment but the URL ended.")
                    )

                ]
            )

            (*
                Query.Optional.custom
            *)

            testList (
                "Query.Optional.custom",
                [

                    test (
                        "returns None when key is absent",
                        fun _ -> assertThat (parsePath "/filter") (isRoute (Filter None))
                    )

                    test (
                        "returns Some when key is present and converts successfully",
                        fun _ ->
                            assertThat
                                (parsePath "/filter?level=medium")
                                (isRoute (Filter(Some Medium)))
                    )

                    test (
                        "fails with BadType when the conversion function returns None",
                        fun _ ->
                            assertThat
                                (parsePath "/filter?level=urgent")
                                (isErrorMsg "Could not convert 'urgent' to Level.")
                    )

                    test (
                        "fails with AmbiguousQueryParameter when key appears more than once",
                        fun _ ->
                            assertThat
                                (parsePath "/filter?level=low&level=high")
                                (isErrorMsg
                                    "Query parameter 'level' appeared multiple times with values: low, high.")
                    )

                ]
            )

            (*
                Query.Required.custom
            *)

            testList (
                "Query.Required.custom",
                [

                    test (
                        "succeeds when key is present and converts successfully",
                        fun _ -> assertThat (parsePath "/sort?level=low") (isRoute (Sort Low))
                    )

                    test (
                        "fails with MissingQueryParameter when key is absent",
                        fun _ ->
                            assertThat
                                (parsePath "/sort")
                                (isErrorMsg "Required query parameter 'level' was missing.")
                    )

                    test (
                        "fails with BadType when the conversion function returns None",
                        fun _ ->
                            assertThat
                                (parsePath "/sort?level=urgent")
                                (isErrorMsg "Could not convert 'urgent' to Level.")
                    )

                ]
            )

            (*
                map
            *)

            testList (
                "map",
                [

                    test (
                        "transforms the output of a finished parser",
                        fun _ -> assertThat (parsePath "/double/21") (isRoute (Double 42))
                    )

                ]
            )

        ]
    )
