﻿namespace minlightcsfs;

public class Camera
{
    internal readonly Vector3f.vT right_m;
    internal readonly Vector3f.vT up_m;
    internal readonly double viewAngle_m;
    internal readonly Vector3f.vT viewDirection_m;
    internal readonly Vector3f.vT viewPosition_m;

    public Camera(TextReader inBuffer_i)
    {
        var viewPosition_c = Vector3f.vRead(inBuffer_i);
        var vd = Vector3f.vUnitize(Vector3f.vRead(inBuffer_i));
        var vT = vd;
        var vZero = Vector3f.vZero;
        var viewDirection_c =
            vT.isZero() ? Vector3f.vOneZ : vd;
        var s = Scanf.getLine(inBuffer_i);
        var text = s;
        var s2 = text;
        var v = double.Parse(s2);
        var viewAngle_c = Math.Max(10.0, Math.Min(v, 160.0)) * (3.1415926535897931 / 180.0);
        var right = Vector3f.vUnitize(Vector3f.vCross(Vector3f.vOneY, viewDirection_c));
        var vT2 = right;
        var vZero2 = Vector3f.vZero;
        Tuple<Vector3f.vT, Vector3f.vT> tuple;
        if (!vT2.isZero())
        {
            tuple = new Tuple<Vector3f.vT, Vector3f.vT>(right,
                Vector3f.vUnitize(Vector3f.vCross(viewDirection_c, right)));
        }
        else
        {
            var up = viewDirection_c.x >= 0.0 ? Vector3f.vNeg(Vector3f.vOneZ) : Vector3f.vOneZ;
            tuple = new Tuple<Vector3f.vT, Vector3f.vT>(Vector3f.vUnitize(Vector3f.vCross(up, viewDirection_c)),
                up);
        }

        var tuple2 = tuple;
        var up_c = tuple2.Item2;
        var right_c = tuple2.Item1;
        viewPosition_m = viewPosition_c;
        viewAngle_m = viewAngle_c;
        viewDirection_m = viewDirection_c;
        right_m = right_c;
        up_m = up_c;
    }

    public Vector3f.vT eyePoint => viewPosition_m;

    public Image frame(Scene scene, Image image, Random random,  int total_iterations, int iteration)
    {
        var i = 0;
        var rayTracer = new RayTracer(scene);
        var _lock = new object();
        var rand = new Random();
        var total = (image.Height - 1) * (image.Width - 1);
        Parallel.For(0, total, delegate(int xy)
        {
            var x = xy % image.Width;
            var y = (xy - x) / image.Width;

            int num_samples = total_iterations-iteration;
            Stack<Vector3f.vT> stack = new Stack<Vector3f.vT>();

            var rad = Vector3f.vZero;
            for (int i = 0; i < num_samples; i++)
            {
                for (int k = 0; k < num_samples; k++)
                {
                    rad = Vector3f.op_Plus(rad, VT(image, random, x, y, rayTracer, rand, i,k, num_samples));
                }
            }

            image.AddToPixel(x, Math.Abs(y - (image.Width - 1)), 
                Vector3f.op_Div(rad,num_samples));
            const int _div = 100000;
            var j = Interlocked.Increment(ref i);
            if (j % _div == 0) Console.WriteLine(j / (total / 100) + " % of pixels processed:" + DateTime.Now);
        });
        return image;
    }

    private Vector3f.vT VT(Image image, Random random, int x, int y, RayTracer rayTracer, Random rand,int sample_i,int sample_j,int total_samples)
    {
        double offset_mul = sample_i / total_samples;

        var xF = (x + random.NextDouble()) * (2.0d*offset_mul) / image.Width - (1.0d*offset_mul/2);
        var yF = (y + random.NextDouble()) * (2.0d*offset_mul) / image.Height - (1.0d*offset_mul/2);

        // make minImage plane offset vector
        var offset = Vector3f.op_Plus(Vector3f.op_Mul(right_m, xF),
            Vector3f.op_Mul(up_m, yF * (image.Height / image.Width)));
        var sampleDirection = Vector3f.vUnitize(Vector3f.op_Plus(viewDirection_m,
            Vector3f.op_Mul(offset, Math.Tan(viewAngle_m * 0.5 *  (sample_j/total_samples)  ))));
        var radiance = rayTracer.radiance(viewPosition_m, sampleDirection, null, rand);
        return radiance;
    }
}