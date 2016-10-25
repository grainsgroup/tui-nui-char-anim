using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using System.Threading.Tasks;

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    public class Packet
    {
        public ArrayList payload { get; set; }
        public string header { get; set; }

        public Packet() 
        {
            payload = new ArrayList();
        }
    }

        
    class Motions
    {
        public string objectName { get; set; }
        public string boneName { get; set; }
        public Position vectorPos { get; set; }
        public Orientation vectorOr { get; set; }
        public bool locXTrack { get; set; }
        public bool locYTrack { get; set; }
        public bool locZTrack { get; set; }
        public bool rotTrack { get; set; }
               
    }

    class Orientation
    {
        public float W { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Orientation ShallowCopy()
        {
            return (Orientation)this.MemberwiseClone();
        }
    }
    
    class Position
    {
        public float locX { get; set; }
        public float locY { get; set; }
        public float locZ { get; set; }

        public Position ShallowCopy()
        {
            return (Position)this.MemberwiseClone();
        }
    }

    public class Bone
    {
        public string name { get; set; }
        public string parent { get; set; }
        public List<string> children { get; set; }
        public List<char> rot_DoF { get; set; }
        public List<char> loc_DoF { get; set; }
        public int level { get; set; }

        public Bone(string name) 
        {
            this.name = name;
            this.parent = string.Empty;
            this.level = 0;
            rot_DoF = new List<char>();
            loc_DoF = new List<char>();
            children = new List<string>();
        }

        public override bool Equals(Object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
                return false;

            Bone s = (Bone)obj;
            return (s.name.Equals(this.name));
        }

        public string GetHash() 
        {
            
            string a = Metrics.GetDofString(this.rot_DoF).GetHashCode().ToString();
            string b = Metrics.GetDofString(this.loc_DoF).GetHashCode().ToString();
            string c = this.level.ToString().GetHashCode().ToString();
            return a + b + c;
        }

    }

  

}
