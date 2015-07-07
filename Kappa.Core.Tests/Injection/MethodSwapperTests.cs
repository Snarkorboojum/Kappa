using System;
using System.Reflection;
using System.Reflection.Emit;
using Kappa.Core.Injection;
using Kappa.Core.Reflection.Emit;
using Kappa.Core.Tests.Injection.Examples;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParameterInfo = Kappa.Core.Reflection.Emit.ParameterInfo;

namespace Kappa.Core.Tests.Injection
{
	/// <summary>
	/// Provides the methods that tests the methods replacements.
	/// </summary>
	[TestClass]
	public class MethodSwapperTests
	{
		/// <summary>
		/// The constant test text to validate successfullness of the method replacement.
		/// </summary>
		const String TestText = "I was here!";

		/// <summary>
		/// Tests the replacement of property getter method with dynamic method.
		/// </summary>
		[TestMethod]
		public void InjectionReplaceTest()
		{
			var dynamicMethod = new DynamicMethod("Test", typeof(String), new[] { typeof(ClassWithInstanceMethod) });
			//var dynamicMethod = new DynamicMethod("Test",
			//	MethodAttributes.Static | MethodAttributes.Public,
			//	CallingConventions.Standard,
			//	typeof(void),
			//	new[] { typeof(ClassWithInstanceMethod) },
			//	typeof(ClassWithInstanceMethod) ,
			//	true
			//	);

			var ilGenerator = dynamicMethod.GetILGenerator();
			ilGenerator.Emit(OpCodes.Ldarg_0);
			ilGenerator.Emit(OpCodes.Callvirt, typeof(ClassWithInstanceMethod).GetProperty("Property").GetMethod);
			ilGenerator.Emit(OpCodes.Ret);

			var method = typeof(ClassWithInstanceMethod).GetMethod("InstanceMethod");

			MethodSwapper.ReplaceMethod(dynamicMethod, method, true);

			var testObject = new ClassWithInstanceMethod(TestText);

			Assert.AreEqual(TestText, testObject.InstanceMethod());
		}

		/// <summary>
		/// Tests the replacement of the class method with methdo of other class.
		/// </summary>
		[TestMethod]
		public void InjectionTrapTest()
		{
			var method = typeof(ClassWithOtherInstanceMethod).GetMethod("InstanceMethod");

			var typeBuilder = ClassBuilder.DefineType("TestType");
			var methodBuilder = ClassBuilder.DefineMethod(typeBuilder, "a", MethodAttributes.Public | MethodAttributes.Static, typeof(String),
				CallingConventions.Standard,
				new[] { new ParameterInfo { ParameterType = typeof(ClassWithOtherInstanceMethod) } });
			var ilGenerator = methodBuilder.GetILGenerator();
			ilGenerator.Emit(OpCodes.Ldnull);
			ilGenerator.Emit(OpCodes.Ret);

			var type = typeBuilder.CreateType();
			var ilMethod = type.GetMethod("a", BindingFlags.Public | BindingFlags.Static);
			MethodSwapper.ReplaceMethod(method, ilMethod, true);

			var dynamicMethod = new DynamicMethod("Test", typeof(String), new[] { typeof(ClassWithOtherInstanceMethod) });
			ilGenerator = dynamicMethod.GetILGenerator();
			ilGenerator.Emit(OpCodes.Ldarg_0);
			ilGenerator.Emit(OpCodes.Call, ilMethod);
			ilGenerator.Emit(OpCodes.Ret);

			MethodSwapper.ReplaceMethod(dynamicMethod, method, true);
			var testObject = new ClassWithOtherInstanceMethod(TestText);

			try
			{
				testObject.InstanceMethod();
			}
			catch (InvalidOperationException exception)
			{
				Assert.AreEqual(TestText, exception.Message);
			}
		}
	}
}