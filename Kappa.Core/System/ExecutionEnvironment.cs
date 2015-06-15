using System;
using System.IO;

namespace Kappa.Core.System
{
	/// <summary>
	/// Provides information about current execution environment.
	/// </summary>
	public static class ExecutionEnvironment
	{
		/// <summary>
		/// Initializes the <see cref="ExecutionEnvironment"/> type.
		/// </summary>
		static ExecutionEnvironment()
		{
			var platformId = Environment.OSVersion.Platform;

			// TODO Implement PlatformID.MacOSX in mono, because this implementation is sucks.
			if (platformId == PlatformID.Unix && File.Exists("/mach_kernel"))
				platformId = PlatformID.MacOSX;

			switch (platformId)
			{
				case PlatformID.Win32NT:
					Platform = Platform.Windows;
					break;

				case PlatformID.Unix:
					Platform = Platform.Unix;
					break;

				case PlatformID.MacOSX:
					Platform = Platform.Mac;
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}

			IsMono = Type.GetType("Mono.Runtime") != null;
		}

		#region Properties

		/// <summary>
		/// Gets the <see cref="Boolean"/> value indicating that code is executing under the mono regarless of the operating system.
		/// </summary>
		public static readonly Boolean IsMono;

		/// <summary>
		/// Gets the <see cref="Platform"/> value that describes the type of the current operating system.
		/// </summary>
		public static readonly Platform Platform;

		#endregion Properties
	}
}