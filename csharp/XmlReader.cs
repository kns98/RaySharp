using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

class XmlReader
{
    public string Text { get; private set; }
    public XmlReader(string filePath)
    {
       
        using (StreamReader reader = GetFileStreamReader(filePath))
        {
            Text = ConvertXmlToText(reader);
        }
    }

     StreamReader GetFileStreamReader(string filePath)
    {
        // Check if the file exists
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"The file '{filePath}' was not found.");
        }

        // Return a StreamReader for the file
        return new StreamReader(filePath);
    }

     string ConvertXmlToText(StreamReader reader)
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

        StringBuilder output = new StringBuilder();

        output.AppendLine($"#{format}");
        output.AppendLine();
        output.AppendLine(samplesPerPixel);
        output.AppendLine();
        output.AppendLine($"{width} {height}");
        output.AppendLine();
        output.AppendLine($"({position.Attribute("x").Value} {position.Attribute("y").Value} {position.Attribute("z").Value}) " +
                          $"({direction.Attribute("x").Value} {direction.Attribute("y").Value} {direction.Attribute("z").Value}) {fieldOfView}");
        output.AppendLine();

        var geometry = doc.Root.Element("Geometry");
        var triangles = geometry.Elements("Triangle");

        foreach (var triangle in triangles)
        {
            var vertices = triangle.Elements().Where(e => e.Name.LocalName.StartsWith("Vertex")).ToList();
            var material = triangle.Element("Material");
            var reflectance = material.Element("Reflectance");
            var emission = material.Element("Emission");

            output.Append(string.Join(" ", vertices.Select(v =>
                $"({v.Attribute("x").Value} {v.Attribute("y").Value} {v.Attribute("z").Value})")));

            output.Append($" ({reflectance.Attribute("r").Value} {reflectance.Attribute("g").Value} {reflectance.Attribute("b").Value})");
            output.AppendLine($" ({emission.Attribute("r").Value} {emission.Attribute("g").Value} {emission.Attribute("b").Value})");
        }

        return output.ToString();
    }
}
