module Persimmon.Runner.Tests.SinkTest

open System
open System.IO
open Persimmon
open UseTestNameByReflection
open Persimmon.Runner
open Persimmon.Runner.Sinks
open Microsoft.Extensions.Testing.Abstractions
open Newtonsoft.Json

type FakeReference = FakeReference

let execute (f: MemoryStream -> BinaryWriter -> TestCase<unit>) = test {
  let stream = new MemoryStream()
  let writer = new BinaryWriter(stream)
  do! f stream writer
  writer.Dispose()
  return stream.Dispose() 
}

let getMessage (stream: MemoryStream) = test {
  stream.Position <- 0L
  let reader = new BinaryReader(stream)
  let json = reader.ReadString()
  let msg = JsonConvert.DeserializeObject<Message>(json)
  do! assertPred (not <| isNull msg)
  return msg
}

let getPayload<'T> (msg: Message) = test {
  do! assertPred (not <| isNull msg)
  do! assertPred (not <| isNull msg.Payload)
  return msg.Payload.ToObject<'T>()
}

let createMockTest () =
  Test(
    DisplayName = "NUnitTest",
    FullyQualifiedName = "NUnit.Runner.Test.NUnitTest"
  )

let createMockTestResult () =
  let endTime = DateTimeOffset.Now
  TestResult(
    createMockTest(),
    StartTime = endTime.AddSeconds(-2.0),
    EndTime = endTime,
    Duration = TimeSpan.FromSeconds(2.0),
    ErrorMessage = "An error occured",
    ErrorStackTrace = "Stack trace",
    Outcome = TestOutcome.Failed
  )

let assertTestEqual (expected: Test) (actual: Test) = test {
  do! assertEquals expected.DisplayName actual.DisplayName
  do! assertEquals expected.FullyQualifiedName actual.FullyQualifiedName
}

let assertTestResultEqual (expected: TestResult) (actual: TestResult) = test {
  do! assertEquals expected.DisplayName actual.DisplayName
  do! assertEquals expected.StartTime actual.StartTime
  do! assertEquals expected.EndTime actual.EndTime
  do! assertEquals expected.Duration actual.Duration
  do! assertEquals expected.ErrorMessage actual.ErrorMessage
  do! assertEquals expected.ErrorStackTrace actual.ErrorStackTrace
  do! assertEquals expected.Outcome actual.Outcome
}

module RemoteTestSinkTest =

  let ``send command`` = parameterize {
    source [
      (fun (testSink: ITestSink) -> testSink.SendTestCompleted()), Messages.TestCompleted
      (fun (testSink: ITestSink) -> testSink.SendWaitingCommand()), Messages.WaitingCommand
    ]
    run (fun (f, expected) -> execute (fun stream writer -> test {
      let testSink = RemoteTestDiscoverySink(writer)
      f testSink
      let! msg = getMessage stream
      do! assertEquals expected msg.MessageType
    }))
  }

module RemoteTestDiscoverySinkTest =

  let SendTestFound = execute (fun stream writer -> test {
    let testSink = RemoteTestDiscoverySink(writer)
    let t = createMockTest()
    testSink.SendTestFound(t)
    let! msg = getMessage stream
    do! assertEquals Messages.TestFound msg.MessageType
    let! payload = getPayload<Test> msg
    do! assertTestEqual t payload
  })

  let SendTestFoundThrowsWithNull = execute (fun stream writer -> test {
    let testSink = RemoteTestDiscoverySink(writer)
    let! e = trap { it (testSink.SendTestFound(null)) }
    do! assertEquals typeof<ArgumentNullException> (e.GetType())
  })

module RemoteTestExecutionSinkTest =

  let SendTestStarted = execute (fun stream writer -> test {
    let testSink = RemoteTestExecutionSink(writer)
    let t = createMockTest()
    testSink.SendTestStarted(t)
    let! msg = getMessage stream
    do! assertEquals Messages.TestStarted msg.MessageType
    let! payload = getPayload<Test> msg
    do! assertTestEqual t payload
  })

  let SendTestStartedThrowsWithNull = execute (fun stream writer -> test {
    let testSink = RemoteTestExecutionSink(writer)
    let! e = trap { it (testSink.SendTestStarted(null)) }
    do! assertEquals typeof<ArgumentNullException> (e.GetType())
  })

  let SendTestResult = execute (fun stream writer -> test {
    let testSink = RemoteTestExecutionSink(writer)
    let result = createMockTestResult()
    testSink.SendTestResult(result)
    let! msg = getMessage stream
    do! assertEquals Messages.TestResult msg.MessageType
    let! payload = getPayload<TestResult> msg
    do! assertTestResultEqual result payload
  })

  let SendTestResultThrowsWithNull = execute (fun stream writer -> test {
    let testSink = RemoteTestExecutionSink(writer)
    let! e = trap { it (testSink.SendTestResult(null)) }
    do! assertEquals typeof<ArgumentNullException> (e.GetType())
  })
