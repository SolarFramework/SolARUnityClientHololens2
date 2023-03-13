using System;

namespace Com.Bcom.Solar
{
    [Serializable]
    public class CamIntrinsics
    {
        public readonly float fx;
        public readonly float fy;
        public readonly float cx;
        public readonly float cy;

        public CamIntrinsics(float fx, float fy, float cx, float cy)
        {
            this.fx = fx;
            this.fy = fy;
            this.cx = cx;
            this.cy = cy;
        }
    }

}
