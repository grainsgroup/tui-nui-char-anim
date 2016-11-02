using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    class UserPreference
    {
        public bool LocRotCheckBox;
        public bool UseSensorCheckBox;
        public float NodSim;
        public float DofCov;
        public float ComRan;
        public float ComAnn;
        public float PosInC;
        public float Sym;
        public float ParCou;

        public UserPreference(bool locRotCheckBox, bool useSensorCheckBox, float nodSim, float dofCov, float comRan, float comAnn, float posInC, float sym, float parCou)
        {
            // TODO: Complete member initialization
            this.LocRotCheckBox = locRotCheckBox;
            this.UseSensorCheckBox = useSensorCheckBox;
            this.NodSim = nodSim;
            this.DofCov = dofCov;
            this.ComRan = comRan;
            this.ComAnn = comAnn;
            this.PosInC = posInC;
            this.Sym = sym;
            this.ParCou = parCou;
        }
    }
}
