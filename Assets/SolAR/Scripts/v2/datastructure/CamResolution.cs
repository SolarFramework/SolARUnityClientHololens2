using System;

namespace Com.Bcom.Solar
{
    [Serializable]
    public class CamResolution
    {
        public readonly uint width;
        public readonly uint height;

        public CamResolution(uint width, uint height)
        {
            this.width = width;
            this.height = height;
        }
    }
}
