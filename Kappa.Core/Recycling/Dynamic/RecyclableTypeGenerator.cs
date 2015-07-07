using System;
using System.Collections.Concurrent;
using System.Reflection;
using Eco.Recycling;
using Kappa.Core.Recycling.Dynamic.Attributes;
using Kappa.Core.Reflection.Emit;

namespace Kappa.Core.Recycling.Dynamic
{
	/// <summary>
	/// Provides functionality to dynamically create nested type that implemented of <see cref="IRecyclable"/> interface.
	/// </summary>
	public sealed partial class RecyclableTypeGenerator
	{
		#region Fields

		/// <summary>
		/// The set of the binding flags to get all of members owned by specified type.
		/// </summary>
		internal const BindingFlags InstanceBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

		/// <summary>
		/// The dictionary that contains dynamically created nested typed that implements <see cref="IRecyclable"/> interface by <see cref="Type.TypeHandle"/>.
		/// </summary>
		private readonly ConcurrentDictionary<Type, Type> _recyclableTypes;

		/// <summary>
		/// The delegate for caching the <see cref="MakeRecyclable"/> method.
		/// </summary>
		private static readonly Func<Type, Type> MakeRecyclableDelegate;

		/// <summary>
		/// The instance of the <see cref="RecyclableTypeGenerator"/> for static use.
		/// </summary>
		public static readonly RecyclableTypeGenerator Instance;

		#endregion

		#region Constuctors

		/// <summary>
		/// Initializes static fields of the <see cref="RecyclableTypeGenerator"/> class.
		/// </summary>
		static RecyclableTypeGenerator()
		{
			MakeRecyclableDelegate = MakeRecyclable;

			InitializeRecyclable();
			InitializeEmitParametersActions();
			Instance = new RecyclableTypeGenerator();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RecyclableTypeGenerator"/> class.
		/// </summary>
		public RecyclableTypeGenerator()
		{
			_recyclableTypes = new ConcurrentDictionary<Type, Type>();
		}

		#endregion

		/// <summary>
		/// Generate from specified <paramref name="parentType"/> new dynamic type that implements <see cref="IRecyclable"/> interface.
		/// </summary>
		/// <param name="parentType">The source type which we want to recycle.</param>
		/// <returns>The newly generated nested from <paramref name="parentType"/> type that implements <see cref="IRecyclable"/> interface.</returns>
		public Type GetRecyclable(Type parentType)
		{
			return _recyclableTypes.GetOrAdd(parentType, MakeRecyclableDelegate);
		}

		/// <summary>
		/// Makes the recyclable type from specified parent type.
		/// </summary>
		/// <param name="sourceType">The source type which we want to recycle.</param>
		/// <returns>The newly generated nested from <paramref name="sourceType"/> type that implements <see cref="IRecyclable"/> interface.</returns>
		private static Type MakeRecyclable(Type sourceType)
		{
			if (sourceType == null)
				throw new ArgumentNullException("sourceType");

			if (sourceType.IsSealed)
				throw new InvalidOperationException(String.Format("Cannot create recyclable class based on the {0} because it is sealed.", sourceType.Name));

			var recyclableAttribute = sourceType.GetCustomAttribute<RecyclableAttribute>();
			if (recyclableAttribute == null)
				throw new InvalidOperationException(String.Format("Cannot recycle instance of the {0} class that does not have Recyclable attribute.", sourceType.Name));

			// Creation of the recycling context.
			var context = CreateRecyclingGeneratorContext(sourceType);

			// Creation of the default constructor.
			CreateRecyclableConstructor(context);

			// Creation SourceFactory property and SetSourceFactory method.
			CreateSourceFactoryMembers(context);

			// Creation IsInFactory property.
			CreateIsInFactoryMembers(context);

			// Implementation of the IRecyclableExtended interface.
			CreateIRecyclablExtendedImplementation(context);

			// Create Cleanup method.
			CreateCleanupMethod(context);

			// Create Resolve method.
			CreateResolveMethod(context);

			return context.CreateType();
		}

		/// <summary>
		/// Creates recycling context for the specified source type.
		/// </summary>
		/// <param name="sourceType">The source type for recycling.</param>
		/// <returns>The newly created recycling generator context that contains information that needed to dynamically create new type.</returns>
		private static RecyclingGeneratorContext CreateRecyclingGeneratorContext(Type sourceType)
		{
			var typeName = "recycle<" + sourceType.Name + ">" + Guid.NewGuid().ToString("N");
			// Creation of the builder for new type to emit dynamic code in it.
			var typeBuilder = ClassBuilder.DefineType(typeName, TypeAttributes.Public, sourceType, new[] { typeof(IRecyclableExtended), typeof(IRecyclable) }, false);
			// Creation of the backing field in new type to implement IRecyclable.SourceFactory property.
			var sourceFactoryField = typeBuilder.DefineField("_sourceFactory", typeof(IRecycleFactory), FieldAttributes.Private);
			// Creation of the backing field in new type to implement IRecyclable.IsInFactory property.
			var isInFactoryField = typeBuilder.DefineField("_isInFactory", typeof(Boolean), FieldAttributes.Private);
			// Creation of the backing field in new type to implement IRecyclableExtended.canRecycle property.
			var recyclingSuppressedCount = typeBuilder.DefineField("_recyclingSuppressedCount", typeof(Int32), new[] { typeof(global::System.Runtime.CompilerServices.IsVolatile) }, new Type[0], FieldAttributes.Private);

			return new RecyclingGeneratorContext(sourceType, typeBuilder, isInFactoryField, sourceFactoryField, recyclingSuppressedCount);
		}
	}
}