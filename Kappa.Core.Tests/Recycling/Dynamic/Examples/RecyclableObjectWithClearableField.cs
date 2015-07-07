using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Kappa.Core.Recycling.Dynamic.Attributes;
using Kappa.Core.Recycling.Dynamic.Strategies;

namespace Kappa.Core.Tests.Recycling.Dynamic.Examples
{
	/// <summary>
	/// Provides example of recyclable class with fields that must be cleared when recycle instead of zeroing for the <see cref="RecyclingGeneratorTests"/>.
	/// </summary>
	[Recyclable(RecyclingStrategyType.Include)]
	public class RecyclableObjectWithClearableField
	{
		[Recycle, RecycleCleanMethod("Clear")]
		private readonly List<Int32> _somePrivateList;

		[SuppressMessage("StyleCopPlus.StyleCopPlusRules", "SP0100:AdvancedNamingRules")]
		[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate")]
		[Recycle, RecycleCleanMethod("Clear")]
		public List<Int32> _somePublicList;

		public RecyclableObjectWithClearableField()
		{
			_somePrivateList = new List<Int32>();
			_somePublicList = new List<Int32>();
		}

		public List<Int32> SomePrivateList
		{
			get { return _somePrivateList; }
		}
	}
}