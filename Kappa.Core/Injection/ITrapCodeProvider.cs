using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Kappa.Core.Injection
{
	/// <summary>
	/// Describes the object that determine which type's methods can be trapped and provides code for entrappment.
	/// </summary>
	public interface ITrapCodeProvider
	{
		/// <summary>
		/// Determines the possibility of the entrapment for the specified method.
		/// </summary>
		/// <param name="method">The object that describes the method that will be verified for possibility of entrapment.</param>
		/// <returns>True if method can be trapped; otherwise, false.</returns>
		Boolean CanBeTrapped(MethodInfo method);

		/// <summary>
		/// Emits the trap code into provided method.
		/// </summary>
		/// <param name="method">The object that describes method that will be trapped.</param>
		/// <param name="ilGenerator">The generator of the IL code for trapped method.</param>
		void EmitTrap(MethodInfo method, ILGenerator ilGenerator);
	}
}