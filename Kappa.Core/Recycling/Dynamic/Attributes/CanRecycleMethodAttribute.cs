using System;

namespace Kappa.Core.Recycling.Dynamic.Attributes
{
	/// <summary>
	/// The class of the attribute that specified method in dynamic recyclable object that provides value for CanRecycle property.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public sealed class CanRecycleMethodAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CanRecycleMethodAttribute"/> class.
		/// </summary>
		/// <param name="ignoreBaseMethods">The <see cref="Boolean"/> value indicating that method with this attribute must ignore all methods with this attribute in base classes.</param>
		public CanRecycleMethodAttribute(Boolean ignoreBaseMethods = false)
		{
			IgnoreBaseMethods = ignoreBaseMethods;
		}

		/// <summary>
		/// Gets the <see cref="Boolean"/> value indicating that method with this attribute must ignore all methods with this attribute in base classes.
		/// </summary>
		public Boolean IgnoreBaseMethods { get; private set; }
	}
}