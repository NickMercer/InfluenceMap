using System;

namespace Natick.InfluenceMaps
{
    [Serializable]
    public enum InfluenceCurve
    {
        Linear,
        Quad,
        FourPoly,
        Solid,
        InverseQuad,
        InverseFourPoly
    }
}