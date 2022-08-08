using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;

namespace minlightcsfs;

public class Scene
{
    internal Triangle[] emitters_m;
    internal Vector3f.vT groundReflection_m;
    internal SpatialIndex.SpatialIndex_t index_m;
    internal Vector3f.vT skyEmission_m;

    public Scene(TextReader inBuffer_i, Vector3f.vT eyePosition_i)
    {
        var scene = this;
        var num = 1048576;
        var v1 = Vector3f.vRead(inBuffer_i);
        var v2 = Vector3f.vRead(inBuffer_i);
        var _list = FSharpList<Triangle>.Empty;
        var fsharpList = ListModule.Reverse(FSharpFunc<FSharpList<Triangle>, int>.InvokeFast(
            new readTriangle(inBuffer_i), _list, num));
        var list = ListModule.Filter(isEmitter.instance, fsharpList);
        ArrayModule.OfList(fsharpList);
        emitters_m = ArrayModule.OfList(list);
        index_m = SpatialIndex.create(eyePosition_i, fsharpList);
        skyEmission_m = Vector3f.vClamp(Vector3f.vZero, Vector3f.vMaximum, v1);
        groundReflection_m = Vector3f.op_Mul(skyEmission_m, Vector3f.vClamp(Vector3f.vZero, Vector3f.vOne, v2));
    }

    public int emittersCount => ArrayModule.Length(emitters_m);

    public FSharpOption<Tuple<Triangle, Vector3f.vT>> intersection(
        Vector3f.vT rayOrigin,
        Vector3f.vT rayDirection,
        FSharpOption<Triangle> lastHit)
    {
        return SpatialIndex.intersection(index_m, rayOrigin, rayDirection, null, lastHit);
    }

    public Tuple<FSharpOption<Triangle>, Vector3f.vT> emitter(
        Random random)
    {
        if (emittersCount <= 0)
            return new Tuple<FSharpOption<Triangle>, Vector3f.vT>(null, Vector3f.vZero);
        var triangle = emitters_m[((random.Next() & ((1 << 16) - 1)) * emitters_m.Length) >> 16];
        return new Tuple<FSharpOption<Triangle>, Vector3f.vT>(FSharpOption<Triangle>.Some(triangle),
            triangle.samplePoint(random));
    }

    public Vector3f.vT defaultEmission(Vector3f.vT eyeDirection)
    {
        return eyeDirection.x < 0.0 ? skyEmission_m : groundReflection_m;
    }
}

internal sealed class
    readTriangle : OptimizedClosures.FSharpFunc<FSharpList<Triangle>, int, FSharpList<Triangle>>
{
    public TextReader inBuffer_i;

    internal readTriangle(TextReader inBuffer_i)
    {
        this.inBuffer_i = inBuffer_i;
    }

    public override FSharpList<Triangle> Invoke(
        FSharpList<Triangle> ts,
        int i)
    {
        try
        {
            return i == 0
                ? ts
                : FSharpFunc<FSharpList<Triangle>, int>.InvokeFast(this,
                    FSharpList<Triangle>.Cons(new Triangle(inBuffer_i), ts), i - 1);
        }
        catch
        {
            return ts;
        }
    }
}

internal sealed class isEmitter : FSharpFunc<Triangle, bool>
{
    internal static readonly isEmitter instance = new();

    public override bool Invoke(Triangle t)
    {
        return !t.emitivity.isZero() && t.area > 0.0;
    }
}