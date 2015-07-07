using System;

namespace Kappa.Core.Recycling.Dynamic.Attributes
{
	/// <summary>
	/// Provides functionality to mark method in other class to call it when recycling procedure is getting instance of that class from recycle factory.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public sealed class RecycleResolveAttribute : Attribute
	{
	}
}