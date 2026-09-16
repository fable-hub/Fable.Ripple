module Fable.UrlParser.Base

open Fable.Core
open System

// https://example.com:8042/over/there?name=ferret#nose
// \___/   \______________/\_________/ \_________/ \__/
// |            |            |            |        |
// scheme     authority       path        query   fragment

// The UrlParser project about parsing the path, query and fragment

module Helpers =

    // Bind the JavaScript globals directly (instead of going through `window`)
    // so they also work outside of a browser environment (e.g. Node.js)
    [<Global("encodeURIComponent")>]
    let private jsEncodeURIComponent (_uriComponent: string) : string = jsNative

    [<Global("decodeURIComponent")>]
    let private jsDecodeURIComponent (_uriComponent: string) : string = jsNative

    let tryEncodeURIComponent (uriComponent: string) =
        try
            jsEncodeURIComponent uriComponent |> Some
        with
        // encodeURIComponent calls can fails if there is a lone surrogate encodeURIComponent("\x")
        | _ ->
            None

    /// <summary>
    /// Decode the URI component
    ///
    /// In case of error, it default to the original uriComponent string
    /// </summary>
    /// <param name="uriComponent">Value to decode</param>
    /// <returns></returns>
    let decodeURIComponent (uriComponent: string) =
        try
            jsDecodeURIComponent uriComponent
        with _ ->
            uriComponent

    let rec removeLeadingEmpty (segments: string list) =
        match segments with
        | "" :: rest -> removeLeadingEmpty rest
        | list -> list

    let rec removeTrailingEmpty (segments: string list) =
        match segments with
        | [] -> []
        | [ "" ] -> []
        | "" :: rest when List.forall ((=) "") rest -> []
        | head :: rest -> head :: removeTrailingEmpty rest

type UrlContext =
    {
        Segments: string list
        QueryParameters: Map<string, string list>
        QueryFlags: Set<string>
        Fragment: string option
    }

type ParserError =
    | MissingSegment
    | SegmentMismatch of expected: string * availabe: string
    | LeftoverSegments of segments: string list
    | NoParsers
    | BadType of typeName: string * value: string
    | AmbiguousQueryParameter of key: string * values: string list
    | MissingQueryParameter of key: string
    | BadQueryValues of typeName: string * key: string * values: string list

    /// <summary>
    /// Format a <see cref="ParserError"/> as a human-readable string.
    /// </summary>
    /// <param name="error">The error to format</param>
    /// <returns>
    /// A human-readable description of the error
    /// </returns>
    static member format(error: ParserError) =
        match error with
        | MissingSegment -> "Expected a path segment but the URL ended."
        | SegmentMismatch(expected, available) ->
            $"Expected segment '{expected}' but got '{available}'."
        | LeftoverSegments segments ->
            let joined = segments |> String.concat "/"
            $"URL matched but had leftover segments: '{joined}'."
        | NoParsers -> "No parsers were provided."
        | BadType(typeName, value) -> $"Could not convert '{value}' to {typeName}."
        | AmbiguousQueryParameter(key, values) ->
            let joined = values |> String.concat ", "
            $"Query parameter '{key}' appeared multiple times with values: {joined}."
        | MissingQueryParameter key -> $"Required query parameter '{key}' was missing."
        | BadQueryValues(typeName, key, values) ->
            let joined = values |> String.concat ", "
            $"Not all values for query parameter '{key}' could be converted to {typeName}: {joined}."

type ParserState<'Output> =
    {
        /// <summary>
        /// Succesfully parsed route value
        /// </summary>
        Output: 'Output
        /// <summary>
        /// Remaining segments to consume
        /// </summary>
        Segments: string list
        /// <summary>
        /// Current depth (aka how many segments were consumed so far)
        /// </summary>
        Depth: int
    }

type ParseFailure =
    {
        /// <summary>
        /// What went wrong
        /// </summary>
        Error: ParserError
        /// <summary>
        /// Depth at which the error happened
        /// </summary>
        Depth: int
    }

