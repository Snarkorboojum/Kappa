using System;
using Eco.Recycling;
using Kappa.Core.Recycling.Dynamic.Attributes;
using Kappa.Core.Recycling.Dynamic.Strategies;

namespace Kappa.Core.Tests.Recycling.Dynamic.Examples
{
	/// <summary>
	/// The example of the class that override <see cref="IRecyclableExtended"/> functionality.
	/// </summary>
	[Recyclable(RecyclingStrategyType.Include)]
	public class ExtendedRecyclableObject
	{
		public Boolean StopRecycling { get; set; }

		[CanRecycleMethod]
		private Boolean CanRecyclingOverride()
		{
			return !StopRecycling;
		}
	}
}