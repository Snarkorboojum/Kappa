using System;
using System.Collections.Generic;
using Kappa.Core.Recycling.Dynamic.Attributes;
using Kappa.Core.Recycling.Dynamic.Strategies;

namespace Kappa.Core.Tests.Recycling.Dynamic.Examples
{
	/// <summary>
	/// Provides recyclable with alternative strategy class for the <see cref="RecyclingGeneratorTests"/>.
	/// </summary>
	[Recyclable(RecyclingStrategyType.Exclude)]
	public class RecyclableObjectWithExcludeStrategy
	{
#pragma warning disable 649
		[RecycleCleanValue(true)]
		private Boolean _someBoolean;

		[RecycleCleanValue("This is the empty string")]
		private String _someString;
#pragma warning restore 649

		public RecyclableObjectWithExcludeStrategy()
		{
			NullableList = new List<Int32>();
			ClearableList = new List<Int32>();
			SomethingHappened += () => { };
		}

		[RecycleIgnore]
		public Boolean SomeBoolean
		{
			get { return _someBoolean; }
		}

		[RecycleIgnore]
		public String SomeString
		{
			get { return _someString; }
		}

		public Int32 SomeAutoProperty { get; set; }

		[RecycleCleanValue(1)]
		public Int32 SomeAutoPropertyWithValue { get; set; }

		public List<Int32> NullableList { get; private set; }

		[RecycleCleanMethod("Clear")]
		public List<Int32> ClearableList { get; private set; }

		public void FireSomethingHappened()
		{
			SomethingHappened();
		}

		private event Action SomethingHappened;
	}
}