module Repro.Main

open System.Collections.Generic
open Fable.Ripple

// 1 - erased type as generic argument of a collection
let cells = Dictionary<string, Signal<float>>()
let v = Var.create 1.5
cells["x"] <- v.Signal

// 2 - record field of an erased type (reflection emitted for records)
type Holder =
    {
        Name: string
        S: Signal<float>
    }

let h =
    {
        Name = "x"
        S = v.Signal
    }

// 3 - union case carrying the erased type
type Slot =
    | Empty
    | Filled of Signal<float>

let s = Filled v.Signal

// 4 - typeof of the erased type
let t = typeof<Signal<float>>

// 5 - structural equality over a collection of erased values
let list = ResizeArray<Signal<float>>()
list.Add v.Signal
let contains = list.Contains v.Signal

printfn "%f %s %b" cells["x"].Value t.Name contains
