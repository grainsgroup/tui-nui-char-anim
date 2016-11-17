using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;




namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    class SensorInfo
    {
        
        // name of bone moved in Blender
        public string BoneName { get; set; }

        public string ObjectName { get; set; }

        public bool OrientationTrack { get; set; }
        

        public SensorInfo() { }

        public SensorInfo(string boneName, string objectName, bool orientationTrack) 
        {
            this.ObjectName = objectName;
            this.BoneName = boneName;
            this.OrientationTrack = orientationTrack;
        }

    }

    class SensorLegoInfo : SensorInfo
     {
        public int InputPort { get; set; }
         
        public string Axis { get; set; }

        public float Offset { get; set; }

        public float LocPos { get; set; }

        public bool LocationTrack { get; set; }

        public int RotationOrder { get; set; }

        public float Value { get; set; }

        public SensorLegoInfo(int inputPort, string boneName, string objectName, bool locationTrack, bool orientationTrack, string axis, float offset, float locPos, int rotationOrder) 
         {
            this.InputPort = inputPort;
            this.ObjectName = objectName;
            this.BoneName = boneName;
            this.LocationTrack = locationTrack;
            this.OrientationTrack = orientationTrack;
            this.Axis = axis;
            this.Offset = offset;
            this.LocPos = locPos;
            this.RotationOrder = rotationOrder;

            this.Value = 0;
        }
         


     }

    class SensorKinectInfo : SensorInfo
     {
         public Microsoft.Kinect.JointType Joint { get; set; }
         
         public float Factor { get; set; }

         public float OffsetLocX { get; set; }
         public float OffsetLocY { get; set; }
         public float OffsetLocZ { get; set; }
         public float OffsetW { get; set; }         
         public float OffsetX { get; set; }         
         public float OffsetY { get; set; }         
         public float OffsetZ { get; set; }
         public bool LocationXTrack { get; set; }
         public bool LocationYTrack { get; set; }
         public bool LocationZTrack { get; set; }
         public float LocPosX { get; set; }
         public float LocPosY { get; set; }
         public float LocPosZ { get; set; }
         public string takeXFrom { get; set; }
         public string takeYFrom { get; set; }
         public string takeZFrom { get; set; }

         public SensorKinectInfo(Microsoft.Kinect.JointType joint, string boneName, string objectName, bool locationXTrack, bool locationYTrack, bool locationZTrack, bool orientationTrack, float factor, float offsetW, float offsetX, float offsetY, float offsetZ, float offsetLocX, float offsetLocY, float offsetLocZ, float locPosX, float locPosY, float locPosZ, string xFrom, string yFrom, string zFrom)
         {
             this.Joint = joint;
             this.BoneName = boneName;
             this.ObjectName = objectName;
             this.LocationXTrack = locationXTrack;
             this.LocationYTrack = locationYTrack;
             this.LocationZTrack = locationZTrack;
             this.OrientationTrack = orientationTrack;
             this.Factor = factor;
             this.OffsetW = offsetW;
             this.OffsetX = offsetX;
             this.OffsetY = offsetY;
             this.OffsetZ = offsetZ;
             this.OffsetLocX = offsetLocX;
             this.OffsetLocX = offsetLocY;
             this.OffsetLocX = offsetLocZ;
             this.LocPosX = locPosX;
             this.LocPosY = locPosY;
             this.LocPosZ = locPosZ;
             this.takeXFrom = xFrom;
             this.takeYFrom = yFrom;
             this.takeZFrom = zFrom;
         }


         
     }    

    public class Preset {
        
        public string Name { get; set; }

        public string TouchCommand { get; set; }

        public ArrayList sensorKinectInfoSet {get; set;}

        public ArrayList sensorLegoInfoSet { get; set; }

        
        public Preset(string name)
        {
            this.Name = name;
            this.sensorKinectInfoSet = new ArrayList();
            this.sensorLegoInfoSet = new ArrayList();
        }

        public override bool Equals(Object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
                return false;

            Preset p = (Preset)obj;
            return (Name == p.Name);
        }
    }

    public class ObjectRotationMatrix
    {
        public string ObjectName { get; set; }
        public List<Matrix4x4> RotationMatrix { get; set; }

        public ObjectRotationMatrix(string name) 
        {
            this.ObjectName = name;
            this.RotationMatrix = new List<Matrix4x4>(3);
            Matrix4x4 IdentityMatrix = Matrix4x4.Identity;
            RotationMatrix.Add(IdentityMatrix);
            RotationMatrix.Add(IdentityMatrix);
            RotationMatrix.Add(IdentityMatrix);            
        }

        public override bool Equals(Object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
                return false;

            ObjectRotationMatrix p = (ObjectRotationMatrix)obj;
            return (ObjectName == p.ObjectName);
        }
    }
}
