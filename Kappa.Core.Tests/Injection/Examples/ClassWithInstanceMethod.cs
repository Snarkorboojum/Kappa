using System;
using System.Runtime.CompilerServices;

namespace Kappa.Core.Tests.Injection.Examples
{
	/// <summary>
	/// The example of the class with instance method in it.
	/// </summary>
	public sealed class ClassWithInstanceMethod
	{
		private String _propertyBackingField;

		public ClassWithInstanceMethod(String text)
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