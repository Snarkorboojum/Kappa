using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using Eco.Recycling;
using Kappa.Core.Recycling.Dynamic.Attributes;
using Kappa.Core.Recycling.Dynamic.Strategies;
using Kappa.Core.Reflection.Emit;
using ParameterInfo = Kappa.Core.Reflection.Emit.ParameterInfo;

namespace Kappa.Core.Recycling.Dynamic
{
	public sealed partial class RecyclableTypeGenerator
	{
		#region Fields

		/// <summary>
		/// The object that implement <see cref="RecyclingStrategyType.Include"/> strategy in recycling procedure.
		/// </summary>
		private static IRecyclingStrategy _includeStrategy;

		/// <summary>
		/// The object that implement <see cref="RecyclingStrategyType.Exclude"/> strategy in recycling procedure.
		/// </summary>
		private static IRecyclingStrategy _excludeStrategy;

		#endregion

		/// <summary>
		/// Initializes the static fields of the <see cref="RecyclableTypeGenerator"/> class for the <see cref="IRecyclable"/> imeplementation procedure.
		/// </summary>
		private static void InitializeRecyclable()
		{
			_includeStrategy = new IncludeRecyclingStrategy();
			_excludeStrategy = new ExcludeRecyclingStrategy();
		}

		#region Creation of constructor

		/// <summary>
		/// Creates constructor for the dynamic recyclable type.
		/// </summary>
		/// <param name="context">The <see cref="RecyclingGeneratorContext"/> object that contains information about dynamically creating type.</param>
		private static void CreateRecyclableConstructor(RecyclingGeneratorContext context)
		{
			var parentConstructor = context.SourceType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);

			if (parentConstructor == null)
				throw new ArgumentException("Parent type does not have a constructor without parameters.");

			var constructorBuilder = context.TypeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);
			var ilGenerator = constructorBuilder.GetILGenerator();

