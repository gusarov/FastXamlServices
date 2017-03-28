using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace FastXamlServices.UnitTests.SampleData
{
	public class Sample
	{
		[DefaultValue(null)]
		public string Prop1 { get; set; }
		[DefaultValue(0)]
		public int Prop2 { get; set; }
		[DefaultValue(null)]
		public DateTime? DateTime { get; set; }
		[DefaultValue(null)]
		public Amount? Amount { get; set; }
	}

	public class SampleWithComposite : Sample
	{
		[DefaultValue(null)]
		public Sample MainSample { get; set; }
	}

	public class SampleDefaulted
	{
		[DefaultValue(TradeTypeDto.Sell)]
		public TradeTypeDto DefaultedEnum { get; set; } = TradeTypeDto.Sell;
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

	public class SampleWithPrimitiveCollection
	{
		public string Name { get; set; }

		public IList<string> Strings { get; } = new List<string>();

		public IList<int> Ints { get; } = new List<int>();

		public IList<Sample> Samples { get; } = new List<Sample>();
	}

	public class TradeDto
	{
		public ulong Id { get; set; }
		public ulong OrderId { get; set; }
		public bool IsYourOrder { get; set; }
		public string PairCode { get; set; }
		public TradeTypeDto Type { get; set; }
		public decimal Amount { get; set; }
		public decimal Price { get; set; }
		public DateTime UtcCreatedAt { get; set; }

	}

	public enum TradeTypeDto
	{
		Unknown,
		Buy,
		Sell,
	}

}