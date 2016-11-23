module Persimmon.Runner.Tests.ColorConsoleTest

open System
open Persimmon
open UseTestNameByReflection
open Persimmon.Runner

let constructor = test {
  let testStyle =
    if Console.ForegroundColor <> ColorConsole.GetColor(ColorStyle.Error) then
      ColorStyle.Error
    elif Console.ForegroundColor <> ColorConsole.GetColor(ColorStyle.Pass) then
      ColorStyle.Pass
    else failwith "Could not find a color to test with"

  try
    Console.ForegroundColor <- ConsoleColor.Magenta

    let expected = ColorConsole.GetColor(testStyle)
    use _ = new ColorConsole(testStyle)
    do! assertEquals expected Console.ForegroundColor

  finally Console.ResetColor()
}
