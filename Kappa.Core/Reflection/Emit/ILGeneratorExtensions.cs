using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;

namespace Kappa.Core.Reflection.Emit
{
	/// <summary>
	/// Extends the functionality of the <see cref="ILGenerator"/> class.
	/// </summary>
	[SuppressMessage("StyleCopPlus.StyleCopPlusRules", "SP0100:AdvancedNamingRules")]
	// ReSharper disable once InconsistentNaming
	internal static class ILGeneratorExtensions
	{
		/// <summary>
		/// Inserts into IL code that specified by <paramref name="ilGenerator"/> instruction that stores current value in stack into local variable with <paramref name="index"/>.
		/// </summary>
		/// <param name="ilGenerator">The object that controls writing of IL code in current method.</param>
		/// <param name="index">The index of local variable in current method.</param>
		public static void StoreInLocalVariable(this ILGenerator ilGenerator, UInt16 index)
		{
			if (index > 3)
			{
				ilGenerator.Emit(OpCodes.Stloc_S, (Byte)index);
				return;
			}

			switch (index)
			{
				case 0:
					ilGenerator.Emit(OpCodes.Stloc_0);
					break;

				case 1:
					ilGenerator.Emit(OpCodes.Stloc_1);
					break;

				case 2:
					ilGenerator.Emit(OpCodes.Stloc_2);
					break;

				case 3:
					ilGenerator.Emit(OpCodes.Stloc_3);
					break;
			}
		}

		/// <summary>
		/// Inserts into IL code that specified by <paramref name="ilGenerator"/> instruction that loads value from local variable with <paramref name="index"/> into stack.
		/// </summary>
		/// <param name="ilGenerator">The object that controls writing of IL code in current method.</param>
		/// <param name="index">The index of local variable in current method.</param>
		public static void LoadFromLocalVariable(this ILGenerator ilGenerator, UInt16 index)
		{
			if (index > 3)
			{
				ilGenerator.Emit(OpCodes.Ldloc_S, (Byte)index);
				return;
			}

			switch (index)
			{
				case 0:
					ilGenerator.Emit(OpCodes.Ldloc_0);
					break;

				case 1:
					ilGenerator.Emit(OpCodes.Ldloc_1);
					break;

				case 2:
					ilGenerator.Emit(OpCodes.Ldloc_2);
					break;

				case 3:
					ilGenerator.Emit(OpCodes.Ldloc_3);
					break;
			}
		}

		/// <summary>
		/// Inserts into IL code that specified by <paramref name="ilGenerator"/> instruction that loads value of <paramref name="referenceType"/> whose reference lies on top into stack.
		/// </summary>
		/// <param name="ilGenerator">The object that controls writing of IL code in current method.</param>
		/// <param name="referenceType">The type of value which reference lies on top of the stack.</param>
		public static void LoadFromReference(this ILGenerator ilGenerator, Type referenceType)
		{
			if (referenceType == typeof(Byte))
			{
				ilGenerator.Emit(OpCodes.Ldind_I1);
			}
			else if (referenceType == typeof(Int16))
			{
				ilGenerator.Emit(OpCodes.Ldind_I2);
			}
			else if (referenceType == typeof(Int32))
			{
				ilGenerator.Emit(OpCodes.Ldind_I4);
			}
			else if (referenceType == typeof(Int64))
			{
				ilGenerator.Emit(OpCodes.Ldind_I8);
			}
			else if (referenceType == typeof(Boolean))
			{
				ilGenerator.Emit(OpCodes.Ldind_U1);
			}
			else if (referenceType == typeof(UInt16))
			{
				ilGenerator.Emit(OpCodes.Ldind_U2);
			}
			else if (referenceType == typeof(UInt32))
			{
				ilGenerator.Emit(OpCodes.Ldind_U4);
			}
			else if (referenceType == typeof(Single))
			{
				ilGenerator.Emit(OpCodes.Ldind_R4);
			}
			else if (referenceType == typeof(Double))
			{
				ilGenerator.Emit(OpCodes.Ldind_R8);
			}
			else
			{
				ilGenerator.Emit(OpCodes.Ldobj, referenceType);
			}
		}

