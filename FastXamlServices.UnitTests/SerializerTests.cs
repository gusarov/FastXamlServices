﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FastXamlServices.UnitTests.SampleData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FastXamlServices.UnitTests
{
	[TestClass]
	public class SerializerTests
	{
		[TestInitialize]
		public void Init()
		{
			_notExact = false;
			_initialCi = Thread.CurrentThread.CurrentCulture;
		}

		private CultureInfo _initialCi;

		[TestCleanup]
		public void Clean()
		{
			if (_notExact)
			{
				Assert.Inconclusive("Not Exactly matched, but works");
			}
			Thread.CurrentThread.CurrentCulture = _initialCi;
		}

		[TestMethod]
		public void Should_10_serialize_simple_node()
		{
			Verify(new Sample
			{
				Prop1 = "asd",
				Prop2 = 123,
			});
		}

		[TestMethod]
		public void Should_20_serialize_primitive()
		{
			Verify("asd");
			Verify(15m);
		}

		[TestMethod]
		public void Should_20_serialize_collection()
		{
			var data = new SampleWithPrimitiveCollection
			{
				Name = "bob",
			};
			data.Samples.Add(new Sample
			{
				Prop1 = "asd",
				Prop2 = 321,
			});
			Verify(data);
		}

		[TestMethod]
		public void Should_30_serialize_primitive_collection()
		{
			var data = new SampleWithPrimitiveCollection
			{
				Name = "bob",
			};
			data.Strings.Add("bob1");
			data.Strings.Add("bob2");
			data.Ints.Add(321);
			data.Ints.Add(123);
			Verify(data);
		}

		[TestMethod]
		public void Should_30_serialize_content_property()
		{
			Assert.Inconclusive();
		}

		[TestMethod]
		public void Should_30_serialize_nulls()
		{
			var data = new SampleNulls();
			Verify(data);

			data.Primitive = 1;
			data.NullablePrimitive = 1;
			data.DefaultedPrimitive = 5;
			data.DefaultedNullablePrimitive = 5;
			data.DefaultedNullablePrimitive5 = 5;
			Verify(data);
		}


		[TestMethod]
		public void Should_30_respect_default_value()
		{
			Verify(new SampleDefaulted
			{
				DefaultedEnum = TradeTypeDto.Sell,
			});
			Verify(new SampleDefaulted
			{
				DefaultedEnum = TradeTypeDto.Buy,
			});
		}

		[TestMethod]
		public void Should_30_serialize_composite_value()
		{
			var data = new SampleWithComposite();
			data.Prop1 = "asd";
			Verify(data);

			data.MainSample = new Sample
			{
				Prop1 = "ddd",
			};
			Verify(data);
		}


		[TestMethod]
		public void Should_30_serialize_class_value_with_converter()
		{
			Assert.Inconclusive();
		}

		[TestMethod]
		public void Should_30_be_culture_independant()
		{
			var data = new Sample
			{
				DateTime = new DateTime(2000, 1, 2, 3, 4, 5, 6),
				Amount = 1001000m,
			};

			Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-RU", false);
			Verify(data);
			Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US", false);
			Verify(data);

		}

		[TestMethod]
		public void Should_30_store_details()
		{
			var data = new Sample
			{
				DateTime = new DateTime(2000, 1, 2, 3, 4, 5, 6),
				Amount = 1001000.123456789m,
			};

			Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-RU", false);
			Verify(data);
			Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US", false);
			Verify(data);

		}

		[TestMethod]
		public void Should_30_store_details_date()
		{
			var data = new Sample
			{
				DateTime = new DateTime(2000, 1, 2),
			};

			Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-RU", false);
			var act = Verify(data);

			Assert.IsTrue(act.Contains("\"2000-01-02\""), "Should contains date without time");

			Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US", false);
			Verify(data);

		}

		[TestMethod]
		public void Should_30_be_time_zone_independant()
		{
			var data = new Sample
			{
				DateTime = new DateTime(2000, 1, 2, 3, 4, 5, 6),
			};
			data.DateTime = new DateTime(2000, 1, 2, 3, 4, 5, 6, DateTimeKind.Unspecified);
			Verify(data);
			data.DateTime = new DateTime(2000, 1, 2, 3, 4, 5, 6, DateTimeKind.Local);
			Verify(data);
			data.DateTime = new DateTime(2000, 1, 2, 3, 4, 5, 6, DateTimeKind.Utc);
			Verify(data);
		}

		[TestMethod]
		public void Should_30_serialize_as_propery_when_instance_have_string_converter()
		{
			Assert.Inconclusive();
		}

		[TestMethod]
		public void Should_30_serialize_as_subnode_when_instance_not_have_string_converter()
		{
			Assert.Inconclusive();
		}

		bool _notExact;

		private string Verify(object data)
		{
			var ms = new MicrosoftXamlServices();
			var fx = new FastXamlServices();

			var exp = ms.Save(data);
			WriteTrace(exp, "Expected");
			var act = new FastXamlServices().Save(data);
			WriteTrace(act, "Actual");

			var reSaveMs = ms.Save(ms.Parse(act));
			WriteTrace(reSaveMs, "Resaved via Microsoft XamlServices");
			Assert.AreEqual(exp, reSaveMs);

			var reSaveFx = fx.Save(fx.Parse(act));
			WriteTrace(reSaveFx, "Resaved via FastXamlServices");
			Assert.AreEqual(act, reSaveFx);


			if (!string.Equals(exp, act, StringComparison.Ordinal))
			{
				_notExact = true;
			}

			return act;
		}

		private static void WriteTrace(string str, string title)
		{
			Trace.WriteLine(title + ":");
			Trace.WriteLine(str);
			Trace.WriteLine(string.Join("", str.Select(x =>" "+ ((int)x).ToString("X2") + (x == (int)'\n' ? "\r\n" : null))));
		}
	}
}
