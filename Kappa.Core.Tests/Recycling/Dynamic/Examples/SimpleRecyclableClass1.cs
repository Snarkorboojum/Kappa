using Eco.Recycling;

namespace Kappa.Core.Tests.Recycling.Dynamic.Examples
{
	/// <summary>
	/// Provides example of the class that implements <see cref="IRecyclable"/>.
	/// </summary>
	public class SimpleRecyclableClass1 : SimpleRecyclable
	{
		/// <summary>
		/// Cleans the state of the current object to prepare it to be recycled.
		/// </summary>
		protected override void OnCleanup()
		{
		}
	}
}