using System.Collections;
using NUnit.Framework;

namespace EventApi.Implementation.UnitTests.TestCases
{
    class EventApiTestCases
    {
        public static IEnumerable TestCases
        {
            get
            {
                yield return new TestCaseData(130, 140, new int[0])
                {
                    TestName = "NoEvents_middle"
                };
                yield return new TestCaseData(1000, 1200, new int[0])
                {
                    TestName = "NoEvents_right"
                };
                yield return new TestCaseData(0, 10, new int[0])
                {
                    TestName = "NoEvents_left"
                };
                yield return new TestCaseData(0, 1000, new[] {0, 1, 2, 3, 4, 5})
                {
                    TestName = "GetAllEvents"
                };
                yield return new TestCaseData(0, 80, new[] { 0, 1 })
                {
                    TestName = "GetOneInterval_First"
                };
                yield return new TestCaseData(90, 130, new[] { 2, 3 })
                {
                    TestName = "GetOneInterval_Second"
                };
                yield return new TestCaseData(140, 260, new[] { 4, 5 })
                {
                    TestName = "GetOneInterval_Third"
                };
                yield return new TestCaseData(0, 130, new[] { 0, 1, 2, 3 })
                {
                    TestName = "GetTwoInterval_First"
                };
                yield return new TestCaseData(90, 260, new[] { 2, 3, 4, 5 })
                {
                    TestName = "GetTwoInterval_Second"
                };
                yield return new TestCaseData(60, 65, new[] { 0, 1 })
                {
                    TestName = "SearchingIntervalInsideEventInterval_First"
                };
                yield return new TestCaseData(110, 115, new[] { 2, 3 })
                {
                    TestName = "SearchingIntervalInsideEventInterval_Second"
                };
                yield return new TestCaseData(160, 180, new[] { 4, 5 })
                {
                    TestName = "SearchingIntervalInsideEventInterval_Third"
                };
                yield return new TestCaseData(40, 60, new[] { 0, 1 })
                {
                    TestName = "LeftIntersectionWithOneInterval_First"
                };
                yield return new TestCaseData(90, 110, new[] { 2, 3 })
                {
                    TestName = "LeftIntersectionWithOneInterval_Second"
                };
                yield return new TestCaseData(140, 180, new[] { 4, 5 })
                {
                    TestName = "LeftIntersectionWithOneInterval_Third"
                };
                yield return new TestCaseData(60, 80, new[] { 0, 1 })
                {
                    TestName = "RightIntersectionWithOneInterval_First"
                };
                yield return new TestCaseData(110, 130, new[] { 2, 3 })
                {
                    TestName = "RightIntersectionWithOneInterval_Second"
                };
                yield return new TestCaseData(180, 220, new[] { 4, 5 })
                {
                    TestName = "RightIntersectionWithOneInterval_Third"
                };

                yield return new TestCaseData(60, 110, new[] { 0,1, 2, 3 })
                {
                    TestName = "IntersectionWithTwoInterval_First"
                };
                yield return new TestCaseData(110, 180, new[] { 2,3, 4, 5 })
                {
                    TestName = "IntersectionWithTwoInterval_Second"
                };

                yield return new TestCaseData(60, 180, new[] { 0,1, 2, 3, 4, 5 })
                {
                    TestName = "IntersectionWithAllInterval"
                };
            }
        }
    }
}
