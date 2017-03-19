using System.Collections.Generic;

namespace FastXamlServices.UnitTests
{
	public class SampleWithPrimitiveCollection
	{
		public string Name { get; set; }

		public IList<string> Strings { get; } = new List<string>();

		public IList<int> Ints { get; } = new List<int>();

		public IList<Sample> Samples { get; } = new List<Sample>();
	}
}