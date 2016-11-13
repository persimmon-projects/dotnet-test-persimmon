module Persimmon.Runner.Tests.Runner

open System
open System.IO
open System.Reflection
open System.Diagnostics
open Persimmon
open Persimmon.Runner
open Persimmon.Output

[<EntryPoint>]
let main _ =
  let watch = Stopwatch()
  let outputs = [
    {
      Writer = Console.Out
      Formatter = Formatter.SummaryFormatter.normal watch
    }
  ]
  use reporter =
    new Reporter(
      new Printer<_>(Console.Out, Formatter.ProgressFormatter.dot),
      new Printer<_>(outputs),
      new Printer<_>(Console.Error, Formatter.ErrorFormatter.normal))

  let asms = [|
    typeof<Persimmon.Runner.Tests.SinkTest.FakeReference>.GetTypeInfo().Assembly
  |]
  let tests = TestCollector.collectRootTestObjects asms

  watch.Start()
  let res = TestRunner.runAllTests reporter.ReportProgress tests
  watch.Stop()

  reporter.ReportProgress(TestResult.endMarker)
  reporter.ReportSummary(res.Results)
  res.Errors
