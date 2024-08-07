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

    public RenderedImage frame(Scene scene, RenderedImage renderedImage, Random random)
    {
        var rayTracer = new RayTracer(scene);
        var action = new Render2d(rayTracer, renderedImage, new Random(),
            up_m, right_m, viewDirection_m, viewAngle_m, viewPosition_m);
        ParallelHelper.For2D(0, renderedImage.Height - 1, 0, renderedImage.Width - 1, action);
        return renderedImage;
    }

    private class Render2d : IAction2D
    {
        private readonly double ang;
        private int count;
        private readonly Vector3f.vT dir;
        private readonly RenderedImage renderedImage;
        private readonly Vector3f.vT pos;
        private readonly Random rand;
        private readonly RayTracer rayTracer;
        private readonly Vector3f.vT right;
        private readonly int total;
        private readonly Vector3f.vT up;

        public Render2d(RayTracer r, RenderedImage i, Random ra, Vector3f.vT up, Vector3f.vT right,
            Vector3f.vT dir, double ang, Vector3f.vT pos)
        {
            rayTracer = r;
            renderedImage = i;
            rand = ra;
            this.up = up;
            this.right = right;
            this.dir = dir;
            this.ang = ang;
            this.pos = pos;
            total = (renderedImage.Height - 1) * (renderedImage.Width - 1);
        }

        public void Invoke(int x, int y)
        {
            var xF = (x + rand.NextDouble()) * 2.0d / renderedImage.Width - 1.0d;
            var yF = (y + rand.NextDouble()) * 2.0d / renderedImage.Height - 1.0d;

            // make minImage plane offset vector
            var offset = Vector3f.op_Plus(Vector3f.op_Mul(right, xF),
                Vector3f.op_Mul(up, yF * (renderedImage.Height / renderedImage.Width)));
            var sampleDirection = Vector3f.vUnitize(Vector3f.op_Plus(dir,
                Vector3f.op_Mul(offset, Math.Tan(ang * 0.5))));
            var radiance = rayTracer.radiance(pos, sampleDirection, null, rand);
            renderedImage.AddToPixel(x, Math.Abs(y - (renderedImage.Width - 1)), radiance);
            const int _div = 100000;
            var j = Interlocked.Increment(ref count);
            if (j % _div == 0) Console.WriteLine(j / (total / 100) + " % of pixels processed:" + DateTime.Now);
        }
    }
}