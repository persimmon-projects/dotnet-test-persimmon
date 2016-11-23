module Persimmon.Runner.Tests.ColorStyleTest

open System
open Persimmon
open UseTestNameByReflection
open Persimmon.Runner

let ``get color`` = parameterize {
  source [
    (ColorStyle.Pass, ConsoleColor.Green)
    (ColorStyle.Failure, ConsoleColor.Red)
    (ColorStyle.Error, ConsoleColor.Red)
  ]
  run (fun (style, expected) -> test {
    do! assertEquals expected (ColorConsole.GetColor(style))
  })
}
