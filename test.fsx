#r "nuget: FSharp.Data.Adaptive, 1.2.26"

open FSharp.Data.Adaptive

let prefix = cval ""

let names =
    clist
        [
            "Emil, Hans"
            "Mustermann, Max"
            "Tisch, Roman"
        ]

let filteredNames =
    (prefix, AList.toAVal names)
    ||> AVal.map2 (fun prefix names ->
        printfn "Filtering names with prefix '%s'..." prefix

        names
        |> Seq.filter (fun name ->
            name.StartsWith(prefix, System.StringComparison.InvariantCultureIgnoreCase)
        )
    )
    |> AList.ofAVal

filteredNames |> AList.force |> Seq.iter (printfn "Filtered name: %s")

transact (fun () -> prefix.Value <- "M")

filteredNames |> AList.force |> Seq.iter (printfn "Filtered name: %s")
