using System;
using System.Runtime.CompilerServices;
using Eco.Recycling;
using Kappa.Core.Recycling.Dynamic.Attributes;
using Kappa.Core.Reflection.Emit;

namespace Kappa.Core.Recycling.Dynamic
{
	/// <summary>
	/// Represents a factory that produces <see cref="IRecyclable"/> objects from dynamically created type to reuse them if possible to decrease memory consumption.
	/// </summary>
	/// <typeparam name="T">The type that is marked by <see cref="RecyclableAttribute"/>.</typeparam>
	public sealed class DynamicRecycleFactory<T> : DynamicRecycleFactoryBase
	{
		/// <summary>
		/// The constructor of the recycle factory for specified <typeparamref name="T"/>.
		/// </summary>
		private static readonly Func<StoragePolicy, IRecycleFactory> RecycleFactoryConstructor;

		#region Constructors

		/// <summary>
		/// Initializes static fields of the <see cref="DynamicRecycleFactory{T}"/> class.
		/// </summary>
		static DynamicRecycleFactory()
		{
			var type = typeof(T);
			Type recyclableType;
			if (!DynamicRecycleFactory.TryGetRecyclableType(type, out recyclableType))
				throw new InvalidOperationException(String.Format("The type {0} is not implement {1} interface and does not marked by {2} attribute.", type.Name, typeof(IRecyclable).Name, typeof(RecyclableAttribute).Name));

			var recycleFactoryType = typeof(DefaultRecycleFactory<>).MakeGenericType(recyclableType);

			RecycleFactoryConstructor = (Func<StoragePolicy, IRecycleFactory>)ClassBuilder.GetConstructor(recycleFactoryType, new[] { typeof(StoragePolicy) });
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DynamicRecycleFactory{T}"/> class.
		/// </summary>
		public DynamicRecycleFactory()
			: this(StoragePolicy.ThreadStatic)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DynamicRecycleFactory{T}"/> class.
		/// </summary>
		/// <param name="storagePolicy">A storing policy for recycled items.</param>
		public DynamicRecycleFactory(StoragePolicy storagePolicy)
			: base(RecycleFactoryConstructor(storagePolicy))
		{
		}

		#endregion

		/// <summary>
		/// Creates a new recyclable item.
		/// </summary>
		/// <returns>The item of <typeparamref name="T"/> class.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Create()
		{
			return (T)InnerRecycleFactory.Create();
		}
	}
}