using System;
using Kappa.Core.Recycling.Dynamic.Strategies;

namespace Kappa.Core.Recycling.Dynamic.Attributes
{
	/// <summary>
	/// Provides functionality to mark other classes (in metadata) as recyclable.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public sealed class RecyclableAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="RecyclableAttribute"/> class.
		/// </summary>
		/// <param name="strategy">A <see cref="RecyclingStrategyType"/> value that indicating which fields and properties must be used in recycling procedure.</param>
		public RecyclableAttribute(RecyclingStrategyType strategy)
		{
			Strategy = strategy;
		}

		/// <summary>
		/// Gets the <see cref="RecyclingStrategyType"/> value that indicating to recycling procedure which fields and properties must be used in it.
		/// </summary>
		public RecyclingStrategyType Strategy { get; private set; }
	}
}