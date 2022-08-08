namespace minlightcsfs

open System
open System.Text
open System.Text.RegularExpressions
open Microsoft.FSharp.Reflection

module Scanf =

    let getLine (f: System.IO.TextReader) =

        let mutable empty = true
        let mutable s: string = ""

        while empty do
            let l = f.ReadLine().Trim()
            if l.Length = 0 then empty <- false else s <- l

        s

    let sscanf (pf: PrintfFormat<_, _, _, _, 't>) s : 't =
        let formatStr = pf.Value
        let constants = formatStr.Split([| "%s" |], StringSplitOptions.None)

        let regex =
            Regex("^" + String.Join("(.*?)", constants |> Array.map Regex.Escape) + "$")

        let matches =
            regex.Match(s).Groups
            |> Seq.cast<Group>
            |> Seq.skip 1
            |> Seq.map (fun g -> g.Value |> box)

        FSharpValue.MakeTuple(matches |> Seq.toArray, typeof<'t>) :?> 't
