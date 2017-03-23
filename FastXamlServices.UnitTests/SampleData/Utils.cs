namespace FastXamlServices.UnitTests
{
	static class Utils
	{
		public static decimal Normalize(this decimal value)
		{
			return value / 1.000000000000000000000000000000000m;
		}
	}
}