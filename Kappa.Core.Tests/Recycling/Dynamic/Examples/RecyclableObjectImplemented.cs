using System;
using Eco.Recycling;
using Kappa.Core.Recycling.Dynamic.Attributes;
using Kappa.Core.Recycling.Dynamic.Strategies;

namespace Kappa.Core.Tests.Recycling.Dynamic.Examples
{
	/// <summary>
	/// Provides example of recyclable class that already implements <see cref="IRecyclable"/> interface for the <see cref="RecyclingGeneratorTests"/>.
	/// </summary>
	[Recyclable(RecyclingStrategyType.Exclude)]
	public class RecyclableObjectImplemented : IRecyclable
	{
		[RecycleIgnore]
		private Boolean _isInFactory;

		[RecycleIgnore]
		private IRecycleFactory _sourceFactory;

		[RecycleIgnore]
		public IRecycleFactory SourceFactory
		{
			get { return _sourceFactory; }
		}

		[RecycleIgnore]
		public Boolean IsInFactory
		{
			get { return _isInFactory; }
		}

		[RecycleIgnore]
		public Boolean? SomeProperty1 { get; set; }

		[RecycleCleanValue(false)]
		public Boolean? SomeProperty2 { get; set; }

		public void Resolve()
		{
			_isInFactory = false;
			SomeProperty1 = false;
		}

		public void Cleanup()
		{
			_isInFactory = true;
			SomeProperty1 = true;
		}

		public void SetSourceFactory(IRecycleFactory factory)
		{
			_sourceFactory = factory;
		}
	}
}