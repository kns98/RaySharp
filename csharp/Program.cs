using Microsoft.FSharp.Core;
using System.Text;

namespace minlightcsfs;

internal class Program
{
    public static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Invalid args");
        }
        else
        {
            var modelFilePathname = args[0];
            var imageFilePathname = Path.ChangeExtension(modelFilePathname, ".png");

            string modelFileContent = "";


            if (Path.GetExtension(modelFilePathname).Equals(".xml", StringComparison.OrdinalIgnoreCase))
            {
                // Handle XML file
                var xmlReader = new XmlReader(modelFilePathname);
                Console.WriteLine("XML File Content:");
                Console.WriteLine(xmlReader.Text);
            }
            else
            {
                // Read the entire model file into a string
                modelFileContent = File.ReadAllText(modelFilePathname);
            }

            // Handle non-XML model file
            using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(modelFileContent)))
            using (var modelFile = new StreamReader(memoryStream))
            { 
                var filetype = modelFile.ReadLine();
                    if (!"#MiniLight".Equals(filetype))
                    {
                        var message = "invalid model file";
                        throw Operators.Failure(message);
                    }

                    var iterations = int.Parse(modelFile.ReadLine());
                    var dimensions = modelFile.ReadLine().Split(" ");
                    var width = int.Parse(dimensions[0]);
                    var height = int.Parse(dimensions[1]);

                    var image = new RenderedImage(modelFile, width, height);
                    var camera = new Camera(modelFile);

                    var scene = new Scene(modelFile, camera.eyePoint);
                    var random = new Random(1);
                    var lastTime = Operators.Ref(-181.0);
                    for (var frameNo = 0; frameNo < iterations; frameNo++)
                    {
                        camera.frame(scene, image, random);
                        SaveImage(imageFilePathname, image, frameNo);
                        Console.WriteLine($"iteration: {frameNo}");
                    }
                }
            }
        }

        static void SaveImage(string imageFilePathname, RenderedImage renderedImage, int frameNo)
        {
            renderedImage.SaveImage(imageFilePathname, frameNo, false);
        }
    }
