using System.IO;

namespace FastXamlServices
{
	public interface IXamlServices
	{
		/// <summary>Loads a <see cref="T:System.IO.Stream" /> source for a XAML reader and returns an object graph.</summary>
		/// <returns>The object graph that is returned.</returns>
		/// <param name="fileName">The file name to load and use as source.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// <paramref name="fileName" /> input is null.</exception>
		object Load(string fileName);

		/// <summary>Loads a <see cref="T:System.IO.Stream" /> source for a XAML reader and writes its output as an object graph.</summary>
		/// <returns>The object graph that is written as output.</returns>
		/// <param name="stream">The stream to load as input.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// <paramref name="stream" /> is null.</exception>
		object Load(Stream stream);

		/// <summary>Reads XAML as string output and returns an object graph.</summary>
		/// <returns>The object graph that is returned.</returns>
		/// <param name="xaml">The XAML string input to parse.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// <paramref name="xaml" /> input is null.</exception>
		object Parse(string xaml);

		/// <summary>Processes a provided object tree into a XAML node representation, and returns a string representation of the output XAML.</summary>
		/// <returns>The XAML markup output as a string. </returns>
		/// <param name="instance">The root of the object graph to process.</param>
		string Save(object instance);

		/// <summary>Processes a provided object graph into a XAML node representation and then writes it to an output file at a provided location.</summary>
		/// <param name="fileName">The name and location of the file to write the output to.</param>
		/// <param name="instance">The root of the object graph to process.</param>
		/// <exception cref="T:System.ArgumentException">
		/// <paramref name="fileName" /> is an empty string.</exception>
		/// <exception cref="T:System.ArgumentNullException">
		/// <paramref name="fileName" /> is null.</exception>
		void Save(string fileName, object instance);

		/// <summary>Processes a provided object graph into a XAML node representation and then into an output stream for serialization.</summary>
		/// <param name="stream">The destination stream.</param>
		/// <param name="instance">The root of the object graph to process.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// <paramref name="stream" /> input is null.</exception>
		void Save(Stream stream, object instance);

		void Save(StreamWriter stream, object instance);
	}
}