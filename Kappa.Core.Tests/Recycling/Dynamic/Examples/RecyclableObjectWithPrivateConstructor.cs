using System;
using Kappa.Core.Recycling.Dynamic.Attributes;
using Kappa.Core.Recycling.Dynamic.Strategies;

namespace Kappa.Core.Tests.Recycling.Dynamic.Examples
{
	/// <summary>
	/// Provides example of recyclable class with private constructor for the <see cref="RecyclingGeneratorTests"/>.
	/// </summary>
	[Recyclable(RecyclingStrategyType.Include)]
	public class RecyclableObjectWithPrivateConstructor
	{
		private RecyclableObjectWithPrivateConstructor()
		{
			SomeProperty++;
		}

		public Int32 SomeProperty { get; set; }
	}
}