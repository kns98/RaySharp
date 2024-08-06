using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

// Class to represent 3D points or vectors
class Vector3Text
{
    public float X { get; }
    public float Y { get; }
    public float Z { get; }

    public Vector3Text(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public override string ToString()
    {
        return $"({X} {Y} {Z})";
    }
}

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
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"The file '{filePath}' was not found.");
        }

        return new StreamReader(filePath);
    }

    string ConvertXmlToText(StreamReader reader)
    {
        XDocument doc = XDocument.Load(reader);

        var header = doc.Root.Element("Header");
        string format = header.Element("Format").Value;
        int samplesPerPixel = int.Parse(header.Element("SamplesPerPixel").Value);
        var resolution = header.Element("Resolution");
        int width = int.Parse(resolution.Element("Width").Value);
        int height = int.Parse(resolution.Element("Height").Value);

        var camera = doc.Root.Element("Camera");
        var position = CreateVector3(camera.Element("Position"));
        var direction = CreateVector3(camera.Element("Direction"));
        float fieldOfView = float.Parse(camera.Element("FieldOfView").Value);

        StringBuilder output = new StringBuilder();

        output.AppendLine($"#{format}");
        output.AppendLine();
        output.AppendLine(samplesPerPixel.ToString());
        output.AppendLine();
        output.AppendLine($"{width} {height}");
        output.AppendLine();

        output.AppendLine($"{position} {direction} {fieldOfView}");
        output.AppendLine();

        var globalIllumination = doc.Root.Element("GlobalIllumination");
        var ambientLight = CreateVector3(globalIllumination.Element("AmbientLight"));
        var groundReflection = CreateVector3(globalIllumination.Element("GroundReflection"));

        output.Append($"{ambientLight}");
        output.Append(" ");
        output.AppendLine($"{groundReflection}");
        output.AppendLine();

        var geometry = doc.Root.Element("Geometry");
        var triangles = geometry.Elements("Triangle");

        foreach (var triangle in triangles)
        {
            var vertices = triangle.Elements().Where(e => e.Name.LocalName.StartsWith("Vertex")).Select(CreateVector3).ToList();
            var material = triangle.Element("Material");
            var reflectance = CreateVector3(material.Element("Reflectance"));
            var emission = CreateVector3(material.Element("Emission"));

            output.Append(string.Join(" ", vertices));
            output.Append($" {reflectance}");
            output.AppendLine($" {emission}");
        }

        return output.ToString();
    }

    Vector3Text CreateVector3(XElement element)
    {
        return new Vector3Text(
            GetAttributeValueAsFloat(element, "x"),
            GetAttributeValueAsFloat(element, "y"),
            GetAttributeValueAsFloat(element, "z")
        );
    }

    float GetAttributeValueAsFloat(XElement element, string attributeName)
    {
        var attribute = element.Attribute(attributeName);
        return string.IsNullOrEmpty(attribute?.Value) ? 0f : float.Parse(attribute.Value);
    }
}
