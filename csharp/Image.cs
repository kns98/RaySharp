//#define PARALLEL // define to make a parallel renderer here and in Camera

using System.Text;

//using SkiaSharp;

namespace minlightcsfs;

public class Image
{
    // format items
    private const string PPM_ID = "P6";
    private const string MINILIGHT_URI = "http://www.hxa7241.org/minilight/";
    private const string LOMONT_URI = "http://www.lomont.org";
    private const string KNS_URI = "Kevin Sheth";

    // guess of average screen maximum brightness
    private static readonly double DISPLAY_LUMINANCE_MAX = 200.0f;

    // ITU-R BT.709 standard RGB luminance weighting
    private static readonly Vector3f.vT RGB_LUMINANCE = new(0.2126f, 0.7152f, 0.0722f);

    // ITU-R BT.709 standard gamma
    private static readonly double GAMMA_ENCODE = 0.45f;
    private readonly Vector3f.vT[,] pixels;
    private readonly int MAX_HEIGHT = 32000;
    private readonly int MAX_WIDTH = 32000;

    /// <summary>
    ///     Create image based on size from stream
    /// </summary>
    /// <param name="infile"></param>
    public Image(StreamReader infile, int width, int height)
    {
        // read width and height
        Width = width;
        Height = height;

        // clamp width and height
        Width = Width < 1 ? 1 : Width > MAX_WIDTH ? MAX_WIDTH : Width;
        Height = Height < 1 ? 1 : Height > MAX_HEIGHT ? MAX_HEIGHT : Height;
        pixels = new Vector3f.vT [Width, Height];
        for (var i = 0; i < Width; ++i)
        for (var j = 0; j < Height; ++j)
            pixels[i, j] = new Vector3f.vT(0, 0, 0);
    }

    /*
     * Pixel sheet with simple tone-mapping and file formatting.<br/><br/>
     *
     * Uses PPM image format:
     * <cite>http://netpbm.sourceforge.net/doc/ppm.html</cite><br/><br/>
     *
     * Uses Ward simple tonemapper:
     * <cite>'A Contrast Based Scalefactor For Luminance Display'
     * Ward;
     * Graphics Gems 4, AP 1994.</cite><br/><br/>
     *
     * Uses RGBE image format:
     * <cite>http://radsite.lbl.gov/radiance/refer/filefmts.pdf</cite>
     * <cite>'Real Pixels' Ward; Graphics Gems 2, AP 1991;</cite><br/><br/>
     *
     * @invariants
     * * width_m  >= 1 and <= 10000
     * * height_m >= 1 and <= 10000
     * * pixels_m.size() == (width_m * height_m)
     */
    public int Width { get; set; }
    public int Height { get; set; }

    /// <summary>
    ///     Get RGB byte array of image data
    /// </summary>
    /// <param name="iteration"></param>
    /// <returns></returns>
    public byte[,,] GetImageBytes(int iteration)
    {
        var data = new byte[Width, Height, 3]; // RGB buffer of bytes

        // make pixel value accumulation divider
        var divider = 1.0f / ((iteration > 0 ? iteration : 0) + 1);
        var tonemapScaling = CalculateToneMapping(pixels, divider);

        // write pixels
        for (var j = 0; j < Height; ++j)
        for (var i = 0; i < Width; ++i)
        {
            // tonemap
            var red = GetColorRed(i, j, divider, tonemapScaling);
            var green = GetColorGreen(i, j, divider, tonemapScaling);
            var blue = GetColorBlue(i, j, divider, tonemapScaling);

            // store as byte
            data[i, j, 0] = (byte)(red <= 255.0d ? red : 255.0d);
            data[i, j, 1] = (byte)(green <= 255.0d ? green : 255.0d);
            data[i, j, 2] = (byte)(blue <= 255.0d ? blue : 255.0d);
        }

        return data;
    }

    private double GetColorRed(int i, int j, float divider, double tonemapScaling)
    {
        var mapped = pixels[i, j].x * divider * tonemapScaling;
        // gamma encode
        mapped = Math.Pow(mapped > 0.0d ? mapped : 0.0d, GAMMA_ENCODE);
        // quantize
        mapped = Math.Floor(mapped * 255.0d + 0.5d);
        return mapped;
    }

    private double GetColorGreen(int i, int j, float divider, double tonemapScaling)
    {
        var mapped = pixels[i, j].y * divider * tonemapScaling;
        // gamma encode
        mapped = Math.Pow(mapped > 0.0d ? mapped : 0.0d, GAMMA_ENCODE);
        // quantize
        mapped = Math.Floor(mapped * 255.0d + 0.5d);
        return mapped;
    }

    private double GetColorBlue(int i, int j, float divider, double tonemapScaling)
    {
        var mapped = pixels[i, j].z * divider * tonemapScaling;
        // gamma encode
        mapped = Math.Pow(mapped > 0.0d ? mapped : 0.0d, GAMMA_ENCODE);
        // quantize
        mapped = Math.Floor(mapped * 255.0d + 0.5d);
        return mapped;
    }

    private static void SaveToPpm(byte[,,] img, int width, int height, string path, int frame)
    {
        using (var writer = new BinaryWriter(File.Open(path + "_frame_" + frame + ".ppm", FileMode.Create)))
        {
            // Write magic number P6, width, height and maximum color value
            writer.Write(
                Encoding.ASCII.GetBytes(string.Format("P6\n{0} {1}\n{2}\n", width, height, 255)));
            for (var _height = 0; _height < height; _height++)
            for (var _width = 0; _width < width; _width++)
            {
                var r = img[_width, _height, 0];
                var g = img[_width, _height, 1];
                var b = img[_width, _height, 2];
                writer.Write(r);
                writer.Write(g);
                writer.Write(b);
            }
        }
    }

    public void SaveImage(string filename, int frame, bool showPNG)
    {
        var data = GetImageBytes(frame);
        SaveToPpm(data, Width, Height, filename, frame);
        if (showPNG)
        {
            //to do:
        }
    }

    public void AddToPixel(int x, int y, Vector3f.vT radiance)
    {
        pixels[x, y] = Vector3f.op_Plus(pixels[x, y], radiance);
    }

    private double CalculateToneMapping(Vector3f.vT[,] pixels, double divider)
    {
        // calculate log mean luminance
        var logMeanLuminance = 1e-4d;
        double sumOfLogs = 0.0f;
        foreach (var p in pixels)
        {
            var Y = Vector3f.vDot(p, RGB_LUMINANCE) * divider;
            sumOfLogs += Math.Log10(Y > 1e-4f ? Y : 1e-4f);
        }

        logMeanLuminance = Math.Pow(10.0f, sumOfLogs / pixels.Length);

        // (what do these mean again? (must check the tech paper...))
        var a = 1.219f + Math.Pow(DISPLAY_LUMINANCE_MAX * 0.25f, 0.4f);
        var b = 1.219f + Math.Pow(logMeanLuminance, 0.4f);
        return Math.Pow(a / b, 2.5f) / DISPLAY_LUMINANCE_MAX;
    }
}