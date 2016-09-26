using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    public class LXFML
    {
        [XmlAttribute]
        public string versionMajor;
        [XmlAttribute]
        public string versionMinor;
        [XmlAttribute]
        public string name;

        public MetaNode Meta;
        public CamerasNode Cameras;
        public BricksNode Bricks;
        public RigidSystemsNode RigidSystems;
        public GroupSystemsNode GroupSystems;
        public BuildingInstructionsNode BuildingInstructions;

        public static void WriteLXFML(List<BrickNode> assembly)
        {
            //Initialize file 
            CameraNode camera = new CameraNode();
            camera.refID = "0";
            camera.fieldOfView = "80";
            camera.distance = "106.46269226074219";
            camera.transformation = "-1,0,0,0,0.00063924590358510613,0.99999982118606567,0,0.99999982118606567,-0.00055945274652913213,-0.00034432366373948753,106.76267242431641,-0.078898213803768158";
            CamerasNode cameras = new CamerasNode();
            cameras.Camera = camera;

            BrickSetNode brickSet = new BrickSetNode();
            brickSet.version = "2075";

            BrandNode brand = new BrandNode();
            brand.name = "LDD";

            ApplicationNode application = new ApplicationNode();
            application.name = "LEGO Digital Designer";
            application.versionMajor = "4";
            application.versionMinor = "3";

            MetaNode meta = new MetaNode();
            meta.Application = application;
            meta.Brand = brand;
            meta.BrickSet = brickSet;

            /*            
            // DEBUG: Manual list of bricks
            BoneNode bn = new BoneNode();
            bn.refID = "0";
            bn.transformation= "-1,0,0,0,1,0,0,0,-1,		1.6,	0,	-0.8";
            PartNode pt = new PartNode();
            pt.refID ="0";
            pt.designID = "2780";
            pt.materials = "26";
            pt.Bone = bn;
            BrickNode br = new BrickNode();
            br.refID = "0";
            br.designID = "2780";
            br.itemNos = "278026";
            br.Part = new List<PartNode>();
            br.Part.Add(pt);

            BoneNode bn2 = new BoneNode();
            bn2.refID = "1";
            bn2.transformation = "1,0,0,0,1,0,0,0,1,		0,	0,	0";
            BoneNode bn3 = new BoneNode();
            bn3.refID = "2";
            bn3.transformation = "1,0,0,0,1,0,0,0,1,		7.2,	0,	0";
            PartNode pt2 = new PartNode();
            pt2.refID = "1";
            pt2.designID = "74042";
            pt2.materials = "194,1,194,194";
            pt2.decoration = "0,0,0";
            pt2.Bone = bn2;
            PartNode pt3 = new PartNode();
            pt3.refID = "2";
            pt3.designID = "54725";
            pt3.materials = "21";
            pt3.Bone = bn3;
            BrickNode br2 = new BrickNode();
            br2.refID = "1";
            br2.designID = "95658";
            br2.itemNos = "6009430";
            br2.Part = new List<PartNode>();
            br2.Part.Add(pt2);
            br2.Part.Add(pt3);

            BricksNode bricks= new BricksNode();
            bricks.cameraRef = "0";
            bricks.Brick = new List<BrickNode>();
            bricks.Brick.Add(br);
            bricks.Brick.Add(br2);

            //------------------------------------
            */
            BricksNode bricks = new BricksNode();
            bricks.cameraRef = "0";
            bricks.Brick = assembly;

            LXFML instr = new LXFML();
            instr.versionMajor = "5";
            instr.versionMinor = "0";
            instr.name = "Instruction.LXFML";
            instr.Meta = meta;
            instr.Cameras = cameras;
            instr.Bricks = bricks;

            // Serializes document
            System.Xml.Serialization.XmlSerializer writer =
                new System.Xml.Serialization.XmlSerializer(typeof(LXFML));
            System.Xml.Serialization.XmlSerializerNamespaces ns = new System.Xml.Serialization.XmlSerializerNamespaces();
            ns.Add("", "");

            // Writse file
            var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "//Instruction.LXFML";
            System.IO.FileStream file = System.IO.File.Create(path);
            writer.Serialize(file, instr, ns);
            file.Close();
        }

        public static LXFML ReadLXFML(LegoJoint joint)
        {

            string jointName = joint.name;
            if(joint.split>1)
                jointName += "_SPLIT[" + joint.split.ToString() + "]";
            LXFML myResponseData = new LXFML();

            System.Xml.Serialization.XmlSerializer mySerializer = new System.Xml.Serialization.XmlSerializer(typeof(LXFML));
            // StreamReader myStreamReader = new StreamReader(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "//Instruction.LXFML");
            StreamReader myStreamReader = new StreamReader("LegoJoint\\" + jointName + ".LXFML");
            myResponseData = (LXFML)mySerializer.Deserialize(myStreamReader);
            return myResponseData;
        }
                
    }
    
    public class MetaNode
    {
        public ApplicationNode Application;
        public BrandNode Brand;
        public BrickSetNode BrickSet;                               
    }
    
    public class ApplicationNode
    {
        [XmlAttribute]
        public string name;
        [XmlAttribute]
        public string versionMajor;
        [XmlAttribute]
        public string versionMinor;
    }

    public class BrandNode
    {
        [XmlAttribute]
        public string name;
    }
    
    public class BrickSetNode
    {
        [XmlAttribute]
        public string version;
    }
    

    public class CamerasNode
    {
        public CameraNode Camera;              
    }

    public class CameraNode
    {
        [XmlAttribute]
        public string refID;
        [XmlAttribute]
        public string fieldOfView;
        [XmlAttribute]
        public string distance;
        [XmlAttribute]
        public string transformation;

    }

    
    public class BricksNode
    {
        [XmlAttribute]
        public string cameraRef;
        [XmlElement("Brick")]
        public List<BrickNode> Brick;

        /*public IEnumerator<BrickNode> GetEnumerator()
        {
            foreach (var brick in Brick)
                yield return brick;
        }
        */
    }


    public class BrickNode
    {
        [XmlAttribute]
        public string refID;
        [XmlAttribute]
        public string designID;
        [XmlAttribute]
        public string itemNos;
        [XmlElement("Part")]
        public List<PartNode> Part;   
     
        
            
    }

    public class PartNode
    {
        [XmlAttribute]
        public string refID;
        [XmlAttribute]
        public string designID;
        [XmlAttribute]
        public string materials;
        [XmlAttribute]
        public string decoration;
        public BoneNode Bone;

     
    }

    public class BoneNode
    {
        [XmlAttribute]
        public string refID;
        [XmlAttribute]
        public string transformation;

        public const int DELTA_SPIT = 8;

        public static float[] TransformationToFloat(string transf)
        {
            float[] result = new float[12];
            string[] values = transf.Split(',');
            for (int i = 0; i < 12; i++)
            {
                result[i] = float.Parse(values[i], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            }
            return result;
        }

        public static string TransformationToString(float[] values)
        {
            string result = Convert.ToString(values[0]);
            
            for (int i = 1; i < 12; i++) 
            {
                string s = values[i].ToString("0.00").Replace(',','.');
                result = result + ", " + s/*Convert.ToString(values[i])*/;
            }
            return result;
        }

        public static BoneNode TranslateBoneNode(float deltaX, float deltaY, float deltaZ, BoneNode boneToMove)
        {
            float[] values = BoneNode.TransformationToFloat(boneToMove.transformation);
            values[9] += deltaX;
            values[10] += deltaY;
            values[11] += deltaZ;
            boneToMove.transformation = BoneNode.TransformationToString(values);            
            return boneToMove;
        }
        
    }
    
    
    public class RigidSystemsNode 
    {
        RigidSystemNode RigidSystem;        
        
        
    }

    public class RigidSystemNode 
    {
 
    }
    
    public class GroupSystemsNode 
    {
        GroupSystemNode GroupSystem;        
    }

    public class GroupSystemNode
    {

    }
   
    public class BuildingInstructionsNode 
    {
 
    }

    public class LegoJoint 
    {
        public string name;
        public float[] position;
        public string port;
        public int split;

        public LegoJoint()
        {
            position = new float[3];
        }
        
    }

}
