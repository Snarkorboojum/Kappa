using System;
using Eco.Recycling;
using Kappa.Core.Recycling.Dynamic;
using Kappa.Core.Reflection.Emit;
using Kappa.Core.Tests.Recycling.Dynamic.Examples;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kappa.Core.Tests.Recycling.Dynamic
{
	/// <summary>
	/// Provides the test methods for dynamic recycling factory.
	/// </summary>
	[TestClass]
	public sealed class DynamicRecycleFactoryTests
	{
		/// <summary>
		/// Tests the ability of generic factory to recycle instance of type that implements <see cref="IRecyclable"/> interface.
		/// </summary>
		[TestMethod]
		public void DynamicGenericRecycleFactorySimpleRecyclable()
		{
			var recycleFactory = new DynamicRecycleFactory<SimpleRecyclableClass1>();
			var recyclable = recycleFactory.Create();

			Assert.IsInstanceOfType(recyclable, typeof(SimpleRecyclableClass1));
			recyclable.Free();
			Assert.IsTrue(((IRecyclable)recyclable).IsInFactory);
		}

		/// <summary>
		/// Tests the ability of non-generic factory to recycle instance of type that implements <see cref="IRecyclable"/> interface.
		/// </summary>
		[TestMethod]
		public void DynamicRecycleFactorySimpleRecyclable()
		{
			var recycleFactory = new DynamicRecycleFactory(typeof(SimpleRecyclableClass1));
			var recyclable = recycleFactory.Create();

			Assert.IsInstanceOfType(recyclable, typeof(SimpleRecyclableClass1));
			recyclable.Free();
			Assert.IsTrue(recyclable.IsInFactory);
		}

		/// <summary>
		/// Tests the ability of generic factory to recycle empty object.
		/// </summary>
		[TestMethod]
		public void DynamicGenericRecycleFactoryEmptyObject()
		{
			var recycleFactory = new DynamicRecycleFactory<RecyclableObject>();
			var recyclable = recycleFactory.Create();

			Assert.IsInstanceOfType(recyclable, typeof(RecyclableObject));
			Recyclable.TryFreeIfRecyclable(recyclable);
			Assert.IsTrue(((IRecyclable)recyclable).IsInFactory);
		}

		/// <summary>
		/// Tests the ability of non-generic factory to recycle empty object.
		/// </summary>
		[TestMethod]
		public void DynamicRecycleFactoryEmptyObject()
		{
			var recycleFactory = new DynamicRecycleFactory(typeof(RecyclableObject));
			var recyclable = recycleFactory.Create();

			Assert.IsInstanceOfType(recyclable, typeof(RecyclableObject));
			recyclable.Free();
			Assert.IsTrue(recyclable.IsInFactory);
		}

		/// <summary>
		/// Tests the functioning of the recycling storage.
		/// </summary>
		/// <remarks>This test verifies that two untyped factory with different types of items have two different thread static storages.</remarks>
		[TestMethod]
		public void DynamicRecycleFactoryStorageTest()
		{
			var factory1 = new DynamicRecycleFactory(typeof(SimpleRecyclableClass1));
			var factory2 = new DynamicRecycleFactory(typeof(SimpleRecyclableClass2));

			var item1 = factory1.Create();
			item1.Free();

			var item2 = factory2.Create();

			Assert.AreNotEqual(item1, item2);
		}

		/// <summary>
		/// Tests the runtime constrains on the type of items in non-generic dynamic recycle factory.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void DynamicRecycleFactoryWithWrongArgumentTest()
		{
			var factory = new DynamicRecycleFactory(typeof(Object));

			GC.KeepAlive(factory);
		}

		/// <summary>
		/// Tests the runtime constrains on the type of items in generic dynamic recycle factory.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void DynamicGenericRecycleFactoryWithWrongArgumentTest()
		{
			var dynamicFactoryType = typeof(DynamicRecycleFactory<>).MakeGenericType(typeof(Object));
			var dynamicFactoryConstructor = (Func<DynamicRecycleFactory<Object>>)ClassBuilder.GetConstructor(dynamicFactoryType);
			try
			{
				var factory = dynamicFactoryConstructor();

				GC.KeepAlive(factory);
			}
			catch (TypeInitializationException exception)
			{
				throw exception.InnerException;
			}
		}
	}
}