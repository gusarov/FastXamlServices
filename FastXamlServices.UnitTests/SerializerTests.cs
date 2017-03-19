using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
		}

		[TestCleanup]
		public void Clean()
		{
			if (_notExact)
			{
				Assert.Inconclusive("Not Exactly matched, but works");
			}
		}

		[TestMethod]
		public void Should_10_serialize_simple_node()
		{
			var data = new Sample
			{
				Prop1 = "asd",
				Prop2 = 123,
			};
			Verify(data);
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
			Assert.Inconclusive();
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
			Assert.Inconclusive();
		}

		bool _notExact = true;

		private void Verify(object data)
		{
			var ms = new MicrosoftXamlServices();
			var exp = ms.Save(data);
			WriteTrace(exp);
			var act = new FastXamlServices().Save(data);
			WriteTrace(act);
			var react = ms.Save(ms.Parse(act));
			WriteTrace(react);
			Assert.AreEqual(exp, react);
			_notExact = !string.Equals(exp, act, StringComparison.Ordinal);
		}

		private static void WriteTrace(string str)
		{
			Trace.WriteLine(str);
			Trace.WriteLine(string.Join("", str.Select(x =>" "+ ((int)x).ToString("X2") + (x == (int)'\n' ? "\r\n" : null))));
		}
	}
}
