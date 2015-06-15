using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Kappa.Core.System;

namespace Kappa.Core.Reflection.Emit
{
	/// <summary>
	/// Provides functionality to dynamically build classes.
	/// </summary>
	public static class ClassBuilder
	{
		#region Fields

		/// <summary>
		/// The maximum number of type parameters in <see cref="Action"/> and <see cref="Func{TResult}"/> delegates.
		/// </summary>
		private const Int32 MaxTypeDelegateParameters = 8;

		/// <summary>
		/// The metadata that describes constructor of the <see cref="DllImportAttribute"/> class.
		/// </summary>
		private static readonly ConstructorInfo DllImportAttributeConstructor;

		/// <summary>
		/// The metadata that describes the <see cref="DllImportAttribute.CharSet"/> field.
		/// </summary>
		private static readonly FieldInfo DllImportAttributeCharSetField;

		/// <summary>
		/// The collection that contains all dynamically created getters.
		/// </summary>
		private static readonly ConcurrentDictionary<TypeMemberInfo, Delegate> MemberGetters;

		/// <summary>
		/// The collection that contains all dynamically created setters.
		/// </summary>
		private static readonly ConcurrentDictionary<TypeMemberInfo, Delegate> MemberSetters;

		/// <summary>
		/// The collection that contains all dynamically created constructors.
		/// </summary>
		private static readonly ConcurrentDictionary<KeyValuePair<Type, Type[]>, Delegate> Constructors;

		/// <summary>
		/// The dictionary that contains types of generic <see cref="Action"/> delegates sorted by number of parameters.
		/// </summary>
		private static readonly Type[] GenericActionTypes;

		/// <summary>
		/// The dictionary that contains types of generic <see cref="Func{T}"/> delegates sorted by number of parameters.
		/// </summary>
		private static readonly Type[] GenericFunctionTypes;

		/// <summary>
		/// The delegate that cached <see cref="CreatePropertyGetter"/> method.
		/// </summary>
		private static readonly Func<TypeMemberInfo, Delegate> CreatePropertyGetterDelegate;

		/// <summary>
		/// The delegate that cached <see cref="CreateFieldGetter"/> method.
		/// </summary>
		private static readonly Func<TypeMemberInfo, Delegate> CreateFieldGetterDelegate;

		/// <summary>
		/// The delegate that cached <see cref="CreatePropertySetter"/> method.
		/// </summary>
		private static readonly Func<TypeMemberInfo, Delegate> CreatePropertySetterDelegate;

		/// <summary>
		/// The delegate that cached <see cref="CreateFieldSetter"/> method.
		/// </summary>
		private static readonly Func<TypeMemberInfo, Delegate> CreateFieldSetterDelegate;

		/// <summary>
		/// The delegate that cached <see cref="CreateMethodInvoker"/> method.
		/// </summary>
		private static readonly Func<TypeMemberInfo, Delegate> CreateMethodInvokerDelegate;

		/// <summary>
		/// The delegate that cached <see cref="CreateConstructor"/> method.
		/// </summary>
		private static readonly Func<KeyValuePair<Type, Type[]>, Delegate> CreateConstructorDelegate;

		#endregion

