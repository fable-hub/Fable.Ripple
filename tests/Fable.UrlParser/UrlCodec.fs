module Fable.UrlParser.Tests.UrlCodec

open Scriptorium.Nib.Assertion
open Hedgehog.FSharp
open Fable.UrlParser.UrlCodec

open type Scriptorium.Quill.Test
open type Scriptorium.Hedgehog.Test

(*
    Route types
*)

type SearchParams =
    {
        Query: string
        Page: int option
    }

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

    let toString =
        function
        | Low -> "low"
        | Medium -> "medium"
        | High -> "high"

type UserId = UserId of int

type Route =
    | Home
    | Counter of int
    | UserPost of userId: int * postId: int
    | Display of
        {|
            ShowOption: bool
        |}
    | Search of SearchParams
    | TagList of string list
    | Post of slug: string
    | Ids of int list
    | Settings of flags: string list
    | File of name: string * section: string option
    | Book of
        authorId: int option *
        {|
            title: string
        |}
    | Priority of Level
    | Filter of level: Level option
    | Sort of Level
    | Profile of UserId

module Route =

    /// Hand-written prisms for the `Route` cases, each one reused as-is by
    /// both dialects' finishers (`Tuple.ofCase*`, `Values.asCase`), and by
    /// `Values.createFor` as its inference witness.
    module Prisms =

        let counter: Prism<Route, int> =
            {
                Embed = Counter
                Project =
                    function
                    | Counter value -> Some value
                    | _ -> None
            }

        let userPost: Prism<Route, int * int> =
            {
                Embed = fun (userId, postId) -> UserPost(userId, postId)
                Project =
                    function
                    | UserPost(userId, postId) -> Some(userId, postId)
                    | _ -> None
            }

        let post: Prism<Route, string> =
            {
                Embed = Post
                Project =
                    function
                    | Post slug -> Some slug
                    | _ -> None
            }

        let tagList: Prism<Route, string list> =
            {
                Embed = TagList
                Project =
                    function
                    | TagList tags -> Some tags
                    | _ -> None
            }

        let ids: Prism<Route, int list> =
            {
                Embed = Ids
                Project =
                    function
                    | Ids values -> Some values
                    | _ -> None
            }

        let settings: Prism<Route, string list> =
            {
                Embed = Settings
                Project =
                    function
                    | Settings flags -> Some flags
                    | _ -> None
            }

        let search: Prism<Route, SearchParams> =
            {
                Embed = Search
                Project =
                    function
                    | Search parameters -> Some parameters
                    | _ -> None
            }

        let display
            : Prism<
                  Route,
                  {|
                      ShowOption: bool
                  |}
               > =
            {
                Embed = Display
                Project =
                    function
                    | Display options -> Some options
                    | _ -> None
            }

        /// `Book`'s payload is a tuple whose second element is itself an
        /// anonymous record, so `Embed` flattens both into one values record
        /// for the `Values` dialect to read `_.Field` getters from, and
        /// reshapes it back on the way in.
        let book
            : Prism<
                  Route,
                  {|
                      AuthorId: int option
                      Title: string
                  |}
               > =
            {
                Embed =
                    fun values ->
                        Book(
                            values.AuthorId,
                            {|
                                title = values.Title
                            |}
                        )
                Project =
                    function
                    | Book(authorId, payload) ->
                        Some
                            {|
                                AuthorId = authorId
                                Title = payload.title
                            |}
                    | _ -> None
            }

        let priority: Prism<Route, Level> =
            {
                Embed = Priority
                Project =
                    function
                    | Priority value -> Some value
                    | _ -> None
            }

        let filter: Prism<Route, Level option> =
            {
                Embed = Filter
                Project =
                    function
                    | Filter value -> Some value
                    | _ -> None
            }

        let sort: Prism<Route, Level> =
            {
                Embed = Sort
                Project =
                    function
                    | Sort value -> Some value
                    | _ -> None
            }

        let profile: Prism<Route, UserId> =
            {
                Embed = Profile
                Project =
                    function
                    | Profile value -> Some value
                    | _ -> None
            }

