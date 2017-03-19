using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FastXamlServices.UnitTests
{
	using static PerformanceHelper;
	[TestClass]
	public class StringBuildingPerformanceUnitTest
	{
		[TestMethod]
		public void BuildViaStringBuilder()
		{
			string r = null;
			var ops = Performance(() =>
			{
				var sb = new StringBuilder();
				sb.Append("asd");
				sb.Append('&');
				sb.Append(new string(' ', 4));
				sb.AppendLine("char&code1");
				sb.AppendLine("char&code2");
				sb.AppendLine("char&code3");
				r = sb.ToString();
			});
			Assert.AreEqual("asd&    char&code1\r\nchar&code2\r\nchar&code3\r\n", r);
			Assert.Inconclusive(ops.ToString("N0"));
		}

		[TestMethod]
		public void BuildViaStream()
		{
			string r = null;
			var ops = Performance(() =>
			{
				var ms = new MemoryStream();
				var sb = new StreamWriter(ms);
				sb.Write("asd");
				sb.Write('&');
				sb.Write(new string(' ', 4));
				sb.WriteLine("char&code1");
				sb.WriteLine("char&code2");
				sb.WriteLine("char&code3");
				sb.Flush();
				r = Encoding.UTF8.GetString(ms.ToArray());
			});
			Assert.AreEqual("asd&    char&code1\r\nchar&code2\r\nchar&code3\r\n", r);
			Assert.Inconclusive(ops.ToString("N0"));
		}
	}

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