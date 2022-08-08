using Microsoft.FSharp.Core;

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
            var modelFile = File.OpenText(modelFilePathname);
            var filetype = Scanf.getLine(modelFile);
            if (!"#MiniLight".Equals(filetype))
            {
                var message = "invalid model file";
                throw Operators.Failure(message);
            }

            var line = Scanf.getLine(modelFile);
            var s = line;
            var _line = Scanf.getLine(modelFile);
            var wh = _line;
            var width = int.Parse(_line.Split(" ")[0]);
            var height = int.Parse(_line.Split(" ")[1]);
            var iterations = int.Parse(s);
            var image = new Image(modelFile, width, height);
            var camera = new Camera(modelFile);
            var scene = new Scene(modelFile, camera.eyePoint);
            var random = new Random(1);
            var lastTime = Operators.Ref(-181.0);
            for (var frameNo = 0; frameNo < iterations; frameNo++)
            {
                camera.frame(scene, image, random);
                saveImage(imageFilePathname, image, frameNo);
                PrintfModule
                    .PrintFormat(
                        new PrintfFormat<FSharpFunc<int, Unit>, TextWriter, Unit, Unit, int>("iteration: %u\n"))
                    .Invoke(frameNo);
            }
        }
    }

    private static void saveImage(string imageFilePathname, Image image, int frameNo)
    {
        image.SaveImage(imageFilePathname, frameNo, false);
    }
}