using System;

namespace Kappa.Core.Recycling.Dynamic.Attributes
{
	/// <summary>
	/// Provides functionality to mark method in other class to call it when recycling procedure is putting instance of that class into recycle factory.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public sealed class RecycleCleanupAttribute : Attribute
	{
	}
}