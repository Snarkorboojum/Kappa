using System;
using System.Reflection;
using System.Reflection.Emit;
using Kappa.Core.Recycling.Dynamic.Attributes;
using Kappa.Core.Reflection.Emit;

namespace Kappa.Core.Recycling.Dynamic
{
	public sealed partial class RecyclableTypeGenerator
	{
		/// <summary>
		/// Creates in dynamic method specified by <paramref name="ilGenerator"/> invoke of the method or constructor specified by <paramref name="methodBase"/>.
		/// </summary>
		/// <param name="context">The <see cref="RecyclingGeneratorContext"/> object that contains information about dynamic type.</param>
		/// <param name="ilGenerator">A generator of IL code for current method.</param>
		/// <param name="methodBase">A <see cref="MethodInfo"/> object that contains metadata that describes the method that must be invoked.</param>
		/// <param name="emitParametersDelegate">A delegate that emits parameters before invoke of the method.</param>
		/// <param name="parameters">An array of the parameters that will be emited before call of the method.</param>
		private static void CreateInvokeMethod(RecyclingGeneratorContext context, ILGenerator ilGenerator, MethodBase methodBase, EmitParametersAction emitParametersDelegate, params Object[] parameters)
		{
			if (methodBase.GetParameters().Length != 0)
				throw new InvalidOperationException(String.Format("Method that marked by {0} attribute must not have parameters.", typeof(RecycleCleanupAttribute).Name));

			if (emitParametersDelegate == null)
				throw new ArgumentNullException("emitParametersDelegate");

			if (methodBase.IsPrivate)
			{
				var methodInvoker = ClassBuilder.GetMethodInvoker(methodBase);
				var fieldBuilder = context.CreateMemberAccessorField(methodInvoker);

				ilGenerator.Emit(OpCodes.Ldsfld, fieldBuilder);
				emitParametersDelegate(context, ilGenerator, parameters);
				ilGenerator.Emit(OpCodes.Call, methodInvoker.GetType().GetMethod("Invoke"));
			}
			else
			{
				emitParametersDelegate(context, ilGenerator, parameters);

				var methodInfo = methodBase as MethodInfo;

				if (methodInfo != null)
				{
					ilGenerator.Emit(OpCodes.Call, methodInfo);
					return;
				}

				var constructorInfo = methodBase as ConstructorInfo;

				if (constructorInfo != null)
					ilGenerator.Emit(OpCodes.Call, constructorInfo);
			}
		}

		/// <summary>
		/// Creates in dynamic method specified by <paramref name="ilGenerator"/> set <paramref name="value"/> of the field specified by <paramref name="fieldInfo"/>.
		/// </summary>
		/// <param name="context">The <see cref="RecyclingGeneratorContext"/> object that contains information about dynamic type.</param>
		/// <param name="ilGenerator">A generator of IL code for current method.</param>
		/// <param name="fieldInfo">A <see cref="FieldInfo"/> object that contains metadata that describes the field which value must be setted.</param>
		/// <param name="value">A value of the field.</param>
		private static void CreateSetField(RecyclingGeneratorContext context, ILGenerator ilGenerator, FieldInfo fieldInfo, Object value)
		{
			var fieldType = fieldInfo.FieldType;
			if (fieldInfo.IsPrivate)
			{
				var setterMethod = ClassBuilder.GetSetter(fieldInfo, fieldInfo.DeclaringType, fieldType);
				var fieldBuilder = context.CreateMemberAccessorField(setterMethod);

				ilGenerator.Emit(OpCodes.Ldsfld, fieldBuilder);
				ilGenerator.Emit(OpCodes.Ldarg_0);
				LoadValue(ilGenerator, fieldType, value);
				ilGenerator.Emit(OpCodes.Callvirt, setterMethod.GetType().GetMethod("Invoke"));
			}
			else
			{
				ilGenerator.Emit(OpCodes.Ldarg_0);
				LoadValue(ilGenerator, fieldType, value);
				ilGenerator.Emit(OpCodes.Stfld, fieldInfo);
			}
		}

