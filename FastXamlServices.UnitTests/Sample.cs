using System.ComponentModel;

namespace FastXamlServices.UnitTests
{
	public class Sample
	{
		public string Prop1 { get; set; }
		public int Prop2 { get; set; }
	}

	public class SampleWithComposite : Sample
	{
		[DefaultValue(null)]
		public Sample MainSample { get; set; }
	}

	public class SampleNulls
	{
		public int Primitive { get; set; }
		public int? NullablePrimitive { get; set; }
		public Sample Composite { get; set; }

		[DefaultValue(5)]
		public int DefaultedPrimitive { get; set; } = 5;

		[DefaultValue(null)]
		public int? DefaultedNullablePrimitive { get; set; }

		[DefaultValue(5)]
		public int? DefaultedNullablePrimitive5 { get; set; } = 5;

		[DefaultValue(null)]
		public Sample DefaultedComposite { get; set; }
	}
}