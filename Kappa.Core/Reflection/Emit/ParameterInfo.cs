using System;

namespace Kappa.Core.Reflection.Emit
{
	/// <summary>
	/// Describes the parameter of the method.
	/// </summary>
	internal sealed class ParameterInfo
	{
		/// <summary>
		/// The backing field for <see cref="DefaultValue"/> property.
		/// </summary>
		private Object _defaultValue;

		/// <summary>
		/// Gets or sets a default value of the parameter if parameter have any.
		/// </summary>
		public Object DefaultValue 
		{
			get
			{
				if (!HasDefaultValue) 
					throw new InvalidOperationException("Parameter does not have a default value.");

				return _defaultValue;
			}
			set
			{
				HasDefaultValue = true;
				_defaultValue = value;
			}
		}

		/// <summary>
		/// Gets or sets a value that indicates whether this parameter has a default value. 
		/// </summary>
		public Boolean HasDefaultValue { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this is an input parameter.
		/// </summary>
		public Boolean IsIn { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this parameter is optional.
		/// </summary>
		public Boolean IsOptional { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this is an output parameter.
		/// </summary>
		public Boolean IsOut { get; set; }

		/// <summary>
		/// Gets or sets the name of the parameter.
		/// </summary>
		public String Name { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="Type"/> of the parameter.
		/// </summary>
		public Type ParameterType { get; set; }

		/// <summary>
		/// Gets or sets the zero-based position of the parameter in the formal parameter list.
		/// </summary>
		public Int32 Position { get; set; }
	}
}