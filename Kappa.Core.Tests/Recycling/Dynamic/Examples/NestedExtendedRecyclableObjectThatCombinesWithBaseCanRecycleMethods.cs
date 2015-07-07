using System;
using Eco.Recycling;
using Kappa.Core.Recycling.Dynamic.Attributes;
using Kappa.Core.Recycling.Dynamic.Strategies;

namespace Kappa.Core.Tests.Recycling.Dynamic.Examples
{
	/// <summary>
	/// Example of the class that override <see cref="IRecyclableExtended.CanRecycle"/> and combines it with previously overriden.
	/// </summary>
	[Recyclable(RecyclingStrategyType.Include)]
	public class NestedExtendedRecyclableObjectThatCombinesWithBaseCanRecycleMethods : ExtendedRecyclableObject
	{
		public Boolean StopRecyclingReally { get; set; }

		[CanRecycleMethod]
		private Boolean CanRecyclingOverride()
		{
			return !StopRecyclingReally;
		}
	}
}