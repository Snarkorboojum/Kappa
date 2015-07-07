using System;
using System.Reflection;

namespace Kappa.Core.Recycling.Dynamic
{
	/// <summary>
	/// Describes the filters to distinguish members of the class that must be recycled.
	/// </summary>
	/// <param name="instanceType">The type that owns members that will be checked for need to be recycled.</param>
	/// <param name="memberInfo">An metadata object that describes member of type.</param>
	/// <param name="attributes">An array of the member attributes.</param>
	/// <returns>True if member of the class must be recycled; otherwise, false.</returns>
	internal delegate Boolean IsRecyclableFunction(Type instanceType, MemberInfo memberInfo, ref Attribute[] attributes);
}