type UrlParser<'Output> = UrlParser of (UrlContext -> Result<ParserState<'Output>, ParseFailure>)

let pathToSegments path =
    path
    |> String.splitBy "/"
    |> Helpers.removeLeadingEmpty
    |> Helpers.removeTrailingEmpty
    |> List.map Helpers.decodeURIComponent

let pathToUrlContext (path: string) =
    let pathOnly = path |> String.takeUntil '?' |> String.takeUntil '#'

    let storeQueryParameter (map: Map<string, list<string>>) key value =
        let newValues =
            match Map.tryFind key map with
            | None -> [ value ]
            | Some values -> value :: values

        Map.add key newValues map

    let flags, queryParams =
        match String.splitBy "?" path with
        | _ :: query :: _ ->
            let rec collect (accFlags: Set<string>) accQueryParameters pairs =
                match pairs with
                | [] -> (accFlags, accQueryParameters)
                | head :: rest ->
                    match String.splitBy "=" head with
                    // "a"
                    | [ flag ] -> collect (Set.add flag accFlags) accQueryParameters rest

                    // "a=b"
                    // "a="
                    | [ key; value ] ->
                        let newQueryParameters =
                            storeQueryParameter
                                accQueryParameters
                                (Helpers.decodeURIComponent key)
                                (Helpers.decodeURIComponent value)

                        collect accFlags newQueryParameters rest

                    // "a=b=c"
                    | _ -> collect accFlags accQueryParameters rest

            let accFlags, accQueryParameters =
                query |> String.splitBy "&" |> collect Set.empty Map.empty

            // `storeQueryParameter` prepends, so each key's values are
            // built up in reverse order of appearance in the URL.
            accFlags, accQueryParameters |> Map.map (fun _ values -> List.rev values)

        // No query parameters found
        | _ -> Set.empty, Map.empty

    {
        Segments = pathToSegments pathOnly
        QueryParameters = queryParams
        QueryFlags = flags
        Fragment =
            match String.splitBy "#" path with
            | [ _; fragment ] -> fragment |> Helpers.decodeURIComponent |> Some

            | _ -> None
    }

