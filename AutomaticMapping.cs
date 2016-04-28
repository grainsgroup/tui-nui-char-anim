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
        public float Score { get; set; }
        public Bone Bones {get; set; }

        public KinectPieces(float score, Bone bones)
        {
            this.Bones = bones;
            this.Score = score;
        }
    }

    public class PartialArmature
    {
        public List<Bone> virtualArmature { get; set; }
        public Bone currentBone { get; set; }
                
        public PartialArmature(List<Bone> virtualArmature, Bone lastBone)
        {
            this.virtualArmature = virtualArmature;
            this.currentBone = lastBone;
        }
    }

    public class DofBoneAssociation
    {
        public Bone ReferenceBone { get; set; }
        public string Dof { get; set; }

        public DofBoneAssociation(Bone refBone, string dof) 
        {
            this.ReferenceBone = refBone;
            this.Dof = dof;
        }
 
    }

    public class PathExpansion
    {
        public List<Bone> NodeToExpand { get; set; }
        public List<Bone> Path { get; set; }
        public int MotorAvailable { get; set; }

        public PathExpansion() 
        {
            NodeToExpand = new List<Bone>();
            Path = new List<Bone>();
        }

        public PathExpansion(List<Bone> nodeToExpand, List<Bone> path, int motorAvailable)
        {
            this.NodeToExpand = nodeToExpand;
            this.Path = path;
            this.MotorAvailable = motorAvailable;
        }
    }

    public static class AutomaticMapping
    {        
        
       
        public static List<List<List<Bone>>> GraphPartitioning(int motors, BidirectionalGraph<Bone, Edge<Bone>> graph, List<List<Bone>> components, List<List<List<Bone>>> graphPartitions, bool splitDofCheckBox, bool isRotOnly)
        {
            if (!splitDofCheckBox)
            {
                foreach (List<Bone> armatureComponent in components)
                {
                    // This list is called partial because contains only the partition for a specific connected component
                                       
                    List<List<List<Bone>>> partialGraphPartitions = new List<List<List<Bone>>>();
                    try
                    {
                        partialGraphPartitions = PartitionArmatureComponent(armatureComponent, graph, motors, isRotOnly);
                    }
                    catch (ApplicationException ex)
                    {
                        throw ex;
                    }

                    if (graphPartitions.Count > 0)
                    {
                        // Combines components partitions
                        int lastItemIndex = graphPartitions.Count;

                        for (int i = 0; i < lastItemIndex; i++)
                        {
                            foreach (List<List<Bone>> partialPartition in partialGraphPartitions)
                            {
                                graphPartitions.Add(graphPartitions[i].Concat(partialPartition).ToList());
                            }                            
                        }

                        graphPartitions.RemoveRange(0, lastItemIndex);

                    }
                    else
                    {
                        graphPartitions = partialGraphPartitions;                        
                    }
                }
              
            }

            // TODO: 
            if (splitDofCheckBox)
            {
                throw new NotImplementedException();             
            }            

            return graphPartitions;
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
                    Bone childToAdd = GetBoneFromName(child, armature);
                    if(childToAdd!=null)
                        graph.AddVerticesAndEdge(new Edge<Bone>(b, childToAdd));
                }
            }
            return graph;
        }


        public static List<List<Bone>> GetKinectPartition()
        {
            /*
            
            // Kinect skeleton full DoFs
            Bone shoulder_right = new Bone("Shoulder.R_NUI");
            shoulder_right.rot_DoF = axis;
            shoulder_right.loc_DoF = axis;
            shoulder_right.level = 0;
            shoulder_right.parent = "Shoulder_NUI";
            shoulder_right.children = new List<string>() { "Elbow.R_NUI" };

            Bone elbow_right = new Bone("Elbow.R_NUI");
            elbow_right.rot_DoF = axis;
            elbow_right.loc_DoF = axis;
            elbow_right.level = 1;
            elbow_right.parent = "Shoulder.R_NUI";
            elbow_right.children = new List<string>() { "Wrist.R_NUI" };

            Bone wrist_right = new Bone("Wrist.R_NUI");
            wrist_right.rot_DoF = axis;
            wrist_right.loc_DoF = axis;
            wrist_right.level = 2;
            wrist_right.parent = "Elbow.R_NUI";
            wrist_right.children = new List<string>() { "Hand.R_NUI" };

            Bone hand_right = new Bone("Hand.R_NUI");
            hand_right.rot_DoF = axis;
            hand_right.loc_DoF = axis;
            hand_right.level = 3;
            hand_right.parent = "Wrist.R_NUI";
            hand_right.children = new List<string>() { };

            List<Bone> upper_right = new List<Bone>();
            upper_right.Add(shoulder_right);
            upper_right.Add(elbow_right);
            upper_right.Add(wrist_right);
            upper_right.Add(hand_right);
            skeleton.Add(upper_right);



            Bone shoulder_left = new Bone("Shoulder.L_NUI");
            shoulder_left.rot_DoF = axis;
            shoulder_left.loc_DoF = axis;
            shoulder_left.level = 0;
            shoulder_left.parent = "Shoulder_NUI";
            shoulder_left.children = new List<string>() { "Elbow.L_NUI" };

            Bone elbow_left = new Bone("Elbow.L_NUI");
            elbow_left.rot_DoF = axis;
            elbow_left.loc_DoF = axis;
            elbow_left.level = 1;
            elbow_left.parent = "Shoulder.L_NUI";
            elbow_left.children = new List<string>() { "Wrist.L_NUI" };

            Bone wrist_left = new Bone("Wrist.L_NUI");
            wrist_left.rot_DoF = axis;
            wrist_left.loc_DoF = axis;
            wrist_left.level = 2;
            wrist_left.parent = "Elbow.L_NUI";
            wrist_left.children = new List<string>() { "Hand.L_NUI" };

            Bone hand_left = new Bone("Hand.L_NUI");
            hand_left.rot_DoF = axis;
            hand_left.loc_DoF = axis;
            hand_left.level = 3;
            hand_left.parent = "Wrist.L_NUI";
            hand_left.children = new List<string>() { };

            List<Bone> upper_left = new List<Bone>();
            upper_left.Add(shoulder_left);
            upper_left.Add(elbow_left);
            upper_left.Add(wrist_left);
            upper_left.Add(hand_left);
            skeleton.Add(upper_left);


            Bone head = new Bone("Head_NUI");
            head.rot_DoF = axis;
            head.loc_DoF = axis;
            head.level = 3;
            head.parent = "Shoulder_NUI";
            head.children = new List<string>() { };

            Bone shoulder_center = new Bone("Shoulder_NUI");
            shoulder_center.rot_DoF = axis;
            shoulder_center.loc_DoF = axis;
            shoulder_center.level = 2;
            shoulder_center.parent = "Spine_NUI";
            shoulder_center.children = new List<string>() { "Shoulder.R_NUI", "Shoulder.L_NUI", "Head_NUI" };

            Bone spine = new Bone("Spine_NUI");
            spine.rot_DoF = axis;
            spine.loc_DoF = axis;
            spine.level = 1;
            spine.parent = "Hip_NUI";
            spine.children = new List<string>() { "Shoulder_NUI" };

            Bone hip_center = new Bone("Hip_NUI");
            hip_center.rot_DoF = axis;
            hip_center.loc_DoF = axis;
            hip_center.level = 0;
            hip_center.parent = "";
            hip_center.children = new List<string>() { "Spine_NUI", "Hip.R_NUI", "Hip.L_NUI" };

            List<Bone> center = new List<Bone>();
            center.Add(head);
            center.Add(shoulder_center);
            center.Add(spine);
            center.Add(hip_center);
            skeleton.Add(center);


            Bone hip_right = new Bone("Hip.R_NUI");
            hip_right.rot_DoF = axis;
            hip_right.loc_DoF = axis;
            hip_right.level = 0;
            hip_right.parent = "Hip";
            hip_right.children = new List<string>() { "Knee.R_NUI" };

            Bone knee_right = new Bone("Knee.R_NUI");
            knee_right.rot_DoF = axis;
            knee_right.loc_DoF = axis;
            knee_right.level = 1;
            knee_right.parent = "Hip.R_NUI";
            knee_right.children = new List<string>() { "Ankle.R_NUI" };

            Bone ankle_right = new Bone("Ankle.R_NUI");
            ankle_right.rot_DoF = axis;
            ankle_right.loc_DoF = axis;
            ankle_right.level = 2;
            ankle_right.parent = "Knee.R_NUI";
            ankle_right.children = new List<string>() { "Foot.R_NUI" };

            Bone foot_right = new Bone("Foot.R_NUI");
            foot_right.rot_DoF = axis;
            foot_right.loc_DoF = axis;
            foot_right.level = 3;
            foot_right.parent = "Ankle.R_NUI";
            foot_right.children = new List<string>() { };

            List<Bone> lower_right = new List<Bone>();
            lower_right.Add(hip_right);
            lower_right.Add(knee_right);
            lower_right.Add(ankle_right);
            lower_right.Add(foot_right);
            skeleton.Add(lower_right);


            Bone hip_left = new Bone("Hip.L_NUI");
            hip_left.rot_DoF = axis;
            hip_left.loc_DoF = axis;
            hip_left.level = 0;
            hip_left.parent = "Hip_NUI";
            hip_left.children = new List<string>() { "Knee.L_NUI" };

            Bone knee_left = new Bone("Knee.L_NUI");
            knee_left.rot_DoF = axis;
            knee_left.loc_DoF = axis;
            knee_left.level = 1;
            knee_left.parent = "Hip.L_NUI";
            knee_left.children = new List<string>() { "Ankle.L_NUI" };

            Bone ankle_left = new Bone("Ankle.L_NUI");
            ankle_left.rot_DoF = axis;
            ankle_left.loc_DoF = axis;
            ankle_left.level = 2;
            ankle_left.parent = "Knee.L_NUI";
            ankle_left.children = new List<string>() { "Foot.L_NUI" };

            Bone foot_left = new Bone("Foot.L_NUI");
            foot_left.rot_DoF = axis;
            foot_left.loc_DoF = axis;
            foot_left.level = 3;
            foot_left.parent = "Ankle.L_NUI";
            foot_left.children = new List<string>() { };

            List<Bone> lower_left = new List<Bone>();
            lower_left.Add(hip_left);
            lower_left.Add(knee_left);
            lower_left.Add(ankle_left);
            lower_left.Add(foot_left);
            skeleton.Add(lower_left);

            return skeleton;
            */
            
            List<List<Bone>> skeleton = new List<List<Bone>>();
            List<char> axis = new List<char>() { 'x', 'y', 'z' };

            Bone shoulder_right = new Bone("Shoulder.R_NUI");
            shoulder_right.rot_DoF = new List<char>() { 'z' };
            shoulder_right.loc_DoF = axis;
            shoulder_right.level = 0;
            shoulder_right.parent = "Shoulder_NUI";
            shoulder_right.children = new List<string>() { "Elbow.R_NUI" };

            Bone elbow_right = new Bone("Elbow.R_NUI");
            elbow_right.rot_DoF = new List<char>() { 'x', 'y', 'z' };
            elbow_right.loc_DoF = axis;
            elbow_right.level = 1;
            elbow_right.parent = "Shoulder.R_NUI";
            elbow_right.children = new List<string>() { "Wrist.R_NUI" };

            Bone wrist_right = new Bone("Wrist.R_NUI");
            wrist_right.rot_DoF = new List<char>() { 'x' };
            wrist_right.loc_DoF = axis;
            wrist_right.level = 2;
            wrist_right.parent = "Elbow.R_NUI";
            wrist_right.children = new List<string>() { "Hand.R_NUI" };

            Bone hand_right = new Bone("Hand.R_NUI");
            hand_right.rot_DoF = new List<char>() { 'x', 'z' };
            hand_right.loc_DoF = axis;
            hand_right.level = 3;
            hand_right.parent = "Wrist.R_NUI";
            hand_right.children = new List<string>() { };
            
            List<Bone> upper_right = new List<Bone>();
            upper_right.Add(shoulder_right);
            upper_right.Add(elbow_right);
            upper_right.Add(wrist_right);
            upper_right.Add(hand_right);
            skeleton.Add(upper_right);



            Bone shoulder_left = new Bone("Shoulder.L_NUI");
            shoulder_left.rot_DoF = new List<char>() { 'z' };
            shoulder_left.loc_DoF = axis;
            shoulder_left.level = 0;
            shoulder_left.parent = "Shoulder_NUI";
            shoulder_left.children = new List<string>() { "Elbow.L_NUI" };

            Bone elbow_left = new Bone("Elbow.L_NUI");
            elbow_left.rot_DoF = new List<char>() { 'x', 'y', 'z' };
            elbow_left.loc_DoF = axis;
            elbow_left.level = 1 ;
            elbow_left.parent = "Shoulder.L_NUI";
            elbow_left.children = new List<string>() { "Wrist.L_NUI" };

            Bone wrist_left = new Bone("Wrist.L_NUI");
            wrist_left.rot_DoF = new List<char>() { 'x' };
            wrist_left.loc_DoF = axis;
            wrist_left.level = 2;
            wrist_left.parent = "Elbow.L_NUI";
            wrist_left.children = new List<string>() { "Hand.L_NUI" };

            Bone hand_left = new Bone("Hand.L_NUI");
            hand_left.rot_DoF = new List<char>() { 'x', 'z' };
            hand_left.loc_DoF = axis;
            hand_left.level = 3;
            hand_left.parent = "Wrist.L_NUI";
            hand_left.children = new List<string>() { };

            List<Bone> upper_left = new List<Bone>();
            upper_left.Add(shoulder_left);
            upper_left.Add(elbow_left);
            upper_left.Add(wrist_left);
            upper_left.Add(hand_left);
            skeleton.Add(upper_left);


            Bone head = new Bone("Head_NUI");
            head.rot_DoF = new List<char>() { 'x', 'z' };
            head.loc_DoF = axis;
            head.level = 3;
            head.parent = "Shoulder_NUI";
            head.children = new List<string>() { };

            Bone shoulder_center = new Bone("Shoulder_NUI");
            shoulder_center.rot_DoF = new List<char>() { '0' };
            shoulder_center.loc_DoF = axis;
            shoulder_center.level = 2;
            shoulder_center.parent = "Spine_NUI";
            shoulder_center.children = new List<string>() { "Shoulder.R_NUI", "Shoulder.L_NUI", "Head_NUI" };

            Bone spine = new Bone("Spine_NUI");
            spine.rot_DoF = new List<char>() { '0' };
            spine.loc_DoF = axis;
            spine.level = 1;
            spine.parent = "Hip_NUI";
            spine.children = new List<string>() { "Shoulder_NUI" };

            Bone hip_center = new Bone("Hip_NUI");
            hip_center.rot_DoF = new List<char>() { 'x', 'y', 'z' };
            hip_center.loc_DoF = axis;
            hip_center.level = 0;
            hip_center.parent = "";
            hip_center.children = new List<string>() { "Spine_NUI", "Hip.R_NUI", "Hip.L_NUI" };
            
            List<Bone> center = new List<Bone>();
            center.Add(head);
            center.Add(shoulder_center);
            center.Add(spine);
            center.Add(hip_center);
            skeleton.Add(center);


            Bone hip_right = new Bone("Hip.R_NUI");
            hip_right.rot_DoF = new List<char>() { '0' };
            hip_right.loc_DoF = axis;
            hip_right.level = 0;
            hip_right.parent = "Hip";
            hip_right.children = new List<string>() { "Knee.R_NUI" };

            Bone knee_right = new Bone("Knee.R_NUI");
            knee_right.rot_DoF = new List<char>() { 'x', 'z' };
            knee_right.loc_DoF = axis;
            knee_right.level = 1;
            knee_right.parent = "Hip.R_NUI";
            knee_right.children = new List<string>() { "Ankle.R_NUI" };

            Bone ankle_right = new Bone("Ankle.R_NUI");
            ankle_right.rot_DoF = new List<char>() { 'x' ,'z' };
            ankle_right.loc_DoF = axis;
            ankle_right.level = 2;
            ankle_right.parent = "Knee.R_NUI";
            ankle_right.children = new List<string>() { "Foot.R_NUI" };

            Bone foot_right = new Bone("Foot.R_NUI");
            foot_right.rot_DoF = new List<char>() { '0' };
            foot_right.loc_DoF = axis;
            foot_right.level = 3;
            foot_right.parent = "Ankle.R_NUI";
            foot_right.children = new List<string>() {};

            List<Bone> lower_right = new List<Bone>();
            lower_right.Add(hip_right);
            lower_right.Add(knee_right);
            lower_right.Add(ankle_right);
            lower_right.Add(foot_right);
            skeleton.Add(lower_right);


            Bone hip_left = new Bone("Hip.L_NUI");
            hip_left.rot_DoF = new List<char>() { '0' };
            hip_left.loc_DoF = axis;
            hip_left.level = 0;
            hip_left.parent = "Hip_NUI";
            hip_left.children = new List<string>() { "Knee.L_NUI" };

            Bone knee_left = new Bone("Knee.L_NUI");
            knee_left.rot_DoF = new List<char>() { 'x', 'z' };
            knee_left.loc_DoF = axis;
            knee_left.level = 1;
            knee_left.parent = "Hip.L_NUI";
            knee_left.children = new List<string>() { "Ankle.L_NUI" };

            Bone ankle_left = new Bone("Ankle.L_NUI");
            ankle_left.rot_DoF = new List<char>() { 'x' ,'z' };
            ankle_left.loc_DoF = axis;
            ankle_left.level = 2;
            ankle_left.parent = "Knee.L_NUI";
            ankle_left.children = new List<string>() { "Foot.L_NUI" };

            Bone foot_left = new Bone("Foot.L_NUI");
            foot_left.rot_DoF = new List<char>() { '0' };
            foot_left.loc_DoF = axis;
            foot_left.level = 3;
            foot_left.parent = "Ankle.L_NUI";
            foot_left.children = new List<string>() { };

            List<Bone> lower_left = new List<Bone>();
            lower_left.Add(hip_left);
            lower_left.Add(knee_left);
            lower_left.Add(ankle_left);
            lower_left.Add(foot_left);
            skeleton.Add(lower_left);            
            
            return skeleton;

        }

        public static List<Bone> GetKinectSkeleton() 
        {
            List<Bone> kinectSkeleton = new List<Bone>();
            foreach (List<Bone> listOfBones in AutomaticMapping.GetKinectPartition())
                foreach (Bone b in listOfBones)
                    kinectSkeleton.Add(b);
            return kinectSkeleton;
        }

        // dofType: { _ROT, _LOC }: type of dofs 
        public static List<List<Bone>> CreateArmaturesFromComb(char[] comb, Brick brick, string[] doFType)
        {
            // IMPLEMENTATION WITH OPERATION SEQUENCE REPRESENTATION (OSR)

            // List of possible bone type ()
            List<string[]> dofTypeSequence = new List<string[]>();
            foreach (var c in Combinatorics.CombinationsWithRepetition(doFType, comb.Length))
            {
                //char[] array = c.ToCharArray();
                //dofTypeSequence.Add(array);
                string[] array = c.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                dofTypeSequence.Add(array);
            }

            /*
            // Permutation of Dof 
            List<string[]> permutations = new List<string[]>();
            var result = Combinatorics.GetPermutations(Enumerable.Range(0, comb.Length), comb.Length);
            foreach (var perm in result)
            {
                string[] g = new string[comb.Length];
                int i = 0;
                foreach (var c in perm)
                {
                    g[i] = c.ToString();
                    i++;
                }
                permutations.Add(g);
            }
            */

           string[] perm  = new string[comb.Length];
           for (int i = 0; i < comb.Length; i++)
           {
               perm[i] = i.ToString();
           }
           List<string[]> permutations = new List<string[]>() {perm};
           


            // combines combPermutation with types of dof
            List<List<string>> typedSequence_Dof = new List<List<string>>();
            foreach (string[] cp in permutations)
            {
                foreach (string[] dt in dofTypeSequence)
                {
                    List<string> sequenceToAdd = new List<string>();

                    for (int i = 0; i < comb.Length; i++)
                    {
                        sequenceToAdd.Add(dt[i] + "(" + comb[Int32.Parse(cp[i])] + ")");
                    }

                    // Assigns the best componet for each dof in the sequence
                    int index = 0;
                    foreach (string assignedComp in Metrics.AssignName(sequenceToAdd.ToList(), true, brick, true))
                    {
                        sequenceToAdd[index] = assignedComp;
                        index++;
                    }

                    // Removes duplicate sequences
                    bool sequenceExist = false;
                    foreach (List<string> item in typedSequence_Dof)
                    {
                        if (item.SequenceEqual(sequenceToAdd))
                        {
                            sequenceExist = true;
                            break;
                        }
                    }
                    if (!sequenceExist) 
                    {
                        typedSequence_Dof.Add(sequenceToAdd);
                    }

                }
            }


            /* Possible operation with two dof
             * - P = creates two bones in parallel; 
             * - S = creates two bones in series; 
             * - B = put dofs in the same bone)
             */
            string[] operations = { "P", "S", "B" };

            // List of possible order of operation to apply
            List<char[]> operationsOrder = new List<char[]>();
            foreach (var c in Combinatorics.CombinationsWithRepetition(operations, comb.Length - 1))
            {
                char[] array = c.ToCharArray();
                operationsOrder.Add(array);
            }

            // Combines dof and componentes permutation with all possible operation order to obtain armature
            List<List<string>> dofSequences = new List<List<string>>();
            for (int seq = 0; seq < typedSequence_Dof.Count; seq++)
            {
                List<string> dofs = typedSequence_Dof[seq];

                foreach (char[] order in operationsOrder)
                {
                    List<string> arm_dof = new List<string>();

                    for (int i = 0; i < comb.Length - 1; i++)
                    {
                        // Adds dof
                        arm_dof.Add(dofs[i]);
                        // Adds operation
                        arm_dof.Add(order[i].ToString());
                    }

                    //Adds the last dof
                    arm_dof.Add(dofs[comb.Length - 1]);
                    dofSequences.Add(arm_dof);
                }
            }

            // For each sequence creates the armor which represent it
            List<List<Bone>> armatures = new List<List<Bone>>();
            for (int i = 0; i < dofSequences.Count; i++)
            {
                
                
                // initialization
                List<string> sequenceItem = dofSequences[i];
                List<Bone> virtualArmature = new List<Bone>();
                List<PartialArmature> partialArmatures = new List<PartialArmature>();
                int level = 0;

                // Adds the first bone of the sequence
                Bone firstBone = InitializeBoneFromItem(sequenceItem[0]);
                // DEBUG TEST:
                firstBone.name += "[seqID: " + i + "]" + Metrics.GetDofString(comb.ToList());
                firstBone.level = level;
                partialArmatures.Add(new PartialArmature(new List<Bone>() { firstBone }, firstBone));


                // For each item of the sequence
                for (int j = 1; j < sequenceItem.Count; j = j + 2)
                {
                    

                    // Gets operation type: S = series; P = parallel; B = bone;
                    if (sequenceItem[j].Equals("S"))
                    {
                        for (int k = 0; k < partialArmatures.Count; k++)
                        {
                            PartialArmature pa = partialArmatures[k];
                            Bone parentToUpdate = pa.currentBone;

                            // Sets the new Bone parameters
                            Bone boneToAdd = InitializeBoneFromItem(sequenceItem[j + 1]);
                            // DEBUG TEST:
                            boneToAdd.name += "[seqID: " + i + "]";
                            boneToAdd.level = parentToUpdate.level + 1;
                            boneToAdd.parent = parentToUpdate.name;
                            // Updates parent
                            pa.virtualArmature.Remove(parentToUpdate);
                            parentToUpdate.children.Add(boneToAdd.name);
                            pa.virtualArmature.Add(parentToUpdate);
                            pa.virtualArmature.Add(boneToAdd);
                            pa.currentBone = boneToAdd;
                        }
                        continue;
                    }

                    if (sequenceItem[j].Equals("P"))
                    {
                        int iterations = partialArmatures.Count;
                        for (int k = 0; k < iterations; k++)
                        {
                            PartialArmature pa = partialArmatures[0];
                            Bone lastBoneAnalyzed = pa.currentBone;

                            if (lastBoneAnalyzed.level > 0)
                            {
                                List<Bone> ancestors = GetAncestor
                                    (pa.virtualArmature, GetBoneFromName(lastBoneAnalyzed.parent, pa.virtualArmature));
                                foreach (Bone ancestor in ancestors) 
                                {
                                    Bone parentToUpdate = new Bone(ancestor.name);
                                    parentToUpdate.level = ancestor.level;
                                    parentToUpdate.rot_DoF = ancestor.rot_DoF.ToList();
                                    parentToUpdate.loc_DoF = ancestor.loc_DoF.ToList();
                                    parentToUpdate.parent = ancestor.parent;
                                    parentToUpdate.children = ancestor.children.ToList();

                                    PartialArmature newPartialArmature = 
                                        new PartialArmature(pa.virtualArmature.ToList(), parentToUpdate);                                    

                                    //Initializes new bone
                                    Bone boneToAdd = InitializeBoneFromItem(sequenceItem[j + 1]);
                                    // DEBUG TEST:
                                    boneToAdd.name += "[seqID: " + i + "]";
                                    boneToAdd.level = newPartialArmature.currentBone.level + 1;
                                    boneToAdd.parent = parentToUpdate.name;
                                    // Updates parent
                                    newPartialArmature.virtualArmature.Remove(ancestor);
                                    parentToUpdate.children.Add(boneToAdd.name);
                                    newPartialArmature.virtualArmature.Add(parentToUpdate);
                                    newPartialArmature.virtualArmature.Add(boneToAdd);
                                    newPartialArmature.currentBone = boneToAdd;
                                    partialArmatures.Add(newPartialArmature);
                                }

                                // Adds bone at level 0
                                Bone boneToAddLev0 = InitializeBoneFromItem(sequenceItem[j + 1]);
                                boneToAddLev0.level = 0;
                                PartialArmature newPartialArmatureLev0 =
                                        new PartialArmature(pa.virtualArmature.ToList(), boneToAddLev0);
                                newPartialArmatureLev0.virtualArmature.Add(boneToAddLev0);
                                newPartialArmatureLev0.currentBone = boneToAddLev0;
                                partialArmatures.Add(newPartialArmatureLev0);

                                //removes the partialArmature not updated used to create the new partialArmatures
                                partialArmatures.RemoveAt(0);
                            }                            

                            else
                            {
                                Bone boneToAdd = InitializeBoneFromItem(sequenceItem[j + 1]);
                                boneToAdd.level = 0;
                                PartialArmature newPartialArmature =
                                        new PartialArmature(pa.virtualArmature.ToList(), pa.currentBone);
                                newPartialArmature.virtualArmature.Add(boneToAdd);
                                newPartialArmature.currentBone = boneToAdd;
                                partialArmatures.Add(newPartialArmature);                                
                                partialArmatures.RemoveAt(0);                                
                            }
                        }
                        continue;
                    } 
                       
                    if (sequenceItem[j].Equals("B"))
                    {
                        Bone boneToAdd = InitializeBoneFromItem(sequenceItem[j + 1]);
                        
                        for (int k = 0; k < partialArmatures.Count; k++)
                        {
                            PartialArmature pa = partialArmatures[k];
                            Bone boneToUpdate = pa.currentBone;
                            pa.virtualArmature.Remove(boneToUpdate);

                            string boneName = pa.currentBone.name;

                            // Adds rot dof
                            foreach (char dof in boneToAdd.rot_DoF)
                            {
                                //if(!parentToUpdate.rot_DoF.Contains(dof))
                                //{
                                //    parentToUpdate.rot_DoF.Add(dof);
                                //    parentToUpdate.name += " | " + boneToAdd.name;
                                //}

                                // Dof duplicates
                                boneToUpdate.rot_DoF.Add(dof);
                                boneToUpdate.name += " | " + boneToAdd.name;
                            }
                            // Adds loc dof
                            foreach (char dof in boneToAdd.loc_DoF)
                            {
                                if (!boneToUpdate.loc_DoF.Contains(dof))
                                {
                                    boneToUpdate.loc_DoF.Add(dof);
                                    boneToUpdate.name += " | " + boneToAdd.name;
                                }
                            }

                            if (!boneToUpdate.parent.Equals(""))
                            {
                                // Updates parent
                                Bone parentToUpdate = GetBoneFromName(boneToUpdate.parent, pa.virtualArmature);
                                pa.virtualArmature.Remove(parentToUpdate);
                                parentToUpdate.children.RemoveAt(parentToUpdate.children.IndexOf(boneName));
                                parentToUpdate.children.Add(boneToUpdate.name);
                                pa.virtualArmature.Add(parentToUpdate);
                            }
                            
                            // Updates current bone
                            pa.virtualArmature.Add(boneToUpdate);
                            pa.currentBone = boneToUpdate;
                        }
                    }
                    continue;
                }

                foreach (PartialArmature pa in partialArmatures)
                {
                    armatures.Add(pa.virtualArmature);
                }
            }
            
            return armatures;
        }

        private static List<Bone> GetAncestor(List<Bone> list, Bone lastBoneAnalyzed)
        {
            List<Bone> ancestors = new List<Bone>();

            while (lastBoneAnalyzed.level > 0)
            {
                ancestors.Add(lastBoneAnalyzed);
                lastBoneAnalyzed = GetBoneFromName(lastBoneAnalyzed.parent,list);
            }

            ancestors.Add(lastBoneAnalyzed);
            return ancestors;
        }       

        private static Bone InitializeBoneFromItem(string item)
        {
            Bone bone = new Bone(item);
            if (item.Contains("ROT")) 
            {
                char[] dof = item.Substring(item.IndexOf("ROT") + 4, 1).ToCharArray();
                bone.rot_DoF.Add(dof[0]);
            }
            if (item.Contains("LOC")) 
            {
                char[] dof = item.Substring(item.IndexOf("LOC") + 4, 1).ToCharArray();
                bone.loc_DoF.Add(dof[0]);
            }
            return bone;
        }
        
        private static List<List<List<Bone>>> PartitionArmatureComponent(List<Bone> armature, BidirectionalGraph<Bone, Edge<Bone>> graph, int motors, bool isRotOnly)
        {
            
            List<List<List<Bone>>> graphPartitions = new List<List<List<Bone>>>();
            if (PartitionCapacityOverflow(armature, motors)) 
            {
                throw new ApplicationException();
            }
            
            foreach(Bone startBone in armature)
            {                                
                List<GraphTraversal> graphTraversalList = new List<GraphTraversal>();

                GraphTraversal graphTraversal = new GraphTraversal(motors);
                var dfs = new QuickGraph.Algorithms.Search.DepthFirstSearchAlgorithm<Bone, Edge<Bone>>(graph);
                dfs.DiscoverVertex += new VertexAction<Bone>(graphTraversal.dfs_DiscoverVertex_MaxRotDoF);
                dfs.Compute(startBone);
                graphTraversalList.Add(graphTraversal);

                while(graphTraversalList.Count > 0)
                {
                    GraphTraversal currGraphTrav = graphTraversalList[0];

                    while(currGraphTrav.BonesToVisit.Count > 0)
                    {
                        Bone currentBone = currGraphTrav.BonesToVisit[0];
                        
                        if (currentBone.children.Count > 1)
                        {                            

                            if((currGraphTrav.MotorAvailable - currentBone.rot_DoF.Count < 0)|| 
                                !IsConnectedBone(currGraphTrav.Partition,currentBone))
                            {
                                // Terminates the inclusion into the current partition
                                currGraphTrav.Decomposition.Add(currGraphTrav.Partition);
                                currGraphTrav.Partition = new List<Bone>();
                                currGraphTrav.MotorAvailable = motors;
                            }
                            else
                            {                                
                                currGraphTrav.Partition.Add(currentBone);
                                currGraphTrav.BonesToVisit.RemoveAt(0);
                                currGraphTrav.MotorAvailable -= currentBone.rot_DoF.Count;
                                bool currGraphTravEdited = false;
                                
                                // Explore neighborhood:
                                
                                // 1. Depth-First 
                                List<List<Bone>> alternativePaths = ChildreWithDepthSearch
                                    (currentBone, currGraphTrav.BonesToVisit, currGraphTrav.MotorAvailable, graph);
                                if (alternativePaths.Count > 0)
                                {
                                    currGraphTravEdited = true;
                                    foreach (List<Bone> path in alternativePaths) 
                                    {
                                        // Copies the old GraphTraversal values 
                                        GraphTraversal newGraphTr = new GraphTraversal(currGraphTrav.MotorAvailable);
                                        newGraphTr.BonesToVisit = currGraphTrav.BonesToVisit.ToList();
                                        newGraphTr.Decomposition = currGraphTrav.Decomposition.ToList();
                                        newGraphTr.Partition = currGraphTrav.Partition.ToList();

                                        // Updates new GraphTraversal object, adding new bone visited
                                        foreach (Bone childToAdd in path)
                                        {
                                            newGraphTr.Partition.Add(childToAdd);
                                            newGraphTr.BonesToVisit.Remove(childToAdd);
                                            newGraphTr.MotorAvailable -= childToAdd.rot_DoF.Count;
                                        }

                                        newGraphTr.Decomposition.Add(newGraphTr.Partition);
                                        newGraphTr.Partition = new List<Bone>();
                                        newGraphTr.MotorAvailable = motors;

                                        graphTraversalList.Add(newGraphTr);
                                    }
                                }

                                // 2. Breadth-first
                                List<Bone> neighborsToAdd =  ChildrenWithBreadthFirst
                                    (currentBone, currGraphTrav.BonesToVisit, currGraphTrav.MotorAvailable, graph); 
                                
                                if(neighborsToAdd.Count > 1)
                                {
                                    currGraphTravEdited = true;

                                    // Copies the old GraphTraversal values 
                                    GraphTraversal newGraphTr = new GraphTraversal(currGraphTrav.MotorAvailable);
                                    newGraphTr.BonesToVisit = currGraphTrav.BonesToVisit.ToList();
                                    newGraphTr.Decomposition = currGraphTrav.Decomposition.ToList();
                                    newGraphTr.Partition = currGraphTrav.Partition.ToList();
                                    
                                    foreach (Bone childToAdd in neighborsToAdd)
                                    {
                                        newGraphTr.Partition.Add(childToAdd);
                                        newGraphTr.BonesToVisit.Remove(childToAdd);
                                        newGraphTr.MotorAvailable -= childToAdd.rot_DoF.Count;
                                    }

                                    newGraphTr.Decomposition.Add(newGraphTr.Partition);
                                    newGraphTr.Partition = new List<Bone>();
                                    newGraphTr.MotorAvailable = motors;

                                    graphTraversalList.Add(newGraphTr);
                                }

                                if (currGraphTravEdited)
                                {
                                    graphTraversalList.Remove(currGraphTrav);
                                    currGraphTrav = graphTraversalList[0];
                                }
                            }
                        }
                        else
                        {
                            // Current bone is a sequential bone
                            UpdatePartition(motors, currGraphTrav.Decomposition, 
                                ref currGraphTrav.MotorAvailable, 
                                ref currGraphTrav.Partition, currentBone);
                            currGraphTrav.BonesToVisit.RemoveAt(0);                            
                        }
                    }

                    if (currGraphTrav.Partition.Count > 0)
                    {
                        // Adds last partition
                        currGraphTrav.Decomposition.Add(currGraphTrav.Partition);
                    }                   

                    graphPartitions.Add(currGraphTrav.Decomposition.ToList());
                    currGraphTrav.Decomposition = new List<List<Bone>>();
                    graphTraversalList.RemoveAt(0);
                }
                            
            }

            // TODO: Remove decompositions from graphPartitions that propose the same partitioning            
            return graphPartitions;
            
        }

        private static List<List<Bone>> ChildreWithDepthSearch(Bone currentBone, List<Bone> armature, int motorAvailable, BidirectionalGraph<Bone, Edge<Bone>> graph)
        {
            List<List<Bone>> result = new List<List<Bone>>();            
            List<Bone> boneToVisit = armature.ToList();                        
            List<PathExpansion> paths = new List<PathExpansion>();

            // Initializes paths
            foreach (var edge in graph.OutEdges(currentBone))
            {
                Bone neighbor = edge.Target;
                if (boneToVisit.Contains(neighbor) && (motorAvailable - neighbor.rot_DoF.Count >= 0))
                {
                    List<Bone> newBoneToVisit = boneToVisit.ToList();
                    newBoneToVisit.Remove(neighbor);
                    List<Bone> newPath = new List<Bone> { neighbor };

                    paths.Add(new PathExpansion(newBoneToVisit, newPath, motorAvailable - neighbor.rot_DoF.Count));
                }
            }                        
            
            while (paths.Count > 0)
            {                
                PathExpansion pathToExpand = paths[0];
                bool pathEdited = false;
                       
                // Finds neighbors
                List<Bone> neighborhood = new List<Bone>();
                foreach (var edge in graph.OutEdges(pathToExpand.Path[pathToExpand.Path.Count-1]))
                {                            
                    if (pathToExpand.NodeToExpand.Contains(edge.Target))
                    {
                        neighborhood.Add(edge.Target);
                    }
                }

                foreach (Bone neighborToAdd in neighborhood)
                {
                    if (pathToExpand.MotorAvailable - neighborToAdd.rot_DoF.Count >= 0)
                    {
                        PathExpansion newPathExpansion = new PathExpansion();
                        newPathExpansion.MotorAvailable = pathToExpand.MotorAvailable;
                        newPathExpansion.NodeToExpand = pathToExpand.NodeToExpand.ToList();
                        newPathExpansion.Path = pathToExpand.Path.ToList();
                        newPathExpansion.Path.Add(neighborToAdd);
                        newPathExpansion.MotorAvailable -= neighborToAdd.rot_DoF.Count;
                        newPathExpansion.NodeToExpand.Remove(neighborToAdd);
                        
                        pathEdited = true;
                        paths.Add(newPathExpansion);
                    }                    
                }
                if (pathEdited)
                {
                    paths.RemoveAt(0);
                }
                else 
                {
                    result.Add(pathToExpand.Path);
                    paths.Remove(pathToExpand);
                }                                                            
            }                            

            return result;
        }

        private static List<Bone> ChildrenWithBreadthFirst(Bone currentBone, List<Bone> armature, int motorAvailable, BidirectionalGraph<Bone, Edge<Bone>> graph)
        {
            List<Bone> neighborsAtLevel = new List<Bone>();
            List<Bone> result = new List<Bone>();            
            List<Bone> bonesToVisit = armature.ToList();

            // Initializes neighborsAtLevel list and updates bonesToVisit
            foreach (var edge in graph.OutEdges(currentBone))
            {
                Bone neighbor = edge.Target;
                if (bonesToVisit.Contains(neighbor))
                {
                    neighborsAtLevel.Add(neighbor);
                    bonesToVisit.Remove(neighbor);                    
                }
            }     

            for(int level = 0; level < GetMaxLengthChain(armature); level++ )
            {
                if (CountArmatureDofs(neighborsAtLevel) <= motorAvailable)
                {
                    result = neighborsAtLevel.ToList();

                    // Adds neighbors of next level
                    List<Bone> boneToAdd = new List<Bone>();                    
                    foreach(Bone boneToVisit in bonesToVisit)
                    {                        
                        foreach(Bone bone in neighborsAtLevel)
                        {
                            // Checks if the boneToVisit is connected to a bone in the list neighborsAtLevel
                            if (bone.children.Contains(boneToVisit.name) || bone.parent.Equals(boneToVisit.name))
                            {
                                boneToAdd.Add(boneToVisit);
                                // Bone is connected, continue with the next boneToVisit
                                break;
                            }
                        }                       
                    }
                    foreach (Bone b in boneToAdd) 
                    {
                        neighborsAtLevel.Add(b);
                        bonesToVisit.Remove(b);
                    }
                }
                else
                {
                    // Partition can not contain another level of children
                    break;
                }                                                                                               
            }

            return result;
        }

        private static bool PartitionCapacityOverflow(List<Bone> armature, int motors)
        {
            bool result = false;
            foreach (Bone b in armature)
            {
                if (b.rot_DoF.Count > motors)
                {
                    return true;
                }
            }
            return result;
        }

        private static void UpdatePartition(int motors, List<List<Bone>> decomposition, ref int motorAvailable, ref List<Bone> partition, Bone currentBone)
        {
            
            // Inserts the first element into the partition
            if ((partition.Count == 0) || 
                (IsConnectedBone(partition, currentBone) && (motorAvailable - currentBone.rot_DoF.Count >= 0)))
            {
                partition.Add(currentBone);
                motorAvailable -= currentBone.rot_DoF.Count;

            }                
            else
            {
                // partitions is full or new bone is not a child or parent of bones in the partition
                decomposition.Add(partition);
                partition = new List<Bone>();
                partition.Add(currentBone);
                motorAvailable = motors - currentBone.rot_DoF.Count;
            }

            
            
        }

        // Recursive function to count DoFs
        private static int CountBoneChildrenDoFs(Bone bone, List<Bone> armature)
        {
            int count = bone.rot_DoF.Count;
            foreach (String child in bone.children) 
            {
                count+= CountBoneChildrenDoFs(GetBoneFromName(child, armature), armature);
            }            
            return count;
        }                

        public static List<Bone> DecomposeMotorCombination(int motor, char[] comb, bool useSensor, Brick brick)
        {
            List<Bone> configurations = new List<Bone>();
            // list of position {0,1,2,3...}
            var list = new List<string>();
            for (int i = 0; i < comb.Length; i++)
            {
                list.Add(i.ToString());
            }


            List<string> combToListString = new List<string>();
            foreach (char c in comb)
                combToListString.Add(c.ToString());
        
            int index = 0;

            for (int size = 1; size <= motor; size++)
            {
                var result = Combinatorics.GetDispositions(list, size);

                foreach (var perm in result)
                {
                    Bone b = new Bone("");
                    //char[] g = new char[motors];                    
                    foreach (var c in perm)
                    {
                        // Adds DoF to the bone
                        b.rot_DoF.Add(comb[Convert.ToInt32(c)]);
                        //Console.Write(c + " ");
                        b.name += combToListString[Convert.ToInt32(c)] + "_";
                    }
                        
                    // Add Bone to the configuration
                    configurations.Add(b);
                    index++;
                }
            }
            return configurations;
        }
        
        private static Bone GetBoneFromName(string boneName, List<Bone> armature)
        {
            foreach (Bone b in armature)
            {
                if (b.name.Equals(boneName))
                {
                    return b;
                }
            }
            return null;
        }                

        public static List<string> GetTuiComponentList(bool useSensor, Brick brick)
        {
            List<string> components = new List<string>();
            if (!brick.Ports[InputPort.A].Type.ToString().Equals(DeviceType.Empty.ToString()) &&
                !brick.Ports[InputPort.A].Type.ToString().Equals(DeviceType.Touch.ToString()))
                components.Add(brick.Ports[InputPort.A].Type.ToString() + "(PORT-A)_TUI");
            if (!brick.Ports[InputPort.B].Type.ToString().Equals(DeviceType.Empty.ToString()) &&
                !brick.Ports[InputPort.B].Type.ToString().Equals(DeviceType.Touch.ToString()))
                components.Add(brick.Ports[InputPort.B].Type.ToString() + "(PORT-B)_TUI");
            if (!brick.Ports[InputPort.C].Type.ToString().Equals(DeviceType.Empty.ToString()) &&
                !brick.Ports[InputPort.C].Type.ToString().Equals(DeviceType.Touch.ToString()))
                components.Add(brick.Ports[InputPort.C].Type.ToString() + "(PORT-C)_TUI");
            if (!brick.Ports[InputPort.D].Type.ToString().Equals(DeviceType.Empty.ToString()) &&
                !brick.Ports[InputPort.D].Type.ToString().Equals(DeviceType.Touch.ToString()))
                components.Add(brick.Ports[InputPort.D].Type.ToString() + "(PORT-D)_TUI");

            if (useSensor)
            {
                if (!brick.Ports[InputPort.One].Type.ToString().Equals(DeviceType.Empty.ToString()) &&
                !brick.Ports[InputPort.One].Type.ToString().Equals(DeviceType.Touch.ToString()))
                    components.Add(brick.Ports[InputPort.One].Type.ToString() + "(PORT-One)_TUI");
                if (!brick.Ports[InputPort.Two].Type.ToString().Equals(DeviceType.Empty.ToString()) &&
                !brick.Ports[InputPort.Two].Type.ToString().Equals(DeviceType.Touch.ToString()))
                    components.Add(brick.Ports[InputPort.Two].Type.ToString() + "(PORT-Two)_TUI");
                if (!brick.Ports[InputPort.Three].Type.ToString().Equals(DeviceType.Empty.ToString()) &&
                !brick.Ports[InputPort.Three].Type.ToString().Equals(DeviceType.Touch.ToString()))
                    components.Add(brick.Ports[InputPort.Three].Type.ToString() + "(PORT-Three)_TUI");
                if (!brick.Ports[InputPort.Four].Type.ToString().Equals(DeviceType.Empty.ToString()) &&
                !brick.Ports[InputPort.Four].Type.ToString().Equals(DeviceType.Touch.ToString()))
                    components.Add(brick.Ports[InputPort.Four].Type.ToString() + "(PORT-Four)_TUI");
            }
            return components;
        }
        
        public static Dictionary<string, List<List<char>>> InitDoFDictionary()
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

        public static int GetMaxLengthChain(List<Bone> Partition)
        {
            int max = 0;
            foreach (Bone b in Partition)
                if (b.level > max)
                    max = b.level;
            return max + 1;
        }

        public static int GetMinLengthChain(List<Bone> Partition)
        {
            int min = int.MaxValue;
            foreach (Bone b in Partition)
                if (b.level < min)
                    min = b.level;
            return min;
        }

        public static bool KinectAssignmentConsistency(PartitionAssignment result)
        {           
            for (int i = 1; i < result.Assignment.Length; i++)
            {
                string componentName1 = result.Handler[result.Assignment[i - 1]].name;
                if (componentName1.Contains("PORT"))
                    continue;
                else
                    componentName1 = componentName1.Split('_')[0]; 
                    
                string componentName2 = result.Handler[result.Assignment[i]].name;
                if (componentName2.Contains("PORT"))
                    continue;
                else
                    componentName2 = componentName2.Split('_')[0]; 
                
                if(componentName1.Equals(componentName2))
                {
                    string boneName1 = result.Partition[i - 1].name.Split('_')[0];
                    string boneName2 = result.Partition[i].name.Split('_')[0];

                    if(!boneName1.Equals(boneName2))
                        return false;
                }                               
                    
            }
            return true;
        }        

        public static List<Bone> GetTuiArmature(List<Bone> referenceAramature, bool useSensor, Brick brick)
        {
            List<Bone> armature = new List<Bone>();
            
            // Gets the dof[i] and the bone to which it belongs
            List<DofBoneAssociation> dofBoneAss = GetDofsBoneAssociation(referenceAramature);

            // Gets only the dof sequence of the reference armature (Blender armature)
            List<string> dofs = new List<string>();
            for (int i = 0; i < dofBoneAss.Count; i++)
            {
                dofs.Add(dofBoneAss[i].Dof);
            }
            
            List<string> dofsAssigned = Metrics.AssignName(dofs, useSensor, brick, true);
            int componentIndex = 0;
            for (int i = 0; i < referenceAramature.Count; i++) 
            {
                Bone bone = new Bone("");
                bone.level = referenceAramature[i].level;
                bone.rot_DoF = referenceAramature[i].rot_DoF.ToList();
                bone.loc_DoF = referenceAramature[i].loc_DoF.ToList();
                bone.parent = referenceAramature.FindIndex(x => x.name.Equals(referenceAramature[i].parent)).ToString();
                foreach (string child in referenceAramature[i].children)
                {
                     referenceAramature[i].children.ToList();
                     bone.children.Add(referenceAramature.FindIndex(x => x.name.Equals(child)).ToString());
                }

                // Creates a name that contain the component name assigned
                for (int j = componentIndex; j < dofsAssigned.Count; j++)
                {
                    if(referenceAramature[i].name.Equals(dofBoneAss[j].ReferenceBone.name))
                    {
                        bone.name +=  dofsAssigned[componentIndex] + " | ";
                        componentIndex++;
                    }
                    else 
                    {
                        break;
                    }
                }
                
                armature.Add(bone);
            }

            // Updates parents relation;
            foreach (Bone b in armature) 
            {
                if (b.parent.Equals("-1"))
                {
                    b.parent = "";
                }
                else
                {
                    b.parent = armature[Int32.Parse(b.parent)].name;
                }
            }

            // Updates childeren relation;
            foreach (Bone b in armature)
            {
                List<string> children = new List<string>();
                foreach (string boneIndex in b.children)
                {
                    if (!boneIndex.Equals("-1"))
                    {
                        children.Add(armature[Int32.Parse(boneIndex)].name);
                    }
                }

                b.children = children;
            }
            return armature;
        }

        public static bool IsConnectedBone(List<Bone> bones, Bone vertex)
        {
            if (bones.Count == 0)             
                return true;
            
            Bone lastBone = bones[bones.Count - 1];

            if (lastBone.parent.Equals(vertex.name))
                return true;
            
            if (lastBone.children.Contains(vertex.name))
                return true;
            
            return false;
                           
            /*    
            bool connectedElement = false;
            
            foreach (Bone b in bones)
            {
                if (b.parent.Equals(vertex.name))
                {
                    connectedElement = true;
                    break;
                }
                if (b.children.Contains(vertex.name))
                {
                    connectedElement = true;
                    break;
                }
            }
            return connectedElement;
            */
        }

        public static List<DofBoneAssociation> GetDofsBoneAssociation(List<Bone> partition)
        {
            List<DofBoneAssociation> armatureDofs = new List<DofBoneAssociation>();
            foreach (Bone b in partition)
            {
                foreach (char dof in b.rot_DoF) 
                {
                    armatureDofs.Add(new DofBoneAssociation(b, "ROT(" + dof + ")"));
                }
                foreach (char dof in b.loc_DoF)
                {
                    armatureDofs.Add(new DofBoneAssociation(b, "LOC(" + dof + ")"));
                }
            }
            return armatureDofs;
        }

        public static int CountArmatureDofs(List<Bone> armature)
        {
            int armatureDofs = 0;
            foreach (Bone b in armature)
            {
                armatureDofs += b.rot_DoF.Count;
                armatureDofs += b.loc_DoF.Count;
            }

            return armatureDofs;
        }
    }
   
}
