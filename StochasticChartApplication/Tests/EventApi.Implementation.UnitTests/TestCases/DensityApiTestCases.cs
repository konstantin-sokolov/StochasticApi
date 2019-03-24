using System.Collections;
using System.Collections.Generic;
using EventApi.Models;
using NUnit.Framework;

namespace EventApi.Implementation.UnitTests.TestCases
{
    class DensityApiTestCases
    {
        public static IEnumerable TestCases
        {
            get
            {
                yield return new TestCaseData(30, 300, 5, new List<DensityInfo>
                {
                    new DensityInfo()
                    {
                        EventsCount = 2,
                        Start = 50,
                        Stop = 70,
                        StartIndex = 0,
                        StopIndex = 1,
                    },
                    new DensityInfo()
                    {
                        EventsCount = 2,
                        Start = 100,
                        Stop = 120,
                        StartIndex = 2,
                        StopIndex = 3,
                    },
                    new DensityInfo()
                    {
                        EventsCount = 2,
                        Start = 150,
                        Stop = 200,
                        StartIndex = 4,
                        StopIndex = 5
                    }
                })
                {
                    TestName = "Little group interval. Interval save their sizes"
                };

                yield return new TestCaseData(25, 300, 60, new List<DensityInfo>
                {
                    new DensityInfo()
                    {
                        EventsCount = 4,
                        Start = 50,
                        Stop = 120,
                        StartIndex = 0,
                        StopIndex = 3
                    },
                    new DensityInfo()
                    {
                        EventsCount = 2,
                        Start = 150,
                        Stop = 200,
                        StartIndex = 4,
                        StopIndex = 5
                    }
                })
                {
                    TestName = "Get all intervals in different densities first intervals"
                };

                yield return new TestCaseData(0, 110, 20, new List<DensityInfo>
                {
                    new DensityInfo()
                    {
                        EventsCount = 2,
                        Start = 50,
                        Stop = 70,
                        StartIndex = 0,
                        StopIndex = 1
                    },
                    new DensityInfo()
                    {
                        EventsCount = 2,
                        Start = 100,
                        Stop = 120,
                        StartIndex = 2,
                        StopIndex = 3
                    }
                })
                {
                    TestName = "Get two first intervals"
                };

                yield return new TestCaseData(0, 1000, 1000, new List<DensityInfo>
                {
                    new DensityInfo()
                    {
                        EventsCount = 6,
                        Start = 50,
                        Stop = 200,
                        StartIndex = 0,
                        StopIndex = 5
                    }
                })
                {
                    TestName = "All events inside group interval"
                };
                yield return new TestCaseData(50, 200, 150, new List<DensityInfo>
                {
                    new DensityInfo()
                    {
                        EventsCount = 6,
                        Start = 50,
                        Stop = 200,
                        StartIndex = 0,
                        StopIndex = 5
                    }
                })
                {
                    TestName = "All events inside group interval(edges)"
                };
            }
        }
    }
}
