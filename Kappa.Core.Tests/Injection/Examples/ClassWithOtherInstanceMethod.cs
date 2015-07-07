using System;
using System.Runtime.CompilerServices;

namespace Kappa.Core.Tests.Injection.Examples
{
	/// <summary>
	/// The example of the class with instance method in it.
	/// </summary>
	/// <remarks>That is copy the <see cref="ClassWithInstanceMethod"/> for two different unit-test methods.</remarks>
	public sealed class ClassWithOtherInstanceMethod
	{
		private String _propertyBackingField;

		public ClassWithOtherInstanceMethod(String text)
		{
			_propertyBackingField = text;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public String InstanceMethod()
		{
			throw new InvalidOperationException(_propertyBackingField);
		}

		public String Property
		{
			get { return _propertyBackingField; }
		}
	}
}