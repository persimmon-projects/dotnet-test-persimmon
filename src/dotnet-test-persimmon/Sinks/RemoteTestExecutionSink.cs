// ***********************************************************************
// Copyright (c) 2016 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;
using System.IO;
using Microsoft.Extensions.Testing.Abstractions;
using System.Collections.Concurrent;

namespace Persimmon.Runner.Sinks
{
    public class RemoteTestExecutionSink : RemoteTestSink, ITestExecutionSink
    {
        private readonly ConcurrentDictionary<string, DateTimeOffset> runningTests;

        public RemoteTestExecutionSink(BinaryWriter binaryWriter) : base(binaryWriter)
        {
            runningTests = new ConcurrentDictionary<string, DateTimeOffset>();
        }

        public void SendTestStarted(Test test)
        {
            if (test == null) throw new ArgumentNullException(nameof(test));
            runningTests.TryAdd(test.FullyQualifiedName, DateTimeOffset.Now);
            SendMessage(Messages.TestStarted, test);
        }

        public void SendTestResult(TestResult testResult)
        {
            if (testResult == null) throw new ArgumentNullException(nameof(testResult));

            DateTimeOffset startTime;
            runningTests.TryRemove(testResult.Test.FullyQualifiedName, out startTime);
            if(testResult.StartTime == default(DateTimeOffset))
            {
                testResult.StartTime = startTime;
            }
            if(testResult.EndTime == default(DateTimeOffset))
            {
                testResult.EndTime = DateTimeOffset.Now;
            }

            SendMessage(Messages.TestResult, testResult);
        }
    }
}
