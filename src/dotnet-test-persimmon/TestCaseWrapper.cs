using Microsoft.Extensions.Testing.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Persimmon.Runner
{
    public class TestCaseWrapper
    {
        private readonly dynamic testCase;

        public TestCaseWrapper(object testCase)
        {
            this.testCase = testCase;
        }

        public Test Test
        {
            get
            {
                return new Test
                {
                    FullyQualifiedName = testCase.UniqueName,
                    DisplayName = testCase.DisplayName,
                };
            }
        }
    }
}
