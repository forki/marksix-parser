﻿module Rexcfnghk.MarkSixParser.Tests.MarkSixProperties

open Rexcfnghk.MarkSixParser
open Models
open FsCheck
open FsCheck.Xunit
open Swensen.Unquote

let markSixNumberGen =
    Gen.elements [1..49]
    |> Gen.map (MarkSixNumber.create >> ValidationResult.extract)

let markSixNumberListGen count =
    markSixNumberGen
    |> Gen.listOfLength count
    |> Gen.map Set.ofList
    |> Gen.suchThat (fun s -> Set.count s >= count)
    |> Gen.map (Seq.take count >> Seq.toList)

let usersDrawArb = 
    markSixNumberListGen 6
    |> Gen.map (MarkSix.toUsersDraw >> ValidationResult.extract)
    |> Arb.fromGen

let drawResultsArb =
    markSixNumberListGen 7
    |> Gen.map (function [] -> failwith "unexpected" | h :: t -> Set.ofList t, h)
    |> Arb.fromGen

let invalidLengthUsersDrawArb =
    Arb.Default.NonNegativeInt().Generator
    |> Gen.suchThat (fun x -> x.Get <> 0 && x.Get <> 6)
    |> Gen.map (fun (NonNegativeInt x) -> x)
    >>= (fun x -> Gen.listOfLength x markSixNumberGen)
    |> Arb.fromGen

let invalidLengthDrawResultsArb =
    Arb.Default.NonNegativeInt().Generator
    |> Gen.suchThat (fun x -> x.Get <> 0 && x.Get <> 7)
    |> Gen.map (fun (NonNegativeInt x) -> x)
    >>= (fun x -> Gen.listOfLength x markSixNumberGen)
    |> Gen.map (function [] -> failwith "unexpected" | h :: t -> Set.ofList t, h)
    |> Arb.fromGen

[<Property>]
let ``drawRandom always returns numbers between 1 and 49`` () =
    let isWithinRange x = x >= 1 && x <= 49
    let (UsersDraw (m1, m2, m3, m4, m5, m6)) = MarkSix.randomUsersDraw ()
    let numbers = [m1; m2; m3; m4; m5; m6]
    test <@ List.forall (MarkSixNumber.value >> isWithinRange) numbers @>

[<Property>]
let ``toUsersDraw fails when given set length not equals to six`` () =
    Prop.forAll invalidLengthUsersDrawArb <| fun l ->
        test <@ match MarkSix.toUsersDraw l with
                | Error _ -> true
                | _ -> false @>

[<Property>]
let ``toDrawResults fails when given list length not equals to seven`` () =
    Prop.forAll invalidLengthDrawResultsArb <| fun l ->
        test <@ match MarkSix.toDrawResults l with
                | Error _ -> true
                | _ -> false @>

[<Property(MaxTest = 500)>]
let ``checkResults returns correct Prize for arbitrary drawResults and usersDraw`` () =
    let splitDrawResults drawResults =
        match List.rev drawResults with
        | [] -> failwith "Should not reach here"
        | h :: t -> t, h

    let calculatePoints usersDraw (drawResultWithoutExtraNumber, extraNumber) =
        let points = 
            (Set.ofList usersDraw, Set.ofList drawResultWithoutExtraNumber)
            ||> Set.intersect
            |> Set.count
            |> decimal

        let extraPoints = 
            match List.tryFind ((=) extraNumber) usersDraw with
            | Some _ -> 0.5m
            | None -> 0.m

        points + extraPoints |> Points
    
    Prop.forAll drawResultsArb <| fun drawResults ->
        Prop.forAll usersDrawArb <| fun usersDraw ->
            let extractedDrawResults =
                let (DrawResults (
                        DrawnNumber n1, DrawnNumber n2, DrawnNumber n3, 
                        DrawnNumber n4, DrawnNumber n5, DrawnNumber n6, 
                        ExtraNumber e)) = drawResults
                [n1; n2; n3; n4; n5; n6; e]

            let extractedUsersDraw =
                let (UsersDraw (n1, n2, n3, n4, n5, n6)) = usersDraw
                [n1; n2; n3; n4; n5; n6]

            let expected = 
                calculatePoints extractedUsersDraw (splitDrawResults extractedDrawResults)
                |> Prize.fromPoints
 
            let actual =
                MarkSix.checkResults ignore drawResults usersDraw
                |> ValidationResult.extract

            actual =! expected