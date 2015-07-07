using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace Kappa.Core.Injection
{
	public static class MethodSwapper
	{
		/// <summary>
		/// Replaces the method.
		/// </summary>
		/// <param name="source">The source.</param>
		/// <param name="dest">The dest.</param>
		/// <param name="skipSignatureCheck"></param>
		public static void ReplaceMethod(MethodBase source, MethodBase dest, Boolean skipSignatureCheck = false)
		{
			if (!skipSignatureCheck && !MethodSignaturesEqual(source, dest))
				throw new ArgumentException("The method signatures are not the same.", "source");

			ReplaceMethod(GetMethodAddress(source), dest, source is DynamicMethod);
		}

		/// <summary>
		/// Replaces the method.
		/// </summary>
		/// <param name="srcAdr">The SRC adr.</param>
		/// <param name="dest">The dest.</param>
		/// <param name="isDynamicSource"></param>
		private static void ReplaceMethod(IntPtr srcAdr, MethodBase dest, Boolean isDynamicSource)
		{
			IntPtr destAdr = GetMethodAddress(dest);
			unsafe
			{
				if (IntPtr.Size == 8)
				{
					ulong* d = (ulong*)destAdr.ToPointer();

					if (isDynamicSource)
					{
						*d = (ulong)srcAdr.ToInt64();
					}
					else
					{
						*d = *((ulong*)srcAdr.ToPointer());
					}
				}
				else
				{
					uint* d = (uint*)destAdr.ToPointer();

					if (isDynamicSource)
					{
						*d = (uint)srcAdr.ToInt32();
					}
					else
					{
						*d = *((uint*)srcAdr.ToPointer());
					}
				}
			}
		}

		/// <summary>
		/// Gets the address of the method stub
		/// </summary>
		/// <param name="methodHandle">The method handle.</param>
		/// <returns></returns>
		public static IntPtr GetMethodAddress(MethodBase method)
		{
			if (method is DynamicMethod)
			{
				return GetDynamicMethodAddress(method);
			}

			// Prepare the method so it gets jited
			RuntimeHelpers.PrepareMethod(method.MethodHandle);

			// If 3.5 sp1 or greater than we have a different layout in memory.
			if (IsNet20Sp2OrGreater())
			{
				return GetMethodAddress20Sp2(method);
			}


			unsafe
			{
				// Skip these
				const int skip = 10;

				// Read the method index.
				UInt64* location = (UInt64*)(method.MethodHandle.Value.ToPointer());
				int index = (int)(((*location) >> 32) & 0xFF);

				if (IntPtr.Size == 8)
				{
					// Get the method table
					ulong* classStart = (ulong*)method.DeclaringType.TypeHandle.Value.ToPointer();
					ulong* address = classStart + index + skip;
					return new IntPtr(address);
				}
				else
				{
					// Get the method table
					uint* classStart = (uint*)method.DeclaringType.TypeHandle.Value.ToPointer();
					uint* address = classStart + index + skip;
					return new IntPtr(address);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="method"></param>
		/// <returns></returns>
		private static RuntimeMethodHandle GetDynamicMethodRuntimeHandle(MethodBase method)
		{
			RuntimeMethodHandle handle;

			if (Environment.Version.Major == 4)
			{
				MethodInfo getMethodDescriptorInfo = typeof(DynamicMethod).GetMethod("GetMethodDescriptor",
												BindingFlags.NonPublic | BindingFlags.Instance);
				handle = (RuntimeMethodHandle)getMethodDescriptorInfo.Invoke(method, null);
			}
			else
			{
				FieldInfo fieldInfo = typeof(DynamicMethod).GetField("m_method", BindingFlags.NonPublic | BindingFlags.Instance);
				handle = ((RuntimeMethodHandle)fieldInfo.GetValue(method));
			}

			return handle;
		}

		private static IntPtr GetDynamicMethodAddress(MethodBase method)
		{
			unsafe
			{
				RuntimeMethodHandle handle = GetDynamicMethodRuntimeHandle(method);
				byte* ptr = (byte*)handle.Value.ToPointer();
				if (IsNet20Sp2OrGreater())
				{
					RuntimeHelpers.PrepareMethod(handle);
					return handle.GetFunctionPointer();

					//if (IntPtr.Size == 8)
					//{
					//	ulong* address = (ulong*)ptr;
					//	address = (ulong*)*(address + 5);
					//	return new IntPtr(address + 12);
					//}
					//else
					//{
					//	uint* address = (uint*)ptr;
					//	address = (uint*)*(address + 5);
					//	return new IntPtr(address + 12);
					//}
				}
				else
				{

					if (IntPtr.Size == 8)
					{
						ulong* address = (ulong*)ptr;
						address += 6;
						return new IntPtr(address);
					}
					else
					{
						uint* address = (uint*)ptr;
						address += 6;
						return new IntPtr(address);
					}
				}

			}
		}

		private static IntPtr GetMethodAddress20Sp2(MethodBase method)
		{
			unsafe
			{
				return new IntPtr(((int*)method.MethodHandle.Value.ToPointer() + 2));
			}
		}
		
		private static bool MethodSignaturesEqual(MethodBase x, MethodBase y)
		{
			if (x.CallingConvention != y.CallingConvention)
			{
				return false;
			}
			Type returnX = GetMethodReturnType(x), returnY = GetMethodReturnType(y);
			if (returnX != returnY)
			{
				return false;
			}
			ParameterInfo[] xParams = x.GetParameters(), yParams = y.GetParameters();
			if (xParams.Length != yParams.Length)
			{
				return false;
			}
			for (int i = 0; i < xParams.Length; i++)
			{
				if (xParams[i].ParameterType != yParams[i].ParameterType)
				{
					return false;
				}
			}
			return true;
		}

		private static Type GetMethodReturnType(MethodBase method)
		{
			MethodInfo methodInfo = method as MethodInfo;
			if (methodInfo == null)
			{
				// Constructor info.
				throw new ArgumentException("Unsupported MethodBase : " + method.GetType().Name, "method");
			}
			return methodInfo.ReturnType;
		}
		
		private static bool IsNet20Sp2OrGreater()
		{
			if (Environment.Version.Major == 4)
				return true;

			return Environment.Version.Major == FrameworkVersions.Net20Sp2.Major &&
							Environment.Version.MinorRevision >= FrameworkVersions.Net20Sp2.MinorRevision;
		}
	}
}
