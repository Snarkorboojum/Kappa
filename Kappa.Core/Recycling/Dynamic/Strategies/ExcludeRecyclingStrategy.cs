using System;
using System.Reflection;
using System.Reflection.Emit;
using Kappa.Core.Recycling.Dynamic.Attributes;

namespace Kappa.Core.Recycling.Dynamic.Strategies
{
	internal sealed class ExcludeRecyclingStrategy : IRecyclingStrategy
	{
		#region Fields

		/// <summary>
		/// The delegate to cache method <see cref="IsRecyclableField"/>.
		/// </summary>
		private static IsRecyclableFunction _isRecyclableFieldDelegate;

		/// <summary>
		/// The delegate to cache method <see cref="IsRecyclableProperty"/>.
		/// </summary>
		private static IsRecyclableFunction _isRecyclablePropertyDelegate;

		#endregion

		/// <summary>
		/// Initializes static fields of the <see cref="ExcludeRecyclingStrategy"/> class.
		/// </summary>
		static ExcludeRecyclingStrategy()
		{
			_isRecyclableFieldDelegate = IsRecyclableField;
			_isRecyclablePropertyDelegate = IsRecyclableProperty;
		}

		/// <summary>
		/// Emits the IL code that cleans up members of instance type.
		/// </summary>
		/// <param name="instanceType">The type that owns memebers that will be used in emiting cleanup code.</param>
		/// <param name="context">The <see cref="RecyclingGeneratorContext"/> object that contains information about dynamic type.</param>
		/// <param name="ilGenerator">The generator of IL code for current method.</param>
		public void EmitInstanceMembersCleanup(Type instanceType, RecyclingGeneratorContext context, ILGenerator ilGenerator)
		{
			RecyclableTypeGenerator.CreateCodeToCleanupFields(instanceType, context, ilGenerator, _isRecyclableFieldDelegate);
			RecyclableTypeGenerator.CreateCodeToCleanupProperties(instanceType, context, ilGenerator, _isRecyclablePropertyDelegate);
		}

		/// <summary>
		/// Filters fields of the class that must be recycled.
		/// </summary>
		/// <param name="instanceType">The type that owns members that will be checked for need to be recycled.</param>
		/// <param name="memberInfo">An metadata object that describes member of type.</param>
		/// <param name="attributes">An array of the member attributes</param>
		/// <returns>True if member of the class must be recycled; otherwise, false.</returns>
		private static Boolean IsRecyclableField(Type instanceType, MemberInfo memberInfo, ref Attribute[] attributes)
		{
			foreach (var attribute in attributes)
			{
				if (attribute is RecycleIgnoreAttribute)
					return false;
			}

			const String startBackingFieldName = "<";
			const String endBackingFieldName = ">k__BackingField";

			if (!memberInfo.Name.StartsWith(startBackingFieldName) || !memberInfo.Name.EndsWith(endBackingFieldName))
				return true;

			var propertyName = memberInfo.Name.Substring(startBackingFieldName.Length, memberInfo.Name.Length - startBackingFieldName.Length - endBackingFieldName.Length);
			var propertyInfo = instanceType.GetProperty(propertyName, RecyclableTypeGenerator.InstanceBindingFlags);

			if (propertyInfo == null)
				throw new InvalidOperationException(String.Format("Property for backing field {0} is not found.", memberInfo.Name));

			attributes = Attribute.GetCustomAttributes(propertyInfo);
			return !IsRecyclablePropertyInsteadOfField(instanceType, propertyInfo, ref attributes);
		}

		/// <summary>
		/// Filters properties of the class that must be recycled.
		/// </summary>
		/// <param name="instanceType">The type that owns members that will be checked for need to be recycled.</param>
		/// <param name="memberInfo">An metadata object that describes member of type.</param>
		/// <param name="attributes">An array of the member attributes</param>
		/// <returns>True if member of the class must be recycled; otherwise, false.</returns>
		private static Boolean IsRecyclableProperty(Type instanceType, MemberInfo memberInfo, ref Attribute[] attributes)
		{
			foreach (var attribute in attributes)
			{
				if (attribute is RecycleIgnoreAttribute)
					return false;

				if (attribute is RecycleCleanAttribute)
					return true;
			}

			return false;
		}

		/// <summary>
		/// Filters properties of the class that must be recycled instead of backing fields.
		/// </summary>
		/// <param name="instanceType">The type that owns members that will be checked for need to be recycled.</param>
		/// <param name="memberInfo">An metadata object that describes member of type.</param>
		/// <param name="attributes">An array of the member attributes</param>
		/// <returns>True if member of the class must be recycled; otherwise, false.</returns>
		private static Boolean IsRecyclablePropertyInsteadOfField(Type instanceType, MemberInfo memberInfo, ref Attribute[] attributes)
		{
			foreach (var attribute in attributes)
			{
				if (attribute is RecycleIgnoreAttribute)
					return true;

				if (attribute is RecycleCleanAttribute)
					return true;
			}

			return false;
		}
	}
}