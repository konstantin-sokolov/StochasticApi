using BenchmarkDotNet.Running;

namespace PerformanceTests
{
    class Program
    {

        static void Main(string[] args)
        {
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
            //BenchmarkRunner.Run<ImageRenderPerfTest>();
            //BenchmarkRunner.Run<MmfGenerationPerfTest>();
            //BenchmarkRunner.Run<DensitiesCalculationPerfTest>();
            //BenchmarkRunner.Run<MmfReadPerfTest>();
        } 
    }
}
