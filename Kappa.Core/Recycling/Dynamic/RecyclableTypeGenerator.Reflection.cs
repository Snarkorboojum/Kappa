using System;
using System.Reflection;
using System.Reflection.Emit;
using Kappa.Core.Recycling.Dynamic.Attributes;
using Kappa.Core.System;

namespace Kappa.Core.Recycling.Dynamic
{
	public sealed partial class RecyclableTypeGenerator
	{
		#region Creating code to cleanup fields

		/// <summary>
		/// Creates code that cleans up the fields of the specified type.
		/// </summary>
		/// <param name="instanceType">The type that owns members that will be used in emiting cleanup code.</param>
		/// <param name="context">The <see cref="RecyclingGeneratorContext"/> object that contains information about dynamic type.</param>
		/// <param name="ilGenerator">The generator of IL code for current method.</param>
		/// <param name="isRecyclableFunction">The filter that distinguish members of the class that must be recycled.</param>
		internal static void CreateCodeToCleanupFields(Type instanceType, RecyclingGeneratorContext context, ILGenerator ilGenerator, IsRecyclableFunction isRecyclableFunction)
		{
			var fields = instanceType.GetFields(InstanceBindingFlags);

			foreach (var field in fields)
			{
				var attributes = Attribute.GetCustomAttributes(field);

				if (!isRecyclableFunction(instanceType, field, ref attributes))
					continue;

				CreateCodeToCleanupField(context, ilGenerator, attributes, field);
			}
		}

		/// <summary>
		/// Creates code that cleans up the <paramref name="field"/> of the specified type.
		/// </summary>
		/// <param name="context">The <see cref="RecyclingGeneratorContext"/> object that contains information about dynamic type.</param>
		/// <param name="ilGenerator">The generator of IL code for current method.</param>
		/// <param name="attributes">An array of the type's member attributes.</param>
		/// <param name="field">An object that describes field that must be cleans up.</param>
		private static void CreateCodeToCleanupField(RecyclingGeneratorContext context, ILGenerator ilGenerator, Attribute[] attributes, FieldInfo field)
		{
			var cleanAttribute = GetCustomAttribute<RecycleCleanAttribute>(attributes);
			var valueAttribute = cleanAttribute as RecycleCleanValueAttribute;
			var methodAttribute = cleanAttribute as RecycleCleanMethodAttribute;

			if (methodAttribute != null)
			{
				var methodInfo = field.FieldType.GetMethod(methodAttribute.MethodName, InstanceBindingFlags);
				if (methodInfo == null)
					throw new InvalidOperationException(String.Format("The method with name \"{0}\" is not found", methodAttribute.MethodName));

				CreateInvokeMethod(context, ilGenerator, methodInfo, _emitFieldDelegate, field);
			}
			else
			{
				var fieldValue = valueAttribute == null ? field.FieldType.Default() : valueAttribute.Value;

				CreateSetField(context, ilGenerator, field, fieldValue);
			}
		}

		#endregion

		#region Creating code to cleanup properties

		/// <summary>
		/// Creates code that cleans up the properties of the specified type.
		/// </summary>
		/// <param name="instanceType">The type that owns members that will be used in emiting cleanup code.</param>
		/// <param name="context">The <see cref="RecyclingGeneratorContext"/> object that contains information about dynamic type.</param>
		/// <param name="ilGenerator">The generator of IL code for current method.</param>
		/// <param name="isRecyclableFunction">The filter that distinguish members of the class that must be recycled.</param>
		internal static void CreateCodeToCleanupProperties(Type instanceType, RecyclingGeneratorContext context, ILGenerator ilGenerator, IsRecyclableFunction isRecyclableFunction)
		{
			var properties = instanceType.GetProperties(InstanceBindingFlags);

			foreach (var property in properties)
			{
				var attributes = Attribute.GetCustomAttributes(property);

				if (!isRecyclableFunction(instanceType, property, ref attributes))
					continue;

				CreateCodeToCleanupProperty(context, ilGenerator, attributes, property);
			}
		}

		/// <summary>
		/// Creates code that cleans up the <paramref name="property"/> of the specified type.
		/// </summary>
		/// <param name="context">The <see cref="RecyclingGeneratorContext"/> object that contains information about dynamic type.</param>
		/// <param name="ilGenerator">The generator of IL code for current method.</param>
		/// <param name="attributes">An array of the type's member attributes.</param>
		/// <param name="property">An object that describes property that must be cleans up.</param>
		private static void CreateCodeToCleanupProperty(RecyclingGeneratorContext context, ILGenerator ilGenerator, Attribute[] attributes, PropertyInfo property)
		{
			var cleanAttribute = GetCustomAttribute<RecycleCleanAttribute>(attributes);
			var valueAttribute = cleanAttribute as RecycleCleanValueAttribute;
			var methodAttribute = cleanAttribute as RecycleCleanMethodAttribute;

			if (methodAttribute != null)
			{
				var methodInfo = property.PropertyType.GetMethod(methodAttribute.MethodName, InstanceBindingFlags);

				if (methodInfo == null)
					throw new InvalidOperationException(String.Format("The method with name \"{0}\" is not found", methodAttribute.MethodName));

				CreateInvokeMethod(context, ilGenerator, methodInfo, _emitPropertyDelegate, property);
			}
			else
			{
				var propertyValue = valueAttribute == null ? property.PropertyType.Default() : valueAttribute.Value;

				CreateSetProperty(context, ilGenerator, property, propertyValue);
			}
		}

		#endregion

		#region Creating code to cleanup events

		/// <summary>
		/// Creates code that cleans up the events of the specified type.
		/// </summary>
		/// <param name="instanceType">The type that owns members that will be used in emiting cleanup code.</param>
		/// <param name="context">The <see cref="RecyclingGeneratorContext"/> object that contains information about dynamic type.</param>
		/// <param name="ilGenerator">The generator of IL code for current method.</param>
		/// <param name="isRecyclableFunction">The filter that distinguish members of the class that must be recycled.</param>
		internal static void CreateCodeToCleanupEvents(Type instanceType, RecyclingGeneratorContext context, ILGenerator ilGenerator, IsRecyclableFunction isRecyclableFunction)
		{
			var events = instanceType.GetEvents(InstanceBindingFlags);

			foreach (var eventInfo in events)
			{
				var attributes = Attribute.GetCustomAttributes(eventInfo);

				if (!isRecyclableFunction(instanceType, eventInfo, ref attributes))
					continue;

				var field = instanceType.GetField(eventInfo.Name, InstanceBindingFlags);
				if (field == null)
					throw new InvalidOperationException(String.Format("Attempt to get event field with name {0} is failed.", eventInfo.Name));

				CreateCodeToCleanupField(context, ilGenerator, attributes, field);
			}
		}

		#endregion

		#region Additional metadata methods.

		/// <summary>
		/// Find the attribute of type <typeparamref name="T"/> in the specified array.
		/// </summary>
		/// <typeparam name="T">The type of attribute that needs to be found.</typeparam>
		/// <param name="attributes">An array of attributes of the class member.</param>
		/// <returns>The found attribute of type <typeparamref name="T"/> in case of success; otheriwse, null.</returns>
		private static T GetCustomAttribute<T>(Attribute[] attributes)
			where T : Attribute
		{
			foreach (var attribute in attributes)
			{
				var result = attribute as T;

				if (result != null)
					return result;
			}

			return null;
		}

		#endregion
	}
}