using System;
using System.Diagnostics.CodeAnalysis;
using Kappa.Core.Recycling.Dynamic.Attributes;
using Kappa.Core.Recycling.Dynamic.Strategies;

namespace Kappa.Core.Tests.Recycling.Dynamic.Examples
{
	/// <summary>
	/// Provides example of recyclable class with properties that nested from other recyclable class for the <see cref="RecyclingGeneratorTests"/>.
	/// </summary>
	[Recyclable(RecyclingStrategyType.Include)]
	public class RecyclableObjectWithProperties : RecyclableObjectWithFields
	{
		[SuppressMessage("StyleCopPlus.StyleCopPlusRules", "SP0100:AdvancedNamingRules")]
		[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate")]
		public Boolean _isSomethingHappened;

		[Recycle, RecycleCleanValue(true)]
		public Boolean IsSomething { get; set; }

		[Recycle, RecycleCleanValue(true)]
		private Boolean IsSomethingHappened
		{
			get { return _isSomethingHappened; }
			set { _isSomethingHappened = value; }
		}
	}
}