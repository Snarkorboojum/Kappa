using System;

namespace Kappa.Core.Recycling.Dynamic.Attributes
{
	/// <summary>
	/// Provides functionality to specifies how member of the source type will be cleaned.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
	public abstract class RecycleCleanAttribute : Attribute
	{
	}
}