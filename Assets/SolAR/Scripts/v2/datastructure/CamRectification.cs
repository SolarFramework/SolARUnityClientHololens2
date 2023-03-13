
using Com.Bcom.Solar.Gprc;
using System;

namespace Com.Bcom.Solar
{
    [Serializable]
    public class CamRectification
    {
        public readonly RotationMatrix rotation;
        public readonly ProjectionMatrix projection;
        public readonly StereoType stereoType;
        public readonly float baseline;

        public CamRectification(RotationMatrix rotation, ProjectionMatrix projection, StereoType stereoType, float baseline)
        {
            this.rotation = rotation;
            this.projection = projection;
            this.stereoType = stereoType;
            this.baseline = baseline;
        }
    }
}
