﻿module Rexcfnghk.MarkSixParser.Runner.Tests.Facts

open Rexcfnghk.MarkSixParser
open Rexcfnghk.MarkSixParser.Models
open Rexcfnghk.MarkSixParser.Runner.MarkSixNumberReader
open Rexcfnghk.MarkSixParser.Runner.Decision
open Xunit
open FsCheck.Xunit
open Swensen.Unquote

[<Fact>]
let ``getDrawNumbers accepts markSixNumberReader parameter`` () =
    let numbers = [| 6; 7; 12; 15; 27; 36; 29 |] 
    let m6Numbers =
        numbers
        |> Array.map (MarkSixNumber.create)

    let drawResults = getDrawResultNumbers (Array.get m6Numbers)

    let (DrawResults (DrawnNumber m1, DrawnNumber m2, DrawnNumber m3,
                    DrawnNumber m4, DrawnNumber m5, DrawnNumber m6, ExtraNumber e)) = drawResults
    let i1, i2, i3, i4, i5, i6, e = MarkSixNumber.value m1, MarkSixNumber.value m2, MarkSixNumber.value m3,
                                        MarkSixNumber.value m4, MarkSixNumber.value m5, MarkSixNumber.value m6,
                                        MarkSixNumber.value e

    [| i1; i2; i3; i4; i5; i6; e |] =! numbers

[<Fact>]
let ``getUsersDraw accepts markSixNumberReader parameter`` () =
    let m6Numbers =
        [| 3; 4; 24; 28; 30; 32 |]
        |> Array.map (MarkSixNumber.create)

    let usersDraw = getUsersDrawNumbers 6 (Array.get m6Numbers)

    let (UsersDraw s) = usersDraw

    match Set.toList s with
    | [m1; m2; m3; m4; m5; m6] ->
        let i1, i2, i3, i4, i5, i6 = MarkSixNumber.value m1, MarkSixNumber.value m2, MarkSixNumber.value m3,
                                        MarkSixNumber.value m4, MarkSixNumber.value m5, MarkSixNumber.value m6

        [| i1; i2; i3; i4; i5; i6 |] =! [| 3; 4; 24; 28; 30; 32 |]
    | _ -> failwith "Unexpected"

[<Fact>]
let ``getUsersDrawNumbers can accept convert UsersDraw list`` () =
    let m6NumberList = [
        [| 3; 4; 24; 28; 30; 32 |]
        [| 1; 2; 3; 4; 5; 6 |] ]
    let list = List.map (Array.map MarkSixNumber.create) m6NumberList

    let usersDrawList = List.map (Array.get >> getUsersDrawNumbers 6) list

    test <@ List.forall2 (fun usersDraw m6Number ->
                let (UsersDraw s) = usersDraw
                match Set.toList s with
                | [m1; m2; m3; m4; m5; m6] ->
                    let i1, i2, i3, i4, i5, i6 = MarkSixNumber.value m1, MarkSixNumber.value m2, MarkSixNumber.value m3,
                                                    MarkSixNumber.value m4, MarkSixNumber.value m5, MarkSixNumber.value m6

                    [| i1; i2; i3; i4; i5; i6 |] = m6Number
                | _ -> failwith "Unexpected")
                usersDrawList m6NumberList @>

[<Fact>]
let ``getMultipleUsersDraw can accept multiple UsersDraw list`` () =
    let usersDrawList =
        [|
            [| 3; 4; 24; 28; 30; 32 |]
            [| 1; 2; 3; 4; 5; 6 |]
        |]
        |> Array.map (Array.map (MarkSixNumber.create >> ValidationResult.extract)
                        >> Set.ofArray
                        >> MarkSix.toUsersDraw)

    let decisions = [| Yes; No |]

    let multipleUsersDrawResult = getMultipleUsersDraw (Array.get usersDrawList) (fun i -> Array.get decisions (i - 1)) |> List.toArray

    test <@ Array.forall2 (fun usersDraw m6Number ->
        match usersDraw, m6Number with
        | Error (ErrorMessage e), _ | _, Error (ErrorMessage e) -> failwith e
        | Success usersDraw, Success m6Number ->
            usersDraw = m6Number) multipleUsersDrawResult usersDrawList @>
