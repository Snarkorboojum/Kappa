using Kappa.Core.Recycling.Dynamic.Attributes;
using Kappa.Core.Recycling.Dynamic.Strategies;

namespace Kappa.Core.Tests.Recycling.Dynamic.Examples
{
	/// <summary>
	/// Provides example of the recyclable class with cleanup and resolve methods that nested from recyclable class for the <see cref="RecyclingGeneratorTests"/>.
	/// </summary>
	[Recyclable(RecyclingStrategyType.Include)]
	public class NestedRecyclableObjectWithMethods : RecyclableObjectWithMethods
	{
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