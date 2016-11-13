namespace Persimmon.Runner
{
    public class Program
    {
        public static int Main(string[] args)
        {
            using(var runner = new TestRunner())
            {
                return runner.Run(args);
            }
        }
    }
}
