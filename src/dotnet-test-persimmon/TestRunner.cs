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

// port from https://github.com/nunit/dotnet-test-nunit/blob/18be0f2d2e367618408cd495aa696514765775e3/src/dotnet-test-nunit/TestRunner.cs

using System;
using Microsoft.Extensions.Testing.Abstractions;
using System.Reflection;
using System.Linq;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Net;
using System.IO;
using Persimmon.Runner.Sinks;
using Newtonsoft.Json;

namespace Persimmon.Runner
{
    public class TestRunner : IDisposable
    {
        private ITestDiscoverySink testDiscoverySink;
        private ITestExecutionSink testExecutionSink;
        private Socket socket;

        public TestRunner() { }

        public int Run(string[] args)
        {
            try
            {
                return Execute(Args.Parse(args));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return -1;
            }
        }

        int Execute(Args args)
        {
            SetupSinks(args);

            var results = new List<TestResultWrapper>();

            foreach(var assembly in args.Inputs)
            {
                var assemblyPath = Path.GetFullPath(assembly);
                var testAssembly = LoadAssembly(assemblyPath);
                var libraryPath = Path.Combine(Path.GetDirectoryName(assemblyPath), "Persimmon.dll");
                var library = LoadAssembly(libraryPath);

                var driver = new PersimmonDriver(library, testAssembly);
                var tests = driver.CollectTests();

                if (args.List)
                {
                    foreach (var test in tests) testDiscoverySink.SendTestFound(test.Test);
                }
                else
                {
                    Action<object> before = testCase =>
                    {
                        if (args.DesignTime)
                        {
                            testExecutionSink.SendTestStarted(new TestCaseWrapper(testCase).Test);
                        }
                    };
                    Action<object> progress = result =>
                    {
                        var wrapper = new TestResultWrapper(result);
                        if (args.DesignTime)
                        {
                            testExecutionSink.SendTestResult(wrapper.TestResult);
                        }
                        else
                        {
                            switch (wrapper.TestResult.Outcome)
                            {
                                case TestOutcome.Passed:
                                    Console.Write(".");
                                    break;
                                case TestOutcome.Failed:
                                    if (wrapper.Exceptions.Any()) Console.Write("E");
                                    else Console.Write("x");
                                    break;
                                case TestOutcome.Skipped:
                                    Console.Write("_");
                                    break;
                                default:
                                    break;
                            }
                        }
                        results.Add(wrapper);
                    };
                    driver.RunTests(tests.Select(t => t.Test.FullyQualifiedName).ToArray(), before, progress);
                }
            }

            if (args.List)
            {
                if (args.DesignTime) testDiscoverySink.SendTestCompleted();
                return 0;
            }

            if (args.DesignTime)
            {
                testExecutionSink.SendTestCompleted();
                return 0;
            }

            using (var reporter = new Reporter(results, Console.Out))
            {
                reporter.Report();
            }

            return results.Where(r => r.TestResult.Outcome == TestOutcome.Failed).Count();
        }

        public void Dispose()
        {
            socket?.Dispose();
        }

        Assembly LoadAssembly(string fileName)
        {
#if NET451
            var assemblyRef = AssemblyName.GetAssemblyName(fileName);
            return Assembly.Load(assemblyRef);
#else
            var assemblyName = System.IO.Path.GetFileNameWithoutExtension(fileName);
            return Assembly.Load(new AssemblyName(assemblyName));
#endif
        }

        IEnumerable<string> SetupSinks(Args args)
        {
            IEnumerable<string> testList = Enumerable.Empty<string>();

            if (args.PortSpecified)
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(new IPEndPoint(IPAddress.Loopback, args.Port));
                var networkStream = new NetworkStream(socket);

                SetupRemoteTestSinks(networkStream);

                if (args.WaitCommand)
                {
                    var reader = new BinaryReader(networkStream);
                    testExecutionSink.SendWaitingCommand();

                    var rawMessage = reader.ReadString();
                    var message = JsonConvert.DeserializeObject<Message>(rawMessage);

                    testList = message.Payload.ToObject<RunTestsMessage>().Tests;
                }
            }
            else
            {
                SetupConsoleTestSinks();
            }
            return testList;
        }

        void SetupRemoteTestSinks(Stream stream)
        {
            var binaryWriter = new BinaryWriter(stream);
            testDiscoverySink = new RemoteTestDiscoverySink(binaryWriter);
            testExecutionSink = new RemoteTestExecutionSink(binaryWriter);
        }
        void SetupConsoleTestSinks()
        {
            testDiscoverySink = new StreamingTestDiscoverySink(Console.OpenStandardOutput());
            testExecutionSink = new StreamingTestExecutionSink(Console.OpenStandardOutput());
        }
    }
}
