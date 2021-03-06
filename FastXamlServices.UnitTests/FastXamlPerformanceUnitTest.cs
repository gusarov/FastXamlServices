using FastXamlServices.Internal;
using FastXamlServices.UnitTests.SampleData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FastXamlServices.UnitTests
{
	[TestClass]
	public class FastXamlPerformanceUnitTest : BasePerformanceUnitTest
	{
		private static readonly FastXamlServices _fxs = new FastXamlServices();

		protected override string Save<T>(T instance)
		{
			return _fxs.Save(instance);
		}

		protected override T Load<T>(string xaml)
		{
			return (T)_fxs.Parse(xaml);
		}
	}

	[TestClass]
	public class FastXamlPerformancePrecompiledUnitTest : BasePerformanceUnitTest
	{
		private static readonly FastXamlServices _fxs = new FastXamlServices();

		[TestInitialize]
		public void Init()
		{
			_fxs.RegisterSerializer(typeof(Sample), PrecompiledWriter, PrecompiledReader);
		}

		private object PrecompiledReader(SerializationReaderContext arg)
		{
			throw new System.NotImplementedException();
		}

		private void PrecompiledWriter(SerializationWriterContext ctx, object obj)
		{
			var sample = (Sample)obj;
			ctx.Write($@"<Sample Prop1=""{sample.Prop1}"" Prop2=""{sample.Prop2}"" xmlns=""test"" />");
		}

		protected override string Save<T>(T instance)
		{
			return _fxs.Save(instance);
		}

		protected override T Load<T>(string xaml)
		{
			return (T)_fxs.Parse(xaml);
		}
	}
}