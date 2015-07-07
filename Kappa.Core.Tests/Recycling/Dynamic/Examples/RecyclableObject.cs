using Kappa.Core.Recycling.Dynamic.Attributes;
using Kappa.Core.Recycling.Dynamic.Strategies;

namespace Kappa.Core.Tests.Recycling.Dynamic.Examples
{
	/// <summary>
	/// Provides example of empty recyclable class for the <see cref="RecyclingGeneratorTests"/>.
	/// </summary>
	[Recyclable(RecyclingStrategyType.Include)]
	public class RecyclableObject
	{
	}
}