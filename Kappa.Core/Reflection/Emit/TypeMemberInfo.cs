using System;
using System.Reflection;

namespace Kappa.Core.Reflection.Emit
{
	/// <summary>
	/// Describes the key for dynamically created getter or setter for filed or peroperty in the global cache.
	/// </summary>
	public struct TypeMemberInfo
	{
		/// <summary>
		/// The object that contains metadata information about foreign class member.
		/// </summary>
		private readonly MemberInfo _memberInfo;

		/// <summary>
		/// The type of the class that contains member.
		/// </summary>
		private readonly Type _entityType;

		/// <summary>
		/// The type of value that created delegate must use.
		/// </summary>
		private readonly Type _valueType;

		/// <summary>
		/// Initializes a new instance of the <see cref="TypeMemberInfo"/> structure.
		/// </summary>
		/// <param name="memberInfo">The object that contains metadata information about foreign class member.</param>
		/// <param name="entityType">The type of the class that contains member, specified by <paramref name="memberInfo"/>.</param>
		/// <param name="valueType">The type of value that created delegate must use.</param>
		public TypeMemberInfo(MemberInfo memberInfo, Type entityType = null, Type valueType = null)
		{
			_memberInfo = memberInfo;
			_entityType = entityType;
			_valueType = valueType;
		}

		/// <summary>
		/// Gets the object that contains metadata information about foreign class field.
		/// </summary>
		public FieldInfo FieldInfo
		{
			get { return _memberInfo as FieldInfo; }
		}

		/// <summary>
		/// Gets the object that contains metadata information about foreign class property.
		/// </summary>
		public PropertyInfo PropertyInfo
		{
			get { return _memberInfo as PropertyInfo; }
		}

		/// <summary>
		/// Gets the object that contains metadata information about foreign class method.
		/// </summary>
		public MethodBase MethodBase
		{
			get { return _memberInfo as MethodBase; }
		}

		/// <summary>
		/// Gets the type of the class that contains member.
		/// </summary>
		public Type EntityType
		{
			get { return _entityType; }
		}

		/// <summary>
		/// Gets the type of value that created delegate must use.
		/// </summary>
		public Type ValueType
		{
			get { return _valueType; }
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		/// <filterpriority>2</filterpriority>
		public override Int32 GetHashCode()
		{
			unchecked
			{
				var hashCode = _memberInfo.GetHashCode();

				if (_entityType != null)
					hashCode = (hashCode * 397) ^ _entityType.GetHashCode();

				if (_valueType != null)
					hashCode = (hashCode * 397) ^ _valueType.GetHashCode();

				return hashCode;
			}
		}

		/// <summary>
		/// Indicates whether this instance and a specified object are equal.
		/// </summary>
		/// <returns>
		/// true if <paramref name="otherObject"/> and this instance are the same type and represent the same value; otherwise, false.
		/// </returns>
		/// <param name="otherObject">Another object to compare to. </param><filterpriority>2</filterpriority>
		public override Boolean Equals(Object otherObject)
		{
			if (!(otherObject is TypeMemberInfo))
				return false;

			var other = (TypeMemberInfo)otherObject;
			return _memberInfo == other._memberInfo && _entityType == other._entityType && _valueType == other._valueType;
		}
	}
}