(*
    Codecs — mixing both dialects in the same list on purpose
*)

// Structural dialect: no getters, arity-indexed finishers

let homeCodec = Tuple.path "home" |> Tuple.ofCase Home

let counterCodec =
    Tuple.path "counter" |> Tuple.int |> Tuple.ofCase1 Route.Prisms.counter

let userPostCodec =
    Tuple.path "user"
    |> Tuple.int
    |> Tuple.segment "post"
    |> Tuple.int
    |> Tuple.ofCase2 Route.Prisms.userPost

let postCodec = Tuple.path "post" |> Tuple.string |> Tuple.ofCase1 Route.Prisms.post

let tagsCodec =
    Tuple.path "tags"
    |> Tuple.Query.strings "tag"
    |> Tuple.ofCase1 Route.Prisms.tagList

let idsCodec =
    Tuple.path "ids" |> Tuple.Query.ints "id" |> Tuple.ofCase1 Route.Prisms.ids

let settingsCodec =
    Tuple.path "settings"
    |> Tuple.Query.allFlags
    |> Tuple.ofCase1 Route.Prisms.settings

// This prism is deliberately NOT in `Route.Prisms`: it is not lawful for the
// `File` case (`Embed` pins `name = "readme"` and `Project` drops `name`, so
// projecting `File("other", ...)` would not round-trip)
let fileCodec =
    Tuple.path "file"
    |> Tuple.segment "readme"
    |> Tuple.fragment
    |> Tuple.ofCase1
        {
            Embed = fun section -> File("readme", section)
            Project =
                function
                | File(_, section) -> Some section
                | _ -> None
        }

let priorityCodec =
    Tuple.path "priority"
    |> Tuple.custom "Level" Level.tryParse Level.toString
    |> Tuple.ofCase1 Route.Prisms.priority

let filterCodec =
    Tuple.path "filter"
    |> Tuple.Query.Optional.custom "level" "Level" Level.tryParse Level.toString
    |> Tuple.ofCase1 Route.Prisms.filter

let sortCodec =
    Tuple.path "sort"
    |> Tuple.Query.Required.custom "level" "Level" Level.tryParse Level.toString
    |> Tuple.ofCase1 Route.Prisms.sort

// `map` reshapes the accumulated tuple: here, wrapping the plain int into the
// `UserId` newtype before finishing, without needing a dedicated `Values` codec.
let profileCodec =
    Tuple.path "profile"
    |> Tuple.int
    |> Tuple.map (fun (u, i) -> (u, UserId i)) (fun (u, UserId i) -> (u, i))
    |> Tuple.ofCase1 Route.Prisms.profile

// Applicative dialect: total `_.Field` getters on a values type

let searchCodec =
    Values.create (fun query page ->
        {
            Query = query
            Page = page
        }
    )
    |> Values.segment "search"
    |> Values.Query.Required.string "q" _.Query
    |> Values.Query.Optional.int "page" _.Page
    |> Values.asCase Route.Prisms.search

// Anonymous record labels cannot be resolved from the environment, so the
// values type must be pinned from the case's prism with `createFor` — the
// same prism is reused below, in `asCase`
let displayCodec =
    Values.createFor
        Route.Prisms.display
        (fun showOption ->
            {|
                ShowOption = showOption
            |}
        )
    |> Values.segment "display"
    |> Values.Query.flag "showOption" _.ShowOption
    |> Values.asCase Route.Prisms.display

// `Book`'s payload doesn't fit a single record (it's a tuple, and one of its
// elements is itself an anonymous record), so `Route.Prisms.book.Embed`
// reshapes a flattened values record into the case's actual curried
// constructor; it doubles as the `createFor` witness here.
let bookCodec =
    Values.createFor
        Route.Prisms.book
        (fun authorId title ->
            {|
                AuthorId = authorId
                Title = title
            |}
        )
    |> Values.segment "book"
    |> Values.Query.Optional.int "authorId" _.AuthorId
    |> Values.Query.Required.string "title" _.Title
    |> Values.asCase Route.Prisms.book

