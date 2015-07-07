using System;
using System.Runtime.CompilerServices;
using Eco.Recycling;

namespace Kappa.Core.Tests.Injection.Examples
{
	/// <summary>
	/// The example of the recyclable object for entrapment test.
	/// </summary>
	public class RecyclableObject : IRecyclable
	{
		public Object Property { get; set; }

		[MethodImpl(MethodImplOptions.NoInlining)]
		public Int32 Method(String stringParameter, Double doubleParameter, out Boolean result)
		{
			result = true;
			return 0;
		}

		#region Implementation of the IRecyclable interface

		private IRecycleFactory _factory;

		private Boolean _isInFactory;

		IRecycleFactory IRecyclable.SourceFactory
		{
			get { return _factory; }
		}

		Boolean IRecyclable.IsInFactory
		{
			get { return _isInFactory; }
		}

		void IRecyclable.Resolve()
		{
			_isInFactory = false;
		}

		void IRecyclable.Cleanup()
		{
			_isInFactory = true;
		}

		void IRecyclable.SetSourceFactory(IRecycleFactory factory)
		{
			_factory = factory;
		}

		#endregion
	}
}