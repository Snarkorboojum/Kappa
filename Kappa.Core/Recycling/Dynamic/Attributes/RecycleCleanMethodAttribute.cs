using System;

namespace Kappa.Core.Recycling.Dynamic.Attributes
{
	/// <summary>
	/// Provides functionality to specify the method of the object in the field or property to "cleans up" that object.
	/// </summary>
	public sealed class RecycleCleanMethodAttribute : RecycleCleanAttribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="RecycleCleanMethodAttribute"/> class.
		/// </summary>
		/// <param name="methodName">A name of the "cleans up" method of the object in field or property.</param>
		public RecycleCleanMethodAttribute(String methodName)
		{
			MethodName = methodName;
		}

		/// <summary>
		/// Gets the name of the "cleans up" method of the object in field or property.
		/// </summary>
		public String MethodName { get; private set; }
	}
}