using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickGraph;
using Lego.Ev3.Core;


namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    public class KinectPieces
    {
        public int Score { get; set; }
        public Bone Bones {get; set; }

        public KinectPieces(int score, Bone bones)
        {
            this.Bones = bones;
            this.Score = score;
        }
    }


    public static class AutomaticMapping
    {
        const int MAX_COST = 100;

        
        public static List<Bone> GetLocHandler(Brick brick)
        {
            List<Bone> handler = GetTuiHandler(GetTuiComponent(true, brick));
            foreach (List<Bone> kinectPartition in GetKinectSkeleton())
                foreach (Bone bone in GetOneDofBones(kinectPartition,false,true))
                    handler.Add(bone);
            return handler;
        }

        public static List<Bone> GetLocBone(List<Bone> armature)
        {
            List<Bone> locBones = new List<Bone>();
            foreach (Bone bone in armature)
            {
                if (bone.loc_DoF.Count > 0)
                {
                    locBones.Add(bone);
                }
            }
            return locBones;
        }

        public static List<List<List<Bone>>> GraphPartitioning(int motors, BidirectionalGraph<Bone, Edge<Bone>> graph, List<List<Bone>> components, List<List<List<Bone>>> graphPartitions, bool splitDofCheckBox, bool isRotOnly)
        {
            if (!splitDofCheckBox)
            {
                foreach (List<Bone> armaturePart in components)
                {
                    // this list is defined partial because contains only the partition for a specific connected component
                    List<List<List<Bone>>> partialGraphPartitions = PartitionByDepthFirstSearch(armaturePart, graph, motors, isRotOnly);
                    int iteration = 0;

                    foreach (List<List<Bone>> partialPartition in partialGraphPartitions)
                    {
                        if (graphPartitions.Count > 0)
                        {
                            iteration = graphPartitions.Count;
                            // combines all partial partition obtained
                            for (int i = 0; i < iteration; i++)
                            {
                                graphPartitions.Add(partialPartition.Concat(graphPartitions[i]).ToList());
                            }                            

                        }
                        else
                        {
                            // is the first or the unique connected component of the graph
                            graphPartitions = partialGraphPartitions;
                            break;
                        }
                    }
                    graphPartitions.RemoveRange(0, iteration);
                }
            }

            // TODO: debuggare
            if (splitDofCheckBox)
            {
                foreach (List<Bone> armaturePart in components)
                {
                    // this list is defined partial because contains only the partition for a specific connected component
                    List<List<List<Bone>>> partialGraphPartitions = PartitionUniformDoF(armaturePart, graph, motors);

                    foreach (List<List<Bone>> partialPartition in partialGraphPartitions)
                    {
                        if (graphPartitions.Count > 0)
                        {
                            int iteration = graphPartitions.Count;

                            // combines all partial partition obtained
                            for (int i = 0; i < iteration; i++)
                            {
                                graphPartitions.Add(partialPartition.Concat(graphPartitions[i]).ToList());
                            }
                            graphPartitions.RemoveRange(0, iteration);

                        }
                        else
                        {
                            // is the first or the unique connected component of the graph
                            graphPartitions = partialGraphPartitions;
                            break;
                        }
                    }
                }
            }


            /*  IMPLEMENTATION WITHOUT GRAPH CONNETCED COMPONENTS
             * 
            // Partition uniform DoF
            if(this.SplitDofCheckBox.IsChecked.Value)
            {            
                foreach (List<List<Bone>> result in PartitionUniformDoF(armature, graph, motors))
                {
                    graphPartitions.Add(result);
                }
            }
            */

            /*
            // Partition with depthFirstSearch
            foreach (List<List<Bone>> result in PartitionByDepthFirstSearch(armaturePart, graph, motors))
            {
                graphPartitions.Add(result);
            }
            */

            return graphPartitions;
        }

        public static List<AxisArrangement> ComputeLocAssignment(List<Bone> locBones, List<Bone> handler/*, bool leftHanded*/)
        {
            List<AxisArrangement> arrangements = new List<AxisArrangement>();
            foreach (Bone bone in locBones) 
            {
                List<Bone> locBoneOneDof = GetOneDofBones(locBones, false, true);
                
                // solves assignment problem with Hungarian Algorithm                                        
                int[,] costsMatrix = new int[locBoneOneDof.Count, handler.Count];
                // initialize costMatrix
                for (int row = 0; row < locBoneOneDof.Count; row++)
                {
                    for (int col = 0; col < handler.Count; col += locBones.Count)
                    {
                        costsMatrix[row, col] = LocDoFSimilarityScore(locBoneOneDof[row], handler[col]) +
                            ComponentRangeScore(handler[col]) +
                            SymmetryScore(locBoneOneDof[row], handler[col / locBones.Count]) + 
                            PiecesPreferenceScore(locBoneOneDof[row], handler[col])/* +
                            UserPreferenceScore(handler[col], leftHanded)*/;
                    }
                }

                int[] assignment = HungarianAlgorithm.FindAssignments(costsMatrix);
                                
                int score = 0;
                for (int ass = 0; ass < assignment.Length; ass++)
                {
                    score += costsMatrix[ass, assignment[ass]];
                }
                
                List<List<Bone>> partition = new List<List<Bone>>();
                partition.Add(locBoneOneDof);
                arrangements.Add(new AxisArrangement(bone.name,assignment,partition,handler,score));
            }                        
            return arrangements;
        }

        /// <summary>
        /// Returns list of one Dof Bone
        /// </summary>
        /// <param name="bones"> List of Bones to convert</param>
        /// <param name="rotDof"> Consider rot DoF</param>
        /// <param name="locDof"> Consider lot Dot</param>
        /// <returns>List of one Dof Bone</returns>
        public static List<Bone> GetOneDofBones(List<Bone> bones, bool rotDof, bool locDof)
        {            
            List<Bone> oneDofBones = new List<Bone>();
            foreach (Bone b in bones)
            {
                if (locDof)
                {
                    foreach (char c in b.loc_DoF)
                    {
                        Bone DofToAdd = new Bone(b.name + "_LOC(" + c + ")");
                        DofToAdd.loc_DoF.Add(c);
                        DofToAdd.level = b.level;
                        oneDofBones.Add(DofToAdd);
                    } 
                }
                if (rotDof)
                {
                    foreach (char c in b.rot_DoF)
                    {
                        Bone DofToAdd = new Bone(b.name + "_ROT(" + c + ")");
                        DofToAdd.rot_DoF.Add(c);
                        DofToAdd.level = b.level;
                        oneDofBones.Add(DofToAdd);
                    } 
                }
            }
            return oneDofBones;    
        }

        /* private static int UserPreferenceScore(Bone bone, bool leftHanded)
        {
            int cost = 1;
            if (leftHanded && bone.name.Contains(".L"))
            {
                cost = 0;
            }
            if (!leftHanded && bone.name.Contains(".R"))
            {
                cost = 0;
            }
            return cost;
        }
        */

        private static int ComponentRangeScore(Bone bone)
        {
            // cost = 4m(Max Distance read from Kinect specification) - range of the sensor
            // range is calculated considering that the animator is at a distance of 3 meters from the kinect
 
            
            int cost = MAX_COST;

            if (bone.name.Contains("_LOC(x)"))
                cost = 1;
            if (bone.name.Contains("_LOC(y)"))
            {
                List<List<Bone>> kinectSkeleton = GetKinectSkeleton();
                if (kinectSkeleton[0].Contains(bone) || kinectSkeleton[1].Contains(bone))
                    cost = 1;
                if (kinectSkeleton[2].Contains(bone))
                    cost = 2;
                //if (kinectSkeleton[3].Contains(bone) || kinectSkeleton[4].Contains(bone))
                //    cost = 3;
            }
            if (bone.name.Contains("_LOC(z)"))
                cost = 2;
            
            if (bone.name.Contains("_ROT("))
                cost = GetCostRange(bone);

            if (bone.name.Contains("(PORT"))
            {
                string componentName = bone.name.Substring(0, bone.name.IndexOf("(PORT-"));
                switch (componentName)
                {
                    case "Gyroscope":
                    case "MMotor":
                    case "LMotor":
                        cost = 0;
                        break;

                    case "Ultrasonic":
                        cost = 2;
                        break;
                }
            }
            return cost;
        }

        private static int GetCostRange(Bone bone)
        {
            int cost = 0;
            string boneName = bone.name;
            if(bone.name.Contains("_ROT"))
                 boneName= bone.name.Remove(bone.name.IndexOf("_ROT"));            
            
            switch (boneName) 
            {
                case "Shoulder.R":
                case "Shoulder.L":
                    cost = 4;
                    break;
                
                case "Elbow.R":
                case "Elbow.L":
                    cost = 1;
                    break;
               
                case "Wrist.R":
                case "Wrist.L":
                    cost = 1;
                    break;

                case "Hand.R":
                case "Hand.L":
                    cost = 4;
                    break;

                case "Head.C":
                    cost = 3;
                    break;
                
                case "Hip.C":
                    cost = 3;
                    break;

                case "Hip.R":
                case "Hip.L":

                case "Knee.R":
                case "Knee.L":
                
                case "Ankle.R":
                case "Ankle.L":
                
                case "Foot.R":
                case "Foot.L":
                    cost = 5;
                    break;
                default:
                    cost = MAX_COST;
                    break;
            }
            return cost;
        }

        private static int PiecesPreferenceScore(Bone bone, Bone handler)
        {
            int cost = MAX_COST;

            if (handler.name.Contains("(x)") || handler.name.Contains("(y)") || handler.name.Contains("(z)"))
            {
                cost = Math.Abs(bone.level - handler.level);                
            }

            if(handler.name.Contains("(PORT-"))
            {                
                string componentName = handler.name.Substring(0, handler.name.IndexOf("(PORT-"));
                if (bone.loc_DoF.Count>0)
                {                    
                    switch (componentName)
                    {
                        case "Ultrasonic":

                            cost = bone.level;
                            break;

                        case "LMotor":
                        case "MMotor":
                        case "Gyroscope":
                            cost = 5 + bone.level;
                            break;
                    }
                }
                if (bone.rot_DoF.Count > 0)
                {
                    cost = ComponentAssignment(bone.rot_DoF[0], componentName);
                }
            }

            return cost;
        }

        private static int LocDoFSimilarityScore(Bone locBone, Bone handler)
        {
            int cost = 0;
            foreach (char c in locBone.loc_DoF)
            {
                if (!handler.loc_DoF.Contains(c))
                {
                    cost = MAX_COST;
                    break;
                }
            }            
            return cost;
        }

        ///<summary>
        ///Returns a list of bones that represent motors and sensors of the tangible Interface
        ///</summary>        
        private static List<Bone> GetTuiHandler(List<string> list)
        {
            List<Bone> result = new List<Bone>();
            List<char> axis = new List<char>() { 'x', 'y', 'z' };
            foreach (String component in list)
            {
                Bone b = new Bone(component);
                b.rot_DoF = axis;
                b.loc_DoF = axis;
                result.Add(b);
            }
            return result;
        }
        
        public static List<List<Bone>> GetConnectedComponentList(BidirectionalGraph<Bone, Edge<Bone>> graph)
        {
            var g = 
                new QuickGraph.Algorithms.ConnectedComponents.WeaklyConnectedComponentsAlgorithm<Bone, Edge<Bone>>(graph);
            g.Compute();           

            List<List<Bone>> components = new List<List<Bone>>();
            for (int i = 0; i < g.ComponentCount; i++)
            {
                components.Add(new List<Bone>());
            }

            foreach (Bone b in g.Components.Keys.ToList())
            {
                components[g.Components[b]].Add(b);
            }

            return components;
        }

        public static BidirectionalGraph<Bone, Edge<Bone>> CreateDirectedGraph(List<Bone> armature)
        {

            var graph = new BidirectionalGraph<Bone, Edge<Bone>>();

            // Creates directed graph 
            foreach (Bone b in armature)
            {
                graph.AddVertex(b);
                foreach (string child in b.children)
                {
                    graph.AddVerticesAndEdge(new Edge<Bone>(b, GetBoneFromName(child, armature)));
                    graph.AddEdge(new Edge<Bone>(GetBoneFromName(child, armature), b));
                }
            }
            return graph;
        }

        public static UndirectedGraph<Bone, Edge<Bone>> CreateUndirectedGraph(List<Bone> armature)
        {

            var graph = new UndirectedGraph<Bone, Edge<Bone>>();

            // Creates directed graph 
            foreach (Bone b in armature)
            {
                graph.AddVertex(b);
                foreach (string child in b.children)
                {
                    graph.AddVerticesAndEdge(new Edge<Bone>(b, GetBoneFromName(child, armature)));                    
                }
            }
            return graph;
        }


        private static List<List<Bone>> GetKinectSkeleton()
        {
            List<List<Bone>> skeleton = new List<List<Bone>>();
            List<char> axis = new List<char>() { 'x', 'y', 'z' };

            Bone shoulder_right = new Bone("Shoulder.R");
            shoulder_right.rot_DoF = new List<char>() { 'z' };
            shoulder_right.loc_DoF = axis;
            shoulder_right.level = 0;
            shoulder_right.parent = "Shoulder";
            shoulder_right.children = new List<string>() { "Elbow.R" };

            Bone elbow_right = new Bone("Elbow.R");
            elbow_right.rot_DoF = new List<char>() { 'x', 'y', 'z' };
            elbow_right.loc_DoF = axis;
            elbow_right.level = 1;
            elbow_right.parent = "Shoulder.R";
            elbow_right.children = new List<string>() {"Wrist.R"};
            
            Bone wrist_right = new Bone("Wrist.R");
            wrist_right.rot_DoF = new List<char>() { 'x' };
            wrist_right.loc_DoF = axis;
            wrist_right.level = 2;
            wrist_right.parent = "Elbow.R";
            wrist_right.children = new List<string>() { "Hand.R" };
            
            Bone hand_right = new Bone("Hand.R");
            hand_right.rot_DoF = new List<char>() { 'x', 'z' };
            hand_right.loc_DoF = axis;
            hand_right.level = 3;
            hand_right.parent = "Wrist.R";
            hand_right.children = new List<string>() { };
            
            List<Bone> upper_right = new List<Bone>();
            upper_right.Add(shoulder_right);
            upper_right.Add(elbow_right);
            upper_right.Add(wrist_right);
            upper_right.Add(hand_right);
            skeleton.Add(upper_right);

            

            Bone shoulder_left = new Bone("Shoulder.L");
            shoulder_left.rot_DoF = new List<char>() { 'z' };
            shoulder_left.loc_DoF = axis;
            shoulder_left.level = 0;
            shoulder_left.parent = "Shoulder";
            shoulder_left.children = new List<string>() { "Elbow.L"};

            Bone elbow_left = new Bone("Elbow.L");
            elbow_left.rot_DoF = new List<char>() { 'x', 'y', 'z' };
            elbow_left.loc_DoF = axis;
            elbow_left.level = 1 ;
            elbow_left.parent = "Shoulder.L";
            elbow_left.children = new List<string>() { "Wrist.L" };

            Bone wrist_left = new Bone("Wrist.L");
            wrist_left.rot_DoF = new List<char>() { 'x' };
            wrist_left.loc_DoF = axis;
            wrist_left.level = 2;
            wrist_left.parent = "Elbow.L";
            wrist_left.children = new List<string>() { "Hand.L" };

            Bone hand_left = new Bone("Hand.L");
            hand_left.rot_DoF = new List<char>() { 'x', 'z' };
            hand_left.loc_DoF = axis;
            hand_left.level = 3;
            hand_left.parent = "Wrist.L";
            hand_left.children = new List<string>() { };

            List<Bone> upper_left = new List<Bone>();
            upper_left.Add(shoulder_left);
            upper_left.Add(elbow_left);
            upper_left.Add(wrist_left);
            upper_left.Add(hand_left);
            skeleton.Add(upper_left);



            Bone head = new Bone("Head");
            head.rot_DoF = new List<char>() { 'x', 'z' };
            head.loc_DoF = axis;
            head.level = 3;
            head.parent = "Shoulder";
            head.children = new List<string>() { };

            Bone shoulder_center = new Bone("Shoulder");
            shoulder_center.rot_DoF = new List<char>() { '0' };
            shoulder_center.loc_DoF = axis;
            shoulder_center.level = 2;
            shoulder_center.parent = "Spine";
            shoulder_center.children = new List<string>() {"Shoulder.R", "Shoulder.L", "Head" };

            Bone spine = new Bone("Spine");
            spine.rot_DoF = new List<char>() { '0' };
            spine.loc_DoF = axis;
            spine.level = 1;
            spine.parent = "Hip";
            spine.children = new List<string>() { "Shoulder" };
            
            Bone hip_center = new Bone("Hip");
            hip_center.rot_DoF = new List<char>() { 'x', 'y', 'z' };
            hip_center.loc_DoF = axis;
            hip_center.level = 0;
            hip_center.parent = "";
            hip_center.children = new List<string>() { "Spine"};
            
            List<Bone> center = new List<Bone>();
            center.Add(head);
            center.Add(shoulder_center);
            center.Add(spine);
            center.Add(hip_center);
            skeleton.Add(center);

            /*
            Bone hip_right = new Bone("Hip.R");
            hip_right.rot_DoF = new List<char>() { '0' };
            hip_right.loc_DoF = axis;
            Bone knee_right = new Bone("Knee.R");
            knee_right.rot_DoF = new List<char>() { 'x', 'z' };
            knee_right.loc_DoF = axis;
            Bone ankle_right = new Bone("Ankle.R");
            ankle_right.rot_DoF = new List<char>() { 'x' ,'z' };
            ankle_right.loc_DoF = axis;
            Bone foot_right = new Bone("Foot.R");
            foot_right.rot_DoF = new List<char>() { '0' };
            foot_right.loc_DoF = axis;

            List<Bone> lower_right = new List<Bone>();
            lower_right.Add(hip_right);
            lower_right.Add(knee_right);
            lower_right.Add(ankle_right);
            lower_right.Add(foot_right);
            skeleton.Add(lower_right);
            
            
            Bone hip_left = new Bone("Hip.L");
            hip_left.rot_DoF = new List<char>() { '0' };
            hip_left.loc_DoF = axis;
            Bone knee_left = new Bone("Knee.L");
            knee_left.rot_DoF = new List<char>() { 'x', 'z' };
            knee_left.loc_DoF = axis;
            Bone ankle_left = new Bone("Ankle.L");
            ankle_left.rot_DoF = new List<char>() { 'x' ,'z' };
            ankle_left.loc_DoF = axis;
            Bone foot_left = new Bone("Foot.L");
            foot_left.rot_DoF = new List<char>() { '0' };
            foot_left.loc_DoF = axis;

            List<Bone> lower_left = new List<Bone>();
            lower_left.Add(hip_left);
            lower_left.Add(knee_left);
            lower_left.Add(ankle_left);
            lower_left.Add(foot_left);
            skeleton.Add(lower_left);

            */

            return skeleton;

        }

        public static AxisArrangement ComputeKinectAssignmentScore(Dictionary<string, List<List<char>>> dictionary, List<List<Bone>> currentPartition)
        {

            List<List<Bone>> kinectSkeleton = GetKinectSkeleton();
            List<Bone> motorDecomposition = new List<Bone>();

            // solves assignment problem with Hungarian Algorithm                                        
            int[,] costsMatrix = new int[currentPartition.Count, kinectSkeleton.Count * currentPartition.Count];
            // initialize costMatrix
            for (int row = 0; row < currentPartition.Count; row++)
            {
                for (int col = 0; col < kinectSkeleton.Count * currentPartition.Count;
                    col += currentPartition.Count)
                {
                    KinectPieces kinectBones = ChainSimilarityScore(currentPartition[row], kinectSkeleton[col / currentPartition.Count]);
                    int cost = kinectBones.Score /*+ currentPartition.Count*/;
                    motorDecomposition.Add(kinectBones.Bones);

                    for (int index = 0; index < currentPartition.Count; index++)
                    {
                        costsMatrix[row, col + index] = cost;                        
                    }

                }
            }


            int[] assignment = HungarianAlgorithm.FindAssignments(costsMatrix);

            int totalCost = 0;
            // computes cost for this assignment
            for (int ass = 0; ass < assignment.Length; ass++)
            {
                totalCost += costsMatrix[ass, assignment[ass]];
                assignment[ass] = assignment[ass] % currentPartition.Count + currentPartition.Count * ass; 
            }

            return new AxisArrangement("Kinect_Configuration", assignment, currentPartition, motorDecomposition, totalCost);
        }

        private static KinectPieces ChainSimilarityScore(List<Bone> subPartition, List<Bone> kinectPartition)
        {
            
            // solves assignment problem with Hungarian Algorithm                                        
            int[,] costsMatrix = new int[subPartition.Count, kinectPartition.Count];
            // initialize costMatrix
            for (int row = 0; row < subPartition.Count; row++)
            {
                for (int col = 0; col < kinectPartition.Count; col++)
                {
                    costsMatrix[row, col] =
                        RotDofSimilarityScore(subPartition[row], kinectPartition[col]) + 
                        SymmetryScore(subPartition[row], kinectPartition[col]) + 
                        Math.Abs(subPartition[row].level - kinectPartition[col].level) + 
                        GetCostRange(kinectPartition[col]);
                }
            }


            int[] assignment = HungarianAlgorithm.FindAssignments(costsMatrix);
            
            Bone kinectBone = new Bone("");
            int totalCost = 0;
            
            // computes cost for this assignment
            for (int ass = 0; ass < assignment.Length; ass++)
            {
                totalCost += costsMatrix[ass, assignment[ass]];
                kinectBone.name += kinectPartition[assignment[ass]].name + "_";
                foreach (char c in kinectPartition[assignment[ass]].rot_DoF)
                    kinectBone.rot_DoF.Add(c);
                foreach (char c in kinectPartition[assignment[ass]].loc_DoF)
                    kinectBone.loc_DoF.Add(c);
            }
            return new KinectPieces(totalCost, kinectBone);
        }

        private static int SymmetryScore(Bone bone1, Bone bone2)
        {
            /*
             * 
            int cost = 0;
            if (bone1.name.Contains(".R") || bone1.name.Contains(".L"))
            {
                if (bone1.name.Contains(".R"))
                    cost += 2;
                if (bone2.name.Contains(".R"))
                    cost -= 2;

                if (bone1.name.Contains(".L"))
                    cost -= 2;
                if (bone2.name.Contains(".L"))
                    cost += 2;
            }

            if ((bone1.name.Contains(".C")) && (!bone2.name.Contains(".C")))
            {
                cost += 2;
            }

            return Math.Abs(cost);
             */

            int cost = 0;
            if (bone1.name.Contains(".R"))
                cost+=2;
            if (bone2.name.Contains(".R"))
                cost-=2;

            if (bone1.name.Contains(".L"))
                cost-=2;
            if (bone2.name.Contains(".L"))
                cost+=2;
            return Math.Abs(cost);

        }
    
        private static int RotDofSimilarityScore(Bone bone, Bone handler)
        {
            int cost = 0;
            foreach (char c in bone.rot_DoF)
            {
                if (!handler.rot_DoF.Contains(c))
                {
                    cost = MAX_COST;
                    break;
                }
            }
            return cost;
        }

        public static AxisArrangement ComputeTUIAssignmentScore(int motors, Dictionary<string, List<List<char>>> dictionary, List<List<Bone>> currentPartition, char[] comb, bool useSensor, Brick brick)
        {
            // creates possible configurations 
            List<Bone> motorDecomposition = DecomposeMotorCombination(motors, comb, useSensor, brick);

            // solves assignment problem with Hungarian Algorithm                                        
            int[,] costsMatrix = new int[currentPartition.Count, motorDecomposition.Count * currentPartition.Count];

            // initialize costMatrix
            for (int row = 0; row < currentPartition.Count; row++)
            {
                for (int col = 0; col < motorDecomposition.Count * currentPartition.Count;
                    col += currentPartition.Count)
                {
                    // currentPartition[row]
                    // motorDecomposition[col/currentPartition.Count].rot_DoF 

                    //computes cost from :
                    //  DoF difference +
                    //  motors/sensors available preferences +
                    //  length of the chain

                    int cost =
                        RotDofDifferenceScore (currentPartition[row], motorDecomposition[col/currentPartition.Count].rot_DoF, dictionary) +
                        ComponentRequiredScore(motorDecomposition[col/currentPartition.Count].rot_DoF, useSensor, brick);

                    for (int index = 0; index < currentPartition.Count; index++)
                    {
                        costsMatrix[row, col + index] = cost;
                    }

                }
            }


            int[] assignment = HungarianAlgorithm.FindAssignments(costsMatrix);

            int totalCost = 0;
            // computes cost for this assignment
            for (int ass = 0; ass < assignment.Length; ass++)
            {
                totalCost += costsMatrix[ass, assignment[ass]];
            }

            return new AxisArrangement(GetDofString(comb.ToList()), assignment, currentPartition, motorDecomposition, totalCost);
        }

        private static List<List<List<Bone>>> PartitionByDepthFirstSearch(List<Bone> armature, BidirectionalGraph<Bone, Edge<Bone>> graph, int motors, bool isRotOnly)
        {
            List<List<List<Bone>>> graphPartitions = new List<List<List<Bone>>>();

            foreach (Bone b in armature)
            {
                List<List<Bone>> partition = new List<List<Bone>>();

                var dfs = new QuickGraph.Algorithms.Search.DepthFirstSearchAlgorithm<Bone, Edge<Bone>>(graph);
                Partition part = new Partition(motors);
                if (isRotOnly)
                {
                    dfs.DiscoverVertex += new VertexAction<Bone>(part.dfs_DiscoverVertex_MaxRotDoF);
                }
                else 
                { 
                    dfs.DiscoverVertex += new VertexAction<Bone>(part.dfs_DiscoverVertex_MaxLocRotDoF); 
                }
                dfs.Compute(b);

                if (part.bones.Count > 0)
                {
                    // Adds the last subpartition
                    part.partition.Add(part.bones);

                    // Add the current partition to the list of partitions computed
                    graphPartitions.Add(part.partition);
                }

            }

            return graphPartitions;
        }

        private static List<List<List<Bone>>> PartitionUniformDoF(List<Bone> armature, BidirectionalGraph<Bone, Edge<Bone>> graph, int motors)
        {
            List<List<List<Bone>>> graphPartitions = new List<List<List<Bone>>>();

            foreach (Bone b in armature)
            {
                List<List<Bone>> partition = new List<List<Bone>>();

                var dfs = new QuickGraph.Algorithms.Search.DepthFirstSearchAlgorithm<Bone, Edge<Bone>>(graph);
                Partition part = new Partition(motors);
                dfs.DiscoverVertex += new VertexAction<Bone>(part.dfs_DiscoverVertex_CostDof);
                dfs.Compute(b);
                // Adds the last subpartition
                part.partition.Add(part.bones);
                // Add the current partition to the list of partitions computed
                graphPartitions.Add(part.partition);
            }

            return graphPartitions;
        }

        private static List<Bone> DecomposeMotorCombination(int motor, char[] comb, bool useSensor, Brick brick)
        {
            List<Bone> configurations = new List<Bone>();
            // list of position {0,1,2,3...}
            var list = new List<string>();
            for (int i = 0; i < comb.Length; i++)
            {
                list.Add(i.ToString());
            }

            List<string> portAssignment = AssignName(comb.ToList(), useSensor, brick);

            int index = 0;

            for (int size = 1; size <= motor; size++)
            {
                var result = GetPermutations(list, size);

                foreach (var perm in result)
                {
                    Bone b = new Bone("");
                    //char[] g = new char[motors];                    
                    foreach (var c in perm)
                    {
                        // Adds DoF to the bone
                        b.rot_DoF.Add(comb[Convert.ToInt32(c)]);
                        //Console.Write(c + " ");
                        b.name += portAssignment[Convert.ToInt32(c)] + "_";
                    }
                    //b.name = AssignName(b.rot_DoF, useSensor,brick);

                    // Add Bone to the configuration
                    configurations.Add(b);
                    index++;
                }
            }
            return configurations;
        }

        private static List<string> AssignName(List<char> boneDoF, bool useSensor, Brick brick)
        {            
            List<string> result = new List<string>();
            List<string> tuiPieces = GetTuiComponent(useSensor,brick);
            int[,] costsMatrix = new int[boneDoF.Count, tuiPieces.Count];
            // initialize costsMatrix
            for (int row = 0; row < boneDoF.Count; row++)
            {
                for (int col = 0; col < tuiPieces.Count; col++)
                {
                    costsMatrix[row, col] = ComponentAssignment(boneDoF[row], tuiPieces[col]);
                }
            }

            int[] assignment = HungarianAlgorithm.FindAssignments(costsMatrix);

            for (int ass = 0; ass < assignment.Length; ass++)
            {
                result.Add(tuiPieces[assignment[ass]]);
            }
            return result;            
        }

        private static Bone GetBoneFromName(string childName, List<Bone> armature)
        {
            foreach (Bone b in armature)
            {
                if (b.name.Equals(childName))
                {
                    return b;
                }
            }
            return null;
        }

        private static int RotDofDifferenceScore(List<Bone> partition, List<char> motorDecomposition, Dictionary<string, List<List<char>>> dictionary)
        {
            // Degrees of fredom of bones which belong to the actual partition
            List<List<char>> partitionDoF = new List<List<char>>();

            foreach (Bone b in partition)
            {
                string DoF = GetDofString(b.rot_DoF);
                List<List<char>> alternativesRepr = new List<List<char>>();
                alternativesRepr = dictionary[DoF];

                // insert the first element;
                if (partitionDoF.Count < 1)
                {
                    foreach (List<char> DoFToAdd in alternativesRepr)
                    {
                        partitionDoF.Add(DoFToAdd);
                    }
                }
                else
                {
                    int iteration = partitionDoF.Count;
                    for (int i = 0; i < iteration; i++)
                    {
                        // creates copies with the same elements in the initial positions
                        List<char> currentState = partitionDoF[0];
                        foreach (List<char> DoFToAdd in alternativesRepr)
                        {
                            // Creta a new partitionDof : partitionDof(currentState) + new alternatives
                            partitionDoF.Add(new List<char>(currentState.Concat(DoFToAdd)));
                        }

                        //elimino partitionDof corrente
                        partitionDoF.Remove(currentState);
                    }
                }
            }

            // Adds padding 
            List<List<char>> partitionDoFPadded = new List<List<char>>();
           
            if (partitionDoF[0].Count == motorDecomposition.Count)
            {
                partitionDoFPadded = partitionDoF;
            }
            else if (partitionDoF[0].Count < motorDecomposition.Count)
            {
                // Adds padding for a better comparison
                foreach (List<char> alternativePart in partitionDoF)
                {
                    // list of possible position in partitionDofPadded
                    var list = new List<string>();
                    for (int i = 0; i < motorDecomposition.Count; i++)
                    {
                        list.Add(i.ToString());
                    }
                    // Calculates permutation of list to identify padding position
                    var result = GetPermutations(list, partitionDoF[0].Count);

                    int index = 0;
                    foreach (var perm in result)
                    {
                        char[] g = new char[motorDecomposition.Count];
                        index = 0;
                        foreach (var c in perm)
                        {
                            g[Convert.ToInt32(c)] = alternativePart[index];
                            //Console.Write(c + " ");
                            index++;
                        }
                        partitionDoFPadded.Add(g.ToList());
                    }
                }
            }
            else
            {
                // this motorDisposition is not able to control this partition
                return MAX_COST;
            }


            // compute min score for all alterantives representation of this partition
            int minScore = int.MaxValue;

            foreach (List<char> item in partitionDoFPadded)
            {
                int tempScore = 0;
                for (int j = 0; j < item.Count; j++)
                {
                    if (item[j] == 0) { tempScore += 1; continue; }
                    if (item[j] == 'x' && motorDecomposition[j] == 'y') { tempScore += 2; continue; }
                    if (item[j] == 'x' && motorDecomposition[j] == 'z') { tempScore += 1; continue; }
                    if (item[j] == 'y' && motorDecomposition[j] == 'x') { tempScore += 2; continue; }
                    if (item[j] == 'y' && motorDecomposition[j] == 'z') { tempScore += 2; continue; }
                    if (item[j] == 'z' && motorDecomposition[j] == 'x') { tempScore += 1; continue; }
                    if (item[j] == 'z' && motorDecomposition[j] == 'y') { tempScore += 2; continue; }

                }

                if (tempScore < minScore)
                {
                    minScore = tempScore;
                }
            }
            return minScore;
        }

        private static int ComponentRequiredScore(List<char> virtualBone, bool useSensor, Brick brick)
        {
            List<string> tuiPieces = GetTuiComponent(useSensor, brick);
            int[,] costsMatrix = new int[virtualBone.Count, tuiPieces.Count];
            // initialize costsMatrix
            for (int row = 0; row < virtualBone.Count; row++)
            {
                for (int col = 0; col < tuiPieces.Count; col++)
                {
                    costsMatrix[row, col] = ComponentAssignment(virtualBone[row], tuiPieces[col]);
                }
            }

            int[] assignment = HungarianAlgorithm.FindAssignments(costsMatrix);

            int totalCost = 0;
            // computes cost for this assignment
            for (int ass = 0; ass < assignment.Length; ass++)
            {
                totalCost += costsMatrix[ass, assignment[ass]];
            }

            return totalCost;
        }

        private static int ComponentAssignment(char DoF, string ComponentType)
        {
            if ((DoF.Equals('x') || DoF.Equals('z')) && ComponentType.Contains(DeviceType.LMotor.ToString()))
                return 0;
            if ((DoF.Equals('x') || DoF.Equals('z')) && ComponentType.Contains(DeviceType.MMotor.ToString()))
                return 1;
            if ((DoF.Equals('x') || DoF.Equals('z')) && ComponentType.Contains(DeviceType.Gyroscope.ToString()))
                return 2;
            if ((DoF.Equals('x') || DoF.Equals('z')) && ComponentType.Contains(DeviceType.Ultrasonic.ToString()))
                return 5;
            if (DoF.Equals('y') && ComponentType.Contains(DeviceType.MMotor.ToString()))
                return 0;
            if (DoF.Equals('y') && ComponentType.Contains(DeviceType.LMotor.ToString()))
                return 1;
            if (DoF.Equals('y') && ComponentType.Contains(DeviceType.Gyroscope.ToString()))
                return 2;
            if (DoF.Equals('y') && ComponentType.Contains(DeviceType.Ultrasonic.ToString()))
                return 5;

            return MAX_COST;
        }

        private static List<string> GetTuiComponent(bool useSensor, Brick brick)
        {
            List<string> components = new List<string>();
            if (!brick.Ports[InputPort.A].Type.ToString().Equals(DeviceType.Empty.ToString()) &&
                !brick.Ports[InputPort.A].Type.ToString().Equals(DeviceType.Touch.ToString()))
                components.Add(brick.Ports[InputPort.A].Type.ToString() + "(PORT-A)");
            if (!brick.Ports[InputPort.B].Type.ToString().Equals(DeviceType.Empty.ToString()) &&
                !brick.Ports[InputPort.B].Type.ToString().Equals(DeviceType.Touch.ToString()))
                components.Add(brick.Ports[InputPort.B].Type.ToString() + "(PORT-B)");
            if (!brick.Ports[InputPort.C].Type.ToString().Equals(DeviceType.Empty.ToString()) &&
                !brick.Ports[InputPort.C].Type.ToString().Equals(DeviceType.Touch.ToString()))
                components.Add(brick.Ports[InputPort.C].Type.ToString() + "(PORT-C)");
            if (!brick.Ports[InputPort.D].Type.ToString().Equals(DeviceType.Empty.ToString()) &&
                !brick.Ports[InputPort.D].Type.ToString().Equals(DeviceType.Touch.ToString()))
                components.Add(brick.Ports[InputPort.D].Type.ToString() + "(PORT-D)");

            if (useSensor)
            {
                if (!brick.Ports[InputPort.One].Type.ToString().Equals(DeviceType.Empty.ToString()) &&
                !brick.Ports[InputPort.One].Type.ToString().Equals(DeviceType.Touch.ToString()))
                    components.Add(brick.Ports[InputPort.One].Type.ToString() + "(PORT-One)");
                if (!brick.Ports[InputPort.Two].Type.ToString().Equals(DeviceType.Empty.ToString()) &&
                !brick.Ports[InputPort.Two].Type.ToString().Equals(DeviceType.Touch.ToString()))
                    components.Add(brick.Ports[InputPort.Two].Type.ToString() + "(PORT-Two)");
                if (!brick.Ports[InputPort.Three].Type.ToString().Equals(DeviceType.Empty.ToString()) &&
                !brick.Ports[InputPort.Three].Type.ToString().Equals(DeviceType.Touch.ToString()))
                    components.Add(brick.Ports[InputPort.Three].Type.ToString() + "(PORT-Three)");
                if (!brick.Ports[InputPort.Four].Type.ToString().Equals(DeviceType.Empty.ToString()) &&
                !brick.Ports[InputPort.Four].Type.ToString().Equals(DeviceType.Touch.ToString()))
                    components.Add(brick.Ports[InputPort.Four].Type.ToString() + "(PORT-Four)");
            }
            return components;
        }

        public static Dictionary<string, List<List<char>>> initDoFDictionary()
        {
            Dictionary<string, List<List<char>>> dictionary = new Dictionary<string, List<List<char>>>();

            List<List<char>> alternatives = new List<List<char>>();
            alternatives.Add(new List<char>() { 'x', 'y', 'x' });
            alternatives.Add(new List<char>() { 'x', 'y', 'z' });
            alternatives.Add(new List<char>() { 'x', 'z', 'x' });
            alternatives.Add(new List<char>() { 'x', 'z', 'y' });
            alternatives.Add(new List<char>() { 'y', 'x', 'y' });
            alternatives.Add(new List<char>() { 'y', 'x', 'z' });
            alternatives.Add(new List<char>() { 'y', 'z', 'x' });
            alternatives.Add(new List<char>() { 'y', 'z', 'y' });
            alternatives.Add(new List<char>() { 'z', 'x', 'y' });
            alternatives.Add(new List<char>() { 'z', 'x', 'z' });
            alternatives.Add(new List<char>() { 'z', 'y', 'x' });
            alternatives.Add(new List<char>() { 'z', 'y', 'z' });
            dictionary.Add("xyz", alternatives);

            alternatives = new List<List<char>>();
            alternatives.Add(new List<char>() { 'x', 'y' });
            alternatives.Add(new List<char>() { 'y', 'x' });
            dictionary.Add("xy", alternatives);

            alternatives = new List<List<char>>();
            alternatives.Add(new List<char>() { 'y', 'z' });
            alternatives.Add(new List<char>() { 'z', 'y' });
            dictionary.Add("yz", alternatives);

            alternatives = new List<List<char>>();
            alternatives.Add(new List<char>() { 'x', 'z' });
            alternatives.Add(new List<char>() { 'z', 'x' });
            dictionary.Add("xz", alternatives);

            alternatives = new List<List<char>>();
            alternatives.Add(new List<char>() { 'x' });
            dictionary.Add("x", alternatives);

            alternatives = new List<List<char>>();
            alternatives.Add(new List<char>() { 'y' });
            dictionary.Add("y", alternatives);

            alternatives = new List<List<char>>();
            alternatives.Add(new List<char>() { 'z' });
            dictionary.Add("z", alternatives);

            alternatives = new List<List<char>>();
            alternatives.Add(new List<char>() { '0' });
            dictionary.Add("", alternatives);

            return dictionary;
        }

        private static string GetDofString(List<char> rot_DoF)
        {
            string DoF = "";
            foreach (char c in rot_DoF)
            {
                DoF += c;
            }
            return DoF;
        }

        public static IEnumerable<String> CombinationsWithRepetition(IEnumerable<char> input, int length)
        {
            if (length <= 0)
                yield return "";
            else
            {
                foreach (var i in input)
                    foreach (var c in CombinationsWithRepetition(input, length - 1))
                        yield return i.ToString() + c;
            }
        }

        public static IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> items, int count)
        {
            int i = 0;
            foreach (var item in items)
            {
                if (count == 1)
                    yield return new T[] { item };
                else
                {
                    foreach (var result in GetPermutations(items.Skip(i + 1), count - 1))
                        yield return new T[] { item }.Concat(result);
                }

                ++i;
            }
        }

        public static int CountComponentAvailable(List<string> componentType, Brick brick)
        {
            int componentAvailable = 0;
            foreach (String mType in componentType)
            {
                if (brick.Ports[InputPort.A].Type.ToString().Equals(mType))
                    componentAvailable++;
                if (brick.Ports[InputPort.B].Type.ToString().Equals(mType))
                    componentAvailable++;
                if (brick.Ports[InputPort.C].Type.ToString().Equals(mType))
                    componentAvailable++;
                if (brick.Ports[InputPort.D].Type.ToString().Equals(mType))
                    componentAvailable++;
                if (brick.Ports[InputPort.One].Type.ToString().Equals(mType))
                    componentAvailable++;
                if (brick.Ports[InputPort.Two].Type.ToString().Equals(mType))
                    componentAvailable++;
                if (brick.Ports[InputPort.Three].Type.ToString().Equals(mType))
                    componentAvailable++;
                if (brick.Ports[InputPort.Four].Type.ToString().Equals(mType))
                    componentAvailable++;
            }
            return componentAvailable;
        }

        public static AxisArrangement ComputeLocRotTuiAssigneme(List<Bone> uniquePartition, Brick brick,  Dictionary<string, List<List<char>>> dictionary)
        {
            
            List<Bone> handler = GetTuiHandler(GetTuiComponent(true,brick));
            Bone hipCenter = new Bone("Hip");
            hipCenter.loc_DoF = new List<char>() { 'x', 'y', 'z' };
            foreach (Bone bone in GetOneDofBones(new List<Bone>() { hipCenter }, false, true))
            {
                handler.Add(bone);
            }
            List<Bone> boneOneDof = GetOneDofBones(uniquePartition, true, true);
            
            // solves assignment problem with Hungarian Algorithm                                        
            int[,] costsMatrix = new int[boneOneDof.Count, handler.Count];
            // initialize costMatrix
            for (int row = 0; row < boneOneDof.Count; row++)
            {
                for (int col = 0; col < handler.Count; col++)
                {
                    costsMatrix[row, col] =                         
                        RotDofSimilarityScore(boneOneDof[row],handler[col]) +                        
                        LocDoFSimilarityScore(boneOneDof[row],handler[col]) +
                        ComponentRangeScore(handler[col]) +
                        SymmetryScore(boneOneDof[row], handler[col]) +
                        PiecesPreferenceScore(boneOneDof[row], handler[col]);
                }
            }

            int[] assignment = HungarianAlgorithm.FindAssignments(costsMatrix);

            int score = 0;
            for (int ass = 0; ass < assignment.Length; ass++)
            {
                score += costsMatrix[ass, assignment[ass]];
            }

            List<List<Bone>> partition = new List<List<Bone>>();
            partition.Add(boneOneDof);
            AxisArrangement result = new AxisArrangement("TuiHip_Configuration", assignment, partition, handler, score);
            if (kinectAssignmentConsistency(result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        private static bool kinectAssignmentConsistency(AxisArrangement result)
        {
           
            for (int i = 1; i < result.Assignment.Length; i++)
            {
                string componentName1 = result.MotorDecomposition[result.Assignment[i - 1]].name;
                if (componentName1.Contains("PORT"))
                    continue;
                else
                    componentName1 = componentName1.Split('_')[0]; 
                    
                string componentName2 = result.MotorDecomposition[result.Assignment[i]].name;
                if (componentName2.Contains("PORT"))
                    continue;
                else
                    componentName2 = componentName2.Split('_')[0]; 
                
                if(componentName1.Equals(componentName2))
                {
                    string boneName1 = result.Partition[0][i - 1].name.Split('_')[0];
                    string boneName2 = result.Partition[0][i].name.Split('_')[0];

                    if(!boneName1.Equals(boneName2))
                        return false;
                }                               
                    
            }
            return true;
        }        

        public static AxisArrangement ComputeLocRotKinectAssignement(List<Bone> uniquePartition)
        {
            List<Bone> kinectSkeleton = new List<Bone>();
            foreach (List<Bone> lb in GetKinectSkeleton())
                foreach (Bone b in lb)
                    kinectSkeleton.Add(b);

            
            

            // solves assignment problem with Hungarian Algorithm                                        
            int[,] costsMatrix = NodeEdgeSimilarity(uniquePartition, kinectSkeleton);

            // initialize costMatrix
            for (int row = 0; row < uniquePartition.Count; row++)
            {
                for (int col = 0; col < kinectSkeleton.Count; col++)
                {
                    costsMatrix[row, col] +=
                        RotDofSimilarityScore(uniquePartition[row], kinectSkeleton[col]) +
                        LocDoFSimilarityScore(uniquePartition[row], kinectSkeleton[col]) +                        
                        DofAnalysisScore(uniquePartition[row], kinectSkeleton[col]);

                    if (uniquePartition[row].name.Contains(".R") || uniquePartition[row].name.Contains(".L"))
                        costsMatrix[row, col] += SymmetryScore(uniquePartition[row], kinectSkeleton[col]);
                }
            }

            int[] assignment = HungarianAlgorithm.FindAssignments(costsMatrix);

            int score = 0;
            for (int ass = 0; ass < assignment.Length; ass++)
            {
                score += costsMatrix[ass, assignment[ass]];
            }

            List<List<Bone>> partition = new List<List<Bone>>();
            partition.Add(uniquePartition);
            AxisArrangement bestArrangement = new AxisArrangement("Kinect_Configuration", assignment, partition, kinectSkeleton, score);               
            
            
            // Gets oneBone-mode mapping representation

            List<Bone> oneDofBone = GetOneDofBones(uniquePartition, true, true);
            List<Bone> oneDofHandler = GetOneDofBones(bestArrangement.MotorDecomposition, true, true);
            List<int> dofAssignament = new List<int>();            

            for (int handlerIndex = 0; handlerIndex < bestArrangement.Assignment.Length; handlerIndex++)
            {
                foreach (char dof in uniquePartition[handlerIndex].rot_DoF) 
                {
                    string boneToFind = 
                        bestArrangement.MotorDecomposition[bestArrangement.Assignment[handlerIndex]].name +
                        "_ROT(" + dof + ")";

                    dofAssignament.Add(oneDofHandler.FindIndex(x => x.name.Equals(boneToFind)));

                }
                foreach (char dof in uniquePartition[handlerIndex].loc_DoF)
                {
                    string boneToFind =
                        bestArrangement.MotorDecomposition[bestArrangement.Assignment[handlerIndex]].name +
                        "_LOC(" + dof + ")";

                    dofAssignament.Add(oneDofHandler.FindIndex(x => x.name.Equals(boneToFind)));
                }

            }

            List<List<Bone>> oneDofPartition = new List<List<Bone>>();
            oneDofPartition.Add(oneDofBone);
            return new AxisArrangement("Kinect_Configuration", dofAssignament.ToArray(), oneDofPartition, oneDofHandler, bestArrangement.Score);/* arrangements[0];*/
            
        }

        private static int[,] NodeEdgeSimilarity(List<Bone> uniquePartition, List<Bone> kinectSkeleton)
        {
            var graphKinectArmature = CreateUndirectedGraph(kinectSkeleton);
            var graphControlledArmature = CreateUndirectedGraph(uniquePartition);

            
            int[,] A = Matrix.GetAdjacencyMatrix(uniquePartition, graphControlledArmature);
            int[,] B = Matrix.GetAdjacencyMatrix(kinectSkeleton, graphKinectArmature);
            
            int[,] AT = Matrix.TransposeMatrix(A);
            int[,] BT = Matrix.TransposeMatrix(B);

            int[,] DAs = Matrix.GetSourceDiagonalMatrix(uniquePartition, graphControlledArmature);
            int[,] DAt = Matrix.GetTerminalDiagonalMatrix(uniquePartition, graphControlledArmature);
            int[,] DBs = Matrix.GetSourceDiagonalMatrix(kinectSkeleton, graphKinectArmature);
            int[,] DBt = Matrix.GetTerminalDiagonalMatrix(kinectSkeleton, graphKinectArmature);
            
            /*
            int[,] A = new int[,] { 
            { 0, 1, 0 }, 
            { 0, 0, 1 }, 
            { 0, 0, 0 } };

            int[,] B = new int[,]{ 
            { 0, 1, 0, 0, 0, 0 }, 
            { 0, 0, 0, 1, 1, 0 },
            { 0, 0, 0, 1, 0, 0 },
            { 0, 0, 0, 0, 1, 0 },
            { 0, 0, 0, 0, 0, 1 },
            { 0, 0, 0, 0, 0, 0 } };


            int[,] AT = Matrix.TransposeMatrix(A);
            int[,] BT = Matrix.TransposeMatrix(B);

            int[,] DAs = new int[,] { 
            { 1, 0, 0 }, 
            { 0, 1, 0 }, 
            { 0, 0, 0 } 
            };

            int[,] DAt = new int[,] { 
            { 0, 0, 0 }, 
            { 0, 1, 0 }, 
            { 0, 0, 1 } 
            };

            int[,] DBs = new int[,] { 
            { 1, 0, 0, 0, 0, 0 }, 
            { 0, 2, 0, 0, 0, 0 },
            { 0, 0, 1, 0, 0, 0 },
            { 0, 0, 0, 1, 0, 0 },
            { 0, 0, 0, 0, 1, 0 },
            { 0, 0, 0, 0, 0, 0 },
            };
            
            int[,] DBt = new int[,] {
            { 0, 0, 0, 0, 0, 0 }, 
            { 0, 1, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 2, 0, 0 },
            { 0, 0, 0, 0, 2, 0 },
            { 0, 0, 0, 0, 0, 1 },
            };
            
                  */
     
            Matrix.PrintMatrix(A, "A");
            Matrix.PrintMatrix(B, "B");
            Matrix.PrintMatrix(AT, "AT");
            Matrix.PrintMatrix(BT, "BT");
            Matrix.PrintMatrix(DAs, "DAs");
            Matrix.PrintMatrix(DAt, "DAt");
            Matrix.PrintMatrix(DBs, "DBs");
            Matrix.PrintMatrix(DBt, "DBt");

            int[,] kroneckerAxB = Matrix.KroneckerProduct(A, B);
            //Matrix.PrintMatrix(kroneckerAxB, "kroneckerAxB");
            
            int[,] kroneckerATxBT = Matrix.KroneckerProduct(AT, BT);
            //Matrix.PrintMatrix(kroneckerATxBT, "kroneckerATxBT");
            
            int[,] kroneckerDAsxDBs = Matrix.KroneckerProduct(DAs, DBs);
            //Matrix.PrintMatrix(kroneckerDAsxDBs, "kroneckerDAsxDBs");
            
            int[,] kroneckerDAtxDBt = Matrix.KroneckerProduct(DAt, DBt);
            //Matrix.PrintMatrix(kroneckerDAtxDBt, "kroneckerDAtxDBt");

            int[,] MatricesSummation = Matrix.ComputeMatricesSummation
                (new List<int[,]>() { kroneckerAxB, kroneckerATxBT, kroneckerDAsxDBs, kroneckerDAtxDBt});
            //Matrix.PrintMatrix(MatricesSummation, "MatricesSummation");


            // Start iterating procedure
            double[] costVector = Matrix.GetAllOneVector(MatricesSummation.GetLength(0));
            
            for (int step = 0; step < 20 ; step++)
            {
                costVector = Matrix.Product(MatricesSummation, costVector);
                costVector = Matrix.NormalizeVector(costVector);
                Console.WriteLine(" ===========================================");
                Console.WriteLine("  STEP n." + step);
                int[,] costMatrix = Matrix.VectorToCostMatrix(costVector, B.GetLength(0), A.GetLength(0), 10);
            }

            Console.WriteLine("KINECT COMPONENT");
            for (int i = 0; i < kinectSkeleton.Count; i++)
            {
                Console.Write("[" + i + "] = " + kinectSkeleton[i].name + "; ");
            }
            Console.WriteLine("\nBLENDER BONE");
            for (int i = 0; i < uniquePartition.Count; i++)
            {
                Console.Write("[" + i + "] = " + uniquePartition[i].name + "; ");
            }



            
            //double[] vector = new double[] {1,2,3,4,5,6,7,8,9 }; 
            //int[,] costMatrixProva = Matrix.VectorToCostMatrix(vector,3,3,10);
            //Matrix.PrintMatrix(costMatrixProva, "PROVA");



            //int[,] costMatrix = Matrix.VectorToCostMatrix(costVector, A.GetLength(0), B.GetLength(0), 10);
            

            //Matrix.PrintMatrix(costMatrix, "cost");            


            //return costMatrix;
            return null;
        }
       

        private static int DofAnalysisScore(Bone bone, Bone handler)
        {
            int rotCost = 0;
            int locCost = 0;
            if(bone.rot_DoF.Count > 0)
            {
                foreach (Bone oneDofHandler in GetOneDofBones(new List<Bone>() { handler }, true, false))
                {
                    rotCost += ComponentRangeScore(oneDofHandler) + PiecesPreferenceScore(bone, oneDofHandler);
                }
            }
            if (bone.loc_DoF.Count > 0)
            {
                foreach (Bone oneDofHandler in GetOneDofBones(new List<Bone>() { handler }, false, true))
                {
                    locCost += ComponentRangeScore(oneDofHandler) + PiecesPreferenceScore(bone, oneDofHandler);
                }
            }

            return rotCost + locCost;
        }
    }
}