		/// <summary>
		/// Initializes the static fields of the <see cref="ClassBuilder"/> class.
		/// </summary>
		static ClassBuilder()
		{
			DllImportAttributeConstructor = typeof(DllImportAttribute).GetConstructor(new[] { typeof(String) });
			DllImportAttributeCharSetField = typeof(DllImportAttribute).GetField("CharSet");
			MemberGetters = new ConcurrentDictionary<TypeMemberInfo, Delegate>();
			MemberSetters = new ConcurrentDictionary<TypeMemberInfo, Delegate>();
			Constructors = new ConcurrentDictionary<KeyValuePair<Type, Type[]>, Delegate>();

			GenericActionTypes = new[]
			{
				typeof(Action),
				typeof(Action<>),
				typeof(Action<,>),
				typeof(Action<,,>),
				typeof(Action<,,,>),
				typeof(Action<,,,,>),
				typeof(Action<,,,,,>),
				typeof(Action<,,,,,,>),
				typeof(Action<,,,,,,,>),
				typeof(Action<,,,,,,,,>),
				typeof(Action<,,,,,,,,,>),
				typeof(Action<,,,,,,,,,,>),
				typeof(Action<,,,,,,,,,,,>),
				typeof(Action<,,,,,,,,,,,,>),
				typeof(Action<,,,,,,,,,,,,,>),
				typeof(Action<,,,,,,,,,,,,,,>),
				typeof(Action<,,,,,,,,,,,,,,,>),
			};

			GenericFunctionTypes = new[]
			{
				typeof(Func<>),
				typeof(Func<,>),
				typeof(Func<,,>),
				typeof(Func<,,,>),
				typeof(Func<,,,,>),
				typeof(Func<,,,,,>),
				typeof(Func<,,,,,,>),
				typeof(Func<,,,,,,,>),
				typeof(Func<,,,,,,,,>),
				typeof(Func<,,,,,,,,,>),
				typeof(Func<,,,,,,,,,,>),
				typeof(Func<,,,,,,,,,,,>),
				typeof(Func<,,,,,,,,,,,,>),
				typeof(Func<,,,,,,,,,,,,,>),
				typeof(Func<,,,,,,,,,,,,,,>),
				typeof(Func<,,,,,,,,,,,,,,,>),
				typeof(Func<,,,,,,,,,,,,,,,,>),
			};


			CreatePropertyGetterDelegate = CreatePropertyGetter;
			CreateFieldGetterDelegate = CreateFieldGetter;
			CreatePropertySetterDelegate = CreatePropertySetter;
			CreateFieldSetterDelegate = CreateFieldSetter;
			CreateMethodInvokerDelegate = CreateMethodInvoker;
			CreateConstructorDelegate = CreateConstructor;
		}

		/// <summary>
		/// Defines dynamic type.
		/// </summary>
		/// <param name="typeName">A name of the defined type.</param>
		/// <param name="attributes">An attributes of the defined type.</param>
		/// <param name="parentType">A <see cref="Type"/> that describes the base type for the defined type.</param>
		/// <param name="interfaces">An array of the <see cref="Type"/> that describes the interfaces that defined type must implement.</param>
		/// <param name="createParameterlessConstructor">A <see cref="Boolean"/> value indicating that simple parameterless constructor will be created.</param>
		/// <returns>The <see cref="TypeBuilder"/> that describes dynamically defined type.</returns>
		public static TypeBuilder DefineType(String typeName, TypeAttributes attributes = TypeAttributes.Public, Type parentType = null, Type[] interfaces = null, Boolean createParameterlessConstructor = true)
		{
			var typeBuilder = DynamicAssembly.ModuleBuilder.DefineType(typeName, attributes, parentType, interfaces);

			// Creates constructor.
			if (createParameterlessConstructor)
			{
				var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard,
					Type.EmptyTypes);
				var ilGenerator = constructorBuilder.GetILGenerator();

				// Calls base constructor.
				ilGenerator.Emit(OpCodes.Ldarg_0);
				ilGenerator.Emit(OpCodes.Call, (parentType ?? typeof(Object)).GetConstructor(Type.EmptyTypes));
				ilGenerator.Emit(OpCodes.Ret);
			}

			return typeBuilder;
		}

		/// <summary>
		/// Defines dynamic method in dynamic type that specified by <paramref name="typeBuilder"/>.
		/// </summary>
		/// <param name="typeBuilder">A <see cref="TypeBuilder"/> that describes dynamic type in which defined method will be created.</param>
		/// <param name="methodName">A name of the defined method.</param>
		/// <param name="attributes">An attributes of the defined method.</param>
		/// <param name="returnType">A <see cref="Type"/> that describes the type of the value that defined method return.</param>
		/// <param name="callingConvention">An order in which parameter will be passed into the dynamic method.</param>
		/// <param name="parameters">A collection of the <see cref="ParameterInfo"/> that describes defined method parameters.</param>
		/// <returns>The <see cref="MethodBuilder"/> that describes defined method.</returns>
		public static MethodBuilder DefineMethod(TypeBuilder typeBuilder, String methodName, MethodAttributes attributes, Type returnType, CallingConventions callingConvention = CallingConventions.Standard, params ParameterInfo[] parameters)
		{
			var methodBuilder = typeBuilder.DefineMethod(methodName, attributes, callingConvention, returnType, parameters.Select(item => item.ParameterType).ToArray());

			foreach (var parameter in parameters)
			{
				var parameterBuilder = methodBuilder.DefineParameter(parameter.Position,
					(parameter.IsIn ? ParameterAttributes.In : ParameterAttributes.None) |
					(parameter.IsOut ? ParameterAttributes.Out : ParameterAttributes.None) |
					(parameter.IsOptional ? ParameterAttributes.Optional : ParameterAttributes.None) |
					(parameter.HasDefaultValue ? ParameterAttributes.HasDefault : ParameterAttributes.None),
					parameter.Name);

				if (parameter.HasDefaultValue)
					parameterBuilder.SetConstant(parameter.DefaultValue);
			}

			return methodBuilder;
		}

