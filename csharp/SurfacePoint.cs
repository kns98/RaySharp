using Microsoft.FSharp.Core;

namespace minlightcsfs;

public class SurfacePoint
{
    internal double pi_k;
    internal Vector3f.vT position_m;
    internal Triangle triangle_m;

    public SurfacePoint(Triangle triangle_i, Vector3f.vT position_i)
    {
        var surfacePoint = this;
        pi_k = Math.PI;
        triangle_m = triangle_i;
        position_m = position_i;
    }

    public Triangle hitObject => triangle_m;
    public Vector3f.vT position => position_m;

    public Vector3f.vT emission(
        Vector3f.vT toPosition,
        Vector3f.vT outDirection,
        bool isSolidAngle)
    {
        var vT = Vector3f.op_Minus(toPosition, position_m);
        var val2 = Vector3f.vDot(vT, vT);
        var num = Vector3f.vDot(outDirection, triangle_m.normal) * triangle_m.area;
        var f1 = !isSolidAngle ? 1.0 : num / Math.Max(1E-06, val2);
        return num > 0.0 ? Vector3f.op_Mul(triangle_m.emitivity, f1) : Vector3f.vZero;
    }

    public Vector3f.vT reflection(
        Vector3f.vT inDirection,
        Vector3f.vT inRadiance,
        Vector3f.vT outDirection)
    {
        var num1 = Vector3f.vDot(inDirection, triangle_m.normal);
        var num2 = Vector3f.vDot(outDirection, triangle_m.normal);
        var num3 = 0.0;
        var num4 = num1;
        var num5 = num3 >= num4
            ? num3 <= num4 ? num3 != num4 ? num4 != num4 ? num3 == num3 ? 1 : 0 : -1 : 0 : 1
            : -1;
        var num6 = 0.0;
        var num7 = num2;
        var num8 = num6 >= num7
            ? num6 <= num7 ? num6 != num7 ? num7 != num7 ? num6 == num6 ? 1 : 0 : -1 : 0 : 1
            : -1;
        return num5 * num8 > 0
            ? Vector3f.op_Mul(inRadiance, triangle_m.reflectivity, Math.Abs(num1) / pi_k)
            : Vector3f.vZero;
    }

    public Tuple<Vector3f.vT, Vector3f.vT> nextDirection(
        Vector3f.vT inDirection,
        Random random)
    {
        var f1 = Vector3f.vDot(triangle_m.reflectivity, Vector3f.vOne) / 3.0;
        if (random.NextDouble() >= f1)
            return new Tuple<Vector3f.vT, Vector3f.vT>(Vector3f.vZero, Vector3f.vZero);
        var vT = Vector3f.op_Div(triangle_m.reflectivity, f1);
        FSharpFunc<Unit, double> fsharpFunc = new rand(random);
        var tuple = new Tuple<double, double>(pi_k * 2.0 * fsharpFunc.Invoke(null),
            Math.Sqrt(fsharpFunc.Invoke(null)));
        var num1 = tuple.Item2;
        var num2 = tuple.Item1;
        var scale =
            Vector3f.vCreate(Math.Cos(num2) * num1, Math.Sin(num2) * num1, Math.Sqrt(1.0 - num1 * num1));
        var tangent = triangle_m.tangent;
        var normal = triangle_m.normal;
        var v0 = Vector3f.vDot(normal, inDirection) < 0.0 ? Vector3f.vNeg(normal) : normal;
        return new Tuple<Vector3f.vT, Vector3f.vT>(Vector3f.vScaleFrame(new Vector3f.vT[3]
        {
            tangent,
            Vector3f.vCross(v0, tangent),
            v0
        }, scale), vT);
    }

    internal sealed class rand : FSharpFunc<Unit, double>
    {
        public Random random;

        internal rand(Random random)
        {
            this.random = random;
        }

        public override double Invoke(Unit unitVar0)
        {
            return random.NextDouble();
        }
    }
}