using System;
using System.Reflection;
using System.Reflection.Emit;
using Kappa.Core.Recycling.Dynamic.Attributes;

namespace Kappa.Core.Recycling.Dynamic.Strategies
{
	internal sealed class IncludeRecyclingStrategy : IRecyclingStrategy
	{
		/// <summary>
		/// The delegate to cache method <see cref="IsRecyclableMember"/>.
		/// </summary>
		private static readonly IsRecyclableFunction IsRecyclableMemberDelegate;

		/// <summary>
		/// Initializes static fields of the <see cref="IncludeRecyclingStrategy"/> class.
		/// </summary>
		static IncludeRecyclingStrategy()
		{
			IsRecyclableMemberDelegate = IsRecyclableMember;
		}

		/// <summary>
		/// Emits the IL code that cleans up members of instance type.
		/// </summary>
		/// <param name="instanceType">The type that owns members that will be used in emiting cleanup code.</param>
		/// <param name="context">The <see cref="RecyclingGeneratorContext"/> object that contains information about dynamic type.</param>
		/// <param name="ilGenerator">The generator of IL code for current method.</param>
		public void EmitInstanceMembersCleanup(Type instanceType, RecyclingGeneratorContext context, ILGenerator ilGenerator)
		{
			RecyclableTypeGenerator.CreateCodeToCleanupFields(instanceType, context, ilGenerator, IsRecyclableMemberDelegate);
			RecyclableTypeGenerator.CreateCodeToCleanupEvents(instanceType, context, ilGenerator, IsRecyclableMemberDelegate);
			RecyclableTypeGenerator.CreateCodeToCleanupProperties(instanceType, context, ilGenerator, IsRecyclableMemberDelegate);
		}

		/// <summary>
		/// Filters members of the class that must be recycled.
		/// </summary>
		/// <param name="instanceType">The type that owns members that will be checked for need to be recycled.</param>
		/// <param name="memberInfo">An metadata object that describes member of type.</param>
		/// <param name="attributes">An array of the member attributes</param>
		/// <returns>True if member of the class must be recycled; otherwise, false.</returns>
		private static Boolean IsRecyclableMember(Type instanceType, MemberInfo memberInfo, ref Attribute[] attributes)
		{
			foreach (var attribute in attributes)
			{
				if (attribute is RecycleAttribute)
					return true;
			}

			return false;
		}
	}
}