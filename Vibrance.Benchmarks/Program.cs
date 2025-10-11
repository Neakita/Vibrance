using BenchmarkDotNet.Running;

namespace Vibrance.Benchmarks;

internal static class Program
{
	private static void Main(string[] args)
	{
		BenchmarkRunner.Run<SelectManyBenchmark>(args: args);
	}
}