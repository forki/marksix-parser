﻿module Rexcfnghk.MarkSixParser.MarkSixNumber

open System

[<CustomComparison; CustomEquality>]
type T = 
    | MarkSixNumber of int
    interface IComparable<T> with
        member this.CompareTo other =
            let (MarkSixNumber thisInt, MarkSixNumber otherInt) = this, other
            thisInt.CompareTo otherInt
    interface IComparable with
        member this.CompareTo other =
            (this :> IComparable<T>).CompareTo(other :?> T)
    interface IEquatable<T> with
        member this.Equals other =
            this.Equals other
    override this.Equals other =
        (this :> IComparable).CompareTo other = 0
    override this.GetHashCode() =
        let (MarkSixNumber thisInt) = this
        thisInt

let create input = 
    if input >= 1 && input <= 49 then
        input
        |> (MarkSixNumber >> ValidationResult.success)
    else 
        "Input out of range"
        |> ValidationResult.errorFromString

let createOption = create >> ValidationResult.extractOption

let value (MarkSixNumber num) = num