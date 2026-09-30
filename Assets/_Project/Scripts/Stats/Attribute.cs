using System;

namespace EchoesOfTheRift.Stats
{
    [Serializable]
    public struct Attribute
    {
        public float Base;
        public float Flat;
        public float Percent;
        public float Evaluate()
        {
            double value=((double)Finite(Base)+Finite(Flat))*(1.0+Finite(Percent));
            return (float)Math.Max(-float.MaxValue,Math.Min(float.MaxValue,value));
        }
        private static float Finite(float value) => float.IsNaN(value)||float.IsInfinity(value) ? 0 : value;
    }
}
