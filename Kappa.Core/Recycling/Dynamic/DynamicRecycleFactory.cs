using System;
using System.Runtime.CompilerServices;
using Eco.Recycling;
using Kappa.Core.Recycling.Dynamic.Attributes;
using Kappa.Core.Reflection.Emit;
using Kappa.Core.System;

namespace Kappa.Core.Recycling.Dynamic
{
	/// <summary>
	/// Represents a factory that produces <see cref="IRecyclable"/> objects from dynamically created type to reuse them if possible to decrease memory consumption.
	/// </summary>
	internal sealed class DynamicRecycleFactory : DynamicRecycleFactoryBase
	{
		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="DynamicRecycleFactory"/> class.
		/// </summary>
		/// <param name="type">The type that is marked by <see cref="RecyclableAttribute"/> or implements <see cref="IRecyclable"/> interface.</param>
		public DynamicRecycleFactory(Type type)
			: this(type, StoragePolicy.ThreadStatic)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DynamicRecycleFactory"/> class.
		/// </summary>
		/// <param name="type">The type that is marked by <see cref="RecyclableAttribute"/> or implements <see cref="IRecyclable"/> interface.</param>
		/// <param name="storagePolicy">A storing policy for recycled items.</param>
		public DynamicRecycleFactory(Type type, StoragePolicy storagePolicy)
			: base(CreateRecycleFactory(type, storagePolicy))
		{
		}

		#endregion

		/// <summary>
		/// Determines tha specified type can be recycled.
		/// </summary>
		/// <param name="type">The type to verify for recyclableness.</param>
		/// <returns>True if recycling is possible for specified type; otherwise, false.</returns>
		public static Boolean IsRecyclingSupported(Type type)
		{
			return type.HasAttribute<RecyclableAttribute>() || typeof(IRecyclable).IsAssignableFrom(type);
		}

		/// <summary>
		/// Creates the recycle factory for dynamic type or source type in case it implements <see cref="IRecyclable"/> interface.
		/// </summary>
		/// <param name="type">The type that is marked by <see cref="RecyclableAttribute"/> or implements <see cref="IRecyclable"/> interface.</param>
		/// <param name="storagePolicy">A storing policy for recycled items.</param>
		/// <returns>The newly created recycle factory.</returns>
		private static IRecycleFactory CreateRecycleFactory(Type type, StoragePolicy storagePolicy)
		{
			Type recyclableType;
			if (!TryGetRecyclableType(type, out recyclableType))
				throw new ArgumentException(String.Format("The type {0} is not implement {1} interface and does not marked by {2} attribute.", type.Name, typeof(IRecyclable).Name, typeof(RecyclableAttribute).Name));

			var recycleFactoryType = typeof(DefaultRecycleFactory<>).MakeGenericType(recyclableType);
			var recycleFactoryConstructor = (Func<StoragePolicy, IRecycleFactory>)ClassBuilder.GetConstructor(recycleFactoryType, new[] { typeof(StoragePolicy) });

			return recycleFactoryConstructor(storagePolicy);
		}

		/// <summary>
		/// Tries to get recyclable type from specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type">The source type.</param>
		/// <param name="recyclableType">The type that implements <see cref="IRecyclable"/> interface in case of success.</param>
		/// <returns>True if operation completes successfully; otherwise, false.</returns>
		public static Boolean TryGetRecyclableType(Type type, out Type recyclableType)
		{
			if (type.HasAttribute<RecyclableAttribute>())
				recyclableType = RecyclableTypeGenerator.Instance.GetRecyclable(type);
			else if (typeof(IRecyclable).IsAssignableFrom(type))
				recyclableType = type;
			else
				recyclableType = null;

			return recyclableType != null;
		}

		/// <summary>
		/// Creates the new one or gets the instance of the <see cref="IRecyclable"/> item from factory.
		/// </summary>
		/// <returns>The recyclable item.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IRecyclable Create()
		{
			return InnerRecycleFactory.Create();
		}
	}
}