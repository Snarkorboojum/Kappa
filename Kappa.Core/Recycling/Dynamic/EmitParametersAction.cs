using System;
using System.Reflection.Emit;

namespace Kappa.Core.Recycling.Dynamic
{
	/// <summary>
	/// Describes the method that emit IL code that loads parameters for some method into stack.
	/// </summary>
	/// <param name="context">The <see cref="RecyclingGeneratorContext"/> object that contains the information about dynamically creating type.</param>
	/// <param name="ilGenerator">The generator of IL code of current method.</param>
	/// <param name="parameters">The array of the parameters that will be emited.</param>
	internal delegate void EmitParametersAction(RecyclingGeneratorContext context, ILGenerator ilGenerator, Object[] parameters);
}