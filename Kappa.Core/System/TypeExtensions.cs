using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace Kappa.Core.System
{
	/// <summary>
	/// Provides extension methods for the <see cref="Type"/> class.
	/// </summary>
	public static class TypeExtensions
	{
		/// <summary>
		/// The <see cref="MethodInfo"/> value that describes the <see cref="Default{T}"/> method of this class.
		/// </summary>
		private static readonly MethodInfo DefaultValueMethodInfo;

		/// <summary>
		/// Initializes the <see cref="TypeExtensions"/> type.
		/// </summary>
		static TypeExtensions()
		{
			DefaultValueMethodInfo = typeof(TypeExtensions).GetMethod("Default", BindingFlags.NonPublic | BindingFlags.Static, null, Type.EmptyTypes, new[] { new ParameterModifier() });
		}

		/// <summary>
		/// Gets the default value for specified <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">The type for which default value will be returned.</typeparam>
		/// <returns>The default value of the <typeparamref name="T"/>.</returns>
		private static T Default<T>() { return default(T); }

		/// <summary>
		/// Gets the default value for specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type">The type for which default value will be returned.</param>
		/// <returns>The default value of the <paramref name="type"/>.</returns>
		public static object Default(this Type type)
		{
			var methodInfo = DefaultValueMethodInfo.MakeGenericMethod(new[] { type });

			return methodInfo.Invoke(null, null);
		}

		/// <summary>
		/// Determines that <paramref name="type"/> is marked by <typeparamref name="TAttribute"/>.
		/// </summary>
		/// <param name="type">The verified type.</param>
		/// <typeparam name="TAttribute">The type of attribute that will be searched in <paramref name="type"/>.</typeparam>
		/// <returns>True if type marked by specified attribute; otherwise, false.</returns>
		public static Boolean HasAttribute<TAttribute>(this Type type)
			where TAttribute : Attribute
		{
			return TypeAttributeInformation<TAttribute>.HasAttribute(type);
		}

		/// <summary>
		/// Gets the all public and getter methods of the public properties of specified type.
		/// </summary>
		/// <param name="type">The type from which public and getter methods will be obtained.</param>
		/// <returns>The array of the <see cref="MethodInfo"/> objects that describes the public and getter methods of the specified type.</returns>
		public static MethodInfo[] GetPublicMethodsAndPropertyGetters(this Type type)
		{
			return
				(from method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
					where method.DeclaringType == type
					select method)
					.Union(
						from property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
						let getMethod = property.GetGetMethod()
						where getMethod != null && getMethod.DeclaringType == type
						select getMethod)
					.ToArray();
		}

		/// <summary>
		/// Provides functionality to determine: is type is marked by specified attribute or not and caches the results.
		/// </summary>
		/// <typeparam name="TAttribute">The type of attribute to find.</typeparam>
		private static class TypeAttributeInformation<TAttribute>
			where TAttribute : Attribute
		{
			#region Fields

			/// <summary>
			/// The dictionary that contains information about presense of <typeparamref name="TAttribute"/> for each type.
			/// </summary>
			private static readonly ConcurrentDictionary<Type, Boolean> AttributeTypeInformation;

			/// <summary>
			/// The delegate to cache <see cref="GetTypeAttributeInformationAction"/> method.
			/// </summary>
			private static readonly Func<Type, Boolean> GetTypeAttributeInformationDelegate;

			#endregion

			/// <summary>
			/// Initializes the <see cref="TypeAttributeInformation{TAttribute}"/> type.
			/// </summary>
			static TypeAttributeInformation()
			{
				AttributeTypeInformation = new ConcurrentDictionary<Type, Boolean>();
				// ReSharper disable once HeapView.SlowDelegateCreation
				GetTypeAttributeInformationDelegate = GetTypeAttributeInformationAction;
			}

			/// <summary>
			/// Determines that <paramref name="type"/> is marked with <typeparamref name="TAttribute"/>.
			/// </summary>
			/// <param name="type">The verified type.</param>
			/// <returns>True if type has attribute; otherwise, false.</returns>
			public static Boolean HasAttribute(Type type)
			{
				return AttributeTypeInformation.GetOrAdd(type, GetTypeAttributeInformationDelegate);
			}

			/// <summary>
			/// Determines that <paramref name="type"/> is marked with <typeparamref name="TAttribute"/>.
			/// </summary>
			/// <param name="type">The verified type.</param>
			/// <returns>True if type has attribute; otherwise, false.</returns>
			private static Boolean GetTypeAttributeInformationAction(Type type)
			{
				return type.GetCustomAttribute<TAttribute>() != null;
			}
		}
	}
}