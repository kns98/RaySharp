using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace minlight.net
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

    class Program
    {
        static void Main2()
        {
            string filePath = "path/to/your/file.xml";

            try
            {
                using (StreamReader reader = GetFileStreamReader(filePath))
                {
                    string result = ConvertXmlToText(reader);
                    Console.WriteLine(result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        static StreamReader GetFileStreamReader(string filePath)
        {
            // Check if the file exists
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The file '{filePath}' was not found.");
            }

            // Return a StreamReader for the file
            return new StreamReader(filePath);
        }

        static string ConvertXmlToText(StreamReader reader)
        {
            // Load the XML content from the StreamReader
            XDocument doc = XDocument.Load(reader);

            var header = doc.Root.Element("Header");
            var format = header.Element("Format").Value;
            var samplesPerPixel = header.Element("SamplesPerPixel").Value;
            var resolution = header.Element("Resolution");
            var width = resolution.Element("Width").Value;
            var height = resolution.Element("Height").Value;

            var camera = doc.Root.Element("Camera");
            var position = camera.Element("Position");
            var direction = camera.Element("Direction");
            var fieldOfView = camera.Element("FieldOfView").Value;

            string output = $"#{format}\n\n{samplesPerPixel}\n\n{width} {height}\n\n";
            output += $"({position.Attribute("x").Value} {position.Attribute("y").Value} {position.Attribute("z").Value}) ";
            output += $"({direction.Attribute("x").Value} {direction.Attribute("y").Value} {direction.Attribute("z").Value}) {fieldOfView}\n\n";

            var geometry = doc.Root.Element("Geometry");
            var triangles = geometry.Elements("Triangle");

            foreach (var triangle in triangles)
            {
                var vertices = triangle.Elements().Where(e => e.Name.LocalName.StartsWith("Vertex")).ToList();
                var material = triangle.Element("Material");
                var reflectance = material.Element("Reflectance");
                var emission = material.Element("Emission");

                output += string.Join(" ", vertices.Select(v =>
                    $"({v.Attribute("x").Value} {v.Attribute("y").Value} {v.Attribute("z").Value})"));

                output += $" ({reflectance.Attribute("r").Value} {reflectance.Attribute("g").Value} {reflectance.Attribute("b").Value})";
                output += $" ({emission.Attribute("r").Value} {emission.Attribute("g").Value} {emission.Attribute("b").Value})\n";
            }

            return output;
        }
    }

}
