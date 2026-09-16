namespace Fable.UrlParser

type Parser<'Output> = Parser of Base.UrlParser<'Output>

module Parser =

    type ParserError = Base.ParserError

    let getBaseParser (Parser parser) = parser

    let succeed (output: 'Output) = Parser(Base.succeed output)

    let map f (parser: Parser<'A>) : Parser<'B> =
        getBaseParser parser |> Base.map f |> Parser

    let segment expected innerParser =
        getBaseParser innerParser |> Base.segment expected |> Parser

    let string innerParser =
        getBaseParser innerParser |> Base.string |> Parser

    let int innerParser =
        getBaseParser innerParser |> Base.int |> Parser

    let custom typeName tryParse innerParser =
        getBaseParser innerParser |> Base.custom typeName tryParse |> Parser

    let fragment innerParser =
        getBaseParser innerParser |> Base.fragment |> Parser

    module Query =

        let flag name innerParser =
            getBaseParser innerParser |> Base.Query.flag name |> Parser

        let allFlags innerParser =
            getBaseParser innerParser |> Base.Query.allFlags |> Parser

        let strings key innerParser =
            getBaseParser innerParser |> Base.Query.strings key |> Parser

        let ints key innerParser =
            getBaseParser innerParser |> Base.Query.ints key |> Parser

        module Optional =

            let string key innerParser =
                getBaseParser innerParser |> Base.Query.Optional.string key |> Parser

            let int key innerParser =
                getBaseParser innerParser |> Base.Query.Optional.int key |> Parser

            let custom key typeName tryParse innerParser =
                getBaseParser innerParser
                |> Base.Query.Optional.custom key typeName tryParse
                |> Parser

        module Required =

            let string key innerParser =
                getBaseParser innerParser |> Base.Query.Required.string key |> Parser

            let int key innerParser =
                getBaseParser innerParser |> Base.Query.Required.int key |> Parser

            let custom key typeName tryParse innerParser =
                getBaseParser innerParser
                |> Base.Query.Required.custom key typeName tryParse
                |> Parser

    let tryParsePath (parsers: Parser<'Output> list) (path: string) : Result<'Output, string> =
        path
        |> Base.pathToUrlContext
        |> Base.parse (List.map getBaseParser parsers)
        |> Result.mapError ParserError.format

    let tryParseHash (parsers: Parser<'Output> list) (path: string) : Result<'Output, string> =
        path |> String.skipPast '#' |> tryParsePath parsers
