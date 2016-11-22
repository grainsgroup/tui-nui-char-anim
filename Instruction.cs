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

    public class Instruction 
    {
        public static List<Bone> SplitHandlers(List<Bone> handlers)
        {
            List<Bone> result = new List<Bone>();
            handlers.Sort(delegate(Bone b1, Bone b2) { return b1.level.CompareTo(b2.level); });
            foreach (Bone currentHandler in handlers)
            {
                List<string> components = SplitNameInBricks(currentHandler.name);

                // Finds the level of the parent
                int parentIndex = handlers.FindIndex(x => x.name.Equals(currentHandler.parent));
                int startLevel = 0;
                string parentComponentName = "";
                if (parentIndex >= 0)
                {
                    // This bone is not the root of the armature
                    List<string> parentComponents = SplitNameInBricks(handlers[parentIndex].name);

                    // Last component of the parent in assemblyArm
                    parentComponentName = parentComponents[parentComponents.Count - 1];
                    int j = result.FindIndex(x => x.name.Equals(parentComponentName));
                    startLevel = result[j].level + 1;
                }

                for (int level = 0; level < components.Count; level++)
                {
                    string s = components[level];

                    Bone boneToAdd = new Bone(s);
                    //Calculate level from parents
                    boneToAdd.level = startLevel + level;
                    //Updates the parentComponentName for the next bone 
                    boneToAdd.parent = parentComponentName;
                    parentComponentName = s;

                    if (level < components.Count - 1)
                    {
                        boneToAdd.children.Add(components[level + 1]);
                    }
                    else
                    {
                        foreach (string child in currentHandler.children)
                        {
                            boneToAdd.children.Add(SplitNameInBricks(child)[0]);
                        }
                    }

                    result.Add(boneToAdd);

                }
            }
            return result;
        }

        public static List<string> SplitNameInBricks(string name)
        {
            name = name.Replace(" ", "");
            List<string> bricks = name.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            //bricks.RemoveAll(x => x.Contains("_NUI"));
            return bricks;
        }

        public static List<LegoJoint> GetLegoAssembly(List<Bone> armature)
        {
            List<LegoJoint> result = new List<LegoJoint>();
            List<List<Bone>> levels = new List<List<Bone>>();
            LegoJoint jointToAdd;

            // subdivides based on the level nodes
            for (int i = 0; i < AutomaticMapping.GetMaxLengthChain(armature); i++)
            {
                levels.Add(new List<Bone>());
                foreach (Bone b in armature)
                {
                    if (b.level == i)
                    {
                        levels[i].Add(b);
                    }
                }
            }

            int displacementLevel0 = 0;
            for (int i = 0; i < AutomaticMapping.GetMaxLengthChain(armature); i++)
            {
                foreach (Bone currentBone in levels[i])
                {
                    jointToAdd = new LegoJoint();                    
                    if (currentBone.name.Contains("_NUI"))
                    {
                        jointToAdd.name = currentBone.name.Substring(0, currentBone.name.IndexOf("_NUI"));
                        jointToAdd.port = currentBone.name.Substring(currentBone.name.IndexOf("_NUI") + 5, 6 ); 
                    }
                    else
                    {
                        if(currentBone.name.Contains("ROT"))
                        {
                            jointToAdd.name = currentBone.name.Substring(0, currentBone.name.IndexOf("(PORT-")) + "_" +
                                currentBone.name.Substring(currentBone.name.IndexOf("ROT"), 6);
                        }
                        if (currentBone.name.Contains("LOC"))
                        {
                            jointToAdd.name = currentBone.name.Substring(0, currentBone.name.IndexOf("(PORT-")) + "_" +
                                currentBone.name.Substring(currentBone.name.IndexOf("LOC"), 6);
                            jointToAdd.name = jointToAdd.name.Replace("LOC(L)", "ROT(x)");
                        }

                        jointToAdd.port = currentBone.name.Substring(currentBone.name.IndexOf("(PORT-") + 6, 2);
                    }
                    jointToAdd.split = currentBone.children.Count;

                    if (currentBone.level == 0)
                    {
                        jointToAdd.position[0] = 0;
                        jointToAdd.position[1] = 0;
                        jointToAdd.position[2] = displacementLevel0;
                        result.Add(jointToAdd);
                        displacementLevel0 += 30;
                    }
                    else
                    {
                        string jointParent = string.Empty;
                        int parentLegoJointIndex = 0;
                        if (currentBone.parent.Contains("_NUI"))
                        {
                            jointParent = currentBone.parent.Substring(0, currentBone.parent.IndexOf("_NUI"));
                            parentLegoJointIndex = result.FindIndex(
                            x => x.name.Equals(jointParent) &&
                            x.port.Equals(currentBone.parent.Substring(currentBone.parent.IndexOf("_NUI") + 5, 6)));
                        }
                        else 
                        {
                            if (currentBone.parent.Contains("ROT"))
                            {
                                jointParent = currentBone.parent.Substring(0, currentBone.parent.IndexOf("(PORT-")) + "_" +
                                    currentBone.parent.Substring(currentBone.parent.IndexOf("ROT"), 6);
                            }
                            if (currentBone.parent.Contains("LOC"))
                            {
                                jointParent = currentBone.parent.Substring(0, currentBone.parent.IndexOf("(PORT-")) + "_" +
                                    currentBone.parent.Substring(currentBone.parent.IndexOf("LOC"), 6);
                                jointParent = jointParent.Replace("LOC(L)", "ROT(x)");
                            }

                            parentLegoJointIndex = result.FindIndex(
                            x => x.name.Equals(jointParent) &&
                            x.port.Equals(currentBone.parent.Substring(currentBone.parent.IndexOf("(PORT-") + 6, 2)));
                        }
                                                
                        int parentBoneIndex = armature.FindIndex(x => x.name.Equals(currentBone.parent));

                        //
                        // POS(i) = POS(i.parent) + Delta(i.parent) + Delta(neighbor)
                        //

                        // POS(i.parent) 
                        //jointToAdd.position = result[parentLegoJointIndex].position; 
                        jointToAdd.position[0] = result[parentLegoJointIndex].position[0];
                        jointToAdd.position[1] = result[parentLegoJointIndex].position[1];
                        jointToAdd.position[2] = result[parentLegoJointIndex].position[2];

                        // Delta(i.parent)
                        UpdateDeltaFromJointType(jointToAdd, result[parentLegoJointIndex].name);

                        // Delta(neighbor)
                        UpdateDeltaFromNeighbor(jointToAdd, armature[parentBoneIndex], currentBone);
                        result.Add(jointToAdd);

                    }
                }
            }


            return result;
        }

        public static void UpdateDeltaFromNeighbor(LegoJoint jointToAdd, Bone parent, Bone current)
        {
            if (parent.children.Count > 1)
            {
                int indexChild = parent.children.FindIndex(x => x.Equals(current.name));
                int deltaNeigh = (indexChild - parent.children.Count / 2) * BoneNode.DELTA_SPIT;
                if (deltaNeigh >= 0)
                    deltaNeigh += BoneNode.DELTA_SPIT;
                jointToAdd.position[2] += deltaNeigh;
            }
            //+,-8
        }

        public static void UpdateDeltaFromJointType(LegoJoint jointToAdd, string jointType)
        {
            //"Hip_NUI_DoF(x):LOC(L)"
            //string legoJoint = GetLegoJoint(b.name);
            switch (jointType)
            {
                case "Hip":
                    jointToAdd.position[0] += 4f;
                    break;

                case "LMotor_ROT(x)":
                    jointToAdd.position[0] += 16;
                    break;

                case "LMotor_ROT(y)":
                    jointToAdd.position[0] += 13.7f;
                    jointToAdd.position[2] += -5.6f;
                    break;

                case "LMotor_ROT(z)":
                    jointToAdd.position[0] += 12;
                    jointToAdd.position[1] += 4.8f;
                    jointToAdd.position[2] += -3.2f;
                    break;

                case "MMotor_ROT(x)":
                    jointToAdd.position[0] += 13.6f;
                    jointToAdd.position[1] += -4;
                    break;

                case "MMotor_ROT(y)":
                    jointToAdd.position[0] += 14.48f;
                    jointToAdd.position[1] += 0.8f;
                    break;

                case "MMotor_ROT(z)":
                    jointToAdd.position[0] += 9.6f;
                    jointToAdd.position[1] += 2.4f;
                    break;


                case "Gyroscope_ROT(x)":
                    jointToAdd.position[0] += 18.4f;
                    jointToAdd.position[1] += 1.6f;
                    jointToAdd.position[2] += 2.4f;
                    break;

                case "Gyroscope_ROT(y)":
                    jointToAdd.position[0] += 15.2f;
                    jointToAdd.position[2] += 0.8f;
                    break;

                case "Gyroscope_ROT(z)":
                    jointToAdd.position[0] += 16;
                    jointToAdd.position[1] += 1.6f;
                    jointToAdd.position[2] += -1.6f;
                    break;


            }



        }
    }

}
