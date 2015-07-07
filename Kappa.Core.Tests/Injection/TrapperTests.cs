using System;
using Eco.Recycling;
using Kappa.Core.Injection;
using Kappa.Core.Tests.Injection.Examples;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kappa.Core.Tests.Injection
{
	/// <summary>
	/// Provides the methods that tests the entrapment process of the type that implements <see cref="IRecyclable"/> interface.
	/// </summary>
	[TestClass]
	public class TrapperTests
	{
		/// <summary>
		/// Test the entrapment of the type that implements the <see cref="IRecyclable"/> interface.
		/// </summary>
		/// <remarks>The test must replace all public method of the specified type to prevent using it when <see cref="IRecyclable.IsInFactory"/> is true.</remarks>
		[TestMethod]
		public void InjectionIRecyclableEntrapmentTest()
		{
			const String validationMessage = "Access denied. Object is returned to the factory.";

			Trapper.Trap(typeof(RecyclableObject), new RecyclableValidationCodeProvider(validationMessage));
			var factory = new DefaultRecycleFactory<RecyclableObject>(StoragePolicy.NonConcurrent);

			var recyclable = factory.Create();
			recyclable.Free();

			try
			{
				Boolean result;
				recyclable.Method("", 3.14159, out result);
				Assert.Fail();
			}
			catch (InvalidOperationException exception)
			{
				if (exception.Message != validationMessage)
					throw;
			}
		}
	}
}
