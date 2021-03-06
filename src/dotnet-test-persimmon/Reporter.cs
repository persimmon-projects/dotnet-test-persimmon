﻿using Microsoft.Extensions.Testing.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Persimmon.Runner
{
    public class Reporter
    {
        private List<TestResultWrapper> results;
        private ColorConsoleWriter writer;

        public Reporter(List<TestResultWrapper> results, ColorConsoleWriter writer)
        {
            this.results = results;
            this.writer = writer;
        }

        public void Report()
        {
            var violated = 0;
            var error = 0;
            var skipped = 0;
            foreach (var result in results)
            {
                var r = result.TestResult;
                switch (r.Outcome)
                {
                    case TestOutcome.Passed:
                        break;
                    case TestOutcome.Failed:
                        if (result.Exceptions.Any()) error++;
                        else violated++;
                        PrintFailure(result);
                        break;
                    case TestOutcome.Skipped:
                        skipped++;
                        writer.WriteLine(ColorStyle.Warning, string.Format("Skipped: {0}", r.DisplayName));
                        break;
                }
                writer.WriteLine();
            }

            writer.WriteLine(
                string.Format(
                    "run: {0}, error: {1}, violated: {2}, skipped: {3}",
                    results.Count(),
                    error, violated,
                    skipped
                )
            );
        }

        private void PrintFailure(TestResultWrapper result)
        {
            var r = result.TestResult;
            if (result.Exceptions.Any())
            {
                writer.WriteLine(ColorStyle.Error, string.Format("FATAL ERROR: {0}", r.DisplayName));
            }
            else
            {
                writer.WriteLine(ColorStyle.Failure, string.Format("Violated: {0}", r.DisplayName));
            }
            foreach(var msg in r.Messages)
            {
                writer.WriteLine(ColorStyle.Failure, msg);
            }
            writer.WriteLine();
            foreach(var e in result.Exceptions)
            {
                writer.WriteLine(ColorStyle.Error, e.StackTrace);
            }
        }
    }
}
