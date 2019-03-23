using System.Collections;
using NUnit.Framework;

namespace Generators.UnitTests.TestCases
{
    public class Wrapper
    {
        public object[] Params { get; set; }
    }
    internal class MmfTestCases
    {
        public static IEnumerable BadArgumentsTestCase
        {
            get
            {
                yield return new TestCaseData(7L, new Wrapper{Params = new object[] {"Normal.bin"}})
                {
                    TestName = "Odd  collection size"
                };

                yield return new TestCaseData(8L, new Wrapper())
                {
                    TestName = "Null parameters"
                };

                yield return new TestCaseData(8L, new Wrapper {Params = new object[] { "Normal.bin", "And something else" }})
                {
                    TestName = "Not single parameter"
                };

                yield return new TestCaseData(8L, new Wrapper { Params = new object[] { "" }})
                {
                    TestName = "Empty file path"
                };
                yield return new TestCaseData(8L, new Wrapper { Params = new object[] { null }})
                {
                    TestName = "Null file path"
                };
            }
        }
    }
}
