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
        output.AppendLine($"({GetAttributeValue(position, "x")} {GetAttributeValue(position, "y")} {GetAttributeValue(position, "z")}) " +
                          $"({GetAttributeValue(direction, "x")} {GetAttributeValue(direction, "y")} {GetAttributeValue(direction, "z")}) {fieldOfView}");
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
                $"({GetAttributeValue(v, "x")} {GetAttributeValue(v, "y")} {GetAttributeValue(v, "z")})")));

            output.Append($" ({GetAttributeValue(reflectance, "r")} {GetAttributeValue(reflectance, "g")} {GetAttributeValue(reflectance, "b")})");
            output.AppendLine($" ({GetAttributeValue(emission, "r")} {GetAttributeValue(emission, "g")} {GetAttributeValue(emission, "b")})");
        }

        return output.ToString();
    }

    string GetAttributeValue(XElement element, string attributeName)
    {
        // Return the attribute value or "0" if it is null or empty
        var attribute = element.Attribute(attributeName);
        return string.IsNullOrEmpty(attribute?.Value) ? "0" : attribute.Value;
    }
}
