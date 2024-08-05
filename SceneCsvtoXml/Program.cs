using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;

class Program
{
    static void Main(string[] args)
    {
        // Get all .txt files in the current directory
        string[] txtFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.txt");

        if (txtFiles.Length == 0)
        {
            Console.WriteLine("No .txt files found in the current directory.");
            return;
        }

        // Process each .txt file
        foreach (string fileName in txtFiles)
        {
            try
            {
                string sceneContent = File.ReadAllText(fileName);

                // Convert the scene content to XML
                XElement xmlTree = ConvertSceneToXml(sceneContent);

                // Convert the XML tree to a string
                string xmlString = xmlTree.ToString();

                // Create a new XML file name
                string xmlFileName = Path.ChangeExtension(fileName, ".xml");

                // Write the XML string to the file
                File.WriteAllText(xmlFileName, xmlString);

                Console.WriteLine($"Converted {fileName} to {xmlFileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing {fileName}: {ex.Message}");
            }
        }
    }

    // Function to convert a single scene file to XML format
    static XElement ConvertSceneToXml(string sceneText)
    {
        // Regular expressions to capture different parts of the scene file
        Regex headerRe = new Regex(@"#MiniLight\s+(\d+)\s+(\d+)\s+(\d+)", RegexOptions.Singleline);
        Regex cameraRe = new Regex(@"\(([^)]+)\) \(([^)]+)\) ([\d.]+)");
        Regex illuminationRe = new Regex(@"\(([^)]+)\) \(([^)]+)\)");
        Regex trianglesRe = new Regex(@"\(([^)]+)\) \(([^)]+)\) \(([^)]+)\)  \(([^)]+)\) \(([^)]+)\)", RegexOptions.Multiline);

        // Parsing header
        Match headerMatch = headerRe.Match(sceneText);
        string samples = headerMatch.Groups[1].Value;
        string width = headerMatch.Groups[2].Value;
        string height = headerMatch.Groups[3].Value;

        // Parsing camera
        Match cameraMatch = cameraRe.Match(sceneText);
        string[] cameraPos = cameraMatch.Groups[1].Value.Split();
        string[] cameraDir = cameraMatch.Groups[2].Value.Split();
        string fov = cameraMatch.Groups[3].Value;

        // Parsing global illumination
        Match illuminationMatch = illuminationRe.Match(sceneText);
        string[] ambientLight = illuminationMatch.Groups[1].Value.Split();
        string[] groundReflection = illuminationMatch.Groups[2].Value.Split();

        // Create XML root
        XElement scene = new XElement("Scene");

        // Create header element
        XElement header = new XElement("Header",
            new XElement("Format", "MiniLight"),
            new XElement("SamplesPerPixel", samples),
            new XElement("Resolution",
                new XElement("Width", width),
                new XElement("Height", height)));

        // Create camera element
        XElement camera = new XElement("Camera",
            new XElement("Position",
                new XAttribute("x", cameraPos[0]),
                new XAttribute("y", cameraPos[1]),
                new XAttribute("z", cameraPos[2])),
            new XElement("Direction",
                new XAttribute("x", cameraDir[0]),
                new XAttribute("y", cameraDir[1]),
                new XAttribute("z", cameraDir[2])),
            new XElement("FieldOfView", fov));

        // Create global illumination element
        XElement illumination = new XElement("GlobalIllumination",
            new XElement("AmbientLight",
                new XAttribute("r", ambientLight[0]),
                new XAttribute("g", ambientLight[1]),
                new XAttribute("b", ambientLight[2])),
            new XElement("GroundReflection",
                new XAttribute("r", groundReflection[0]),
                new XAttribute("g", groundReflection[1]),
                new XAttribute("b", groundReflection[2])));

        // Create geometry element
        XElement geometry = new XElement("Geometry");
        foreach (Match match in trianglesRe.Matches(sceneText))
        {
            string[] vertex1 = match.Groups[1].Value.Split();
            string[] vertex2 = match.Groups[2].Value.Split();
            string[] vertex3 = match.Groups[3].Value.Split();
            string[] reflectance = match.Groups[4].Value.Split();
            string[] emission = match.Groups[5].Value.Split();

            XElement triangle = new XElement("Triangle",
                new XElement("Vertex1",
                    new XAttribute("x", vertex1[0]),
                    new XAttribute("y", vertex1[1]),
                    new XAttribute("z", vertex1[2])),
                new XElement("Vertex2",
                    new XAttribute("x", vertex2[0]),
                    new XAttribute("y", vertex2[1]),
                    new XAttribute("z", vertex2[2])),
                new XElement("Vertex3",
                    new XAttribute("x", vertex3[0]),
                    new XAttribute("y", vertex3[1]),
                    new XAttribute("z", vertex3[2])),

                new XElement("Material",
                    new XElement("Reflectance",
                        new XAttribute("r", reflectance[0]),
                        new XAttribute("g", reflectance[1]),
                        new XAttribute("b", reflectance[2])),
                    new XElement("Emission",
                        new XAttribute("r", emission[0]),
                        new XAttribute("g", emission[1]),
                        new XAttribute("b", emission[2]))));

            geometry.Add(triangle);
        }

        scene.Add(header);
        scene.Add(camera);
        scene.Add(illumination);
        scene.Add(geometry);

        return scene;
    }
}
