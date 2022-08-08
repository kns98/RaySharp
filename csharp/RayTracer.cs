using Microsoft.FSharp.Core;

namespace minlightcsfs;

public class RayTracer
{
    internal Scene scene_m;

    public RayTracer(Scene scene_i)
    {
        scene_m = scene_i;
    }

    public Vector3f.vT radiance(Vector3f.vT rayOrigin, Vector3f.vT rayDirection,
        FSharpOption<Triangle> lastHit, Random random)
    {
        return iradiance(rayOrigin, rayDirection, lastHit, random);
    }

    internal Vector3f.vT emitterSample(Vector3f.vT rayDirection, SurfacePoint surfacePoint, Random random)
    {
        var tuple = scene_m.emitter(random);
        if (tuple.Item1 == null) return Vector3f.vZero;
        var item = tuple.Item1;
        var emitterPosition = tuple.Item2;
        var emitter = item.Value;
        var emitDirection = Vector3f.vUnitize(Vector3f.op_Minus(emitterPosition, surfacePoint.position));
        var fsharpOption = scene_m.intersection(surfacePoint.position,
            emitDirection, FSharpOption<Triangle>.Some(surfacePoint.hitObject));
        Vector3f.vT vT;
        if (fsharpOption != null)
        {
            var fsharpOption2 = fsharpOption;
            var hitObject = fsharpOption2.Value.Item1;
            var x = hitObject;
            var y = emitter;
            if (!LanguagePrimitives.HashCompare.GenericEqualityIntrinsic(x, y))
            {
                var hitObject2 = fsharpOption2.Value.Item1;
                vT = Vector3f.vZero;
                goto IL_C3;
            }
        }

        var sp = new SurfacePoint(emitter, emitterPosition);
        vT = sp.emission(surfacePoint.position, Vector3f.vNeg(emitDirection), true);
        IL_C3:
        var emissionIn = vT;
        return surfacePoint.reflection(emitDirection,
            Vector3f.op_Mul(emissionIn, scene_m.emittersCount), Vector3f.vNeg(rayDirection));
    }

    internal Vector3f.vT iradiance(Vector3f.vT rayOrigin, Vector3f.vT rayDirection,
        FSharpOption<Triangle> lastHit, Random random)
    {
        var fsharpOption =
            scene_m.intersection(rayOrigin, rayDirection, lastHit);
        if (fsharpOption == null) return scene_m.defaultEmission(Vector3f.vNeg(rayDirection));
        var fsharpOption2 = fsharpOption;
        var triangle = fsharpOption2.Value.Item1;
        var hitPosition = fsharpOption2.Value.Item2;
        var surfacePoint = new SurfacePoint(triangle, hitPosition);
        var localEmission = lastHit == null
            ? surfacePoint.emission(rayOrigin, Vector3f.vNeg(rayDirection), false)
            : Vector3f.vZero;
        var illumination = emitterSample(rayDirection, surfacePoint, random);
        var tuple = surfacePoint.nextDirection(Vector3f.vNeg(rayDirection), random);
        var nextDirection = tuple.Item1;
        var color = tuple.Item2;
        var vT = nextDirection;
        var vZero = Vector3f.vZero;
        var reflection = vT.isZero()
            ? Vector3f.vZero
            : Vector3f.op_Mul(color,
                iradiance(surfacePoint.position, nextDirection,
                    FSharpOption<Triangle>.Some(surfacePoint.hitObject), random));
        return Vector3f.op_Plus(reflection, illumination, localEmission);
    }
}