		/// <summary>
		/// Inserts into IL code that specified by <paramref name="ilGenerator"/> instruction that stores value of <paramref name="referenceType"/> into address that lies on top into stack.
		/// </summary>
		/// <param name="ilGenerator">The object that controls writing of IL code in current method.</param>
		/// <param name="referenceType">The type of value which reference lies on top of the stack.</param>
		public static void StoreIntoReference(this ILGenerator ilGenerator, Type referenceType)
		{
			if (referenceType == typeof(Byte))
			{
				ilGenerator.Emit(OpCodes.Stind_I1);
			}
			else if (referenceType == typeof(Int16))
			{
				ilGenerator.Emit(OpCodes.Stind_I2);
			}
			else if (referenceType == typeof(Int32))
			{
				ilGenerator.Emit(OpCodes.Stind_I4);
			}
			else if (referenceType == typeof(Int64))
			{
				ilGenerator.Emit(OpCodes.Stind_I8);
			}
			else if (referenceType == typeof(Boolean))
			{
				ilGenerator.Emit(OpCodes.Stind_I1);
			}
			else if (referenceType == typeof(UInt16))
			{
				ilGenerator.Emit(OpCodes.Stind_I2);
			}
			else if (referenceType == typeof(UInt32))
			{
				ilGenerator.Emit(OpCodes.Stind_I4);
			}
			else if (referenceType == typeof(Single))
			{
				ilGenerator.Emit(OpCodes.Stind_R4);
			}
			else if (referenceType == typeof(Double))
			{
				ilGenerator.Emit(OpCodes.Stind_R8);
			}
			else
			{
				ilGenerator.Emit(OpCodes.Stind_Ref);
			}
		}

		/// <summary>
		/// Inserts into IL code that specified by <paramref name="ilGenerator"/> instruction that loads value of method's parameter with <paramref name="argIndex"/> innto stack.
		/// </summary>
		/// <param name="ilGenerator">The object that controls writing of IL code in current method.</param>
		/// <param name="argIndex">The index of method argument.</param>
		public static void LoadArgument(this ILGenerator ilGenerator, Int32 argIndex)
		{
			if (argIndex == 0)
			{
				ilGenerator.Emit(OpCodes.Ldarg_0);
			}
			else if (argIndex == 1)
			{
				ilGenerator.Emit(OpCodes.Ldarg_1);
			}
			else if (argIndex == 2)
			{
				ilGenerator.Emit(OpCodes.Ldarg_2);
			}
			else if (argIndex == 3)
			{
				ilGenerator.Emit(OpCodes.Ldarg_3);
			}
			else if (argIndex < 256)
			{
				ilGenerator.Emit(OpCodes.Ldarg_S, (Byte)argIndex);
			}
			else
			{
				ilGenerator.Emit(OpCodes.Ldarg, argIndex);
			}
		}

