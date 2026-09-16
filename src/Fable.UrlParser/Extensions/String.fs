module String

let splitBy (sep: string) (text: string) = text.Split(sep) |> Array.toList

let takeUntil (sep: char) (str: string) =
    let mutable i = 0
    let length = String.length str

    while i < length && str.[i] <> sep do
        i <- i + 1

    str.Substring(0, i)

let skip count (source: string) =
    let source = nullArgCheck (nameof source) source

    source[count..]

let skipUntil (sep: char) (str: string) =
    let mutable i = 0
    let length = String.length str

    while i < length && str.[i] <> sep do
        i <- i + 1

    if i = length then
        ""
    else
        str.Substring(i)

let skipPast (sep: char) (str: string) =
    let mutable i = 0
    let length = String.length str

    while i < length && str.[i] <> sep do
        i <- i + 1

    if i = length then
        ""
    else
        str.Substring(i + 1)
