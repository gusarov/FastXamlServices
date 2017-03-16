using System;
using System.Collections.Generic;
using System.Xaml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FastXamlServices.UnitTests
{
	[TestClass]
	public class MicrosoftPerformanceUnitTest : BasePerformanceUnitTest
	{
		protected override string Save<T>(T instance)
		{
			return XamlServices.Save(instance);
		}

		protected override T Load<T>(string xaml)
		{
			return (T)XamlServices.Parse(xaml);
		}
	}
}