		/// <summary>
		/// Inserts into IL code that specified by <paramref name="ilGenerator"/> instruction that loads value of <paramref name="index"/> innto stack.
		/// </summary>
		/// <param name="ilGenerator">The object that controls writing of IL code in current method.</param>
		/// <param name="index">The index.</param>
		public static void LoadIndex(this ILGenerator ilGenerator, Int32 index)
		{
			if (index == 0)
			{
				ilGenerator.Emit(OpCodes.Ldc_I4_0);
			}
			else if (index == 1)
			{
				ilGenerator.Emit(OpCodes.Ldc_I4_1);
			}
			else if (index == 2)
			{
				ilGenerator.Emit(OpCodes.Ldc_I4_2);
			}
			else if (index == 3)
			{
				ilGenerator.Emit(OpCodes.Ldc_I4_3);
			}
			else if (index == 4)
			{
				ilGenerator.Emit(OpCodes.Ldc_I4_4);
			}
			else if (index == 5)
			{
				ilGenerator.Emit(OpCodes.Ldc_I4_5);
			}
			else if (index == 6)
			{
				ilGenerator.Emit(OpCodes.Ldc_I4_6);
			}
			else if (index == 7)
			{
				ilGenerator.Emit(OpCodes.Ldc_I4_7);
			}
			else if (index == 8)
			{
				ilGenerator.Emit(OpCodes.Ldc_I4_8);
			}
			else if (index < 256)
			{
				ilGenerator.Emit(OpCodes.Ldc_I4_S, (Byte)index);
			}
			else
			{
				ilGenerator.Emit(OpCodes.Ldc_I4, index);
			}
		}

		/// <summary>
		/// Inserts into IL code that specified by <paramref name="ilGenerator"/> instruction that loads <paramref name="constant"/> value into stack.
		/// </summary>
		/// <param name="ilGenerator">The object that controls writing of IL code in current method.</param>
		/// <param name="constant">The constant value of value type, null or metadata <see cref="String"/>.</param>
		public static void LoadConstant(this ILGenerator ilGenerator, Object constant)
		{
			if (constant == null)
			{
				ilGenerator.Emit(OpCodes.Ldnull);
			}
			else if (constant is string)
			{
				ilGenerator.Emit(OpCodes.Ldstr, constant as String);
			}
			else if (constant is Single)
			{
				ilGenerator.Emit(OpCodes.Ldc_R4, (Single)constant);
			}
			else if (constant is Double)
			{
				ilGenerator.Emit(OpCodes.Ldc_R8, (Double)constant);
			}
			else if (constant is Boolean)
			{
				ilGenerator.Emit(((Boolean)constant) ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
			}
			else if (constant is Int64)
			{
				ilGenerator.Emit(OpCodes.Ldc_I8, (Int64)constant);
			}
			else
			{
				Int32 constValue;
				if (constant is Int32)
					constValue = (Int32)constant;
				else if (constant is UInt32)
					constValue = (Int32)(UInt32)constant;
				else if (constant is Int16)
					constValue = (Int32)(Int16)constant;
				else if (constant is UInt16)
					constValue = (Int32)(UInt16)constant;
				else if (constant is Byte)
					constValue = (Int32)(Byte)constant;
				else if (constant is Char)
					constValue = (Int32)(Char)constant;
				else
				{
					throw new NotSupportedException(String.Format("The type {0} of the constant is not supported.", constant.GetType().Name));
				}

				if (constValue == 0)
					ilGenerator.Emit(OpCodes.Ldc_I4_0);
				else if (constValue == 1)
					ilGenerator.Emit(OpCodes.Ldc_I4_1);
				else if (constValue == 2)
					ilGenerator.Emit(OpCodes.Ldc_I4_2);
				else if (constValue == 3)
					ilGenerator.Emit(OpCodes.Ldc_I4_3);
				else if (constValue == 4)
					ilGenerator.Emit(OpCodes.Ldc_I4_4);
				else if (constValue == 5)
					ilGenerator.Emit(OpCodes.Ldc_I4_5);
				else if (constValue == 6)
					ilGenerator.Emit(OpCodes.Ldc_I4_6);
				else if (constValue == 7)
					ilGenerator.Emit(OpCodes.Ldc_I4_7);
				else if (constValue == 8)
					ilGenerator.Emit(OpCodes.Ldc_I4_8);
				else if (constValue == -1)
					ilGenerator.Emit(OpCodes.Ldc_I4_M1);
				else if (constValue >= -128 && constValue < 128)
					ilGenerator.Emit(OpCodes.Ldc_I4_S, (Byte)constValue);
				else
					ilGenerator.Emit(OpCodes.Ldc_I4, constValue);
			}
		}
	}
}