let allCodecs =
    [
        homeCodec
        counterCodec
        userPostCodec
        postCodec
        tagsCodec
        idsCodec
        settingsCodec
        fileCodec
        priorityCodec
        filterCodec
        sortCodec
        profileCodec
        searchCodec
        displayCodec
        bookCodec
    ]

let parsePath = RouteCodec.tryParsePath allCodecs
let parseHash = RouteCodec.tryParseHash allCodecs
let toPath = RouteCodec.tryToPath allCodecs
let toHash = RouteCodec.tryToHash allCodecs

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
        "UrlCodec",
        [

            (*
                parsing
            *)

            testList (
                "parsing",
                [

                    test (
                        "matches a literal segment (Tuple)",
                        fun _ -> assertThat (parsePath "/home") (isRoute Home)
                    )

                    test (
                        "parses an int segment (Tuple)",
                        fun _ -> assertThat (parsePath "/counter/42") (isRoute (Counter 42))
                    )

                    test (
                        "parses a string segment (Tuple)",
                        fun _ ->
                            assertThat
                                (parsePath "/post/hello-world")
                                (isRoute (Post "hello-world"))
                    )

                    test (
                        "parses multiple segments (Tuple)",
                        fun _ ->
                            assertThat (parsePath "/user/42/post/7") (isRoute (UserPost(42, 7)))
                    )

                    test (
                        "parses query parameters (Values)",
                        fun _ ->
                            assertThat
                                (parsePath "/search?q=hello&page=2")
                                (isRoute (
                                    Search
                                        {
                                            Query = "hello"
                                            Page = Some 2
                                        }
                                ))
                    )

                    test (
                        "parses a query flag into an anonymous record (Values)",
                        fun _ ->
                            assertThat
                                (parsePath "/display?showOption")
                                (isRoute (
                                    Display
                                        {|
                                            ShowOption = true
                                        |}
                                ))
                    )

                    test (
                        "parses a query parameter into a flattened tuple + anonymous record (Values)",
                        fun _ ->
                            assertThat
                                (parsePath "/book?authorId=7&title=Founders%20of%20Arclight")
                                (isRoute (
                                    Book(
                                        Some 7,
                                        {|
                                            title = "Founders of Arclight"
                                        |}
                                    )
                                ))
                    )

                    test (
                        "omits the optional element of the tuple when absent (Values)",
                        fun _ ->
                            assertThat
                                (parsePath "/book?title=Ghostwritten")
                                (isRoute (
                                    Book(
                                        None,
                                        {|
                                            title = "Ghostwritten"
                                        |}
                                    )
                                ))
                    )

                    test (
                        "parses a fragment (Tuple)",
                        fun _ ->
                            assertThat
                                (parsePath "/file/readme#introduction")
                                (isRoute (File("readme", Some "introduction")))
                    )

                    test (
                        "parses a segment using a custom conversion (Tuple)",
                        fun _ -> assertThat (parsePath "/priority/high") (isRoute (Priority High))
                    )

                    test (
                        "fails with BadType when a custom conversion returns None (Tuple)",
                        fun _ ->
                            assertThat
                                (parsePath "/priority/urgent")
                                (isErrorMsg "Could not convert 'urgent' to Level.")
                    )

                    test (
                        "parses an optional query parameter using a custom conversion (Tuple)",
                        fun _ ->
                            assertThat
                                (parsePath "/filter?level=medium")
                                (isRoute (Filter(Some Medium)))
                    )

                    test (
                        "parses a required query parameter using a custom conversion (Tuple)",
                        fun _ -> assertThat (parsePath "/sort?level=low") (isRoute (Sort Low))
                    )

                    test (
                        "reshapes an accumulated tuple via map (Tuple)",
                        fun _ -> assertThat (parsePath "/profile/42") (isRoute (Profile(UserId 42)))
                    )

                    test (
                        "fails with NoParsers when codec list is empty",
                        fun _ ->
                            assertThat
                                (RouteCodec.tryParsePath [] "/home")
                                (isErrorMsg "No parsers were provided.")
                    )

                    test (
                        "reports the error from the deepest-matching codec",
                        fun _ ->
                            assertThat
                                (parsePath "/user/42/comment/7")
                                (isErrorMsg "Expected segment 'post' but got 'comment'.")
                    )

                    test (
                        "parses a hash URL",
                        fun _ -> assertThat (parseHash "#/counter/99") (isRoute (Counter 99))
                    )

                ]
            )

            (*
                building
            *)

            testList (
                "building",
                [

                    test (
                        "builds a literal segment (Tuple)",
                        fun _ -> assertThat (toPath Home) (Option.value >> isEqualTo "home")
                    )

                    test (
                        "builds an int segment (Tuple)",
                        fun _ ->
                            assertThat
                                (toPath (Counter 42))
                                (Option.value >> isEqualTo "counter/42")
                    )

                    test (
                        "builds multiple segments (Tuple)",
                        fun _ ->
                            assertThat
                                (toPath (UserPost(42, 7)))
                                (Option.value >> isEqualTo "user/42/post/7")
                    )

                    test (
                        "builds query parameters in declaration order (Values)",
                        fun _ ->
                            assertThat
                                (toPath (
                                    Search
                                        {
                                            Query = "hello"
                                            Page = Some 2
                                        }
                                ))
                                (Option.value >> isEqualTo "search?q=hello&page=2")
                    )

                    test (
                        "omits optional query parameter when None (Values)",
                        fun _ ->
                            assertThat
                                (toPath (
                                    Search
                                        {
                                            Query = "hello"
                                            Page = None
                                        }
                                ))
                                (Option.value >> isEqualTo "search?q=hello")
                    )

                    test (
                        "builds repeated query parameters (Tuple)",
                        fun _ ->
                            assertThat
                                (toPath (
                                    TagList
                                        [
                                            "fsharp"
                                            "fable"
                                        ]
                                ))
                                (Option.value >> isEqualTo "tags?tag=fsharp&tag=fable")
                    )

                    test (
                        "builds repeated int query parameters (Tuple)",
                        fun _ ->
                            assertThat
                                (toPath (
                                    Ids
                                        [
                                            1
                                            2
                                        ]
                                ))
                                (Option.value >> isEqualTo "ids?id=1&id=2")
                    )

                    test (
                        "omits repeated query parameter when list is empty (Tuple)",
                        fun _ -> assertThat (toPath (TagList [])) (Option.value >> isEqualTo "tags")
                    )

                    test (
                        "builds a query flag when true (Values)",
                        fun _ ->
                            assertThat
                                (toPath (
                                    Display
                                        {|
                                            ShowOption = true
                                        |}
                                ))
                                (Option.value >> isEqualTo "display?showOption")
                    )

                    test (
                        "omits a query flag when false (Values)",
                        fun _ ->
                            assertThat
                                (toPath (
                                    Display
                                        {|
                                            ShowOption = false
                                        |}
                                ))
                                (Option.value >> isEqualTo "display")
                    )

                    test (
                        "builds all query flags (Tuple)",
                        fun _ ->
                            assertThat
                                (toPath (
                                    Settings
                                        [
                                            "admin"
                                            "beta"
                                        ]
                                ))
                                (Option.value >> isEqualTo "settings?admin&beta")
                    )

                    test (
                        "builds a query parameter from a flattened tuple + anonymous record (Values)",
                        fun _ ->
                            assertThat
                                (toPath (
                                    Book(
                                        Some 7,
                                        {|
                                            title = "Founders of Arclight"
                                        |}
                                    )
                                ))
                                (Option.value
                                 >> isEqualTo "book?authorId=7&title=Founders%20of%20Arclight")
                    )

                    test (
                        "omits the optional element of the tuple when None (Values)",
                        fun _ ->
                            assertThat
                                (toPath (
                                    Book(
                                        None,
                                        {|
                                            title = "Ghostwritten"
                                        |}
                                    )
                                ))
                                (Option.value >> isEqualTo "book?title=Ghostwritten")
                    )

                    test (
                        "builds a fragment (Tuple)",
                        fun _ ->
                            assertThat
                                (toPath (File("readme", Some "introduction")))
                                (Option.value >> isEqualTo "file/readme#introduction")
                    )

                    test (
                        "omits the fragment when None (Tuple)",
                        fun _ ->
                            assertThat
                                (toPath (File("readme", None)))
                                (Option.value >> isEqualTo "file/readme")
                    )

                    test (
                        "percent-encodes segments",
                        fun _ ->
                            assertThat
                                (toPath (Post "hello world"))
                                (Option.value >> isEqualTo "post/hello%20world")
                    )

                    test (
                        "percent-encodes query values",
                        fun _ ->
                            assertThat
                                (toPath (
                                    Search
                                        {
                                            Query = "a&b"
                                            Page = None
                                        }
                                ))
                                (Option.value >> isEqualTo "search?q=a%26b")
                    )

                    test (
                        "builds a segment using a custom conversion (Tuple)",
                        fun _ ->
                            assertThat
                                (toPath (Priority High))
                                (Option.value >> isEqualTo "priority/high")
                    )

                    test (
                        "builds an optional query parameter using a custom conversion (Tuple)",
                        fun _ ->
                            assertThat
                                (toPath (Filter(Some Medium)))
                                (Option.value >> isEqualTo "filter?level=medium")
                    )

                    test (
                        "omits an optional query parameter using a custom conversion when None (Tuple)",
                        fun _ ->
                            assertThat (toPath (Filter None)) (Option.value >> isEqualTo "filter")
                    )

                    test (
                        "builds a required query parameter using a custom conversion (Tuple)",
                        fun _ ->
                            assertThat
                                (toPath (Sort Low))
                                (Option.value >> isEqualTo "sort?level=low")
                    )

                    test (
                        "builds a reshaped tuple via map (Tuple)",
                        fun _ ->
                            assertThat
                                (toPath (Profile(UserId 42)))
                                (Option.value >> isEqualTo "profile/42")
                    )

                    test (
                        "returns None when no codec matches the value",
                        fun _ ->
                            assertThat (RouteCodec.tryToPath [ counterCodec ] Home) Option.isNone
                    )

                    test (
                        "returns None when codec list is empty",
                        fun _ -> assertThat (RouteCodec.tryToPath [] Home) Option.isNone
                    )

                    test (
                        "builds a hash URL",
                        fun _ ->
                            assertThat
                                (toHash (Counter 42))
                                (Option.value >> isEqualTo "#/counter/42")
                    )

                ]
            )

            (*
                round-trip
            *)

            testList (
                "round-trip",
                [

                    test (
                        "Values codec: build then parse returns the original route",
                        fun _ ->
                            let route =
                                Search
                                    {
                                        Query = "hello world"
                                        Page = Some 3
                                    }

                            assertThat (toPath route |> Option.get |> parsePath) (isRoute route)
                    )

                    test (
                        "Tuple codec: build then parse returns the original route",
                        fun _ ->
                            let route = UserPost(1, 2)

                            assertThat (toHash route |> Option.get |> parseHash) (isRoute route)
                    )

                    test (
                        "percent-encoded segment survives the round-trip",
                        fun _ ->
                            let route = Post "hello world"

                            assertThat (toPath route |> Option.get |> parsePath) (isRoute route)
                    )

                    test (
                        "Values codec with a flattened tuple + anonymous record: build then parse returns the original route",
                        fun _ ->
                            let route =
                                Book(
                                    Some 42,
                                    {|
                                        title = "The Long Way"
                                    |}
                                )

                            assertThat (toPath route |> Option.get |> parsePath) (isRoute route)
                    )

                    test (
                        "custom conversion codec: build then parse returns the original route",
                        fun _ ->
                            let route = Sort Medium

                            assertThat (toPath route |> Option.get |> parsePath) (isRoute route)
                    )

                    test (
                        "map-reshaped codec: build then parse returns the original route",
                        fun _ ->
                            let route = Profile(UserId 7)

                            assertThat (toPath route |> Option.get |> parsePath) (isRoute route)
                    )

                ]
            )

            (*
                round-trip (property-based)
            *)

            testList (
                "round-trip properties",
                [

                    testProperty (
                        "Counter round-trips for any int",
                        Gen.int32 (Range.linear -1_000_000 1_000_000),
                        fun value ->
                            let route = Route.Prisms.counter.Embed value
                            assertThat (toPath route |> Option.get |> parsePath) (isRoute route)
                    )

                    testProperty (
                        "UserPost round-trips for any pair of ints",
                        Gen.zip
                            (Gen.int32 (Range.linear 0 1_000_000))
                            (Gen.int32 (Range.linear 0 1_000_000)),
                        fun (userId, postId) ->
                            let route = Route.Prisms.userPost.Embed(userId, postId)
                            assertThat (toPath route |> Option.get |> parsePath) (isRoute route)
                    )

                    testProperty (
                        "Post round-trips for any non-empty slug",
                        Gen.string (Range.linear 1 20) Gen.alphaNum,
                        fun slug ->
                            let route = Route.Prisms.post.Embed slug
                            assertThat (toPath route |> Option.get |> parsePath) (isRoute route)
                    )

                    testProperty (
                        "TagList round-trips for any list of tags",
                        Gen.list (Range.linear 0 5) (Gen.string (Range.linear 1 10) Gen.alphaNum),
                        fun tags ->
                            let route = Route.Prisms.tagList.Embed tags
                            assertThat (toPath route |> Option.get |> parsePath) (isRoute route)
                    )

                    testProperty (
                        "Ids round-trips for any list of ints",
                        Gen.list (Range.linear 0 5) (Gen.int32 (Range.linear 0 1_000_000)),
                        fun ids ->
                            let route = Route.Prisms.ids.Embed ids
                            assertThat (toPath route |> Option.get |> parsePath) (isRoute route)
                    )

                    testProperty (
                        "Settings round-trips for any sorted, distinct list of flags",
                        Gen.list (Range.linear 0 5) (Gen.string (Range.linear 1 10) Gen.alphaNum)
                        |> Gen.map (List.distinct >> List.sort),
                        fun flags ->
                            let route = Route.Prisms.settings.Embed flags
                            assertThat (toPath route |> Option.get |> parsePath) (isRoute route)
                    )

                    testProperty (
                        "Search round-trips for any query and optional page",
                        Gen.zip
                            (Gen.string (Range.linear 1 15) Gen.alphaNum)
                            (Gen.option (Gen.int32 (Range.linear 0 1_000))),
                        fun (query, page) ->
                            let route =
                                Route.Prisms.search.Embed
                                    {
                                        Query = query
                                        Page = page
                                    }

                            assertThat (toPath route |> Option.get |> parsePath) (isRoute route)
                    )

                    testProperty (
                        "Display round-trips for any bool",
                        Gen.bool,
                        fun showOption ->
                            let route =
                                Route.Prisms.display.Embed
                                    {|
                                        ShowOption = showOption
                                    |}

                            assertThat (toPath route |> Option.get |> parsePath) (isRoute route)
                    )

                    testProperty (
                        "Book round-trips for any optional author id and title",
                        Gen.zip
                            (Gen.option (Gen.int32 (Range.linear 0 1_000_000)))
                            (Gen.string (Range.linear 1 15) Gen.alphaNum),
                        fun (authorId, title) ->
                            let route =
                                Route.Prisms.book.Embed
                                    {|
                                        AuthorId = authorId
                                        Title = title
                                    |}

                            assertThat (toPath route |> Option.get |> parsePath) (isRoute route)
                    )

                    testProperty (
                        "Sort round-trips for any level",
                        Gen.item
                            [
                                Low
                                Medium
                                High
                            ],
                        fun level ->
                            let route = Route.Prisms.sort.Embed level
                            assertThat (toPath route |> Option.get |> parsePath) (isRoute route)
                    )

                    testProperty (
                        "Profile round-trips for any int",
                        Gen.int32 (Range.linear -1_000_000 1_000_000),
                        fun value ->
                            let route = Route.Prisms.profile.Embed(UserId value)
                            assertThat (toPath route |> Option.get |> parsePath) (isRoute route)
                    )

                ]
            )

        ]
    )
