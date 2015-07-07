using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Kappa.Core.Recycling.Dynamic.Attributes;
using Kappa.Core.Recycling.Dynamic.Strategies;

namespace Kappa.Core.Tests.Recycling.Dynamic.Examples
{
	/// <summary>
	/// Provides example of the recyclable class with properties that must be cleared when recycle instead of zeroing for the <see cref="RecyclingGeneratorTests"/>.
	/// </summary>
	[Recyclable(RecyclingStrategyType.Include)]
	public class RecyclableObjectWithClearableProperty
	{
		[SuppressMessage("StyleCopPlus.StyleCopPlusRules", "SP0100:AdvancedNamingRules")]
		[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate")]
		public List<Int32> _somePrivateList;

		public RecyclableObjectWithClearableProperty()
		{
			SomePublicList = new List<Int32>();
			SomePrivateList = new List<Int32>();
		}

		[Recycle, RecycleCleanMethod("Clear")]
		public List<Int32> SomePublicList { get; set; }

		[Recycle, RecycleCleanMethod("Clear")]
		private List<Int32> SomePrivateList
		{
			get { return _somePrivateList; }
			set { _somePrivateList = value; }
		}
	}
}