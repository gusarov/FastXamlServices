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
			var data = new Sample
			{
				Prop1 = "asd",
				Prop2 = 123,
			};
			Save(data);
			var perf = PerformanceHelper.Performance(() => Save(data));
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