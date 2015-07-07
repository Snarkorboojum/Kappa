using System;

namespace Kappa.Core.Recycling.Dynamic.Attributes
{
	/// <summary>
	/// Provides functionality to specifies the "clean" value for fields and properties of recyclable object.
	/// </summary>
	public sealed class RecycleCleanValueAttribute : RecycleCleanAttribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="RecycleCleanValueAttribute"/> class.
		/// </summary>
		/// <param name="value">A value of the object's field or property in recycled state.</param>
		public RecycleCleanValueAttribute(Object value)
		{
			Value = value;
		}

		/// <summary>
		/// Gets the value of the object's field or property in the recycled state.
		/// </summary>
		public Object Value { get; private set; }
	}
}