﻿module Rexcfnghk.MarkSixParser.Tests.Properties

open System
open Rexcfnghk.MarkSixParser
open Models
open FsCheck
open FsCheck.Xunit
open Swensen.Unquote

let markSixNumberGen =
    Gen.elements [1..49]
    |> Gen.map (MarkSixNumber.create >> ValidationResult.extract)

let usersDrawArb = 
    markSixNumberGen
    |> Gen.listOfLength 6
    |> Arb.fromGen

let drawResultsArb =
    let drawnNumbersGen =
        markSixNumberGen
        |> Gen.map DrawnNumber
        |> Gen.listOfLength 6

    let extraNumberGen =
        markSixNumberGen
        |> Gen.map ExtraNumber
        |> Gen.listOfLength 1

    List.append <!> drawnNumbersGen <*> extraNumberGen
    |> Arb.fromGen
    

[<Property>]
let ``drawRandom always returns six elements`` () =
    let numbers = MarkSix.drawNumbers ()
    numbers.Length =! 6

[<Property>]
let ``drawRandom always returns numbers between 1 and 49`` () =
    let isWithinRange x = x >= 1 && x <= 49
    let numbers = MarkSix.drawNumbers ()
    test <@ List.forall (MarkSixNumber.value >> isWithinRange) numbers @>

[<Property>]
let ``getDrawResultNumbers always add ExtraNumber at the end`` () =
    let r = Random()
    let resultListRev = List.rev <| MarkSix.getDrawResultNumbers ignore (fun () -> r.Next(1, 50))
    test <@ match resultListRev with (ExtraNumber _) :: _ -> true | _ -> false @>

[<Property>]
let ``getDrawResultNumbers always returns seven elements`` () =
    let r = Random()
    let result = MarkSix.getDrawResultNumbers ignore (fun () -> r.Next(1, 50))
    result.Length =! 7

[<Property>]
let ``drawResultsArb returns six DrawnNumbers and one ExtraNumber`` () =
    Prop.forAll drawResultsArb <| fun l ->
        let drawnNumbers, extraNumbers = List.partition (function DrawnNumber _ -> true | ExtraNumber _ -> false) l
        test <@ List.length drawnNumbers = 6 && List.length extraNumbers = 1 @>