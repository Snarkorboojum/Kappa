using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Kappa.Core.Recycling.Dynamic
{
	/// <summary>
	/// Contains the information that needed for the building dynamic recyclable type.
	/// </summary>
	internal struct RecyclingGeneratorContext
	{
		#region Fields

		/// <summary>
		/// The source type for recycling.
		/// </summary>
		private readonly Type _sourceType;

		/// <summary>
		/// The <see cref="TypeBuilder"/> object that contains information about building new dynamic type.
		/// </summary>
		private readonly TypeBuilder _typeBuilder;

		/// <summary>
		/// The <see cref="FieldBuilder"/> object that contains information about implementaion of the backing field for the <see cref="IRecyclable.IsInFactory"/> property.
		/// </summary>
		private readonly FieldBuilder _isInFactoryField;

		/// <summary>
		/// The <see cref="FieldBuilder"/> object that contains information about implementaion of the backing field for the <see cref="IRecyclable.SourceFactory"/> property.
		/// </summary>
		private readonly FieldBuilder _sourceFactoryField;

		/// <summary>
		/// The <see cref="FieldBuilder"/> object that contains information about implementaion of the backing field for the <see cref="IRecyclableExtended.CanRecycle"/> property.
		/// </summary>
		private readonly FieldBuilder _recyclingSuppressedCountField;

		/// <summary>
		/// The dictionary that contains delegates to access non public methods, fields and properties.
		/// </summary>
		private readonly IDictionary<FieldBuilder, Delegate> _membersAccessors;

		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="RecycleFactorySettings"/> structure.
		/// </summary>
		/// <param name="sourceType">The source type for recycling.</param>
		/// <param name="typeBuilder">The <see cref="TypeBuilder"/> object that contains information about building new dynamic type.</param>
		/// <param name="isInFactoryField">The <see cref="FieldBuilder"/> object that contains information about implementaion of the backing field for the <see cref="IRecyclable.IsInFactory"/> property.</param>
		/// <param name="sourceFactoryField">The <see cref="FieldBuilder"/> object that contains information about implementaion of the backing field for the <see cref="IRecyclable.SourceFactory"/> property.</param>
		/// <param name="recyclingSuppressedCount">The <see cref="FieldBuilder"/> object that contains information about implementation of the backing field for the <see cref=" IRecyclableExtended.CanRecycle"/> property.</param>
		public RecyclingGeneratorContext(Type sourceType, TypeBuilder typeBuilder, FieldBuilder isInFactoryField, FieldBuilder sourceFactoryField, FieldBuilder recyclingSuppressedCount)
		{
			_sourceType = sourceType;
			_typeBuilder = typeBuilder;
			_isInFactoryField = isInFactoryField;
			_sourceFactoryField = sourceFactoryField;
			_recyclingSuppressedCountField = recyclingSuppressedCount;
			_membersAccessors = new Dictionary<FieldBuilder, Delegate>();
		}

		/// <summary>
		/// Gets the source type for recycling.
		/// </summary>
		public Type SourceType
		{
			get { return _sourceType; }
		}

		/// <summary>
		/// Gets the <see cref="TypeBuilder"/> object that contains information about building new dynamic type.
		/// </summary>
		public TypeBuilder TypeBuilder
		{
			get { return _typeBuilder; }
		}

		/// <summary>
		/// Gets the <see cref="FieldBuilder"/> object that contains information about implementaion of the backing field for the <see cref="IRecyclable.IsInFactory"/> property.
		/// </summary>
		public FieldBuilder IsInFactoryField
		{
			get { return _isInFactoryField; }
		}

		/// <summary>
		/// Gets the <see cref="FieldBuilder"/> object that contains information about implementaion of the backing field for the <see cref="IRecyclable.SourceFactory"/> property.
		/// </summary>
		public FieldBuilder SourceFactoryField
		{
			get { return _sourceFactoryField; }
		}

		/// <summary>
		/// Gets the <see cref="FieldBuilder"/> object that contains information about implementaion of the backing field for the <see cref="IRecyclableExtended.CanRecycle"/> property.
		/// </summary>
		public FieldBuilder RecyclingSuppressedCountField
		{
			get { return _recyclingSuppressedCountField; }
		}

		/// <summary>
		/// Gets the dictionary that contains delegates to access non public methods, fields and properties.
		/// </summary>
		public IDictionary<FieldBuilder, Delegate> MembersAccessors
		{
			get { return _membersAccessors; }
		}

		/// <summary>
		/// Gets the hierarchy of the base types from source type.
		/// </summary>
		/// <returns>The enumerable collection of types sorted in order from subtypes to base types.</returns>
		public IEnumerable<Type> GetSourceTypesHierarchy()
		{
			var typesHierarchy = new List<Type>();

			for (var currentType = _sourceType; currentType != null; currentType = currentType.BaseType)
				typesHierarchy.Add(currentType);

			return typesHierarchy;
		}

		/// <summary>
		/// Creates the static field in dynamic type to store delegate to access private member of source type.
		/// </summary>
		/// <param name="memberAccessor">The delegate that can somehow(get, set, invoke) access member of the source type.</param>
		/// <returns>The <see cref="FieldBuilder"/> object that contains informatio about created static field.</returns>
		public FieldBuilder CreateMemberAccessorField(Delegate memberAccessor)
		{
			var memberAccessorType = memberAccessor.GetType();
			var fieldBuilder = _typeBuilder.DefineField("_f_" + _membersAccessors.Count, memberAccessorType, FieldAttributes.Private | FieldAttributes.Static);

			_membersAccessors.Add(fieldBuilder, memberAccessor);
			return fieldBuilder;
		}

		/// <summary>
		/// Creates dynamic type.
		/// </summary>
		/// <returns>The newly created dynamic type.</returns>
		public Type CreateType()
		{
			var result = _typeBuilder.CreateType();

			// Fill the static fields of generated class with obtained while generation members accessors.
			foreach (var pair in _membersAccessors)
			{
				var fieldBuilder = pair.Key;
				var accessorDelegate = pair.Value;
				var fieldInfo = result.GetField(fieldBuilder.Name,
					BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);

				if (fieldInfo == null)
					throw new InvalidOperationException(String.Format("The field with name {0} was not found in generated type.", fieldBuilder.Name));

				fieldInfo.SetValue(null, accessorDelegate);
			}

			return result;
		}
	}
}