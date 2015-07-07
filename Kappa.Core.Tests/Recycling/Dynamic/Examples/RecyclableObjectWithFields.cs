using System;
using Kappa.Core.Recycling.Dynamic.Attributes;
using Kappa.Core.Recycling.Dynamic.Strategies;

namespace Kappa.Core.Tests.Recycling.Dynamic.Examples
{
	/// <summary>
	/// Provides example of class that have ability to be recycled dynamically for the <see cref="RecyclingGeneratorTests"/>.
	/// </summary>
	[Recyclable(RecyclingStrategyType.Include)]
	public class RecyclableObjectWithFields
	{
		[Recycle, RecycleCleanValue(true)]
		private Boolean _someBoolean;

		[Recycle, RecycleCleanValue("This is the empty string")]
		private String _someString;

		public RecyclableObjectWithFields()
		{
			_someBoolean = false;
			_someString = null;
			SomethingHappened += () => { };
		}

		public Boolean SomeBoolean
		{
			get { return _someBoolean; }
		}

		public String SomeString
		{
			get { return _someString; }
		}

		public void FireSomethingHappened()
		{
			SomethingHappened();
		}

		[Recycle]
		private event Action SomethingHappened;
	}
}