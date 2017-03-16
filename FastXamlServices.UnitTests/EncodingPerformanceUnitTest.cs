using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FastXamlServices.UnitTests
{
	[TestClass]
	public class EncodingPerformanceUnitTest
	{
		[TestMethod]
		public void EncodeUtf16GetBytes()
		{
			var sample = "<Sample xmlns='test' />";
			var perf = PerformanceHelper.Performance(() => Encoding.Unicode.GetBytes(sample));
			Assert.Inconclusive($"{perf:N} OpS");
		}
		[TestMethod]
		public void EncodeUtf16GetString()
		{
			var sample = Encoding.Unicode.GetBytes("<Sample xmlns='test' />");
			var perf = PerformanceHelper.Performance(() => Encoding.Unicode.GetString(sample));
			Assert.Inconclusive($"{perf:N} OpS");
		}

		[TestMethod]
		public void EncodeUtf8GetBytes()
		{
			var sample = "<Sample xmlns='test' />";
			var perf = PerformanceHelper.Performance(() => Encoding.UTF8.GetBytes(sample));
			Assert.Inconclusive($"{perf:N} OpS");
		}
		[TestMethod]
		public void EncodeUtf8GetString()
		{
			var sample = Encoding.UTF8.GetBytes("<Sample xmlns='test' />");
			var perf = PerformanceHelper.Performance(() => Encoding.UTF8.GetString(sample));
			Assert.Inconclusive($"{perf:N} OpS");
		}
	}
}