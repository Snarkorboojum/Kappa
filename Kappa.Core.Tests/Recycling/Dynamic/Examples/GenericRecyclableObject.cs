using Kappa.Core.Recycling.Dynamic.Attributes;
using Kappa.Core.Recycling.Dynamic.Strategies;

namespace Kappa.Core.Tests.Recycling.Dynamic.Examples
{
	/// <summary>
	/// The example of the generic dynamically recyclable class.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[Recyclable(RecyclingStrategyType.Include)]
	public class GenericRecyclableObject<T>
	{
	}
}