let succeed (output: 'Output) : UrlParser<'Output> =
    UrlParser(fun context ->
        {
            Output = output
            Segments = context.Segments
            Depth = 0
        }
        |> Ok
    )

let apply (UrlParser parser) (context: UrlContext) = parser context

let makeError depth error =
    {
        Error = error
        Depth = depth
    }
    |> Error

/// <summary>
/// Transform the output of a finished parser.
/// </summary>
/// <param name="f">Function to transform the output</param>
/// <param name="parser">Parser to transform</param>
let map (f: 'A -> 'B) (parser: UrlParser<'A>) : UrlParser<'B> =
    UrlParser(fun context ->
        apply parser context
        |> Result.map (fun state ->
            {
                Output = f state.Output
                Segments = state.Segments
                Depth = state.Depth
            }
        )
    )

let private runParser
    (parser: UrlParser<'Output>)
    (context: UrlContext)
    : Result<'Output, ParseFailure>
    =
    apply parser context
    |> Result.bind (fun state ->
        if List.isEmpty state.Segments then
            Ok state.Output
        else
            LeftoverSegments state.Segments |> makeError state.Depth
    )

let rec parse
    (parsers: UrlParser<'Output> list)
    (context: UrlContext)
    : Result<'Output, ParserError>
    =
    match parsers with
    | [] -> Error NoParsers
    | first :: rest ->

        match runParser first context with
        | Ok output -> Ok output
        | Error firstError ->
            let rec tryParse lastError parsers =
                match parsers with
                | [] -> Error lastError.Error
                | currentParser :: restParser ->
                    match runParser currentParser context with
                    | Ok output -> Ok output
                    | Error currentError ->
                        let errorsToReport =
                            // Report the error that went the farther
                            if lastError.Depth > currentError.Depth then
                                lastError
                            else
                                currentError

                        tryParse errorsToReport restParser

            tryParse firstError rest

/// <summary>
/// Match a literal path segment.
/// </summary>
/// <remarks>
/// This function will fail if:
/// - There are no remaining segments to consume.
/// - The next segment does not match <paramref name="expected"/>.
/// </remarks>
/// <param name="expected">The literal segment value to match</param>
/// <param name="innerParser">Parser to apply</param>
let segment (expected: string) (innerParser: UrlParser<'Output>) : UrlParser<'Output> =
    UrlParser(fun context ->
        apply innerParser context
        |> Result.bind (fun parserState ->
            match parserState.Segments with
            | first :: rest ->
                if first = expected then
                    {
                        Output = parserState.Output
                        Segments = rest
                        Depth = parserState.Depth + 1
                    }
                    |> Ok
                else
                    {
                        Error = SegmentMismatch(expected, first)
                        Depth = parserState.Depth
                    }
                    |> Error

            | [] -> makeError parserState.Depth MissingSegment
        )
    )

/// <summary>
/// Consume the next path segment and parse it as an integer.
/// </summary>
/// <remarks>
/// This function will fail if:
/// - There are no remaining segments to consume.
/// - The next segment cannot be parsed as a valid integer.
/// </remarks>
/// <param name="innerParser">Parser to apply</param>
/// <returns>
/// The integer value of the consumed segment
/// </returns>
let int (innerParser: UrlParser<int -> 'Output>) : UrlParser<'Output> =
    UrlParser(fun context ->
        apply innerParser context
        |> Result.bind (fun parserState ->
            match parserState.Segments with
            | [] -> makeError parserState.Depth MissingSegment
            | first :: rest ->
                match Int32.TryParse first with
                | true, int32 ->
                    {
                        Output = parserState.Output int32
                        Segments = rest
                        Depth = parserState.Depth + 1
                    }
                    |> Ok
                | false, _ -> BadType("int", first) |> makeError parserState.Depth

        )
    )

/// <summary>
/// Consume the next path segment and parse it with a custom conversion function.
/// </summary>
/// <remarks>
/// This function will fail if:
/// - There are no remaining segments to consume.
/// - <paramref name="tryParse"/> returns <c>None</c> for the segment value.
/// </remarks>
/// <param name="typeName">Name reported in the error message when conversion fails, e.g. <c>"Guid"</c></param>
/// <param name="tryParse">Function attempting to convert the segment string into <c>'A</c></param>
/// <param name="innerParser">Parser to apply</param>
/// <returns>
/// The converted value of the consumed segment
/// </returns>
let custom
    (typeName: string)
    (tryParse: string -> 'A option)
    (innerParser: UrlParser<'A -> 'Output>)
    : UrlParser<'Output>
    =
    UrlParser(fun context ->
        apply innerParser context
        |> Result.bind (fun parserState ->
            match parserState.Segments with
            | [] -> makeError parserState.Depth MissingSegment
            | first :: rest ->
                match tryParse first with
                | Some value ->
                    {
                        Output = parserState.Output value
                        Segments = rest
                        Depth = parserState.Depth + 1
                    }
                    |> Ok
                | None -> BadType(typeName, first) |> makeError parserState.Depth
        )
    )

/// <summary>
/// Consume the next path segment as a string.
/// </summary>
/// <remarks>
/// This function will fail if:
/// - There are no remaining segments to consume.
/// </remarks>
/// <param name="innerParser">Parser to apply</param>
/// <returns>
/// The string value of the consumed segment
/// </returns>
let string (innerParser: UrlParser<string -> 'Output>) : UrlParser<'Output> =
    UrlParser(fun context ->
        apply innerParser context
        |> Result.bind (fun parserState ->
            match parserState.Segments with
            | [] -> makeError parserState.Depth MissingSegment
            | first :: rest ->
                {
                    Output = parserState.Output first
                    Segments = rest
                    Depth = parserState.Depth + 1
                }
                |> Ok
        )
    )

module Query =

    /// <summary>
    /// Parse a query flag as a bool.
    /// </summary>
    /// <param name="name">Flag name to look up</param>
    /// <param name="innerParser">Parser to apply</param>
    /// <returns>
    /// <c>true</c> if the flag is present
    ///
    /// <c>false</c> if the flag is absent
    /// </returns>
    let flag (name: string) (innerParser: UrlParser<bool -> 'Output>) : UrlParser<'Output> =
        UrlParser(fun context ->
            apply innerParser context
            |> Result.bind (fun parserState ->
                {
                    Output = parserState.Output(Set.contains name context.QueryFlags)
                    Segments = parserState.Segments
                    Depth = parserState.Depth + 1
                }
                |> Ok
            )
        )

    /// <summary>
    /// Parse all query flags as a string list.
    /// </summary>
    /// <param name="innerParser">Parser to apply</param>
    /// <returns>
    /// List of all flag names present in the query string
    ///
    /// <c>[]</c> if no flags are present
    /// </returns>
    let allFlags (innerParser: UrlParser<string list -> 'Output>) : UrlParser<'Output> =
        UrlParser(fun context ->
            apply innerParser context
            |> Result.bind (fun parserState ->
                {
                    Output = parserState.Output(context.QueryFlags |> Set.toList)
                    Segments = parserState.Segments
                    Depth = parserState.Depth + 1
                }
                |> Ok
            )
        )

    /// <summary>
    /// Parse all values for a query key as a string list.
    /// </summary>
    /// <param name="key">Key to look up</param>
    /// <param name="innerParser">Parser to apply</param>
    /// <returns>
    /// <c>[]</c> if the key is absent
    ///
    /// List of all values if the key is present one or more times
    /// </returns>
    let strings
        (key: string)
        (innerParser: UrlParser<string list -> 'Output>)
        : UrlParser<'Output>
        =
        UrlParser(fun context ->
            apply innerParser context
            |> Result.bind (fun parserState ->
                {
                    Output =
                        Map.tryFind key context.QueryParameters
                        |> Option.defaultValue []
                        |> parserState.Output
                    Segments = parserState.Segments
                    Depth = parserState.Depth + 1
                }
                |> Ok
            )
        )

    /// <summary>
    /// Parse all values for a query key as an int list.
    /// </summary>
    /// <remarks>
    /// This function will fail if:
    /// - Any value cannot be parsed as a valid integer.
    /// </remarks>
    /// <param name="key">Key to look up</param>
    /// <param name="innerParser">Parser to apply</param>
    /// <returns>
    /// <c>[]</c> if the key is absent
    ///
    /// List of all values as ints if the key is present one or more times
    /// </returns>
    let ints (key: string) (innerParser: UrlParser<int list -> 'Output>) : UrlParser<'Output> =
        UrlParser(fun context ->
            apply innerParser context
            |> Result.bind (fun parserState ->
                let values = Map.tryFind key context.QueryParameters |> Option.defaultValue []

                let parsed =
                    values
                    |> List.choose (fun v ->
                        match Int32.TryParse v with
                        | true, n -> Some n
                        | _ -> None
                    )

                if parsed.Length = values.Length then
                    {
                        Output = parserState.Output parsed
                        Segments = parserState.Segments
                        Depth = parserState.Depth + 1
                    }
                    |> Ok
                else
                    BadQueryValues("int", key, values) |> makeError parserState.Depth
            )
        )

    module Optional =

        /// <summary>
        /// Parse a single query parameter as a string option.
        /// </summary>
        /// <remarks>
        /// This function will fail if:
        /// - The key is present more than once in the query.
        /// </remarks>
        /// <param name="key">Key to look up</param>
        /// <param name="innerParser">Parser to apply</param>
        /// <returns>
        /// <c>Some string</c> if the key is present once
        ///
        /// <c>None</c> if the key is absent
        /// </returns>
        let string
            (key: string)
            (innerParser: UrlParser<string option -> 'Output>)
            : UrlParser<'Output>
            =
            UrlParser(fun context ->
                apply innerParser context
                |> Result.bind (fun parserState ->
                    let values = Map.tryFind key context.QueryParameters |> Option.defaultValue []

                    match values with
                    | [] ->
                        {
                            Output = parserState.Output None
                            Segments = parserState.Segments
                            Depth = parserState.Depth + 1
                        }
                        |> Ok
                    | [ single ] ->
                        {
                            Output = parserState.Output(Some single)
                            Segments = parserState.Segments
                            Depth = parserState.Depth + 1
                        }
                        |> Ok
                    | many -> AmbiguousQueryParameter(key, many) |> makeError parserState.Depth
                )
            )

        /// <summary>
        /// Parse a single query parameter as an int option.
        /// </summary>
        /// <remarks>
        /// This function will fail if:
        /// - The key is present but cannot be parsed as a valid integer.
        /// - The key is present more than once in the query.
        /// </remarks>
        /// <param name="key">Key to look up</param>
        /// <param name="innerParser">Parser to apply</param>
        /// <returns>
        /// <c>Some int</c> if the key is present once and a valid int
        ///
        /// <c>None</c> if the key is absent
        /// </returns>
        let int (key: string) (innerParser: UrlParser<int option -> 'Output>) : UrlParser<'Output> =
            UrlParser(fun context ->
                apply innerParser context
                |> Result.bind (fun parserState ->
                    let values = Map.tryFind key context.QueryParameters |> Option.defaultValue []

                    match values with
                    | [] ->
                        {
                            Output = parserState.Output None
                            Segments = parserState.Segments
                            Depth = parserState.Depth + 1
                        }
                        |> Ok
                    | [ single ] ->
                        match Int32.TryParse single with
                        | true, n ->
                            {
                                Output = parserState.Output(Some n)
                                Segments = parserState.Segments
                                Depth = parserState.Depth + 1
                            }
                            |> Ok
                        | false, _ -> BadType("int", single) |> makeError parserState.Depth
                    | many -> AmbiguousQueryParameter(key, many) |> makeError parserState.Depth
                )
            )

        /// <summary>
        /// Parse a single query parameter as an option, using a custom conversion function.
        /// </summary>
        /// <remarks>
        /// This function will fail if:
        /// - The key is present but <paramref name="tryParse"/> returns <c>None</c> for its value.
        /// - The key is present more than once in the query.
        /// </remarks>
        /// <param name="key">Key to look up</param>
        /// <param name="typeName">Name reported in the error message when conversion fails, e.g. <c>"Guid"</c></param>
        /// <param name="tryParse">Function attempting to convert the value string into <c>'A</c></param>
        /// <param name="innerParser">Parser to apply</param>
        /// <returns>
        /// <c>Some value</c> if the key is present once and converts successfully
        ///
        /// <c>None</c> if the key is absent
        /// </returns>
        let custom
            (key: string)
            (typeName: string)
            (tryParse: string -> 'A option)
            (innerParser: UrlParser<'A option -> 'Output>)
            : UrlParser<'Output>
            =
            UrlParser(fun context ->
                apply innerParser context
                |> Result.bind (fun parserState ->
                    let values = Map.tryFind key context.QueryParameters |> Option.defaultValue []

                    match values with
                    | [] ->
                        {
                            Output = parserState.Output None
                            Segments = parserState.Segments
                            Depth = parserState.Depth + 1
                        }
                        |> Ok
                    | [ single ] ->
                        match tryParse single with
                        | Some value ->
                            {
                                Output = parserState.Output(Some value)
                                Segments = parserState.Segments
                                Depth = parserState.Depth + 1
                            }
                            |> Ok
                        | None -> BadType(typeName, single) |> makeError parserState.Depth
                    | many -> AmbiguousQueryParameter(key, many) |> makeError parserState.Depth
                )
            )

    module Required =

        /// <summary>
        /// Parse a single query parameter as a string.
        /// </summary>
        /// <remarks>
        /// This function will fail if:
        /// - The key is absent.
        /// - The key is present more than once in the query.
        /// </remarks>
        /// <param name="key">Key to look up</param>
        /// <param name="innerParser">Parser to apply</param>
        /// <returns>
        /// The string value if the key is present exactly once
        /// </returns>
        let string (key: string) (innerParser: UrlParser<string -> 'Output>) : UrlParser<'Output> =
            UrlParser(fun context ->
                apply innerParser context
                |> Result.bind (fun parserState ->
                    let values = Map.tryFind key context.QueryParameters |> Option.defaultValue []

                    match values with
                    | [] -> MissingQueryParameter key |> makeError parserState.Depth
                    | [ single ] ->
                        {
                            Output = parserState.Output single
                            Segments = parserState.Segments
                            Depth = parserState.Depth + 1
                        }
                        |> Ok
                    | many -> AmbiguousQueryParameter(key, many) |> makeError parserState.Depth
                )
            )

        /// <summary>
        /// Parse a single query parameter as an int.
        /// </summary>
        /// <remarks>
        /// This function will fail if:
        /// - The key is absent.
        /// - The key is present but cannot be parsed as a valid integer.
        /// - The key is present more than once in the query.
        /// </remarks>
        /// <param name="key">Key to look up</param>
        /// <param name="innerParser">Parser to apply</param>
        /// <returns>
        /// The integer value if the key is present exactly once and is a valid int
        /// </returns>
        let int (key: string) (innerParser: UrlParser<int -> 'Output>) : UrlParser<'Output> =
            UrlParser(fun context ->
                apply innerParser context
                |> Result.bind (fun parserState ->
                    let values = Map.tryFind key context.QueryParameters |> Option.defaultValue []

                    match values with
                    | [] -> MissingQueryParameter key |> makeError parserState.Depth
                    | [ single ] ->
                        match Int32.TryParse single with
                        | true, n ->
                            {
                                Output = parserState.Output n
                                Segments = parserState.Segments
                                Depth = parserState.Depth + 1
                            }
                            |> Ok
                        | false, _ -> BadType("int", single) |> makeError parserState.Depth
                    | many -> AmbiguousQueryParameter(key, many) |> makeError parserState.Depth
                )
            )

        /// <summary>
        /// Parse a single query parameter, using a custom conversion function.
        /// </summary>
        /// <remarks>
        /// This function will fail if:
        /// - The key is absent.
        /// - <paramref name="tryParse"/> returns <c>None</c> for the value.
        /// - The key is present more than once in the query.
        /// </remarks>
        /// <param name="key">Key to look up</param>
        /// <param name="typeName">Name reported in the error message when conversion fails, e.g. <c>"Guid"</c></param>
        /// <param name="tryParse">Function attempting to convert the value string into <c>'A</c></param>
        /// <param name="innerParser">Parser to apply</param>
        /// <returns>
        /// The converted value if the key is present exactly once and converts successfully
        /// </returns>
        let custom
            (key: string)
            (typeName: string)
            (tryParse: string -> 'A option)
            (innerParser: UrlParser<'A -> 'Output>)
            : UrlParser<'Output>
            =
            UrlParser(fun context ->
                apply innerParser context
                |> Result.bind (fun parserState ->
                    let values = Map.tryFind key context.QueryParameters |> Option.defaultValue []

                    match values with
                    | [] -> MissingQueryParameter key |> makeError parserState.Depth
                    | [ single ] ->
                        match tryParse single with
                        | Some value ->
                            {
                                Output = parserState.Output value
                                Segments = parserState.Segments
                                Depth = parserState.Depth + 1
                            }
                            |> Ok
                        | None -> BadType(typeName, single) |> makeError parserState.Depth
                    | many -> AmbiguousQueryParameter(key, many) |> makeError parserState.Depth
                )
            )

/// <summary>
/// Read the URL fragment (the part after <c>#</c>).
/// </summary>
/// <param name="innerParser">Parser to apply</param>
/// <returns>
/// <c>Some string</c> if a fragment is present
///
/// <c>None</c> if there is no fragment
/// </returns>
let fragment (innerParser: UrlParser<string option -> 'Output>) =
    UrlParser(fun context ->
        apply innerParser context
        |> Result.bind (fun parserState ->
            {
                Output = parserState.Output context.Fragment
                Segments = parserState.Segments
                Depth = parserState.Depth + 1
            }
            |> Ok
        )
    )
