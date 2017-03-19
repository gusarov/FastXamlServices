using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FastXamlServices.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FastXamlServices.UnitTests
{
	[TestClass]
	public class DynamicInvokeOptimizerTest
	{
		private static int _staticGet;
		public static int StaticGet()
		{
			return _staticGet;
		}

		private static decimal _staticGetBigValue;
		public static decimal StaticGetBigValue()
		{
			return _staticGetBigValue;
		}

		private static string _staticGetReference;
		public static string StaticGetReference()
		{
			return _staticGetReference;
		}

		private int _get;
		public int Get()
		{
			return _get;
		}

		private decimal _getBigValue;
		public decimal GetBigValue()
		{
			return _getBigValue;
		}

		private string _getReference;
		public string GetReference()
		{
			return _getReference;
		}


		[TestMethod]
		public void Should_invoke_static_fast_no_arg()
		{
			var staticGetter = (Func<object>)GetType().GetMethod("StaticGet").CompileObject();
			_staticGet = 5;
			Assert.AreEqual(5, staticGetter());
			_staticGet = 15;
			Assert.AreEqual(15, staticGetter());
			_staticGet = int.MaxValue;
			Assert.AreEqual(int.MaxValue, staticGetter());

			staticGetter = (Func<object>)GetType().GetMethod("StaticGetBigValue").CompileObject();
			_staticGetBigValue = 123;
			Assert.AreEqual(123m, staticGetter());
			_staticGetBigValue = decimal.MaxValue;
			Assert.AreEqual(decimal.MaxValue, staticGetter());

			staticGetter = (Func<object>)GetType().GetMethod("StaticGetReference").CompileObject();
			_staticGetReference = "abc";
			Assert.AreEqual("abc", staticGetter());
		}

		[TestMethod]
		public void Should_invoke_instance_fast_no_arg()
		{
			var getter = (Func<object, object>)GetType().GetMethod("Get").CompileObject();
			
			_get = 5;
			Assert.AreEqual(5, getter(this));
			_get = 15;
			Assert.AreEqual(15, getter(this));
			_get = int.MaxValue;
			Assert.AreEqual(int.MaxValue, getter(this));

			getter = (Func<object, object>)GetType().GetMethod("GetBigValue").CompileObject();
			_getBigValue = 123;
			Assert.AreEqual(123m, getter(this));
			_getBigValue = decimal.MaxValue;
			Assert.AreEqual(decimal.MaxValue, getter(this));

			getter = (Func<object, object>)GetType().GetMethod("GetReference").CompileObject();
			_getReference = "abc";
			Assert.AreEqual("abc", getter(this));
		}
	}
}
