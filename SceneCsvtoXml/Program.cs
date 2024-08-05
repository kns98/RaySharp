using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Xml.Linq;

class Program
{
    static void Main(string[] args)
    {
        // Check if the correct number of arguments is provided
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: <program> <output_zip_path> <input_scene1> <input_scene2> ...");
            return;
        }

        // Read command-line arguments
        string outputZipPath = args[0];
        var sceneContents = new Dictionary<string, string>();

        // Read each input scene file
        for (int i = 1; i < args.Length; i++)
        {
            string fileName = args[i];
            if (File.Exists(fileName))
            {
                string sceneContent = File.ReadAllText(fileName);
                sceneContents[Path.GetFileName(fileName)] = sceneContent;
            }
            else
            {
                Console.WriteLine($"File not found: {fileName}");
                return;
            }
        }

        // Create a zip file with converted XML scenes
        using (ZipArchive zipFile = ZipFile.Open(outputZipPath, ZipArchiveMode.Create))
        {
            foreach (var kvp in sceneContents)
            {
                // Convert each scene to XML
                XElement xmlTree = ConvertSceneToXml(kvp.Value);

                // Convert the XML tree to a string
                string xmlString = xmlTree.ToString();

                // Create a new XML file name
                string xmlFileName = kvp.Key.Replace(".ml.txt", ".xml").Replace(".txt", ".xml");

                // Add the XML string to the zip file
                var zipEntry = zipFile.CreateEntry(xmlFileName);
                using (var writer = new StreamWriter(zipEntry.Open()))
                {
                    writer.Write(xmlString);
                }
            }
        }

        // Provide the path to the created zip file
        Console.WriteLine($"Zip file created at: {outputZipPath}");
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