		/// <summary>
		/// Creates the delegate of type <typeparamref name="TDelegate"/> that wrapped method <paramref name="methodName"/> from native library <paramref name="methodName"/> with <paramref name="defaultCharSet"/>.
		/// </summary>
		/// <typeparam name="TDelegate">A type of delegate that will be created.</typeparam>
		/// <param name="nativeLibraryName">A name of the native library.</param>
		/// <param name="methodName">A name of the method in the native library that needs to be wrapped.</param>
		/// <param name="defaultCharSet">A <see cref="CharSet"/> for the <see cref="DllImportAttribute"/>.</param>
		/// <returns>The delegate of <typeparamref name="TDelegate"/>.</returns>
		public static TDelegate CreateNativeWrapper<TDelegate>(String nativeLibraryName, String methodName, CharSet defaultCharSet = CharSet.Unicode)
			where TDelegate : class
		{
			var targetType = typeof(TDelegate);

			if (!targetType.IsSubclassOf(typeof(Delegate)))
				throw new ArgumentException(String.Format("Only descendants of type \"Delegate\" is allowed, not {0}", targetType.Name));

			var targetMethod = targetType.GetMethod("Invoke");

			// Define dynamic type with name from constant nativeLibraryName.
			var typeBuilder = DefineType("NativeWrapper_" + Guid.NewGuid().ToString("D"), TypeAttributes.Public, typeof(Object));

			// Define static method in newly created dynamic type.
			var methodBuilder = typeBuilder.DefineMethod(methodName, MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, CallingConventions.Standard, targetMethod.ReturnType, targetMethod.GetParameters().Select(item => item.ParameterType).ToArray());

			// Create the new attribute builder for DllImportAttribute.
			var dllImportAttributeBuilder = new CustomAttributeBuilder(DllImportAttributeConstructor, new Object[] { nativeLibraryName }, new[] { DllImportAttributeCharSetField }, new Object[] { defaultCharSet });

			// Assign attribute DllImport to the method.
			methodBuilder.SetCustomAttribute(dllImportAttributeBuilder);

			if (!ExecutionEnvironment.IsMono)
			{
				// Create DefaultDllImportSearchPathsAttribute to specified that native text measurement library must be found in addition user directories.
				var defaultDllImportSearchPathsAttributeBuilder = CreateDefaultDllImportSearchPathsAttribute();

				// Assign attribute DefaultDllImportSearchPathsAttribute to the method.
				methodBuilder.SetCustomAttribute(defaultDllImportSearchPathsAttributeBuilder);
			}

			// Create the dynamic type.
			var dynamicType = typeBuilder.CreateType();

			// Create delegate from MethodBase and return it.
			return Delegate.CreateDelegate(targetType, dynamicType.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public), true) as TDelegate;
		}

		/// <summary>
		/// Creates <see cref="CustomAttributeBuilder"/> for the attribute <see cref="DefaultDllImportSearchPathsAttribute"/>.
		/// </summary>
		/// <returns>
		/// The newly created <see cref="CustomAttributeBuilder"/> that describes attribute 
		/// <see cref="DefaultDllImportSearchPathsAttribute"/> with parameters <see cref="DllImportSearchPath.UserDirectories"/> 
		/// and <see cref="DllImportSearchPath.SafeDirectories"/>.
		/// </returns>
		/// <remarks>Dynamic load of classes <see cref="DefaultDllImportSearchPathsAttribute"/> and <see cref="DllImportSearchPath"/> because mono does not implement them at all.</remarks>
		private static CustomAttributeBuilder CreateDefaultDllImportSearchPathsAttribute()
		{
			// Find enum DllImportSearchPath if that type exists.
			var dllImportSearchPath = Type.GetType("System.Runtime.InteropServices.DllImportSearchPath");
			// Get DllImportSearchPath enum values UserDirectories and SafeDirectories.
			var userDirectories = dllImportSearchPath.GetField("UserDirectories");
			var safeDirectories = dllImportSearchPath.GetField("SafeDirectories");

			// Return CustomAttributeBuilder that describes DefaultDllImportSearchPathsAttribute.
			return new CustomAttributeBuilder(
					Type.GetType("System.Runtime.InteropServices.DefaultDllImportSearchPathsAttribute")
						.GetConstructor(new[] { dllImportSearchPath }),
					new Object[] { (Int32)userDirectories.GetValue(null) | (Int32)safeDirectories.GetValue(null) });
		}

		/// <summary>
		/// Creates the getter for specified <paramref name="memberInfo"/>.
		/// </summary>
		/// <typeparam name="TEntity">The type of the class that contains field, specified by <paramref name="memberInfo"/>.</typeparam>
		/// <typeparam name="TValue">The type of value that created delegate must used.</typeparam>
		/// <param name="memberInfo">The object that contains metadata information about foreign class property.</param>
		/// <returns>The delegate that can get value of the specified by <paramref name="memberInfo"/> nonpublic property.</returns>
		public static Func<TEntity, TValue> GetGetter<TEntity, TValue>(MemberInfo memberInfo)
		{
			return (Func<TEntity, TValue>)GetGetter(memberInfo, typeof(TEntity), typeof(TValue));
		}

		/// <summary>
		/// Creates the getter for specified <paramref name="memberInfo"/>.
		/// </summary>
		/// <param name="memberInfo">The object that contains metadata information about foreign class member.</param>
		/// <param name="entityType">The type of the class that contains member, specified by <paramref name="memberInfo"/>.</param>
		/// <param name="valueType">The type of value that created delegate must used.</param>
		/// <returns>The delegate that can get value of the specified by <paramref name="memberInfo"/> nonpublic member.</returns>
		public static Delegate GetGetter(MemberInfo memberInfo, Type entityType, Type valueType)
		{
			if (!(memberInfo is PropertyInfo) && !(memberInfo is FieldInfo))
				throw new ArgumentOutOfRangeException("memberInfo");

			var info = new TypeMemberInfo(memberInfo, entityType, valueType);

			return MemberGetters.GetOrAdd(info, info.PropertyInfo == null ? CreateFieldGetterDelegate : CreatePropertyGetterDelegate);
		}

		/// <summary>
		/// Creates getter for specified <paramref name="info"/>.
		/// </summary>
		/// <param name="info">The structure that contains information about member of the class for which getter will be created.</param>
		/// <returns>The delegate that can get value of the member info specified by <paramref name="info"/>.</returns>
		private static Delegate CreatePropertyGetter(TypeMemberInfo info)
		{
			var propertyInfo = info.PropertyInfo;
			var entityType = info.EntityType;
			var valueType = info.ValueType;

			ValidateArgument(propertyInfo.DeclaringType, propertyInfo.PropertyType, entityType, valueType);

			var accessMethod = propertyInfo.GetGetMethod(true);
			if (accessMethod == null)
				throw new InvalidOperationException(String.Format("The property {0} does not have a getter.", propertyInfo.Name));

			// create a method without a name, object as result type and one parameter of type object
			// the last parameter is very import for accessing private fields
			var method = new DynamicMethod(String.Empty, valueType, new[] { entityType }, DynamicAssembly.ModuleBuilder, true);
			var ilGenerator = method.GetILGenerator();

			ilGenerator.Emit(OpCodes.Ldarg_0); // load the first argument onto the stack (source of type object)
			ilGenerator.Emit(accessMethod.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, accessMethod);
			ilGenerator.Emit(OpCodes.Ret); // return the value on the stack

			return method.CreateDelegate(typeof(Func<,>).MakeGenericType(entityType, valueType));
		}

		/// <summary>
		/// Creates getter for specified <paramref name="info"/>.
		/// </summary>
		/// <param name="info">The structure that contains information about member of the class for which getter will be created.</param>
		/// <returns>The delegate that can get value of the member info specified by <paramref name="info"/>.</returns>
		public static Delegate CreateFieldGetter(TypeMemberInfo info)
		{
			var fieldInfo = info.FieldInfo;
			var entityType = info.EntityType;
			var valueType = info.ValueType;

			ValidateArgument(fieldInfo.DeclaringType, fieldInfo.FieldType, entityType, valueType);

			// create a method without a name, object as result type and one parameter of type object
			// the last parameter is very import for accessing private fields

			var method = new DynamicMethod(String.Empty, valueType, new[] { entityType }, DynamicAssembly.ModuleBuilder, true);
			var ilGenerator = method.GetILGenerator();

			ilGenerator.Emit(OpCodes.Ldarg_0); // load the first argument onto the stack (source of type object)
			ilGenerator.Emit(OpCodes.Ldfld, fieldInfo);
			// store the value of the given field on the stack. The casted version of source is used as instance
			ilGenerator.Emit(OpCodes.Ret); // return the value on the stack

			return method.CreateDelegate(typeof(Func<,>).MakeGenericType(entityType, valueType));
		}

		/// <summary>
		/// Creates the setter for specified <paramref name="memberInfo"/>.
		/// </summary>
		/// <typeparam name="TEntity">The type of the class that contains member, specified by <paramref name="memberInfo"/>.</typeparam>
		/// <typeparam name="TValue">The type of value that created delegate must used.</typeparam>
		/// <param name="memberInfo">The object that contains metadata information about foreign class property.</param>
		/// <returns>The delegate that can set value of the specified by <paramref name="memberInfo"/> nonpublic property.</returns>
		public static Action<TEntity, TValue> GetSetter<TEntity, TValue>(MemberInfo memberInfo)
		{
			return (Action<TEntity, TValue>)GetSetter(memberInfo, typeof(TEntity), typeof(TValue));
		}

		/// <summary>
		/// Creates the setter for specified <paramref name="memberInfo"/>.
		/// </summary>
		/// <param name="memberInfo">The object that contains metadata information about foreign class member.</param>
		/// <param name="entityType">The type of the class that contains property, specified by <paramref name="memberInfo"/>.</param>
		/// <param name="valueType">The type of value that created delegate must used.</param>
		/// <returns>The delegate that can set value of the specified by <paramref name="memberInfo"/> nonpublic member.</returns>
		public static Delegate GetSetter(MemberInfo memberInfo, Type entityType, Type valueType)
		{
			if (!(memberInfo is PropertyInfo) && !(memberInfo is FieldInfo))
				throw new ArgumentOutOfRangeException("memberInfo");

			var info = new TypeMemberInfo(memberInfo, entityType, valueType);

			return MemberSetters.GetOrAdd(info, info.PropertyInfo == null ? CreateFieldSetterDelegate : CreatePropertySetterDelegate);
		}

		/// <summary>
		/// Creates setter for specified <paramref name="info"/>.
		/// </summary>
		/// <param name="info">The structure that contains information about member of the class for which setter will be created.</param>
		/// <returns>The delegate that can set value of the member specified by <paramref name="info"/>.</returns>
		public static Delegate CreateFieldSetter(TypeMemberInfo info)
		{
			var fieldInfo = info.FieldInfo;
			var entityType = info.EntityType;
			var valueType = info.ValueType;

			ValidateArgument(fieldInfo.DeclaringType, fieldInfo.FieldType, entityType, valueType);

			var method = new DynamicMethod(String.Empty, null, new[] { entityType, valueType }, DynamicAssembly.ModuleBuilder, true);
			var ilGenerator = method.GetILGenerator();

			ilGenerator.Emit(OpCodes.Ldarg_0); // load the first argument onto the stack (source of type object)
			ilGenerator.Emit(OpCodes.Ldarg_1); // push the second argument onto the stack (this is the value)
			ilGenerator.Emit(OpCodes.Stfld, fieldInfo); // store the value on stack in the field
			ilGenerator.Emit(OpCodes.Ret); // return

			return method.CreateDelegate(typeof(Action<,>).MakeGenericType(entityType, valueType));
		}

		/// <summary>
		/// Creates setter for specified <paramref name="info"/>.
		/// </summary>
		/// <param name="info">The structure that contains information about member of the class for which setter will be created.</param>
		/// <returns>The delegate that can set value of the member specified by <paramref name="info"/>.</returns>
		public static Delegate CreatePropertySetter(TypeMemberInfo info)
		{
			var propertyInfo = info.PropertyInfo;
			var entityType = info.EntityType;
			var valueType = info.ValueType;

			ValidateArgument(propertyInfo.DeclaringType, propertyInfo.PropertyType, entityType, valueType);

			var accessMethod = propertyInfo.GetSetMethod(true);
			if (accessMethod == null)
				throw new InvalidOperationException(String.Format("The property {0} does not have a setter.", propertyInfo.Name));

			// create a method without a name, object as result type and one parameter of type object
			// the last parameter is very import for accessing private fields

			var method = new DynamicMethod(String.Empty, null, new[] { entityType, valueType }, DynamicAssembly.ModuleBuilder, true);
			var ilGenerator = method.GetILGenerator();

			ilGenerator.Emit(OpCodes.Ldarg_0); // load the first argument onto the stack (source of type object)
			ilGenerator.Emit(OpCodes.Ldarg_1); // load the second argument onto the stack (value of the property)
			ilGenerator.Emit(accessMethod.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, accessMethod);
			ilGenerator.Emit(OpCodes.Ret); // return the value on the stack

			return method.CreateDelegate(typeof(Action<,>).MakeGenericType(entityType, valueType));
		}

		/// <summary>
		/// Gets the static wrapper to call non-public method of class specified by <paramref name="methodInfo"/>.
		/// </summary>
		/// <param name="methodInfo">The <see cref="MethodInfo"/> object that describes the method from another class.</param>
		/// <returns>The delegate that can call method specified by <paramref name="methodInfo"/>.</returns>
		public static Delegate GetMethodInvoker(MethodBase methodInfo)
		{
			var info = new TypeMemberInfo(methodInfo);

			return MemberSetters.GetOrAdd(info, CreateMethodInvokerDelegate);
		}

		/// <summary>
		/// Creates static wrapper to call non-public method of class specified by <paramref name="info"/>.
		/// </summary>
		/// <param name="info">The structure that contains information about member of the class for which setter will be created.</param>
		/// <returns>The delegate that can call method specified by <paramref name="info"/>.</returns>
		public static Delegate CreateMethodInvoker(TypeMemberInfo info)
		{
			var methodBase = info.MethodBase;

			if (methodBase.DeclaringType == null)
				throw new ArgumentException("The declaring type of the method in unknown.", "info");

			var methodParameters = methodBase.GetParameters();
			if (methodParameters.Length > MaxTypeDelegateParameters)
				throw new ArgumentOutOfRangeException(String.Format("The specified method {0} has more than {1} parameters.", methodBase.Name, MaxTypeDelegateParameters));

			Delegate result;
			var parametersTypes = new[] { methodBase.DeclaringType }.Union(methodParameters.Select(item => item.ParameterType)).ToArray();

			Type methodType;
			var methodInfo = methodBase as MethodInfo;
			var constructorInfo = methodBase as ConstructorInfo;

			if (methodInfo != null)
				methodType = methodInfo.ReturnType;
			else if (constructorInfo != null)
				methodType = typeof(void);
			else
				throw new ArgumentOutOfRangeException("info");

			var method = new DynamicMethod(String.Empty, methodType, parametersTypes, DynamicAssembly.ModuleBuilder, true);
			var ilGenerator = method.GetILGenerator();

			for (var index = 0; index < parametersTypes.Length; index++)
				ilGenerator.LoadArgument(index);

			if (methodInfo != null)
				ilGenerator.Emit(OpCodes.Call, methodInfo);
			else
				ilGenerator.Emit(OpCodes.Call, constructorInfo);

			ilGenerator.Emit(OpCodes.Ret);

			if (methodType == typeof(void))
			{
				result = method.CreateDelegate(GenericActionTypes[parametersTypes.Length].MakeGenericType(parametersTypes));
			}
			else
			{
				Array.Resize(ref parametersTypes, parametersTypes.Length + 1);
				parametersTypes[parametersTypes.Length - 1] = methodType;
				result = method.CreateDelegate(GenericFunctionTypes[parametersTypes.Length - 1].MakeGenericType(parametersTypes));
			}

			return result;
		}

		/// <summary>
		/// Creates the constructor for specified <typeparamref name="TEntity"/>.
		/// </summary>
		/// <typeparam name="TEntity">The type of the class that will be produced by dynaically created constructor.</typeparam>
		/// <returns>The delegate that call parameterless constructor of the <typeparamref name="TEntity"/>.</returns>
		public static Func<TEntity> GetConstructor<TEntity>()
		{
			return (Func<TEntity>)GetConstructor(typeof(TEntity));
		}

		/// <summary>
		/// Creates the constructor for specified <paramref name="entityType"/>.
		/// </summary>
		/// <param name="entityType">The type of the class that will be produced by dynaically created constructor.</param>
		/// <param name="parametersTypes">The array of constrcutor's parameters types.</param>
		/// <returns>The delegate that call with specified types of parameters constructor of the <paramref name="entityType"/>.</returns>
		public static Delegate GetConstructor(Type entityType, params Type[] parametersTypes)
		{
			return Constructors.GetOrAdd(new KeyValuePair<Type, Type[]>(entityType, parametersTypes), CreateConstructorDelegate);
		}

		/// <summary>
		/// Creates dynamic constructor for specified <paramref name="constructorTypes"/>.
		/// </summary>
		/// <param name="constructorTypes">The pair that contains information about type that owns constructor and constructors parameters.</param>
		/// <returns>The dynamically created constructor that produces the instances that specified by <paramref name="constructorTypes"/>.</returns>
		public static Delegate CreateConstructor(KeyValuePair<Type, Type[]> constructorTypes)
		{
			const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
			var entityType = constructorTypes.Key;
			var parametersTypes = constructorTypes.Value;
			var constructorInfo = entityType.GetConstructor(bindingFlags, null, parametersTypes, null);

			if (constructorInfo == null)
				throw new InvalidOperationException(String.Format("The constructor for type {0} not found.", entityType.Name));

			var method = new DynamicMethod(String.Empty, entityType, parametersTypes, DynamicAssembly.ModuleBuilder, true);
			var ilGenerator = method.GetILGenerator();

			for (var index = 0; index < parametersTypes.Length; index++)
				ilGenerator.LoadArgument(index);

			ilGenerator.Emit(OpCodes.Newobj, constructorInfo);
			ilGenerator.Emit(OpCodes.Ret); // return the value on the stack

			var genericArguments = new List<Type>(parametersTypes);

			genericArguments.Add(entityType);
			return method.CreateDelegate(GenericFunctionTypes[genericArguments.Count - 1].MakeGenericType(genericArguments.ToArray()));
		}

		/// <summary>
		/// Validate the specified <paramref name="declaringType"/> for the <paramref name="entityType"/> and <paramref name="memberType"/> for <paramref name="valueType"/>.
		/// </summary>
		/// <param name="declaringType">The verified for assignable from <paramref name="entityType"/> class that contains verified member.</param>
		/// <param name="memberType">The verified for assignable from <paramref name="valueType"/> class that specifies the member type.</param>
		/// <param name="entityType">The type of the class that contains verified member.</param>
		/// <param name="valueType">The type of value that created delegate must used.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void ValidateArgument(Type declaringType, Type memberType, Type entityType, Type valueType)
		{
			if (declaringType == null)
				throw new ArgumentException(@"The declaring type of the specified field is unknown.");

			if (!declaringType.IsAssignableFrom(entityType))
				throw new ArgumentException(String.Format(@"The type {0} is not base class for {1}.", declaringType.Name, entityType.Name));

			if (!memberType.IsAssignableFrom(valueType))
				throw new ArgumentException(String.Format(@"The type {0} of the field is not assignable from the {1}.", valueType.Name, valueType.Name));
		}
	}
}