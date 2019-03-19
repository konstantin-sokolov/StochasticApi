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
                        Count = 2,
                        Start = 50,
                        Stop = 70
                    },
                    new DensityInfo()
                    {
                        Count = 2,
                        Start = 100,
                        Stop = 120
                    }
                    ,
                    new DensityInfo()
                    {
                        Count = 2,
                        Start = 150,
                        Stop = 200
                    }
                })
                {
                    TestName = "Little group interval. Interval save their sizes"
                };

                yield return new TestCaseData(25, 300, 60, new List<DensityInfo>
                {
                    new DensityInfo()
                    {
                        Count = 4,
                        Start = 50,
                        Stop = 120
                    },
                    new DensityInfo()
                    {
                        Count = 2,
                        Start = 150,
                        Stop = 200
                    }
                })
                {
                    TestName = "Get all intervals in different densities first intervals"
                };

                yield return new TestCaseData(0, 110, 20, new List<DensityInfo>
                {
                    new DensityInfo()
                    {
                        Count = 2,
                        Start = 50,
                        Stop = 70
                    },
                    new DensityInfo()
                    {
                        Count = 2,
                        Start = 100,
                        Stop = 120
                    }
                })
                {
                    TestName = "Get two first intervals"
                };

                yield return new TestCaseData(0, 1000, 1000, new List<DensityInfo>
                {
                    new DensityInfo()
                    {
                        Count = 6,
                        Start = 50,
                        Stop = 200
                    }
                })
                {
                    TestName = "All events inside group interval"
                };
                yield return new TestCaseData(50, 200, 150, new List<DensityInfo>
                {
                    new DensityInfo()
                    {
                        Count = 6,
                        Start = 50,
                        Stop = 200
                    }
                })
                {
                    TestName = "All events inside group interval(edges)"
                };
            }
        }
    }
}
