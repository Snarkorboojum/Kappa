using System;
using Eco.Recycling;

namespace Kappa.Core.Tests.Recycling.Dynamic
{
	/// <summary>
	/// Provides additional functionality to wotk with dynamically generated recycling types.
	/// </summary>
	internal static class RecycleFactoryExtensions
	{
		/// <summary>
		/// Creates the object of <typeparamref name="T"/> type with help of specified <paramref name="factory"/>.
		/// </summary>
		/// <typeparam name="T">The type of creating object.</typeparam>
		/// <param name="factory">A factory that have ability to produce objects of type <typeparamref name="T"/>.</param>
		/// <returns>The object of type <typeparamref name="T"/>.</returns>
		public static T Create<T>(this IRecycleFactory factory)
			where T : class
		{
			var createMethod = factory.GetType().GetMethod("Create");
			var recycleObject = createMethod.Invoke(factory, null) as T;

			return recycleObject;
		}

		/// <summary>
		/// Return specified <paramref name="someObject"/> into the recycle factory if it implements <see cref="IRecyclable"/> interface.
		/// </summary>
		/// <param name="someObject">An object to recycle.</param>
		public static void Release(this Object someObject)
		{
			var recyclableObject = someObject as IRecyclable;

			if (recyclableObject != null)
				recyclableObject.Free();
		}
	}
}