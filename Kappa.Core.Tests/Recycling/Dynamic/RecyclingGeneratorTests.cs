using System;
using System.Diagnostics.CodeAnalysis;
using Eco.Recycling;
using Kappa.Core.Recycling.Dynamic;
using Kappa.Core.Tests.Recycling.Dynamic.Examples;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kappa.Core.Tests.Recycling.Dynamic
{
	/// <summary>
	/// Provides the test method for dynamic recycling system.
	/// </summary>
	[TestClass]
	public sealed class RecyclingGeneratorTests
	{
		/// <summary>
		/// The object that can generate nested types that implements <see cref="IRecyclable"/> interface.
		/// </summary>
		private static RecyclableTypeGenerator _recyclableGenerator;

		/// <summary>
		/// Creates recycling factory for the dynamically generated type.
		/// </summary>
		/// <param name="type">A type of objects that can be recycled.</param>
		/// <returns>The recycling factory for objects of specified <paramref name="type"/>.</returns>
		private static IRecycleFactory CreateDynamicRecycleFactory(Type type)
		{
			var dynamicType = _recyclableGenerator.GetRecyclable(type);
			var factoryType = typeof(DefaultRecycleFactory<>);
			factoryType = factoryType.MakeGenericType(dynamicType);

			var factory = Activator.CreateInstance(factoryType, StoragePolicy.Concurrent) as IRecycleFactory;

			return factory;
		}

		/// <summary>
		/// Initializes a test assembly.
		/// </summary>
		/// <param name="testContext">The object that contains context information of the unit test.</param>
		[AssemblyInitialize]
		public static void Initialize(TestContext testContext)
		{
			_recyclableGenerator = new RecyclableTypeGenerator();
		}

		#region Test methods

		/// <summary>
		/// Tests ability to create dynamic recyclable object from provided type.
		/// </summary>
		[TestMethod]
		public void DynamicRecyclingCreateRecyclableObject()
		{
			var recyclableObjectType = _recyclableGenerator.GetRecyclable(typeof(RecyclableObject));

			GC.KeepAlive(recyclableObjectType);
		}

		/// <summary>
		/// Tests the dynamic implementation of the <see cref="IRecyclable.IsInFactory"/> property.
		/// </summary>
		[TestMethod]
		public void DynamicRecyclingIsInFactory()
		{
			var factory = CreateDynamicRecycleFactory(typeof(RecyclableObject));
			var recycleObject = factory.Create<RecyclableObject>();
			Assert.AreEqual(false, (recycleObject as IRecyclable).IsInFactory);

			recycleObject.Release();
			Assert.AreEqual(true, (recycleObject as IRecyclable).IsInFactory);
		}

		/// <summary>
		/// Tests ability to cleans up private <see cref="Boolean"/> field in the dynamically recyclable object.
		/// </summary>
		[TestMethod]
		public void DynamicRecyclingCreateRecyclableObjectWithBooleanField()
		{
			var factory = CreateDynamicRecycleFactory(typeof(RecyclableObjectWithFields));
			var recycleObject = factory.Create<RecyclableObjectWithFields>();

			recycleObject.Release();
			Assert.IsTrue(recycleObject.SomeBoolean);
		}

		/// <summary>
		/// Tests ability to cleans up publiv <see cref="Boolean"/> property in the dynamically recyclable object.
		/// </summary>
		[TestMethod]
		public void DynamicRecyclingCreateRecyclableObjectWithBooleanProperty()
		{
			var factory = CreateDynamicRecycleFactory(typeof(RecyclableObjectWithProperties));
			var recycleObject = factory.Create<RecyclableObjectWithProperties>();

			recycleObject.Release();
			Assert.IsTrue(recycleObject.IsSomething);
		}

		/// <summary>
		/// Tests ability to cleans up private <see cref="Boolean"/> property in the dynamically recyclable object.
		/// </summary>
		[TestMethod]
		public void DynamicRecyclingCreateRecyclableObjectWithBooleanPrivateProperty()
		{
			var factory = CreateDynamicRecycleFactory(typeof(RecyclableObjectWithProperties));
			var recycleObject = factory.Create<RecyclableObjectWithProperties>();

			recycleObject.Release();
			Assert.IsTrue(recycleObject._isSomethingHappened);
		}

		/// <summary>
		/// Tests ability to cleans up private reference type field in the dynamically recyclable object.
		/// </summary>
		[TestMethod]
		public void DynamicRecyclingCreateRecyclableObjectWithStringField()
		{
			var factory = CreateDynamicRecycleFactory(typeof(RecyclableObjectWithFields));
			var recycleObject = factory.Create<RecyclableObjectWithFields>();

			recycleObject.Release();
			Assert.AreEqual("This is the empty string", recycleObject.SomeString);
		}

		/// <summary>
		/// Tests ability to cleans up private event in the dynamically recyclable object.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(NullReferenceException))]
		public void DynamicRecyclingCreateRecyclableObjectWithEvent()
		{
			var factory = CreateDynamicRecycleFactory(typeof(RecyclableObjectWithFields));
			var recycleObject = factory.Create<RecyclableObjectWithFields>();

			recycleObject.Release();

			recycleObject.FireSomethingHappened();
		}

		/// <summary>
		/// Tests ability to cleans up private field by method in the dynamically recyclable object.
		/// </summary>
		[TestMethod]
		public void DynamicRecyclingCreateRecyclableObjectWithClearablePrivateField()
		{
			var factory = CreateDynamicRecycleFactory(typeof(RecyclableObjectWithClearableField));
			var recycleObject = factory.Create<RecyclableObjectWithClearableField>();

			recycleObject.SomePrivateList.Add(0);
			recycleObject.Release();
			Assert.AreEqual(0, recycleObject.SomePrivateList.Count);
		}

		/// <summary>
		/// Tests ability to cleans up public field by method in the dynamically recyclable object.
		/// </summary>
		[TestMethod]
		public void DynamicRecyclingCreateRecyclableObjectWithClearablePublicField()
		{
			var factory = CreateDynamicRecycleFactory(typeof(RecyclableObjectWithClearableField));
			var recycleObject = factory.Create<RecyclableObjectWithClearableField>();

			recycleObject._somePublicList.Add(0);
			recycleObject.Release();
			Assert.AreEqual(0, recycleObject._somePublicList.Count);
		}

		/// <summary>
		/// Tests ability to cleans up private property in the dynamically recyclable object.
		/// </summary>
		[TestMethod]
		public void DynamicRecyclingCreateRecyclableObjectWithClearablePrivateProperty()
		{
			var factory = CreateDynamicRecycleFactory(typeof(RecyclableObjectWithClearableProperty));
			var recycleObject = factory.Create<RecyclableObjectWithClearableProperty>();

			recycleObject._somePrivateList.Add(0);
			recycleObject.Release();
			Assert.AreEqual(0, recycleObject._somePrivateList.Count);
		}

		/// <summary>
		/// Tests ability to cleans up public property in the dynamically recyclable object.
		/// </summary>
		[TestMethod]
		public void DynamicRecyclingCreateRecyclableObjectWithClearablePublicProperty()
		{
			var factory = CreateDynamicRecycleFactory(typeof(RecyclableObjectWithClearableProperty));
			var recycleObject = factory.Create<RecyclableObjectWithClearableProperty>();

			recycleObject.SomePublicList.Add(0);
			recycleObject.Release();
			Assert.AreEqual(0, recycleObject.SomePublicList.Count);
		}

		/// <summary>
		/// Tests the attempt to recycle non-recyclable object.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void DynamicRecyclingAttemptToRecycleNonRecyclableObject()
		{
			var factory = CreateDynamicRecycleFactory(typeof(NestedNotRecyclableObject));
			GC.KeepAlive(factory);
		}

		/// <summary>
		/// Tests ability to cleans up private <see cref="Boolean"/> field in the nested dynamically recyclable object.
		/// </summary>
		[TestMethod]
		public void DynamicRecyclingCreateNestedRecyclableObjectWithBooleanField()
		{
			var factory = CreateDynamicRecycleFactory(typeof(NestedRecyclableObject));
			var recycleObject = factory.Create<NestedRecyclableObject>();

			recycleObject.Release();
			Assert.IsTrue(recycleObject.SomeBoolean);
		}

		/// <summary>
		/// Tests ability to cleans up private reference type field in the nested dynamically recyclable object.
		/// </summary>
		[TestMethod]
		public void DynamicRecyclingCreateNestedRecyclableObjectWithStringField()
		{
			var factory = CreateDynamicRecycleFactory(typeof(NestedRecyclableObject));
			var recycleObject = factory.Create<NestedRecyclableObject>();

			recycleObject.Release();
			Assert.AreEqual("This is the empty string", recycleObject.SomeString);
		}

		/// <summary>
		/// Tests ability to cleans up private event in the nested dynamically recyclable object.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(NullReferenceException))]
		public void DynamicRecyclingCreateNestedRecyclableObjectWithEvent()
		{
			var factory = CreateDynamicRecycleFactory(typeof(NestedRecyclableObject));
			var recycleObject = factory.Create<NestedRecyclableObject>();

			recycleObject.Release();

			recycleObject.FireSomethingHappened();
		}

		/// <summary>
		/// Tests ability to invoke methods marked by cleanup and resolve attribute in dynamically recyclable object.
		/// </summary>
		[TestMethod]
		public void DynamicRecyclingRecyclableObjectWithMethods()
		{
			var factory = CreateDynamicRecycleFactory(typeof(RecyclableObjectWithMethods));
			var recycleObject = factory.Create<RecyclableObjectWithMethods>();

			Assert.AreEqual(0, recycleObject.RecycleCounter);
			recycleObject.Release();
			Assert.AreEqual(1, recycleObject.RecycleCounter);
			recycleObject = factory.Create<RecyclableObjectWithMethods>();
			Assert.AreEqual(2, recycleObject.RecycleCounter);
		}

		/// <summary>
		/// Tests ability to invoke methods marked by cleanup and resolve attribute in nested dynamically recyclable object.
		/// </summary>
		[TestMethod]
		public void DynamicRecyclingNestedRecyclableObjectWithMethods()
		{
			var factory = CreateDynamicRecycleFactory(typeof(NestedRecyclableObjectWithMethods));
			var recycleObject = factory.Create<NestedRecyclableObjectWithMethods>();

			Assert.AreEqual(0, recycleObject.RecycleCounter);
			recycleObject.Release();
			Assert.AreEqual(2, recycleObject.RecycleCounter);
			recycleObject = factory.Create<NestedRecyclableObjectWithMethods>();
			Assert.AreEqual(4, recycleObject.RecycleCounter);
		}

		/// <summary>
		/// Tests ability to create recyclable object from type with private constructor.
		/// </summary>
		[TestMethod]
		public void DynamicRecyclingCreateRecyclableObjectWithPrivateConstructor()
		{
			var factory = CreateDynamicRecycleFactory(typeof(RecyclableObjectWithPrivateConstructor));
			var recycleObject = factory.Create<RecyclableObjectWithPrivateConstructor>();

			Assert.AreEqual(1, recycleObject.SomeProperty);
		}

		/// <summary>
		/// Tests ability to recycle object with <see cref="RecyclingStrategy.Exclude"/>.
		/// </summary>
		[TestMethod]
		public void DynamicRecyclingCreateRecyclableObjectWithExcludeStrategy()
		{
			var factory = CreateDynamicRecycleFactory(typeof(RecyclableObjectWithExcludeStrategy));
			var recycleObject = factory.Create<RecyclableObjectWithExcludeStrategy>();

			recycleObject.ClearableList.Add(0);

			recycleObject.Release();

			Assert.IsTrue(recycleObject.SomeBoolean);
			Assert.AreEqual("This is the empty string", recycleObject.SomeString);
			Assert.AreEqual(0, recycleObject.SomeAutoProperty);
			Assert.AreEqual(1, recycleObject.SomeAutoPropertyWithValue);
			Assert.IsNull(recycleObject.NullableList);
			Assert.IsNotNull(recycleObject.ClearableList);
			Assert.AreEqual(0, recycleObject.ClearableList.Count);
		}

		/// <summary>
		/// Tests ability to recycle object with <see cref="RecyclingStrategy.Exclude"/> with event.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(NullReferenceException))]
		public void DynamicRecyclingCreateRecyclableObjectWithExcludeStrategyWithEvent()
		{
			var factory = CreateDynamicRecycleFactory(typeof(RecyclableObjectWithExcludeStrategy));
			var recycleObject = factory.Create<RecyclableObjectWithExcludeStrategy>();

			recycleObject.ClearableList.Add(0);

			recycleObject.Release();

			recycleObject.FireSomethingHappened();
		}

		/// <summary>
		/// Tests ability to recycle object that already implements <see cref="IRecyclable"/> interface.
		/// </summary>
		[TestMethod]
		public void DynamicRecyclingCreateRecyclableObjectThatAlreadyImplementRecyclable()
		{
			var factory = CreateDynamicRecycleFactory(typeof(RecyclableObjectImplemented));
			var recycleObject = factory.Create<RecyclableObjectImplemented>();

			Assert.IsNull(recycleObject.SomeProperty1);
			Assert.IsNull(recycleObject.SomeProperty2);

			recycleObject.Free();
			Assert.IsTrue(recycleObject.SomeProperty1.Value);
			Assert.IsFalse(recycleObject.SomeProperty2.Value);

			recycleObject = factory.Create<RecyclableObjectImplemented>();
			Assert.IsFalse(recycleObject.SomeProperty1.Value);
			Assert.IsFalse(recycleObject.SomeProperty2.Value);
		}

		/// <summary>
		/// Test implementation of the <see cref="IRecyclableExtended"/> interface.
		/// </summary>
		[TestMethod]
		[SuppressMessage("StyleCopPlus.StyleCopPlusRules", "SP0100:AdvancedNamingRules")] // I want name of the interface in the name of the method.
		public void DynamicRecyclingTestIRecyclableExtendedImplementation()
		{
			var factory = CreateDynamicRecycleFactory(typeof(RecyclableObject));
			var recycleObject = factory.Create<RecyclableObject>();
			var recyclableExtended = recycleObject as IRecyclableExtended;

			Assert.IsNotNull(recyclableExtended);
			recyclableExtended.SuppressRecycling();
			Assert.IsFalse(recyclableExtended.CanRecycle);
			recyclableExtended.SuppressRecycling();
			Assert.IsFalse(recyclableExtended.CanRecycle);
			recyclableExtended.CheckAndFree();
			Assert.IsFalse(recyclableExtended.IsInFactory);
			recyclableExtended.ResumeRecycling();
			Assert.IsFalse(recyclableExtended.CanRecycle);
			recyclableExtended.ResumeRecycling();
			Assert.IsTrue(recyclableExtended.CanRecycle);
			recyclableExtended.CheckAndFree();
			Assert.IsTrue(recyclableExtended.IsInFactory);
		}

		/// <summary>
		/// Test implementation of the <see cref="IRecyclableExtended"/> interface.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		[SuppressMessage("StyleCopPlus.StyleCopPlusRules", "SP0100:AdvancedNamingRules")] // I want name of the interface in the name of the method.
		public void DynamicRecyclingTestIRecyclableExtendedWrongResume()
		{
			var factory = CreateDynamicRecycleFactory(typeof(RecyclableObject));
			var recycleObject = factory.Create<RecyclableObject>();
			var recyclableExtended = recycleObject as IRecyclableExtended;

			recyclableExtended.ResumeRecycling();
		}

		/// <summary>
		/// Test implementation of the <see cref="IRecyclableExtended"/> interface.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		[SuppressMessage("StyleCopPlus.StyleCopPlusRules", "SP0100:AdvancedNamingRules")] // I want name of the interface in the name of the method.
		public void DynamicRecyclingTestIRecyclableExtendedTrySuppressInFactory()
		{
			var factory = CreateDynamicRecycleFactory(typeof(RecyclableObject));
			var recycleObject = factory.Create<RecyclableObject>();
			var recyclableExtended = recycleObject as IRecyclableExtended;

			Recyclable.TryFreeIfRecyclable(recyclableExtended);
			Assert.IsTrue(recyclableExtended.IsInFactory);
			recyclableExtended.SuppressRecycling();
		}

		/// <summary>
		/// Test implementation of the <see cref="IRecyclableExtended"/> interface with overriden <see cref="IRecyclableExtended.CanRecycle"/> functionality.
		/// </summary>
		[TestMethod]
		[SuppressMessage("StyleCopPlus.StyleCopPlusRules", "SP0100:AdvancedNamingRules")] // I want name of the interface in the name of the method.
		public void DynamicRecyclingTestIRecyclableExtendedWithCanRecycleOverride()
		{
			var factory = CreateDynamicRecycleFactory(typeof(ExtendedRecyclableObject));
			var extendedRecycleObject = factory.Create<ExtendedRecyclableObject>();

			extendedRecycleObject.StopRecycling = true;
			Assert.IsFalse(Recyclable.TryFreeIfRecyclable(extendedRecycleObject));

			extendedRecycleObject.StopRecycling = false;
			Assert.IsTrue(Recyclable.TryFreeIfRecyclable(extendedRecycleObject));
		}

		/// <summary>
		/// Test implementation of the <see cref="IRecyclableExtended"/> interface with overriden <see cref="IRecyclableExtended.CanRecycle"/> functionality that override previously overriden functionality.
		/// </summary>
		[TestMethod]
		[SuppressMessage("StyleCopPlus.StyleCopPlusRules", "SP0100:AdvancedNamingRules")] // I want name of the interface in the name of the method.
		public void DynamicRecyclingTestIRecyclableExtendedWithCanRecycleOverrideThatOverridesPrevious()
		{
			var factory = CreateDynamicRecycleFactory(typeof(NestedExtendedRecyclableObjectThatIgnoresBaseCanRecycleMethods));
			var nestedExtededRecyclableObject = factory.Create<NestedExtendedRecyclableObjectThatIgnoresBaseCanRecycleMethods>();

			nestedExtededRecyclableObject.StopRecycling = true;
			Assert.IsTrue(Recyclable.TryFreeIfRecyclable(nestedExtededRecyclableObject));

			nestedExtededRecyclableObject = factory.Create<NestedExtendedRecyclableObjectThatIgnoresBaseCanRecycleMethods>();
			nestedExtededRecyclableObject.StopRecyclingReally = true;
			Assert.IsFalse(Recyclable.TryFreeIfRecyclable(nestedExtededRecyclableObject));

			nestedExtededRecyclableObject.StopRecyclingReally = false;
			Assert.IsTrue(Recyclable.TryFreeIfRecyclable(nestedExtededRecyclableObject));
		}

		/// <summary>
		/// Test implementation of the <see cref="IRecyclableExtended"/> interface with overriden <see cref="IRecyclableExtended.CanRecycle"/> functionality that combine with previously overriden functionality.
		/// </summary>
		[TestMethod]
		[SuppressMessage("StyleCopPlus.StyleCopPlusRules", "SP0100:AdvancedNamingRules")] // I want name of the interface in the name of the method.
		public void DynamicRecyclingTestIRecyclableExtendedWithCanRecycleOverrideThatCombinesWithPrevious()
		{
			var factory = CreateDynamicRecycleFactory(typeof(NestedExtendedRecyclableObjectThatCombinesWithBaseCanRecycleMethods));
			var nestedExtededRecyclableObject = factory.Create<NestedExtendedRecyclableObjectThatCombinesWithBaseCanRecycleMethods>();

			nestedExtededRecyclableObject.StopRecycling = true;
			Assert.IsFalse(Recyclable.TryFreeIfRecyclable(nestedExtededRecyclableObject));

			nestedExtededRecyclableObject.StopRecyclingReally = true;
			Assert.IsFalse(Recyclable.TryFreeIfRecyclable(nestedExtededRecyclableObject));

			nestedExtededRecyclableObject.StopRecyclingReally = false;
			Assert.IsFalse(Recyclable.TryFreeIfRecyclable(nestedExtededRecyclableObject));

			nestedExtededRecyclableObject.StopRecycling = false;
			Assert.IsTrue(Recyclable.TryFreeIfRecyclable(nestedExtededRecyclableObject));
		}

		/// <summary>
		/// Tests the implementation of the dynamic recyclable class for the generic class.
		/// </summary>
		[TestMethod]
		public void DynamicRecyclingTestGenericRecycling()
		{
			var intFactory = CreateDynamicRecycleFactory(typeof(GenericRecyclableObject<Int32>));
			var intRecyclableObject = intFactory.Create<GenericRecyclableObject<Int32>>();

			Assert.IsTrue(Recyclable.TryFreeIfRecyclable(intRecyclableObject));
			Assert.IsTrue(((IRecyclable)intRecyclableObject).IsInFactory);

			var doubleFactory = CreateDynamicRecycleFactory(typeof(GenericRecyclableObject<Double>));
			var doubleRecyclableObject = doubleFactory.Create<GenericRecyclableObject<Double>>();

			Assert.IsTrue(Recyclable.TryFreeIfRecyclable(doubleRecyclableObject));
			Assert.IsTrue(((IRecyclable)doubleRecyclableObject).IsInFactory);

			intRecyclableObject = intFactory.Create<GenericRecyclableObject<Int32>>();

			Assert.IsTrue(Recyclable.TryFreeIfRecyclable(intRecyclableObject));
			Assert.IsTrue(((IRecyclable)intRecyclableObject).IsInFactory);
		}

		#endregion
	}
}