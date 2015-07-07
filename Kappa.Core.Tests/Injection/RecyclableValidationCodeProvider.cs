using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Eco.Recycling;
using Kappa.Core.Injection;
using Kappa.Core.System;

namespace Kappa.Core.Tests.Injection
{
	/// <summary>
	/// The provider of entrapment code.
	/// </summary>
	class RecyclableValidationCodeProvider : ITrapCodeProvider
	{
		#region Fields

		/// <summary>
		/// The object that describes the getter method of the <see cref="IRecyclable.IsInFactory"/> property.
		/// </summary>
		private static readonly MethodInfo IsInFactoryPropertyGetMethod;

		/// <summary>
		/// The object that describes the constructor of the <see cref="InvalidOperationException"/> type.
		/// </summary>
		private static readonly ConstructorInfo InvalidOperationExceptionConstructor;

		/// <summary>
		/// The array that contains the descriptions of methods that must be trapped.
		/// </summary>
		private static readonly MethodInfo[] RecyclableMethods;

		/// <summary>
		/// The validation text which helps determine if entrapmnt was successfull. 
		/// </summary>
		private readonly String _validationMessage;

		#endregion

		/// <summary>
		/// Initializes a <see cref="RecyclableValidationCodeProvider"/> type.
		/// </summary>
		static RecyclableValidationCodeProvider()
		{
			IsInFactoryPropertyGetMethod = typeof(IRecyclable).GetProperty("IsInFactory").GetGetMethod();
			InvalidOperationExceptionConstructor = typeof(InvalidOperationException).GetConstructor(new[] { typeof(String) });
			RecyclableMethods = typeof(IRecyclable).GetPublicMethodsAndPropertyGetters();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RecyclableValidationCodeProvider"/> type.
		/// </summary>
		/// <param name="validationMessage">The validation text which helps determine if entrapmnt was successfull.</param>
		public RecyclableValidationCodeProvider(String validationMessage)
		{
			_validationMessage = validationMessage;
		}

		/// <summary>
		/// Determines the possibility of the entrapment for the specified method.
		/// </summary>
		/// <param name="method">The object that describes the method that will be verified for possibility of entrapment.</param>
		/// <returns>True if method can be trapped; otherwise, false.</returns>
		public Boolean CanBeTrapped(MethodInfo method)
		{
			return !RecyclableMethods.Contains(method);
		}

		/// <summary>
		/// Emits the trap code into provided method.
		/// </summary>
		/// <param name="method">The object that describes method that will be trapped.</param>
		/// <param name="ilGenerator">The generator of the IL code for trapped method.</param>
		public void EmitTrap(MethodInfo method, ILGenerator ilGenerator)
		{
			var type = method.DeclaringType;
			if (type == null)
				throw new InvalidOperationException();

			var property = type.GetProperty("IsInFactory");
			if (property == null)
				property = type.GetProperty(typeof(IRecyclable).FullName + ".IsInFactory", BindingFlags.Instance | BindingFlags.NonPublic);

			if (property == null)
				throw new InvalidOperationException();

			var isInFactoryPropertyGetMethod = property.GetGetMethod(true);
			var notInFactoryLabel = ilGenerator.DefineLabel();

			ilGenerator.Emit(OpCodes.Ldarg_0); // Load this
			ilGenerator.Emit(OpCodes.Callvirt, isInFactoryPropertyGetMethod);
			ilGenerator.Emit(OpCodes.Ldc_I4_0);
			ilGenerator.Emit(OpCodes.Ceq);
			ilGenerator.Emit(OpCodes.Brtrue_S, notInFactoryLabel);
			ilGenerator.Emit(OpCodes.Ldstr, _validationMessage);
			ilGenerator.Emit(OpCodes.Newobj, InvalidOperationExceptionConstructor);
			ilGenerator.Emit(OpCodes.Throw);
			ilGenerator.MarkLabel(notInFactoryLabel);
		}
	}
}
