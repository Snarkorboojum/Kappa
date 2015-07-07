using System;
using System.Reflection;
using System.Reflection.Emit;
using Kappa.Core.Reflection.Emit;

namespace Kappa.Core.Recycling.Dynamic
{
	public sealed partial class RecyclableTypeGenerator
	{
		#region Fields
		/// <summary>
		/// The delegate for caching the <see cref="EmitThisAction"/> method.
		/// </summary>
		private static EmitParametersAction _emitThisDelegate;

		/// <summary>
		/// The delegate for caching the <see cref="EmitFieldAction"/> method.
		/// </summary>
		private static EmitParametersAction _emitFieldDelegate;

		/// <summary>
		/// The delegate for caching the <see cref="EmitPropertyAction"/> method.
		/// </summary>
		private static EmitParametersAction _emitPropertyDelegate;

		#endregion

		/// <summary>
		/// Initializes static fields of the <see cref="RecyclableTypeGenerator"/> for emitting parameters.
		/// </summary>
		private static void InitializeEmitParametersActions()
		{
			_emitThisDelegate = EmitThisAction;
			_emitFieldDelegate = EmitFieldAction;
			_emitPropertyDelegate = EmitPropertyAction;
		}

		/// <summary>
		/// Emits loading into stack "this" reference.
		/// </summary>
		/// <param name="context">The <see cref="RecyclingGeneratorContext"/> object that contains the information about dynamically creating type.</param>
		/// <param name="ilGenerator">The generator of IL code of current method.</param>
		/// <param name="parameters">The array of the parameters that will be emited.</param>
		private static void EmitThisAction(RecyclingGeneratorContext context, ILGenerator ilGenerator, params Object[] parameters)
		{
			ilGenerator.Emit(OpCodes.Ldarg_0);
		}

		/// <summary>
		/// Emits loading into stack value of the field.
		/// </summary>
		/// <param name="context">The <see cref="RecyclingGeneratorContext"/> object that contains the information about dynamically creating type.</param>
		/// <param name="ilGenerator">The generator of IL code of current method.</param>
		/// <param name="parameters">The array of the parameters that will be emited.</param>
		private static void EmitFieldAction(RecyclingGeneratorContext context, ILGenerator ilGenerator, params Object[] parameters)
		{
			if (parameters.Length != 1)
				throw new ArgumentException("The length of the parameters must be 1.");

			var fieldInfo = parameters[0] as FieldInfo;
			if (fieldInfo == null)
				throw new ArithmeticException("Parameter must have type " + typeof(FieldInfo).Name);

			if (fieldInfo.IsPrivate)
			{
				var getterMethod = ClassBuilder.GetGetter(fieldInfo, fieldInfo.DeclaringType, fieldInfo.FieldType);
				var fieldBuilder = context.CreateMemberAccessorField(getterMethod);

				ilGenerator.Emit(OpCodes.Ldsfld, fieldBuilder);
				ilGenerator.Emit(OpCodes.Ldarg_0);
				ilGenerator.Emit(OpCodes.Callvirt, getterMethod.GetType().GetMethod("Invoke"));
			}
			else
			{
				ilGenerator.Emit(OpCodes.Ldarg_0);
				ilGenerator.Emit(OpCodes.Ldfld, fieldInfo);
			}
		}

		/// <summary>
		/// Emits loading into stack value of the property.
		/// </summary>
		/// <param name="context">The <see cref="RecyclingGeneratorContext"/> object that contains the information about dynamically creating type.</param>
		/// <param name="ilGenerator">The generator of IL code of current method.</param>
		/// <param name="parameters">The array of the parameters that will be emited.</param>
		private static void EmitPropertyAction(RecyclingGeneratorContext context, ILGenerator ilGenerator, params Object[] parameters)
		{
			if (parameters.Length != 1)
				throw new ArgumentException("The length of the parameters must be 1.");

			var propertyInfo = parameters[0] as PropertyInfo;
			if (propertyInfo == null)
				throw new ArithmeticException("Parameter must have type " + typeof(PropertyInfo).Name);

			var propertyGetMethod = propertyInfo.GetGetMethod(true);
			if (propertyGetMethod == null)
				throw new InvalidOperationException(String.Format("The property {0} does not have a getter.", propertyInfo.Name));

			if (propertyGetMethod.IsPrivate)
			{
				var getterMethod = ClassBuilder.GetGetter(propertyInfo, propertyInfo.DeclaringType, propertyInfo.PropertyType);
				var fieldBuilder = context.CreateMemberAccessorField(getterMethod);

				ilGenerator.Emit(OpCodes.Ldsfld, fieldBuilder);
				ilGenerator.Emit(OpCodes.Ldarg_0);
				ilGenerator.Emit(OpCodes.Call, getterMethod.GetType().GetMethod("Invoke"));
			}
			else
			{
				ilGenerator.Emit(OpCodes.Ldarg_0);
				ilGenerator.Emit(OpCodes.Call, propertyGetMethod);
			}
		}
	}
}