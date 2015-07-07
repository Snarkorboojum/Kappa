using System;

namespace Kappa.Core.Recycling.Dynamic.Attributes
{
	/// <summary>
	/// Provides functionality to mark fields and properties of other classes to hide them from recycling procedure.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event, AllowMultiple = false, Inherited = true)]
	public sealed class RecycleIgnoreAttribute : Attribute
	{
	}
}