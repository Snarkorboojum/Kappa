using System;
using Eco.Recycling;
using Kappa.Core.Recycling.Dynamic.Attributes;
using Kappa.Core.Recycling.Dynamic.Strategies;

namespace Kappa.Core.Tests.Recycling.Dynamic.Examples
{
	/// <summary>
	/// Example of the class that override <see cref="IRecyclableExtended.CanRecycle"/> that was previously overriden.
	/// </summary>
	[Recyclable(RecyclingStrategyType.Include)]
	public class NestedExtendedRecyclableObjectThatIgnoresBaseCanRecycleMethods : ExtendedRecyclableObject
	{
		public Boolean StopRecyclingReally { get; set; }

		[CanRecycleMethod(true)]
		private Boolean CanRecyclingOverride()
		{
			return !StopRecyclingReally;
		}
	}
}