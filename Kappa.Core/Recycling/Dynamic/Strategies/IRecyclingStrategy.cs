using System;
using System.Reflection.Emit;

namespace Kappa.Core.Recycling.Dynamic.Strategies
{
	/// <summary>
	/// Describes the strategy-dependent functionality of the creating dynamic recyclable type.
	/// </summary>
	internal interface IRecyclingStrategy
	{
		/// <summary>
		/// Emits the IL code that cleans up members of instance type.
		/// </summary>
		/// <param name="instanceType">The type that owns memebers that will be used in emiting cleanup code.</param>
		/// <param name="context">The <see cref="RecyclingGeneratorContext"/> object that contains information about dynamic type.</param>
		/// <param name="ilGenerator">The generator of IL code for current method.</param>
		void EmitInstanceMembersCleanup(Type instanceType, RecyclingGeneratorContext context, ILGenerator ilGenerator);
	}
}