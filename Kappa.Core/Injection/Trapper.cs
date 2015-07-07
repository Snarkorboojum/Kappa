using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Kappa.Core.Reflection.Emit;
using Kappa.Core.System;
using ParameterInfo = Kappa.Core.Reflection.Emit.ParameterInfo;

namespace Kappa.Core.Injection
{
	/// <summary>
	/// Provides the methods to inster trap code into the public methods of other types. 
	/// </summary>
	public static class Trapper
	{
		/// <summary>
		/// The object that describes the constructor of the <see cref="NotImplementedException"/> class.
		/// </summary>
		private static readonly ConstructorInfo NotImplementedExceptionConstructor;

		/// <summary>
		/// Initializes a <see cref="Trapper"/> class.
		/// </summary>
		static Trapper()
		{
			NotImplementedExceptionConstructor = typeof(NotImplementedException).GetConstructor(Type.EmptyTypes);
		}

		/// <summary>
		/// Saves the original method body into the provided storage type.
		/// </summary>
		/// <param name="typeBuilder">The object that describes the storage type into which original method body will be saved.</param>
		/// <param name="originalMethod">The object that describes te original method that must be saved inside the storage type.</param>
		/// <returns>The object that describes the saved in storage type body of the original method.</returns>
		private static MethodBuilder CreateStorageForOriginalMethod(TypeBuilder typeBuilder, MethodInfo originalMethod)
		{
			var originalParameters = originalMethod.GetParameters();
			var parametersInfo = new ParameterInfo[originalParameters.Length + 1];

			parametersInfo[0] = new ParameterInfo { ParameterType = originalMethod.DeclaringType };
			for (var index = 0; index < originalParameters.Length; index++)
			{
				var originalParameter = originalParameters[index];
				parametersInfo[index + 1] = new ParameterInfo
				{
					ParameterType = originalParameter.ParameterType,
					IsIn = originalParameter.IsIn,
					IsOptional = originalParameter.IsOptional,
					IsOut = originalParameter.IsOut,
					DefaultValue = originalParameter.DefaultValue,
					HasDefaultValue = originalParameter.RawDefaultValue != DBNull.Value,
					Name = originalParameter.Name,
					Position = originalParameter.Position
				};
			}

			var methodBuilder = ClassBuilder.DefineMethod(typeBuilder,
				originalMethod.Name,
				MethodAttributes.Public | MethodAttributes.Static,
				originalMethod.ReturnType,
				CallingConventions.Standard,
				parametersInfo);

			var ilGenerator = methodBuilder.GetILGenerator();
			ilGenerator.Emit(OpCodes.Newobj, NotImplementedExceptionConstructor);
			ilGenerator.Emit(OpCodes.Throw);

			return methodBuilder;
		}

		/// <summary>
		/// Traps the specified method by inserting code that provided by <paramref name="codeProvider"/> and call previous version of the method from storage.
		/// </summary>
		/// <param name="codeProvider">The object that determine which methods of the specified type can be trapped and provide trap code.</param>
		/// <param name="originalMethod">The object tat describes the original method that about to be trapped.</param>
		/// <param name="storageMethod">The object that describes the copy of original method that saved inside the storage class.</param>
		private static void TrapMethod(ITrapCodeProvider codeProvider, MethodInfo originalMethod, MethodInfo storageMethod)
		{
			var parametersTypes = storageMethod.GetParameters().Select(parameter => parameter.ParameterType).ToArray();
			//			var dynamicMethod = new DynamicMethod(originalMethod.Name, originalMethod.ReturnType, parametersTypes);
			var dynamicMethod = new DynamicMethod(originalMethod.Name, MethodAttributes.Static | MethodAttributes.Public,
				CallingConventions.Standard, originalMethod.ReturnType, parametersTypes, originalMethod.DeclaringType, false);
			var ilGenerator = dynamicMethod.GetILGenerator();

			codeProvider.EmitTrap(originalMethod, ilGenerator);

			for (var index = 0; index < parametersTypes.Length; ++index)
				ilGenerator.LoadArgument(index);

			ilGenerator.Emit(OpCodes.Call, storageMethod);
			ilGenerator.Emit(OpCodes.Ret);

			MethodSwapper.ReplaceMethod(dynamicMethod, originalMethod, true);
		}

		/// <summary>
		/// Traps the methods of the specified type that can be trapped according to the specified trap code provider.
		/// </summary>
		/// <param name="type">The type whose methods must be trapped.</param>
		/// <param name="codeProvider">The object that determine which methods of the specified type can be trapped and provide trap code.</param>
		public static void Trap(Type type, ITrapCodeProvider codeProvider)
		{
			var methods = type.GetPublicMethodsAndPropertyGetters();

			// Create storage class for old method pointers.
			var typeBuilder = ClassBuilder.DefineType("storage<" + type.Name + ">");
			var storageMethods = new List<MethodBuilder>();

			// Creation of the storage methods.
			// i.e. methods in some class that will be replaced by original methods.
			foreach (var method in methods)
			{
				if (codeProvider.CanBeTrapped(method))
					storageMethods.Add(CreateStorageForOriginalMethod(typeBuilder, method));
			}

			// Actualization of the storage class.
			var storageType = typeBuilder.CreateType();

			// Replace methods of storage class with original methods and then trap original methods.
			for (var index = 0; index < methods.Length; ++index)
			{
				if (!codeProvider.CanBeTrapped(methods[index]))
					continue;

				var storageMethod = (MethodInfo)storageMethods[index];

				storageMethod = storageType.GetMethod(storageMethod.Name, storageMethod.GetParameters().Select(info => info.ParameterType).ToArray());

				MethodSwapper.ReplaceMethod(methods[index], storageMethod, true);
				// Trap original method with dynamically created method that consists of emitted code from provider and call original method.
				TrapMethod(codeProvider, methods[index], storageMethod);
			}

		}
	}
}