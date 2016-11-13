using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Persimmon.Runner
{
    public class PersimmonDriver
    {
        private readonly Assembly library;
        private readonly Assembly target;

        public PersimmonDriver(Assembly library, Assembly target)
        {
            this.library = library;
            this.target = target;
        }

        public IEnumerable<TestCaseWrapper> CollectTests()
        {
            var collector = CreateObject("Persimmon.Internals.TestCollector", library);
            var tests = ExecuteMethod(collector, "CollectOnlyTestCases") as IEnumerable<object>;
            return tests.Select(t => new TestCaseWrapper(t));
        }

        public void RunTests(string[] fullyQualifiedTestNames, Action<object> before, Action<object> callback)
        {
            var collector = CreateObject("Persimmon.Internals.TestRunner", library);
            ExecuteMethod(collector, "RunTestsAndCallback", target, fullyQualifiedTestNames, before, callback);
        }

        private static object CreateObject(string typeName, Assembly library, params object[] args)
        {
            var typeinfo = library.DefinedTypes.FirstOrDefault(t => t.FullName == typeName);
            if (typeinfo == null)
            {
                return null;
            }
            return Activator.CreateInstance(typeinfo.AsType(), args);
        }

        private object ExecuteMethod(object instance, string methodName, params object[] args)
        {
            var method = instance.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
            return ExecuteMethod(instance, method, args);
        }

        private object ExecuteMethod(object instance, MethodInfo method, params object[] args)
        {
            if (method == null)
            {
                throw new Exception("oops!");
            }
            return method.Invoke(instance, args);
        }
    }
}
