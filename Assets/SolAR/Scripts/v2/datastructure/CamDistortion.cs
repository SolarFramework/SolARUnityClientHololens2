using System;

namespace Com.Bcom.Solar
{
    [Serializable]
    public class CamDistortion
    {
        public readonly float k1;
        public readonly float k2;
        public readonly float p1;
        public readonly float p2;
        public readonly float k3;

        public CamDistortion(float k1, float k2, float p1, float p2, float k3)
        {
            this.k1 = k1;
            this.k2 = k2;
            this.p1 = p1;
            this.p2 = p2;
            this.k3 = k3;
        }
    }
}
