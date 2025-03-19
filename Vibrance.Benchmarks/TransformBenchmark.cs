using BenchmarkDotNet.Attributes;
using DynamicData;
using Vibrance.Changes;

namespace Vibrance.Benchmarks;

public class TransformBenchmark
{
	[Params(10, 1_000, 10_000, 100_000)]
	public int Quantity;

	[GlobalSetup]
	public void Setup()
	{
		_itemsToAdd = Enumerable.Range(0, Quantity).ToList();
	}

	[IterationSetup(Target = nameof(VibranceTransformAdd))]
	public void VibranceSetup()
	{
		_vibranceSubscription?.Dispose();
		_vibranceSourceList = new ObservableList<int>();
		_vibranceSubscription = _vibranceSourceList
			.Transform(number => number.ToString())
			.ToObservableList();
	}

	[Benchmark]
	public void VibranceTransformAdd()
	{
		_vibranceSourceList!.AddRange(_itemsToAdd!);
	}

	[IterationSetup(Target = nameof(DynamicDataTransformAdd))]
	public void DynamicDataSetup()
	{
		_dynamicDataSubscription?.Dispose();
		_dynamicDataSourceList?.Dispose();
		_dynamicDataSourceList = new SourceList<int>();
		_dynamicDataSubscription = _dynamicDataSourceList
			.Connect()
			.Transform(number => number.ToString())
			.Bind(out _)
			.Subscribe();
	}

	[Benchmark]
	public void DynamicDataTransformAdd()
	{
		_dynamicDataSourceList!.AddRange(_itemsToAdd!);
	}

	private List<int>? _itemsToAdd;
	private ObservableList<int>? _vibranceSourceList;
	private IDisposable? _vibranceSubscription;
	private SourceList<int>? _dynamicDataSourceList;
	private IDisposable? _dynamicDataSubscription;
}