			// Calls base constructor.
			CreateInvokeMethod(context, ilGenerator, parentConstructor, _emitThisDelegate);
			ilGenerator.Emit(OpCodes.Ret);
		}

		#endregion

		#region Creation of IRecyclable properties

		/// <summary>
		/// Creates members of the <see cref="IRecyclable"/> interface that can get and set <see cref="IRecycleFactory"/>.
		/// </summary>
		/// <param name="context">The <see cref="RecyclingGeneratorContext"/> object that contains information about dynamically creating type.</param>
		private static void CreateSourceFactoryMembers(RecyclingGeneratorContext context)
		{
			// Generating "SetSourceFactory" method
			var iRecyclableName = typeof(IRecyclable).FullName;
			var setSourceFactoryMethod = ClassBuilder.DefineMethod(context.TypeBuilder, iRecyclableName + ".SetSourceFactory", MethodAttributes.Private | MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.HideBySig, typeof(void), CallingConventions.Standard, new ParameterInfo { Name = "factory", IsIn = true, ParameterType = typeof(IRecycleFactory) });
			var ilGenerator = setSourceFactoryMethod.GetILGenerator();

			ilGenerator.Emit(OpCodes.Ldarg_0);
			ilGenerator.Emit(OpCodes.Ldarg_1);
			ilGenerator.Emit(OpCodes.Stfld, context.SourceFactoryField);
			ilGenerator.Emit(OpCodes.Ret);

			context.TypeBuilder.DefineMethodOverride(setSourceFactoryMethod, typeof(IRecyclable).GetMethod("SetSourceFactory"));

			// Generating "SourceFactory" property.
			var sourceFactoryProperty = context.TypeBuilder.DefineProperty(iRecyclableName + ".SourceFactory", PropertyAttributes.None, typeof(IRecycleFactory), Type.EmptyTypes);
			var getSourceFactoryMethod = ClassBuilder.DefineMethod(context.TypeBuilder, iRecyclableName + ".get_SourceFactory", MethodAttributes.Private | MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Final, typeof(IRecycleFactory));

			ilGenerator = getSourceFactoryMethod.GetILGenerator();
			ilGenerator.Emit(OpCodes.Ldarg_0);
			ilGenerator.Emit(OpCodes.Ldfld, context.SourceFactoryField);
			ilGenerator.Emit(OpCodes.Ret);
			sourceFactoryProperty.SetGetMethod(getSourceFactoryMethod);

			context.TypeBuilder.DefineMethodOverride(getSourceFactoryMethod, typeof(IRecyclable).GetMethod("get_SourceFactory", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance));
		}

		/// <summary>
		/// Creates members of the <see cref="IRecyclableExtended"/> interface.
		/// </summary>
		/// <param name="context">The <see cref="RecyclingGeneratorContext"/> object that contains information about dynamically creating type.</param>
		[SuppressMessage("StyleCopPlus.StyleCopPlusRules", "SP0100:AdvancedNamingRules")] // I want name of the interface in the name of the method.
		private static void CreateIRecyclablExtendedImplementation(RecyclingGeneratorContext context)
		{
			CreateCanRecyclePropertyImplementation(context);

			// Implementing SupressRecycleMethod.
			CreateSuppressRecyclingMethodImplementation(context);

			// Implementing ResumeRecycleMethod.
			CreateResumeRecyclingMethodImplementation(context);
		}

		/// <summary>
		/// Creates implementation of the <see cref="IRecyclableExtended.SuppressRecycling"/> method.
		/// </summary>
		/// <param name="context">The <see cref="RecyclingGeneratorContext"/> object that contains information about dynamically creating type.</param>
		private static void CreateSuppressRecyclingMethodImplementation(RecyclingGeneratorContext context)
		{
			const String methodName = "SuppressRecycling";
			var iRecyclableExtendedName = typeof(IRecyclableExtended).FullName;
			var resumeRecyclingMethod = ClassBuilder.DefineMethod(context.TypeBuilder, iRecyclableExtendedName + "." + methodName, MethodAttributes.Private | MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.HideBySig, typeof(void), CallingConventions.Standard);
			var ilGenerator = resumeRecyclingMethod.GetILGenerator();

			CreateVerifyNotInFactoryMethodImplementation(context, ilGenerator);

			ilGenerator.Emit(OpCodes.Ldarg_0);
			ilGenerator.Emit(OpCodes.Dup);
			ilGenerator.Emit(OpCodes.Volatile);
			ilGenerator.Emit(OpCodes.Ldfld, context.RecyclingSuppressedCountField);
			ilGenerator.Emit(OpCodes.Ldc_I4_1);
			ilGenerator.Emit(OpCodes.Add);
			ilGenerator.Emit(OpCodes.Volatile);
			ilGenerator.Emit(OpCodes.Stfld, context.RecyclingSuppressedCountField);
			ilGenerator.Emit(OpCodes.Ret);

			context.TypeBuilder.DefineMethodOverride(resumeRecyclingMethod, typeof(IRecyclableExtended).GetMethod(methodName));
		}

		/// <summary>
		/// Creates implementation of the <see cref="IRecyclableExtended.ResumeRecycling"/> method.
		/// </summary>
		/// <param name="context">The <see cref="RecyclingGeneratorContext"/> object that contains information about dynamically creating type.</param>
		private static void CreateResumeRecyclingMethodImplementation(RecyclingGeneratorContext context)
		{
			const String methodName = "ResumeRecycling";
			var iRecyclableExtendedName = typeof(IRecyclableExtended).FullName;
			var suppressRecyclingMethod = ClassBuilder.DefineMethod(context.TypeBuilder, iRecyclableExtendedName + "." + methodName, MethodAttributes.Private | MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.HideBySig, typeof(void), CallingConventions.Standard);
			var ilGenerator = suppressRecyclingMethod.GetILGenerator();
			var label = ilGenerator.DefineLabel();

			ilGenerator.DeclareLocal(typeof(Int32));

			CreateVerifyNotInFactoryMethodImplementation(context, ilGenerator);

			ilGenerator.Emit(OpCodes.Ldarg_0);
			ilGenerator.Emit(OpCodes.Volatile);
			ilGenerator.Emit(OpCodes.Ldfld, context.RecyclingSuppressedCountField);
			ilGenerator.Emit(OpCodes.Stloc_0);
			ilGenerator.Emit(OpCodes.Ldloc_0);

			// if (_recyclingSuppressedCount == 0)
			ilGenerator.Emit(OpCodes.Ldc_I4_0);
			ilGenerator.Emit(OpCodes.Ceq);
			ilGenerator.Emit(OpCodes.Brfalse_S, label);

			//	throw new InvalidOperationException("Cannot resume recycling for object couse it was not suppressed.");
			var constructorInfo = typeof(InvalidOperationException).GetConstructor(new[] { typeof(String) });

			ilGenerator.Emit(OpCodes.Ldstr, "Cannot resume recycling for object because it was not suppressed.");
			// ReSharper disable once AssignNullToNotNullAttribute
			ilGenerator.Emit(OpCodes.Newobj, constructorInfo);
			ilGenerator.Emit(OpCodes.Throw);

			ilGenerator.MarkLabel(label);
			//--_recyclingSuppressedCount;
			ilGenerator.Emit(OpCodes.Ldarg_0);
			ilGenerator.Emit(OpCodes.Ldloc_0);
			ilGenerator.Emit(OpCodes.Ldc_I4_1);
			ilGenerator.Emit(OpCodes.Sub);

			ilGenerator.Emit(OpCodes.Volatile);
			ilGenerator.Emit(OpCodes.Stfld, context.RecyclingSuppressedCountField);
			ilGenerator.Emit(OpCodes.Ret);

			context.TypeBuilder.DefineMethodOverride(suppressRecyclingMethod, typeof(IRecyclableExtended).GetMethod(methodName));
		}

		/// <summary>
		/// Generates implementation of the VerifyNotInFactory method body.
		/// </summary>
		/// <param name="context">The <see cref="RecyclingGeneratorContext"/> object that contains information about dynamically creating type.</param>
		/// <param name="ilGenerator">A generator of IL code for <see cref="IRecyclable.Cleanup"/> method.</param>
		private static void CreateVerifyNotInFactoryMethodImplementation(RecyclingGeneratorContext context, ILGenerator ilGenerator)
		{
			var labelIsInFactoryFalse = ilGenerator.DefineLabel();

			ilGenerator.Emit(OpCodes.Ldarg_0);
			ilGenerator.Emit(OpCodes.Ldfld, context.IsInFactoryField);
			ilGenerator.Emit(OpCodes.Brfalse_S, labelIsInFactoryFalse);

			//	throw new InvalidOperationException("Cannot access recyclable object that is still in factory. Try to resolve item from recycle factory.");
			var constructorInfo = typeof(InvalidOperationException).GetConstructor(new[] { typeof(String) });

			ilGenerator.Emit(OpCodes.Ldstr, "Cannot access recyclable object that is still in factory. Try to resolve item from recycle factory.");
			// ReSharper disable once AssignNullToNotNullAttribute
			ilGenerator.Emit(OpCodes.Newobj, constructorInfo);
			ilGenerator.Emit(OpCodes.Throw);

			ilGenerator.MarkLabel(labelIsInFactoryFalse);
		}

		/// <summary>
		/// Generates implementation of the <see cref="IRecyclableExtended.CanRecycle"/> property.
		/// </summary>
		/// <param name="context">The <see cref="RecyclingGeneratorContext"/> object that contains information about dynamically creating type.</param>
		private static void CreateCanRecyclePropertyImplementation(RecyclingGeneratorContext context)
		{
			var iRecyclableExtendedName = typeof(IRecyclableExtended).FullName;
			// Generating "CanRecycle" property.
			var canRecycleProperty = context.TypeBuilder.DefineProperty(iRecyclableExtendedName + ".CanRecycle", PropertyAttributes.None, typeof(Boolean), Type.EmptyTypes);

			// Generating "getter" method for "CanRecycle" property.
			var getCanRecycleMethod = ClassBuilder.DefineMethod(context.TypeBuilder, iRecyclableExtendedName + ".get_CanRecycle", MethodAttributes.Private | MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Final, typeof(Boolean));
			var ilGenerator = getCanRecycleMethod.GetILGenerator();
			var labelIsInFactoryEqualsFalse = ilGenerator.DefineLabel();
			var labelRecyclingSuppressedCountIsZero = ilGenerator.DefineLabel();

			// Defines the boolean local variable, that will be referenced simply by number "0".
			ilGenerator.DeclareLocal(typeof(Boolean));

			// if(IsInFactory)
			//  return false;
			ilGenerator.Emit(OpCodes.Ldarg_0);
			ilGenerator.Emit(OpCodes.Ldfld, context.IsInFactoryField);
			ilGenerator.Emit(OpCodes.Stloc_0);
			ilGenerator.Emit(OpCodes.Ldloc_0);
			ilGenerator.Emit(OpCodes.Brfalse_S, labelIsInFactoryEqualsFalse);
			ilGenerator.Emit(OpCodes.Ldloc_0);
			ilGenerator.Emit(OpCodes.Not);
			ilGenerator.Emit(OpCodes.Ret);

			ilGenerator.MarkLabel(labelIsInFactoryEqualsFalse);
			// if(RecyclingSuppressedCount != 0)
			//  return false;
			ilGenerator.Emit(OpCodes.Ldarg_0);
			ilGenerator.Emit(OpCodes.Volatile);
			ilGenerator.Emit(OpCodes.Ldfld, context.RecyclingSuppressedCountField);
			ilGenerator.Emit(OpCodes.Ldc_I4_0);
			ilGenerator.Emit(OpCodes.Ceq);
			ilGenerator.Emit(OpCodes.Stloc_0);
			ilGenerator.Emit(OpCodes.Ldloc_0);
			ilGenerator.Emit(OpCodes.Brtrue_S, labelRecyclingSuppressedCountIsZero);
			ilGenerator.Emit(OpCodes.Ldloc_0);
			ilGenerator.Emit(OpCodes.Ret);

			ilGenerator.MarkLabel(labelRecyclingSuppressedCountIsZero);
			// Call methods thats overrides "CanRecycle" functionality if they exists.
			var typesHierarchy = context.GetSourceTypesHierarchy();
			var step = 0;
			foreach (var type in typesHierarchy)
			{
				var attribute = CreateInstanceCanRecycleMethod(type, context, ilGenerator);

				if (attribute == null)
					continue; // We do nothing - no IL was emitted.

				if (++step > 1) // On first step we skip AND with local variable because it's value always True.
				{
					ilGenerator.Emit(OpCodes.Ldloc_0);
					ilGenerator.Emit(OpCodes.And);
				}

				ilGenerator.Emit(OpCodes.Stloc_0);

				if (attribute.IgnoreBaseMethods)
					break;
			}

			ilGenerator.Emit(OpCodes.Ldloc_0);
			ilGenerator.Emit(OpCodes.Ret);
			canRecycleProperty.SetGetMethod(getCanRecycleMethod);

			context.TypeBuilder.DefineMethodOverride(getCanRecycleMethod, typeof(IRecyclableExtended).GetMethod("get_CanRecycle", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance));
		}

		/// <summary>
		/// Creates members of the <see cref="IRecyclable"/> interface that can get and set <see cref="Boolean"/> value indicating that instance of building type stored in recycling factory.
		/// </summary>
		/// <param name="context">The <see cref="RecyclingGeneratorContext"/> object that contains information about dynamically creating type.</param>
		private static void CreateIsInFactoryMembers(RecyclingGeneratorContext context)
		{
			// Generating "IsInFactory" property.
			var iRecyclableName = typeof(IRecyclable).FullName;
			var isInFactoryProperty = context.TypeBuilder.DefineProperty(iRecyclableName + ".IsInFactory", PropertyAttributes.None, typeof(Boolean), Type.EmptyTypes);
			var getIsInFactoryMethod = ClassBuilder.DefineMethod(context.TypeBuilder, iRecyclableName + ".get_IsInFactory", MethodAttributes.Private | MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Final, typeof(Boolean));
			var ilGenerator = getIsInFactoryMethod.GetILGenerator();

			ilGenerator.Emit(OpCodes.Ldarg_0);
			ilGenerator.Emit(OpCodes.Ldfld, context.IsInFactoryField);
			ilGenerator.Emit(OpCodes.Ret);
			isInFactoryProperty.SetGetMethod(getIsInFactoryMethod);

			context.TypeBuilder.DefineMethodOverride(getIsInFactoryMethod, typeof(IRecyclable).GetMethod("get_IsInFactory", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance));
		}

		#endregion

		#region Creation of IRecyclable methods

		/// <summary>
		/// Creates implementation of <see cref="IRecyclable.Cleanup"/> method.
		/// </summary>
		/// <param name="context">The <see cref="RecyclingGeneratorContext"/> object that contains information about dynamically creating type.</param>
		private static void CreateCleanupMethod(RecyclingGeneratorContext context)
		{
			var iRecyclableName = typeof(IRecyclable).FullName;
			var cleanupMethod = ClassBuilder.DefineMethod(context.TypeBuilder, iRecyclableName + ".Cleanup", MethodAttributes.Private | MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.HideBySig | MethodAttributes.Final, typeof(void));
			var ilGenerator = cleanupMethod.GetILGenerator();

			// IsInfactory = true;
			ilGenerator.Emit(OpCodes.Ldarg_0);
			ilGenerator.Emit(OpCodes.Ldc_I4_1);
			ilGenerator.Emit(OpCodes.Stfld, context.IsInFactoryField);

			var typesHierarchy = context.GetSourceTypesHierarchy();
			foreach (var type in typesHierarchy)
				CreateInstanceCleanupMethod(type, context, ilGenerator);

			if (typeof(IRecyclable).IsAssignableFrom(context.SourceType))
			{
				var interfaceMap = context.SourceType.GetInterfaceMap(typeof(IRecyclable));
				var index = 0;

				for (; index < interfaceMap.InterfaceMethods.Length; ++index)
				{
					if (interfaceMap.InterfaceMethods[index].Name == "Cleanup")
					{
						ilGenerator.Emit(OpCodes.Ldarg_0);
						ilGenerator.Emit(OpCodes.Call, interfaceMap.TargetMethods[index]);
						break;
					}
				}
			}

			ilGenerator.Emit(OpCodes.Ret);

			context.TypeBuilder.DefineMethodOverride(cleanupMethod, typeof(IRecyclable).GetMethod("Cleanup"));
		}

		/// <summary>
		/// Creates implementation of <see cref="IRecyclable.Resolve"/> method.
		/// </summary>
		/// <param name="context">The <see cref="RecyclingGeneratorContext"/> object that contains information about dynamically creating type.</param>
		private static void CreateResolveMethod(RecyclingGeneratorContext context)
		{
			var iRecyclableName = typeof(IRecyclable).FullName;
			var resolveMethod = ClassBuilder.DefineMethod(context.TypeBuilder, iRecyclableName + ".Resolve", MethodAttributes.Private | MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.HideBySig | MethodAttributes.Final, typeof(void));
			var ilGenerator = resolveMethod.GetILGenerator();

			// IsInfactory = false;
			ilGenerator.Emit(OpCodes.Ldarg_0);
			ilGenerator.Emit(OpCodes.Ldc_I4_0);
			ilGenerator.Emit(OpCodes.Stfld, context.IsInFactoryField);

			var typesHierarchy = context.GetSourceTypesHierarchy();
			foreach (var type in typesHierarchy)
				CreateInstanceResolveMethod(type, context, ilGenerator);

			if (typeof(IRecyclable).IsAssignableFrom(context.SourceType))
			{
				var interfaceMap = context.SourceType.GetInterfaceMap(typeof(IRecyclable));
				var index = 0;

				for (; index < interfaceMap.InterfaceMethods.Length; ++index)
				{
					if (interfaceMap.InterfaceMethods[index].Name == "Resolve")
					{
						ilGenerator.Emit(OpCodes.Ldarg_0);
						ilGenerator.Emit(OpCodes.Call, interfaceMap.TargetMethods[index]);
						break;
					}
				}
			}

			ilGenerator.Emit(OpCodes.Ret);

			context.TypeBuilder.DefineMethodOverride(resolveMethod, typeof(IRecyclable).GetMethod("Resolve"));
		}

		#endregion

		#region Additional methods to create parts of IRecyclable methods for each class in hierarchy

		/// <summary>
		/// Creates part of cleansing procedure for specified <paramref name="instanceType"/>.
		/// </summary>
		/// <param name="instanceType">The type that owns method that will be emitted.</param>
		/// <param name="context">The <see cref="RecyclingGeneratorContext"/> object that contains information about dynamically creating type.</param>
		/// <param name="ilGenerator">A generator of IL code for <see cref="IRecyclable.Cleanup"/> method.</param>
		private static void CreateInstanceCleanupMethod(Type instanceType, RecyclingGeneratorContext context, ILGenerator ilGenerator)
		{
			// If current type from hierarchy does not marker as recyclable - its member will not be cleansed.
			var recyclableAttribute = instanceType.GetCustomAttribute<RecyclableAttribute>();
			if (recyclableAttribute == null)
				return;

			var strategy = GetRecyclingStrategy(recyclableAttribute);

			strategy.EmitInstanceMembersCleanup(instanceType, context, ilGenerator);
			EmitInvokeActionWithAttribute<RecycleCleanupAttribute>(instanceType, context, ilGenerator);
		}

		/// <summary>
		/// Creates part of resolving procedure for specified <paramref name="instanceType"/>.
		/// </summary>
		/// <param name="instanceType">The type that owns method that will be emitted.</param>
		/// <param name="context">The <see cref="RecyclingGeneratorContext"/> object that contains information about dynamically creating type.</param>
		/// <param name="ilGenerator">A generator of IL code for <see cref="IRecyclable.Cleanup"/> method.</param>
		private static void CreateInstanceResolveMethod(Type instanceType, RecyclingGeneratorContext context, ILGenerator ilGenerator)
		{
			EmitInvokeActionWithAttribute<RecycleResolveAttribute>(instanceType, context, ilGenerator);
		}

		/// <summary>
		/// Creates part of procedure that override default functionality of the <see cref="IRecyclableExtended.CanRecycle"/> for specified <paramref name="instanceType"/>.
		/// </summary>
		/// <param name="instanceType">The type that owns method that will be emitted.</param>
		/// <param name="context">The <see cref="RecyclingGeneratorContext"/> object that contains information about dynamically creating type.</param>
		/// <param name="ilGenerator">A generator of IL code for <see cref="IRecyclable.Cleanup"/> method.</param>
		/// <returns>The <see cref="CanRecycleMethodAttribute"/> if current type have one; otherwise, null..</returns>
		private static CanRecycleMethodAttribute CreateInstanceCanRecycleMethod(Type instanceType, RecyclingGeneratorContext context, ILGenerator ilGenerator)
		{
			return EmitInvokeActionWithAttribute<CanRecycleMethodAttribute>(instanceType, context, ilGenerator);
		}

		#endregion

		/// <summary>
		/// Gets the object that determines strategy of the recycling procedure for members of each type in types hierarchy.
		/// </summary>
		/// <param name="recyclableAttribute">The attribute that specifies recycling strategy type.</param>
		/// <returns>The object that implement <see cref="IRecyclingStrategy"/> interface.</returns>
		private static IRecyclingStrategy GetRecyclingStrategy(RecyclableAttribute recyclableAttribute)
		{
			switch (recyclableAttribute.Strategy)
			{
				case RecyclingStrategyType.Include:
					return _includeStrategy;

				case RecyclingStrategyType.Exclude:
					return _excludeStrategy;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}