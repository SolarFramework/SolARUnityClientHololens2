using Com.Bcom.Solar.Gprc;
using System;

namespace Com.Bcom.Solar
{
    public class CamParameters
    {
        public readonly string name;
        public readonly uint id;
        public readonly CameraType type;
        public readonly CamResolution resolution;
        public readonly CamIntrinsics intrisincs;
        public readonly CamDistortion distortion;

        public CamParameters(
            string name,
            uint id,
            CameraType type,
            CamResolution resolution,
            CamIntrinsics intrisincs,
            CamDistortion distortion)
        {
            this.name = name;
            this.id = id;
            this.type = type;
            this.resolution = resolution;
            this.intrisincs = intrisincs;
            this.distortion = distortion;
        }
    }
}