		/// <summary>
		/// Loads specified <paramref name="value"/> into stack.
		/// </summary>
		/// <param name="ilGenerator">A generator of IL code for current method.</param>
		/// <param name="valueType">A target type of value that must be loaded into stack.</param>
		/// <param name="value">A value of the field.</param>
		private static void LoadValue(ILGenerator ilGenerator, Type valueType, Object value)
		{
			var isNullable = valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Nullable<>);

			if (isNullable)
			{
				var localVariable = ilGenerator.DeclareLocal(valueType);

				if (value == null)
				{
					ilGenerator.Emit(OpCodes.Ldloca_S, localVariable);
					ilGenerator.Emit(OpCodes.Initobj, valueType);
					ilGenerator.Emit(OpCodes.Ldloc_S, localVariable.LocalIndex);
				}
				else
				{
					ilGenerator.LoadConstant(value);
					ilGenerator.Emit(OpCodes.Newobj, valueType.GetConstructor(new[] { valueType.GetGenericArguments()[0] }));
				}
			}
			else
			{
				ilGenerator.LoadConstant(value);
			}
		}

		/// <summary>
		/// Creates in dynamic method specified by <paramref name="ilGenerator"/> set <paramref name="value"/> of the property specified by <paramref name="propertyInfo"/>.
		/// </summary>
		/// <param name="context">The <see cref="RecyclingGeneratorContext"/> object that contains information about dynamic type.</param>
		/// <param name="ilGenerator">A generator of IL code for current method.</param>
		/// <param name="propertyInfo">A <see cref="PropertyInfo"/> object that contains metadata that describes the property which value must be setted.</param>
		/// <param name="value">A value of the field.</param>
		private static void CreateSetProperty(RecyclingGeneratorContext context, ILGenerator ilGenerator, PropertyInfo propertyInfo, Object value)
		{
			var propertySetMethod = propertyInfo.GetSetMethod(true);

			if (propertySetMethod == null)
				throw new InvalidOperationException(String.Format("The property {0} does not have a setter.", propertyInfo.Name));

			var propertyType = propertyInfo.PropertyType;

			if (propertySetMethod.IsPrivate)
			{
				var setterMethod = ClassBuilder.GetSetter(propertyInfo, propertyInfo.DeclaringType, propertyInfo.PropertyType);
				var fieldBuilder = context.CreateMemberAccessorField(setterMethod);

				ilGenerator.Emit(OpCodes.Ldsfld, fieldBuilder);
				ilGenerator.Emit(OpCodes.Ldarg_0);
				LoadValue(ilGenerator, propertyType, value);
				ilGenerator.Emit(OpCodes.Call, setterMethod.GetType().GetMethod("Invoke"));
			}
			else
			{
				ilGenerator.Emit(OpCodes.Ldarg_0);
				LoadValue(ilGenerator, propertyType, value);
				ilGenerator.Emit(OpCodes.Call, propertySetMethod);
			}
		}

		/// <summary>
		/// Emits the invoking of <see cref="Action"/> method with specified <typeparamref name="TAttribute"/>.
		/// </summary>
		/// <typeparam name="TAttribute">The type of attribute that method with <see cref="Action"/> signature must have to be invoked.</typeparam>
		/// <param name="instanceType">The type that owns method that will be emitted.</param>
		/// <param name="context">The <see cref="RecyclingGeneratorContext"/> object that contains information about dynamic type.</param>
		/// <param name="ilGenerator">A generator of IL code for current method.</param>
		/// <returns>The instance of the <typeparamref name="TAttribute"/> if current type have method with it.</returns>
		private static TAttribute EmitInvokeActionWithAttribute<TAttribute>(Type instanceType, RecyclingGeneratorContext context, ILGenerator ilGenerator)
			where TAttribute : Attribute
		{
			var methods = instanceType.GetMethods(InstanceBindingFlags);

			foreach (var methodInfo in methods)
			{
				var attribute = methodInfo.GetCustomAttribute<TAttribute>();
				if (attribute != null)
				{
					CreateInvokeMethod(context, ilGenerator, methodInfo, _emitThisDelegate);
					return attribute;
				}
			}

			return null;
		}
	}
}