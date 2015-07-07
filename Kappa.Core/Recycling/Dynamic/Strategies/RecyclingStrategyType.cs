using Kappa.Core.Recycling.Dynamic.Attributes;

namespace Kappa.Core.Recycling.Dynamic.Strategies
{
	/// <summary>
	/// Describes the recycling strategy for instance of class marked with <see cref="RecyclableAttribute"/>.
	/// </summary>
	public enum RecyclingStrategyType
	{
		/// <summary>
		/// Signals that recycling procedure must use only fields and properties marked with <see cref="RecycleAttribute"/>.
		/// </summary>
		Include,

		/// <summary>
		/// Signals that recycling procedure must use all fields which not marked by <see cref="RecycleIgnoreAttribute"/> and whose properties(in case of backing field) not marked by that attribute.
		/// </summary>
		Exclude
	}
}