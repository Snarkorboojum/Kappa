using System;

namespace Kappa.Core.Recycling.Dynamic.Attributes
{
	/// <summary>
	/// Provides functionality to mark fields and properties of other classes as recyclable to use it in recycling procedure.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event, AllowMultiple = false, Inherited = false)]
	public sealed class RecycleAttribute : Attribute
	{
	}
}