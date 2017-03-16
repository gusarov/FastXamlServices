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
			Assert.Inconclusive();
		}


		[TestMethod]
		public void Should_30_respect_default_value()
		{
			Assert.Inconclusive();
		}

		private static void Verify(object data)
		{
			var exp = new MicrosoftXamlServices().Save(data);
			var act = new FastXamlServices().Save(data);
			Trace.WriteLine(exp);
			Trace.WriteLine(act);
			Assert.AreEqual(exp, act);
		}
	}
}
