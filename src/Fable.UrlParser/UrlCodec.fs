module Fable.UrlParser.UrlCodec

/// <summary>
/// An unfinished codec.
///
/// <c>'Parsed</c> is what the parse side produces so far; <c>'Env</c> is what
/// the print side reads from. The two dialects use the parameters differently:
/// <c>Tuple</c> keeps them equal (a growing tuple), <c>Values</c> fixes
/// <c>'Env</c> to the values type and burns <c>'Parsed</c> down from a curried
/// constructor.
/// </summary>
type UrlCodec<'Env, 'Parsed> =
    {
        /// <summary>
        /// Parser used when reading an URL
        /// </summary>
        Parser: Base.UrlParser<'Parsed>
        /// <summary>
        /// Getters producing the path segments, in order of appearance
        /// </summary>
        ToSegments: ('Env -> string option) list
        /// <summary>
        /// Getters producing the query parameters (key, values), in order of appearance
        /// </summary>
        ToQueryParameters: ('Env -> (string * string list) option) list
        /// <summary>
        /// Getters producing the query flags, in order of appearance
        /// </summary>
        ToQueryFlags: ('Env -> string list) list
        /// <summary>
        /// Getter producing the fragment
        /// </summary>
        ToFragment: ('Env -> string option) option
    }

/// <summary>
/// An unfinished codec in the structural dialect: the print side reads the
/// same tuple the parse side produces.
/// </summary>
type Tuple<'Fields> = UrlCodec<'Fields, 'Fields>

/// <summary>
/// A finished codec for one route, able to both parse and build URLs.
///
/// Produced by <see cref="M:Fable.UrlParser.UrlCodec.Values.asCase"/> or the
/// <c>Tuple.ofCase*</c> finishers; both dialects converge here, so codecs
/// built with different dialects can live in the same list.
/// </summary>
type RouteCodec<'Route> =
    {
        Parser: Base.UrlParser<'Route>
        /// <summary>
        /// Build the URL for the given value.
        ///
        /// Returns <c>None</c> when the value belongs to another route.
        /// </summary>
        TryBuild: 'Route -> string option
    }

/// <summary>
/// The partial inverse of a union case constructor: builds a route from its
/// values (<c>Embed</c>), and recovers the values back out of a route when it
/// belongs to that case (<c>Project</c>).
/// </summary>
type Prism<'Route, 'Values> =
    {
        /// <summary>
        /// Union case constructor, e.g. <c>Search</c>
        /// </summary>
        Embed: 'Values -> 'Route
        /// <summary>
        /// Extract the values from the route.
        ///
        /// Returns <c>None</c> when the route belongs to another case.
        /// </summary>
        Project: 'Route -> 'Values option
    }

(*
    Shared engine
*)

let private mapParser (f: 'A -> 'B) (parser: Base.UrlParser<'A>) : Base.UrlParser<'B> =
    Base.UrlParser(fun context ->
        Base.apply parser context
        |> Result.map (fun state ->
            let newState: Base.ParserState<'B> =
                {
                    Output = f state.Output
                    Segments = state.Segments
                    Depth = state.Depth
                }

            newState
        )
    )

let private stringFromInt (value: int) = value.ToString()

let private percentEncode (value: string) =
    Base.Helpers.tryEncodeURIComponent value |> Option.defaultValue value

/// Re-target the print side to a new environment by precomposing every getter.
let private contramapPrint
    (f: 'NewEnv -> 'Env)
    (codec: UrlCodec<'Env, 'Parsed>)
    : UrlCodec<'NewEnv, 'Parsed>
    =
    {
        Parser = codec.Parser
        ToSegments = codec.ToSegments |> List.map (fun getter -> f >> getter)
        ToQueryParameters = codec.ToQueryParameters |> List.map (fun getter -> f >> getter)
        ToQueryFlags = codec.ToQueryFlags |> List.map (fun getter -> f >> getter)
        ToFragment = codec.ToFragment |> Option.map (fun getter -> f >> getter)
    }

/// Reshape a finished codec (<c>'Env = 'Parsed</c>) via a total, two-way conversion.
///
/// Unlike a <see cref="T:Fable.UrlParser.UrlCodec.Prism`2"/>, both directions are total —
/// there's no "this value belongs to another case" failure mode, so (unlike `finish`)
/// this doesn't need to produce a `RouteCodec` and can be used mid-pipeline.
let private imap (to': 'A -> 'B) (from': 'B -> 'A) (codec: UrlCodec<'A, 'A>) : UrlCodec<'B, 'B> =
    let retargeted = contramapPrint from' codec

    {
        Parser = codec.Parser |> mapParser to'
        ToSegments = retargeted.ToSegments
        ToQueryParameters = retargeted.ToQueryParameters
        ToQueryFlags = retargeted.ToQueryFlags
        ToFragment = retargeted.ToFragment
    }

/// Build the URL (segments, query, fragment) for a given environment value.
let private tryBuild (codec: UrlCodec<'Env, 'Parsed>) (env: 'Env) : string option =
    let trySegments =
        (Some [], codec.ToSegments)
        ||> List.fold (fun acc getter ->
            match acc, getter env with
            | Some segments, Some segment -> Some(segment :: segments)
            | _ -> None
        )
        |> Option.map List.rev

    trySegments
    |> Option.map (fun segments ->
        let path = segments |> List.map percentEncode |> String.concat "/"

        let parameters =
            codec.ToQueryParameters
            |> List.choose (fun getter -> getter env)
            |> List.collect (fun (key, values) ->
                let encodedKey = percentEncode key

                values |> List.map (fun value -> encodedKey + "=" + percentEncode value)
            )

        let flags =
            codec.ToQueryFlags
            |> List.collect (fun getter -> getter env)
            |> List.map percentEncode

        let query =
            match parameters @ flags with
            | [] -> ""
            | all -> "?" + String.concat "&" all

        let fragmentPart =
            codec.ToFragment
            |> Option.bind (fun getter -> getter env)
            |> Option.map (fun value -> "#" + percentEncode value)
            |> Option.defaultValue ""

        path + query + fragmentPart
    )

let private start (output: 'Parsed) : UrlCodec<'Env, 'Parsed> =
    {
        Parser = Base.succeed output
        ToSegments = []
        ToQueryParameters = []
        ToQueryFlags = []
        ToFragment = None
    }

/// Embed a diagonal codec into the route union.
///
/// `prism.Project` returning `None` means "this value belongs to another
/// route" and makes the build fail over to the next codec.
let private finish
    (prism: Prism<'Route, 'Values>)
    (codec: UrlCodec<'Values, 'Values>)
    : RouteCodec<'Route>
    =
    {
        Parser = mapParser prism.Embed codec.Parser
        TryBuild = fun route -> prism.Project route |> Option.bind (tryBuild codec)
    }

(*
    Drivers
*)

module RouteCodec =

    type ParserError = Base.ParserError

    /// <summary>
    /// Parse the URL path string, trying out multiple codecs if necessary.
    ///
    /// Stops at the first success.
    ///
    /// Prefers to report the error from the codec that had the most success parsing.
    /// </summary>
    /// <param name="codecs">Codecs to try, in order</param>
    /// <param name="path">Path to parse, e.g. <c>"/counter/42?page=1"</c></param>
    let tryParsePath (codecs: RouteCodec<'Route> list) (path: string) : Result<'Route, string> =
        path
        |> Base.pathToUrlContext
        |> Base.parse (codecs |> List.map _.Parser)
        |> Result.mapError ParserError.format

    /// <summary>
    /// Parse an hash based URL, trying out multiple codecs if necessary.
    /// </summary>
    /// <param name="codecs">Codecs to try, in order</param>
    /// <param name="path">Path to parse, e.g. <c>"#/counter/42?page=1"</c></param>
    let tryParseHash (codecs: RouteCodec<'Route> list) (path: string) : Result<'Route, string> =
        path |> String.skipPast '#' |> tryParsePath codecs

    /// <summary>
    /// Convert the given value into an URL path string (without a leading
    /// slash), trying out multiple codecs if necessary.
    /// </summary>
    /// <param name="codecs">Codecs to try, in order</param>
    /// <param name="route">Value to convert</param>
    let rec tryToPath (codecs: RouteCodec<'Route> list) (route: 'Route) : string option =
        match codecs with
        | [] -> None
        | first :: rest ->
            match first.TryBuild route with
            | Some path -> Some path
            | None -> tryToPath rest route

    /// <summary>
    /// Convert the given value into an hash based URL string
    /// (e.g. <c>"#/counter/42?page=1"</c>), trying out multiple codecs if necessary.
    /// </summary>
    /// <param name="codecs">Codecs to try, in order</param>
    /// <param name="route">Value to convert</param>
    let tryToHash (codecs: RouteCodec<'Route> list) (route: 'Route) : string option =
        tryToPath codecs route |> Option.map (fun path -> "#/" + path)

(*
    Applicative dialect: curried constructor + total getters on a values type
*)

module Values =

    /// <summary>
    /// Start a codec definition from a curried constructor of the values type.
    /// </summary>
    /// <remarks>
    /// Getters on <b>nominal</b> record values (<c>_.Field</c>) infer fine
    /// after this starter thanks to record label resolution. For
    /// <b>anonymous</b> record values (whose labels cannot be resolved from
    /// the environment), use <see cref="M:Fable.UrlParser.UrlCodec.Values.createFor"/>
    /// instead.
    /// </remarks>
    /// <param name="constructor">
    /// Curried constructor of the values type, applied progressively while parsing
    /// </param>
    let create (constructor: 'Constructor) : UrlCodec<'Values, 'Constructor> = start constructor

    /// <summary>
    /// Same as <see cref="M:Fable.UrlParser.UrlCodec.Values.create"/>, but pins the
    /// values type from the case's <see cref="T:Fable.UrlParser.UrlCodec.Prism`2"/>.
    /// </summary>
    /// <remarks>
    /// Only <c>prism.Embed</c>'s type is used here, as an inference witness
    /// tying <c>'Values</c> to the case payload: this is what allows
    /// <c>_.Field</c> getters on anonymous record values. Pass the same
    /// <paramref name="prism"/> again to
    /// <see cref="M:Fable.UrlParser.UrlCodec.Values.asCase"/> at the end of the
    /// pipeline — it is the same case, described once.
    /// </remarks>
    /// <param name="prism">Prism for the union case, e.g. <c>Route.Prisms.display</c></param>
    /// <param name="constructor">
    /// Curried constructor of the values type, applied progressively while parsing
    /// </param>
    let createFor
        (_prism: Prism<'Route, 'Values>)
        (constructor: 'Constructor)
        : UrlCodec<'Values, 'Constructor>
        =
        start constructor

    /// Match (when parsing) and emit (when building) a literal path segment.
    let segment
        (expected: string)
        (inner: UrlCodec<'Values, 'Parsed>)
        : UrlCodec<'Values, 'Parsed>
        =
        { inner with
            Parser = Base.segment expected inner.Parser
            ToSegments =
                inner.ToSegments
                @ [
                    fun _ -> Some expected
                ]
        }

    /// Consume (when parsing) and emit (when building) a string path segment.
    ///
    /// An empty string can't round-trip: trailing/leading empty segments are
    /// dropped when a path is parsed, so it comes back as a missing segment.
    let string
        (getter: 'Values -> string)
        (inner: UrlCodec<'Values, string -> 'Parsed>)
        : UrlCodec<'Values, 'Parsed>
        =
        {
            Parser = Base.string inner.Parser
            ToSegments = inner.ToSegments @ [ getter >> Some ]
            ToQueryParameters = inner.ToQueryParameters
            ToQueryFlags = inner.ToQueryFlags
            ToFragment = inner.ToFragment
        }

    /// Consume (when parsing) and emit (when building) an integer path segment.
    let int
        (getter: 'Values -> int)
        (inner: UrlCodec<'Values, int -> 'Parsed>)
        : UrlCodec<'Values, 'Parsed>
        =
        {
            Parser = Base.int inner.Parser
            ToSegments = inner.ToSegments @ [ getter >> stringFromInt >> Some ]
            ToQueryParameters = inner.ToQueryParameters
            ToQueryFlags = inner.ToQueryFlags
            ToFragment = inner.ToFragment
        }

    /// Consume (when parsing) and emit (when building) a path segment using a custom
    /// conversion. <paramref name="typeName"/> is used in the parse error message when
    /// <paramref name="tryParse"/> fails.
    let custom
        (typeName: string)
        (tryParse: string -> 'A option)
        (toString: 'A -> string)
        (getter: 'Values -> 'A)
        (inner: UrlCodec<'Values, 'A -> 'Parsed>)
        : UrlCodec<'Values, 'Parsed>
        =
        {
            Parser = Base.custom typeName tryParse inner.Parser
            ToSegments = inner.ToSegments @ [ getter >> toString >> Some ]
            ToQueryParameters = inner.ToQueryParameters
            ToQueryFlags = inner.ToQueryFlags
            ToFragment = inner.ToFragment
        }

    /// Read (when parsing) and emit (when building) the URL fragment.
    let fragment
        (getter: 'Values -> string option)
        (inner: UrlCodec<'Values, string option -> 'Parsed>)
        : UrlCodec<'Values, 'Parsed>
        =
        {
            Parser = Base.fragment inner.Parser
            ToSegments = inner.ToSegments
            ToQueryParameters = inner.ToQueryParameters
            ToQueryFlags = inner.ToQueryFlags
            ToFragment = Some getter
        }

    module Query =

        /// Read (when parsing) and emit (when building) a query flag.
        /// `false` omits the flag from the built URL.
        let flag
            (name: string)
            (getter: 'Values -> bool)
            (inner: UrlCodec<'Values, bool -> 'Parsed>)
            : UrlCodec<'Values, 'Parsed>
            =
            {
                Parser = Base.Query.flag name inner.Parser
                ToSegments = inner.ToSegments
                ToQueryParameters = inner.ToQueryParameters
                ToQueryFlags =
                    inner.ToQueryFlags
                    @ [
                        fun values ->
                            if getter values then
                                [ name ]
                            else
                                []
                    ]
                ToFragment = inner.ToFragment
            }

        /// Read (when parsing) and emit (when building) all query flags.
        let allFlags
            (getter: 'Values -> string list)
            (inner: UrlCodec<'Values, string list -> 'Parsed>)
            : UrlCodec<'Values, 'Parsed>
            =
            {
                Parser = Base.Query.allFlags inner.Parser
                ToSegments = inner.ToSegments
                ToQueryParameters = inner.ToQueryParameters
                ToQueryFlags = inner.ToQueryFlags @ [ getter ]
                ToFragment = inner.ToFragment
            }

        /// Read (when parsing) and emit (when building) a repeated string query
        /// parameter. `[]` omits the parameter from the built URL.
        let strings
            (key: string)
            (getter: 'Values -> string list)
            (inner: UrlCodec<'Values, string list -> 'Parsed>)
            : UrlCodec<'Values, 'Parsed>
            =
            {
                Parser = Base.Query.strings key inner.Parser
                ToSegments = inner.ToSegments
                ToQueryParameters =
                    inner.ToQueryParameters
                    @ [
                        fun values -> Some(key, getter values)
                    ]
                ToQueryFlags = inner.ToQueryFlags
                ToFragment = inner.ToFragment
            }

        /// Read (when parsing) and emit (when building) a repeated integer query
        /// parameter. `[]` omits the parameter from the built URL.
        let ints
            (key: string)
            (getter: 'Values -> int list)
            (inner: UrlCodec<'Values, int list -> 'Parsed>)
            : UrlCodec<'Values, 'Parsed>
            =
            {
                Parser = Base.Query.ints key inner.Parser
                ToSegments = inner.ToSegments
                ToQueryParameters =
                    inner.ToQueryParameters
                    @ [
                        fun values -> Some(key, getter values |> List.map stringFromInt)
                    ]
                ToQueryFlags = inner.ToQueryFlags
                ToFragment = inner.ToFragment
            }

        module Optional =

            /// Read (when parsing) and emit (when building) a single optional
            /// string query parameter. `None` omits it from the built URL.
            let string
                (key: string)
                (getter: 'Values -> string option)
                (inner: UrlCodec<'Values, string option -> 'Parsed>)
                : UrlCodec<'Values, 'Parsed>
                =
                {
                    Parser = Base.Query.Optional.string key inner.Parser
                    ToSegments = inner.ToSegments
                    ToQueryParameters =
                        inner.ToQueryParameters
                        @ [
                            fun values -> getter values |> Option.map (fun value -> key, [ value ])
                        ]
                    ToQueryFlags = inner.ToQueryFlags
                    ToFragment = inner.ToFragment
                }

            /// Read (when parsing) and emit (when building) a single optional
            /// integer query parameter. `None` omits it from the built URL.
            let int
                (key: string)
                (getter: 'Values -> int option)
                (inner: UrlCodec<'Values, int option -> 'Parsed>)
                : UrlCodec<'Values, 'Parsed>
                =
                {
                    Parser = Base.Query.Optional.int key inner.Parser
                    ToSegments = inner.ToSegments
                    ToQueryParameters =
                        inner.ToQueryParameters
                        @ [
                            fun values ->
                                getter values
                                |> Option.map (fun value -> key, [ stringFromInt value ])
                        ]
                    ToQueryFlags = inner.ToQueryFlags
                    ToFragment = inner.ToFragment
                }

            /// Read (when parsing) and emit (when building) a single optional query
            /// parameter, using a custom conversion. <c>None</c> omits it from the
            /// built URL.
            let custom
                (key: string)
                (typeName: string)
                (tryParse: string -> 'A option)
                (toString: 'A -> string)
                (getter: 'Values -> 'A option)
                (inner: UrlCodec<'Values, 'A option -> 'Parsed>)
                : UrlCodec<'Values, 'Parsed>
                =
                {
                    Parser = Base.Query.Optional.custom key typeName tryParse inner.Parser
                    ToSegments = inner.ToSegments
                    ToQueryParameters =
                        inner.ToQueryParameters
                        @ [
                            fun values ->
                                getter values |> Option.map (fun value -> key, [ toString value ])
                        ]
                    ToQueryFlags = inner.ToQueryFlags
                    ToFragment = inner.ToFragment
                }

        module Required =

            /// Read (when parsing) and emit (when building) a single required
            /// string query parameter.
            let string
                (key: string)
                (getter: 'Values -> string)
                (inner: UrlCodec<'Values, string -> 'Parsed>)
                : UrlCodec<'Values, 'Parsed>
                =
                {
                    Parser = Base.Query.Required.string key inner.Parser
                    ToSegments = inner.ToSegments
                    ToQueryParameters =
                        inner.ToQueryParameters
                        @ [
                            fun values -> Some(key, [ getter values ])
                        ]
                    ToQueryFlags = inner.ToQueryFlags
                    ToFragment = inner.ToFragment
                }

            /// Read (when parsing) and emit (when building) a single required
            /// integer query parameter.
            let int
                (key: string)
                (getter: 'Values -> int)
                (inner: UrlCodec<'Values, int -> 'Parsed>)
                : UrlCodec<'Values, 'Parsed>
                =
                {
                    Parser = Base.Query.Required.int key inner.Parser
                    ToSegments = inner.ToSegments
                    ToQueryParameters =
                        inner.ToQueryParameters
                        @ [
                            fun values -> Some(key, [ stringFromInt (getter values) ])
                        ]
                    ToQueryFlags = inner.ToQueryFlags
                    ToFragment = inner.ToFragment
                }

            /// Read (when parsing) and emit (when building) a single required query
            /// parameter, using a custom conversion.
            let custom
                (key: string)
                (typeName: string)
                (tryParse: string -> 'A option)
                (toString: 'A -> string)
                (getter: 'Values -> 'A)
                (inner: UrlCodec<'Values, 'A -> 'Parsed>)
                : UrlCodec<'Values, 'Parsed>
                =
                {
                    Parser = Base.Query.Required.custom key typeName tryParse inner.Parser
                    ToSegments = inner.ToSegments
                    ToQueryParameters =
                        inner.ToQueryParameters
                        @ [
                            fun values -> Some(key, [ toString (getter values) ])
                        ]
                    ToQueryFlags = inner.ToQueryFlags
                    ToFragment = inner.ToFragment
                }

    /// <summary>
    /// Reshape a finished values codec via a total, two-way conversion.
    /// </summary>
    /// <remarks>
    /// Unlike a <see cref="T:Fable.UrlParser.UrlCodec.Prism`2"/>, both directions are
    /// total: there is no "this value belongs to another case" failure mode. Use this
    /// to lift an already fully-applied values codec into a wrapper type, e.g. right
    /// before <see cref="M:Fable.UrlParser.UrlCodec.Values.asCase"/>.
    /// </remarks>
    /// <param name="to_">Converts the current values type into the new one</param>
    /// <param name="from_">Converts the new values type back into the current one</param>
    /// <param name="codec">Codec to reshape</param>
    let map
        (to_: 'Values -> 'NewValues)
        (from_: 'NewValues -> 'Values)
        (codec: UrlCodec<'Values, 'Values>)
        : UrlCodec<'NewValues, 'NewValues>
        =
        imap to_ from_ codec

    /// <summary>
    /// Finish the codec by embedding the values type into the route union.
    /// </summary>
    /// <param name="prism">Prism for the union case, e.g. <c>Route.Prisms.search</c></param>
    /// <param name="codec">Codec to finish</param>
    let asCase
        (prism: Prism<'Route, 'Values>)
        (codec: UrlCodec<'Values, 'Values>)
        : RouteCodec<'Route>
        =
        finish prism codec

(*
    Structural dialect: values accumulate as a growing tuple
*)

module Tuple =

    /// The empty tuple: matches the root path and carries no value.
    let root: Tuple<unit> = start ()

    /// Match (when parsing) and emit (when building) a literal path segment.
    let segment (expected: string) (inner: Tuple<'Fields>) : Tuple<'Fields> =
        Values.segment expected inner

    /// Start a tuple with a literal path segment. Shorthand for
    /// <c>Tuple.root |> Tuple.segment expected</c>.
    let path (expected: string) : Tuple<unit> = segment expected root

    /// Widen a tuple codec so its parser awaits one more value (curried) and
    /// its print side reads from the resulting `'Fields * 'New` pair — the
    /// exact shape every `Values.*` combinator expects. Every combinator
    /// below delegates into its `Values.*` counterpart through this bridge,
    /// passing `snd` as the getter, so there is only one implementation of
    /// each primitive.
    let private stageNext
        (inner: Tuple<'Fields>)
        : UrlCodec<'Fields * 'New, 'New -> 'Fields * 'New>
        =
        let widened = contramapPrint fst inner

        {
            Parser = inner.Parser |> mapParser (fun fields value -> (fields, value))
            ToSegments = widened.ToSegments
            ToQueryParameters = widened.ToQueryParameters
            ToQueryFlags = widened.ToQueryFlags
            ToFragment = widened.ToFragment
        }

    /// Consume (when parsing) and emit (when building) a string path segment.
    ///
    /// An empty string can't round-trip: trailing/leading empty segments are
    /// dropped when a path is parsed, so it comes back as a missing segment.
    let string (inner: Tuple<'Fields>) : Tuple<'Fields * string> =
        Values.string snd (stageNext inner)

    /// Consume (when parsing) and emit (when building) an integer path segment.
    let int (inner: Tuple<'Fields>) : Tuple<'Fields * int> = Values.int snd (stageNext inner)

    /// Consume (when parsing) and emit (when building) a path segment using a custom
    /// conversion. <paramref name="typeName"/> is used in the parse error message when
    /// <paramref name="tryParse"/> fails.
    let custom
        (typeName: string)
        (tryParse: string -> 'A option)
        (toString: 'A -> string)
        (inner: Tuple<'Fields>)
        : Tuple<'Fields * 'A>
        =
        Values.custom typeName tryParse toString snd (stageNext inner)

    /// Read (when parsing) and emit (when building) the URL fragment.
    let fragment (inner: Tuple<'Fields>) : Tuple<'Fields * string option> =
        Values.fragment snd (stageNext inner)

    module Query =

        /// Read (when parsing) and emit (when building) a query flag.
        /// `false` omits the flag from the built URL.
        let flag (name: string) (inner: Tuple<'Fields>) : Tuple<'Fields * bool> =
            Values.Query.flag name snd (stageNext inner)

        /// Read (when parsing) and emit (when building) all query flags.
        let allFlags (inner: Tuple<'Fields>) : Tuple<'Fields * string list> =
            Values.Query.allFlags snd (stageNext inner)

        /// Read (when parsing) and emit (when building) a repeated string query
        /// parameter. `[]` omits the parameter from the built URL.
        let strings (key: string) (inner: Tuple<'Fields>) : Tuple<'Fields * string list> =
            Values.Query.strings key snd (stageNext inner)

        /// Read (when parsing) and emit (when building) a repeated integer query
        /// parameter. `[]` omits the parameter from the built URL.
        let ints (key: string) (inner: Tuple<'Fields>) : Tuple<'Fields * int list> =
            Values.Query.ints key snd (stageNext inner)

        module Optional =

            /// Read (when parsing) and emit (when building) a single optional
            /// string query parameter. `None` omits it from the built URL.
            let string (key: string) (inner: Tuple<'Fields>) : Tuple<'Fields * string option> =
                Values.Query.Optional.string key snd (stageNext inner)

            /// Read (when parsing) and emit (when building) a single optional
            /// integer query parameter. `None` omits it from the built URL.
            let int (key: string) (inner: Tuple<'Fields>) : Tuple<'Fields * int option> =
                Values.Query.Optional.int key snd (stageNext inner)

            /// Read (when parsing) and emit (when building) a single optional query
            /// parameter, using a custom conversion. `None` omits it from the built URL.
            let custom
                (key: string)
                (typeName: string)
                (tryParse: string -> 'A option)
                (toString: 'A -> string)
                (inner: Tuple<'Fields>)
                : Tuple<'Fields * 'A option>
                =
                Values.Query.Optional.custom key typeName tryParse toString snd (stageNext inner)

        module Required =

            /// Read (when parsing) and emit (when building) a single required
            /// string query parameter.
            let string (key: string) (inner: Tuple<'Fields>) : Tuple<'Fields * string> =
                Values.Query.Required.string key snd (stageNext inner)

            /// Read (when parsing) and emit (when building) a single required
            /// integer query parameter.
            let int (key: string) (inner: Tuple<'Fields>) : Tuple<'Fields * int> =
                Values.Query.Required.int key snd (stageNext inner)

            /// Read (when parsing) and emit (when building) a single required query
            /// parameter, using a custom conversion.
            let custom
                (key: string)
                (typeName: string)
                (tryParse: string -> 'A option)
                (toString: 'A -> string)
                (inner: Tuple<'Fields>)
                : Tuple<'Fields * 'A>
                =
                Values.Query.Required.custom key typeName tryParse toString snd (stageNext inner)

    /// <summary>
    /// Reshape a tuple codec via a total, two-way conversion.
    /// </summary>
    /// <remarks>
    /// Unlike the <c>ofCase*</c> finishers, both directions are total: there is no
    /// "this value belongs to another case" failure mode, so (unlike them) this
    /// composes mid-pipeline, not just at the end, to wrap an accumulated tuple
    /// into a newtype.
    /// </remarks>
    /// <param name="to_">Converts the current tuple into the new one</param>
    /// <param name="from_">Converts the new tuple back into the current one</param>
    /// <param name="tuple">Tuple to reshape</param>
    let map
        (to_: 'Fields -> 'NewFields)
        (from_: 'NewFields -> 'Fields)
        (tuple: Tuple<'Fields>)
        : Tuple<'NewFields>
        =
        imap to_ from_ tuple

    // The numbered finishers are arity-indexed because printing needs a concrete
    // tuple to deconstruct: they flatten the nesting (and swallow the leading unit)
    // so users never see it. The unnumbered `ofCase` is the odd one out on purpose:
    // a case with no fields has no payload to embed, so it takes the route value
    // itself rather than a prism.

    /// <summary>
    /// Finish a tuple carrying no value, for a case with no fields.
    /// </summary>
    /// <remarks>
    /// Takes the route value itself rather than a
    /// <see cref="T:Fable.UrlParser.UrlCodec.Prism`2"/>: with no payload to embed
    /// there is nothing for a prism to describe, and recognising the case on the
    /// build side is just equality.
    /// </remarks>
    /// <param name="route">The route value, e.g. <c>Home</c></param>
    /// <param name="tuple">Tuple to finish</param>
    let ofCase (route: 'Route) (tuple: Tuple<unit>) : RouteCodec<'Route> =
        finish
            {
                Embed = fun () -> route
                Project =
                    fun value ->
                        if value = route then
                            Some()
                        else
                            None
            }
            tuple

    /// <summary>
    /// Finish a tuple carrying one value.
    /// </summary>
    /// <param name="prism">Prism for the union case, e.g. <c>Route.Prisms.counter</c></param>
    /// <param name="tuple">Tuple to finish</param>
    let ofCase1 (prism: Prism<'Route, 'A>) (tuple: Tuple<unit * 'A>) : RouteCodec<'Route> =
        finish
            {
                Embed = fun (_, a) -> prism.Embed a
                Project = fun route -> prism.Project route |> Option.map (fun a -> ((), a))
            }
            tuple

    /// <summary>
    /// Finish a tuple carrying two values.
    /// </summary>
    /// <param name="prism">Prism for the union case, e.g. <c>Route.Prisms.userPost</c></param>
    /// <param name="tuple">Tuple to finish</param>
    let ofCase2
        (prism: Prism<'Route, 'A * 'B>)
        (tuple: Tuple<(unit * 'A) * 'B>)
        : RouteCodec<'Route>
        =
        finish
            {
                Embed = fun ((_, a), b) -> prism.Embed(a, b)
                Project =
                    fun route -> prism.Project route |> Option.map (fun (a, b) -> (((), a), b))
            }
            tuple

    /// <summary>
    /// Finish a tuple carrying three values.
    /// </summary>
    /// <param name="prism">Prism for the union case</param>
    /// <param name="tuple">Tuple to finish</param>
    let ofCase3
        (prism: Prism<'Route, 'A * 'B * 'C>)
        (tuple: Tuple<((unit * 'A) * 'B) * 'C>)
        : RouteCodec<'Route>
        =
        finish
            {
                Embed = fun (((_, a), b), c) -> prism.Embed(a, b, c)
                Project =
                    fun route ->
                        prism.Project route |> Option.map (fun (a, b, c) -> ((((), a), b), c))
            }
            tuple
