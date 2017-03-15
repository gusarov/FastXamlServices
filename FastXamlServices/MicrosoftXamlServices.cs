using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xaml;
using System.Xml;

namespace FastXamlServices
{
	public class MicrosoftXamlServices : IXamlServices
	{
		/// <summary>Loads a <see cref="T:System.IO.Stream" /> source for a XAML reader and returns an object graph.</summary>
		/// <returns>The object graph that is returned.</returns>
		/// <param name="fileName">The file name to load and use as source.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// <paramref name="fileName" /> input is null.</exception>
		public object Load(string fileName)
		{
			return XamlServices.Load(fileName);
		}

		/// <summary>Loads a <see cref="T:System.IO.Stream" /> source for a XAML reader and writes its output as an object graph.</summary>
		/// <returns>The object graph that is written as output.</returns>
		/// <param name="stream">The stream to load as input.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// <paramref name="stream" /> is null.</exception>
		public object Load(Stream stream)
		{
			return XamlServices.Load(stream);
		}

		/// <summary>Reads XAML as string output and returns an object graph.</summary>
		/// <returns>The object graph that is returned.</returns>
		/// <param name="xaml">The XAML string input to parse.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// <paramref name="xaml" /> input is null.</exception>
		public object Parse(string xaml)
		{
			return XamlServices.Parse(xaml);
		}



		/// <summary>Processes a provided object tree into a XAML node representation, and returns a string representation of the output XAML.</summary>
		/// <returns>The XAML markup output as a string. </returns>
		/// <param name="instance">The root of the object graph to process.</param>
		public string Save(object instance)
		{
			return XamlServices.Save(instance);
		}

		/// <summary>Processes a provided object graph into a XAML node representation and then writes it to an output file at a provided location.</summary>
		/// <param name="fileName">The name and location of the file to write the output to.</param>
		/// <param name="instance">The root of the object graph to process.</param>
		/// <exception cref="T:System.ArgumentException">
		/// <paramref name="fileName" /> is an empty string.</exception>
		/// <exception cref="T:System.ArgumentNullException">
		/// <paramref name="fileName" /> is null.</exception>
		public void Save(string fileName, object instance)
		{
			XamlServices.Save(fileName, instance);
		}

		/// <summary>Processes a provided object graph into a XAML node representation and then into an output stream for serialization.</summary>
		/// <param name="stream">The destination stream.</param>
		/// <param name="instance">The root of the object graph to process.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// <paramref name="stream" /> input is null.</exception>
		public void Save(Stream stream, object instance)
		{
			XamlServices.Save(stream, instance);
		}



	}
}
