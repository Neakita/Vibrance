using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using Vibrance.Changes;

namespace Vibrance.Benchmarks;

[EventPipeProfiler(EventPipeProfile.CpuSampling)]
public class SelectManyBenchmark
{
	[IterationSetup(Target = nameof(VibranceComplexChanges))]
	public void VibranceSetup()
	{
		_vibranceSubscription?.Dispose();
		_vibranceSourceList = new ObservableList<ObservableList<int>>();
		_vibranceSubscription = _vibranceSourceList
			.Transform(number => number.ToString())
			.ToObservableList();
	}

	[Benchmark]
	public void VibranceComplexChanges()
	{
		_vibranceSourceList.Add(new ObservableList<int>(Enumerable.Range(0, 100000)));
		_vibranceSourceList.Add(new ObservableList<int>(Enumerable.Range(0, 100000)));
		_vibranceSourceList.AddRange(new ObservableList<int>(Enumerable.Range(0, 100000)), new ObservableList<int>(Enumerable.Range(0, 100000)));
		var innerList = _vibranceSourceList[3565 % _vibranceSourceList.Count];
		innerList.AddRange(Enumerable.Range(0, 1000));
		innerList.RemoveRange(200, 200);
		innerList[1] = 346;
		_vibranceSourceList[3] = new ObservableList<int>(Enumerable.Range(0, 1000));
		_vibranceSourceList.RemoveRange(0, 2);
		_vibranceSourceList.AddRange(Enumerable.Range(0, 10000).Select(_ => new ObservableList<int>(Enumerable.Range(0, 100000))));
		_vibranceSourceList.RemoveRange(2000, 2000);
		innerList = _vibranceSourceList[645212 % _vibranceSourceList.Count];
		innerList.AddRange(Enumerable.Range(0, 1000));
		innerList = _vibranceSourceList[333 % _vibranceSourceList.Count];
		innerList.Clear();
	}

	private ObservableList<ObservableList<int>>? _vibranceSourceList;
	private IDisposable? _vibranceSubscription;
}