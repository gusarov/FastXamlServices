using System;
using System.Diagnostics;

namespace FastXamlServices.UnitTests
{
	public static class PerformanceHelper
	{
		public static long Performance(Action act)
		{
			var sw = Stopwatch.StartNew();
			ulong total = 0;
			const int batch = 1000;
			do
			{
				for (int i = 0; i < batch; i++)
				{
					act();
				}
				checked
				{
					total += batch;
				}
			} while (sw.ElapsedMilliseconds < 1000);
			sw.Stop();
			return checked((long)(total / sw.Elapsed.TotalSeconds));
		}
	}
}