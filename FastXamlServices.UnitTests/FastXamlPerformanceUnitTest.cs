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
}