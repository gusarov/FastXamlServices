using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FastXamlServices.UnitTests
{
	[TestClass]
	public abstract class BasePerformanceUnitTest
	{
		protected abstract string Save<T>(T instance);
		protected abstract T Load<T>(string xaml);

		[TestMethod]
		public void SavePerformance()
		{
			var sample = new Sample();
			var perf = PerformanceHelper.Performance(() => Save(sample));
			Assert.Inconclusive($"{perf:N} OpS");
		}

		[TestMethod]
		public void LoadPerformance()
		{
			var sample = "<Sample xmlns='test' />";
			var perf = PerformanceHelper.Performance(() => Load<Sample>(sample));
			Assert.Inconclusive($"{perf:N} OpS");
		}
	}
}