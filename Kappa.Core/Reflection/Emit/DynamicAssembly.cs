using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;

namespace Kappa.Core.Reflection.Emit
{
	/// <summary>
	/// Provides functionality to create dynamic assembly for IL code emitting.
	/// </summary>
	public static class DynamicAssembly
	{
		// -------------------------------------------------------------------
		#region Constants

		/// <summary>
		/// The name of the dynamically created asembly.
		/// </summary>
		/// <remarks>This is root namespace of the dynamic assembly in which all dinamically emitted types and methods can be found.</remarks>
		private const String DynamicAssemblyName = "Kappa.Core.Ghost";

		/// <summary>
		/// The extension of the dynamic assembly file.
		/// </summary>
		private const String DynamicAssemblyExtension = ".dll";

		/// <summary>
		/// The name of the dynamica assembly sign key file in current assembly's embedded resources.
		/// </summary>
		/// <remarks>If you want sign dynamic assembly (for example - to allow access to internal resources of another assembly) than simply add to this library sign key into embedded resources with this name.</remarks>
		private const String DynamicAssemblySignKeyName = "DynamicKey.snk";

		#endregion Constants

		#region Fields

		/// <summary>
		/// The object that describes dynamic assembly in which IL code dinamically emitted.
		/// </summary>
		static private AssemblyBuilder _assemblyBuilder;

		/// <summary>
		/// The object that describes the module in the dynamic assembly in which IL code dinamically emitted.
		/// </summary>
		static private ModuleBuilder _moduleBuilder;

		#endregion Fields

		#region Properties

		/// <summary>
		/// The object that describes the module in the dynamic assembly in which IL code dinamically emitted.
		/// </summary>
		static public ModuleBuilder ModuleBuilder
		{
			get
			{
				if (_moduleBuilder == null)
				{
					var assemblyName = new AssemblyName(DynamicAssemblyName);

					var currentAssembly = Assembly.GetExecutingAssembly();
					using (var resourceStream = currentAssembly.GetManifestResourceStream(String.Format("{0}.{1}", currentAssembly.GetName().Name, DynamicAssemblySignKeyName)))
					{
						if (resourceStream != null)
						{
							var bytes = new Byte[resourceStream.Length];

							resourceStream.Read(bytes, 0, bytes.Length);
							assemblyName.KeyPair = new StrongNameKeyPair(bytes);
						}
					}

					_assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
					_moduleBuilder = _assemblyBuilder.DefineDynamicModule(assemblyName.Name, assemblyName.Name + DynamicAssemblyExtension);
				}
				return _moduleBuilder;
			}
		}

		#endregion Properties

		/// <summary>
		/// Saves the dynamic assembly with all emitted so far types and methods in the file.
		/// </summary>
		/// <remarks>
		/// For DEBUG use only.
		/// After call of this method next emitting in current dynamic assembly will lead to exception.
		/// </remarks>
		[Conditional("DEBUG")]
		public static void Save()
		{
			if (_assemblyBuilder != null && ModuleBuilder != null)
				_assemblyBuilder.Save(ModuleBuilder.ScopeName + DynamicAssemblyExtension);
		}
	}
}