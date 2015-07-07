using System;
using System.Runtime.CompilerServices;
using Eco.Recycling;

namespace Kappa.Core.Recycling.Dynamic
{
	/// <summary>
	/// Provides base functionality to dynamic recycle factories.
	/// </summary>
	public abstract class DynamicRecycleFactoryBase : IRecycleFactory
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DynamicRecycleFactory"/> class.
		/// </summary>
		/// <param name="innerRecycleFactory">The default implementation of recycle factory.</param>
		protected DynamicRecycleFactoryBase(IRecycleFactory innerRecycleFactory)
		{
			InnerRecycleFactory = innerRecycleFactory;
		}

		/// <summary>
		/// Gets the internal strong-typed implementation of recycle factory.
		/// </summary>
		protected IRecycleFactory InnerRecycleFactory { get; private set; }

		#region Implementation of IRecycleFactory interface.

		/// <summary>
		/// Gets or sets the maximal amount of stored recycled objects which are ready to use.
		/// </summary>
		Int32 IRecycleFactory.Capacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get { return InnerRecycleFactory.Capacity; }

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set { InnerRecycleFactory.Capacity = value; }
		}

		/// <summary>
		/// Recycles provided <paramref name="recyclable"/> object.
		/// </summary>
		/// <param name="recyclable">A not used object.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void IRecycleFactory.Recycle(Object recyclable)
		{
			InnerRecycleFactory.Recycle(recyclable);
		}

		/// <summary>
		/// Creates the new one or gets the instance of the <see cref="IRecyclable"/> item from factory.
		/// </summary>
		/// <returns>The recyclable item.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IRecyclable IRecycleFactory.Create()
		{
			return InnerRecycleFactory.Create();
		}

		#endregion
	}
}