using System;
using Kappa.Core.Recycling.Dynamic.Attributes;
using Kappa.Core.Recycling.Dynamic.Strategies;

namespace Kappa.Core.Tests.Recycling.Dynamic.Examples
{
	/// <summary>
	/// Provides example of the base recyclable class with cleanup and resolve methods for the <see cref="RecyclingGeneratorTests"/>.
	/// </summary>
	[Recyclable(RecyclingStrategyType.Include)]
	public class RecyclableObjectWithMethods
	{
		public Int32 RecycleCounter { get; set; }

		[RecycleResolve]
		private void Resolve()
		{
			RecycleCounter++;
		}

		[RecycleCleanup]
		private void Cleanup()
		{
			RecycleCounter++;
		}
	}
}