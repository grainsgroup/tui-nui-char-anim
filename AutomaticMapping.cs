﻿using System;
using System.Collections.Generic;
using System.Linq;
using QuickGraph;
using Lego.Ev3.Core;


namespace Microsoft.Samples.Kinect.SkeletonBasics
{

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
                    if (childToAdd != null)
                        graph.AddVerticesAndEdge(new Edge<Bone>(b, childToAdd));
                }
            }
            return graph;
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

            string[] perm = new string[comb.Length];
            for (int i = 0; i < comb.Length; i++)
            {
                perm[i] = i.ToString();
            }
            List<string[]> permutations = new List<string[]>() { perm };


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
                    foreach (string assignedComp in Metrics.AssignName
                        (new List<List<string>>() { sequenceToAdd.ToList() }, true, brick, true))
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

            List<List<Bone>> armatures = CreateArmature(comb, typedSequence_Dof);

            return armatures;
        }

        public static List<List<Bone>> CreateArmature(char[] comb, List<List<string>> typedSequence_Dof)
        {
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
                }

                foreach (PartialArmature pa in partialArmatures)
                {
                    armatures.Add(pa.virtualArmature);
                }
            }
            return armatures;
        }

        public static List<Bone> GetAncestor(List<Bone> list, Bone lastBoneAnalyzed)
        {
            List<Bone> ancestors = new List<Bone>();

            while (lastBoneAnalyzed.level > 0)
            {
                ancestors.Add(lastBoneAnalyzed);
                lastBoneAnalyzed = GetBoneFromName(lastBoneAnalyzed.parent, list);
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
       
        public static bool IsEqualDecomposition(List<List<Bone>> list1, List<List<Bone>> list2)
        {
            if (list1.Count != list2.Count)
                return false;
            else
            {
                foreach (List<Bone> partition1 in list1)
                {
                    foreach (Bone b1 in partition1)
                    {
                        List<Bone> partition2 = GetPartitionFromBone(b1, list2);
                        if (partition1.Count != partition2.Count)
                        {
                            return false;
                        }
                        else
                        {
                            foreach (Bone b2 in partition2)
                            {
                                if (!partition1.Contains(b2))
                                    return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        private static List<Bone> GetPartitionFromBone(Bone bone, List<List<Bone>> list)
        {

            foreach (List<Bone> partition in list)
            {
                if (partition.Contains(bone))
                    return partition;
            }
            return null;
        }

        public static List<List<Bone>> ChildrenWithDepthSearch(Bone currentBone, List<Bone> armature, int motorAvailable, BidirectionalGraph<Bone, Edge<Bone>> graph)
        {
            List<List<Bone>> result = new List<List<Bone>>();
            List<Bone> boneToVisit = armature.ToList();
            List<PathExpansion> paths = new List<PathExpansion>();

            // Gets neighbors
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

            bool executeComputation = SymmetricSplitCheck(paths);

            if (executeComputation)
            {
                while (paths.Count > 0)
                {
                    PathExpansion pathToExpand = paths[0];
                    bool pathEdited = false;

                    // Finds neighbors
                    List<Bone> neighborhood = new List<Bone>();
                    foreach (var edge in graph.OutEdges(pathToExpand.Path[pathToExpand.Path.Count - 1]))
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
            }

            return result;
        }

        public static List<List<Bone>> ChildrenWithDepthSearch_LOCROT(Bone currentBone, List<Bone> armature, int motorAvailable, BidirectionalGraph<Bone, Edge<Bone>> graph)
        {
            List<List<Bone>> result = new List<List<Bone>>();
            List<Bone> boneToVisit = armature.ToList();
            List<PathExpansion> paths = new List<PathExpansion>();

            // Gets neighbors
            foreach (var edge in graph.OutEdges(currentBone))
            {
                Bone neighbor = edge.Target;
                if (boneToVisit.Contains(neighbor) && (motorAvailable + 3  - (neighbor.rot_DoF.Count + neighbor.loc_DoF.Count)>= 0))
                {
                    List<Bone> newBoneToVisit = boneToVisit.ToList();
                    newBoneToVisit.Remove(neighbor);
                    List<Bone> newPath = new List<Bone> { neighbor };
                    // For each neighbour found initializes a new path
                    paths.Add(new PathExpansion
                        (newBoneToVisit, newPath, motorAvailable - (neighbor.rot_DoF.Count + neighbor.loc_DoF.Count)));
                }
            }

            bool executeComputation = SymmetricSplitCheck(paths);

            if (executeComputation)
            {
                while (paths.Count > 0)
                {
                    PathExpansion pathToExpand = paths[0];
                    bool pathEdited = false;

                    if (pathToExpand.MotorAvailable>=0)
                    {
                        // Finds neighbors
                        List<Bone> neighborhood = new List<Bone>();
                        foreach (var edge in graph.OutEdges(pathToExpand.Path[pathToExpand.Path.Count - 1]))
                        {
                            if (pathToExpand.NodeToExpand.Contains(edge.Target))
                            {
                                neighborhood.Add(edge.Target);
                            }
                        }

                        foreach (Bone neighborToAdd in neighborhood)
                        {
                            if (pathToExpand.MotorAvailable + 3 - (neighborToAdd.rot_DoF.Count + neighborToAdd.loc_DoF.Count) >= 0)
                            {
                                PathExpansion newPathExpansion = new PathExpansion();
                                newPathExpansion.MotorAvailable = pathToExpand.MotorAvailable;
                                newPathExpansion.NodeToExpand = pathToExpand.NodeToExpand.ToList();
                                newPathExpansion.Path = pathToExpand.Path.ToList();
                                newPathExpansion.Path.Add(neighborToAdd);
                                newPathExpansion.MotorAvailable -= (neighborToAdd.rot_DoF.Count + neighborToAdd.loc_DoF.Count);
                                newPathExpansion.NodeToExpand.Remove(neighborToAdd);

                                pathEdited = true;
                                paths.Add(newPathExpansion);
                            }
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
            }

            return result;
        }

        private static bool SymmetricSplitCheck(List<PathExpansion> paths)
        {
            foreach (PathExpansion p in paths)
            {
                if (p.Path[0].name.Contains(".R") || p.Path[0].name.Contains(".L"))
                    return false;
            }
            return true;
        }

        private static bool SymmetricSplitCheck(List<Bone> neighborsAtLevel)
        {
            int bonesL = 0;
            int bonesR = 0;

            foreach (Bone b in neighborsAtLevel)
            {
                if (b.name.Contains(".R"))
                {
                    bonesR++;
                    continue;
                }

                if (b.name.Contains(".L"))
                {
                    bonesL++;
                    continue;
                }
            }

            return bonesR == bonesL;
        }

        public static List<Bone> ChildrenWithBreadthFirst(Bone currentBone, List<Bone> armature, int motorAvailable, BidirectionalGraph<Bone, Edge<Bone>> graph)
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

            for (int level = 0; level < GetMaxLengthChain(armature); level++)
            {
                if (CountArmatureDofs(neighborsAtLevel) <= motorAvailable && SymmetricSplitCheck(neighborsAtLevel))
                {

                    result = neighborsAtLevel.ToList();

                    // Adds neighbors of next level
                    List<Bone> boneToAdd = new List<Bone>();
                    foreach (Bone boneToVisit in bonesToVisit)
                    {
                        foreach (Bone bone in neighborsAtLevel)
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

        public static List<Bone> ChildrenWithBreadthFirst_LOCROT(Bone currentBone, List<Bone> armature, int motorAvailable, BidirectionalGraph<Bone, Edge<Bone>> graph)
        {
            List<Bone> neighborsAtLevel = new List<Bone>();
            List<Bone> result = new List<Bone>();
            List<Bone> currBonesToVisit = armature.ToList();

            // Initializes neighborsAtLevel list and updates currBonesToVisit
            foreach (var edge in graph.OutEdges(currentBone))
            {
                Bone neighbor = edge.Target;
                if (currBonesToVisit.Contains(neighbor))
                {
                    neighborsAtLevel.Add(neighbor);
                    currBonesToVisit.Remove(neighbor);
                }
            }

            for (int level = 0; level < GetMaxLengthChain(armature); level++)
            {
                bool partitionNotCompleted = true;
                int currMotorAvailabe = motorAvailable;
                for (int boneNeighborsIndex = 0; boneNeighborsIndex < neighborsAtLevel.Count; boneNeighborsIndex++)
                {
                    int dofToAdd = neighborsAtLevel[boneNeighborsIndex].rot_DoF.Count + neighborsAtLevel[boneNeighborsIndex].loc_DoF.Count;
                    
                    // deve essere sempre maggiore o uguale a zero tranne nell'ultimo osso in cui può essere < 0 ma comnque sempre maggiore di 3 altrimenti anche l'hip non ci basta
                    currMotorAvailabe-= dofToAdd;
                    if (currMotorAvailabe < 0)
                    {
                        if (boneNeighborsIndex != neighborsAtLevel.Count - 1 || currMotorAvailabe < -3)
                            partitionNotCompleted = false;
                    }

                }


                //if (CountArmatureDofs(neighborsAtLevel) <= motorAvailable && SymmetricSplitCheck(neighborsAtLevel))
                if (partitionNotCompleted && SymmetricSplitCheck(neighborsAtLevel))
                {
                    result = neighborsAtLevel.ToList();

                    // Adds neighbors of next level
                    List<Bone> connectedNeighbors = new List<Bone>();
                    foreach (Bone newNeighbor in currBonesToVisit)
                    {
                        foreach (Bone bone in neighborsAtLevel)
                        {
                            // Checks if the boneToVisit is connected to a bone in the list neighborsAtLevel
                            if (bone.children.Contains(newNeighbor.name) || bone.parent.Equals(newNeighbor.name))
                            {
                                connectedNeighbors.Add(newNeighbor);
                                // Bone is connected, continue with the next boneToVisit
                                break;
                            }
                        }
                    }
                    foreach (Bone b in connectedNeighbors)
                    {
                        neighborsAtLevel.Add(b);
                        currBonesToVisit.Remove(b);
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


        public static bool PartitionCapacityOverflow(List<Bone> armature, int motors)
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

        public static bool PartitionCapacityOverflow_LOCROT(List<Bone> armature, int motors)
        {
            bool result = false;
            foreach (Bone b in armature)
            {
                if (b.rot_DoF.Count + b.loc_DoF.Count > motors + 3)
                {
                    return true;
                }
            }
            return result;
        }

        public static void UpdatePartition(int motors, List<List<Bone>> decomposition, ref int motorAvailable, ref List<Bone> partition, Bone currentBone)
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

        public static void UpdatePartition_LOCROT(int motors, List<List<Bone>> decomposition, ref int motorAvailable, ref List<Bone> partition, Bone currentBone)
        {
            int dofToAdd = currentBone.loc_DoF.Count + currentBone.rot_DoF.Count;

            // Inserts the first element into the partition
            if (partition.Count == 0)
            {
                partition.Add(currentBone);

                if (motorAvailable - dofToAdd >= 0)
                {
                    motorAvailable -= dofToAdd;
                }
                else
                {
                    //partitions is completed
                    decomposition.Add(partition);
                    partition = new List<Bone>();
                    motorAvailable = motors;
                }
            }                
                
            else if(IsConnectedBone(partition, currentBone))
            {
                // +3 is to consider the HIP
                if (dofToAdd > motorAvailable + 3)
                {
                    AddBoneIntoNewPartition(motors, decomposition, ref motorAvailable, ref partition, currentBone);
                }
                else
                {
                    partition.Add(currentBone);
                    if (motorAvailable - dofToAdd >= 0)
                    {
                        motorAvailable -= dofToAdd;
                    }
                    else
                    {
                        //partitions is completed
                        decomposition.Add(partition);
                        partition = new List<Bone>();
                        motorAvailable = motors;
                    }
                }
            } 
            else
            {
                AddBoneIntoNewPartition(motors, decomposition, ref motorAvailable, ref partition, currentBone);
            }
        }

        private static void AddBoneIntoNewPartition(int motors, List<List<Bone>> decomposition, ref int motorAvailable, ref List<Bone> partition, Bone currentBone)
        {
            // bone is not a child or parent of bones in the partition
            decomposition.Add(partition);
            partition = new List<Bone>();
            partition.Add(currentBone);
            motorAvailable = motors - currentBone.rot_DoF.Count - currentBone.loc_DoF.Count;
        }


        // Recursive function to count DoFs
        private static int CountBoneChildrenDoFs(Bone bone, List<Bone> armature)
        {
            int count = bone.rot_DoF.Count;
            foreach (String child in bone.children)
            {
                count += CountBoneChildrenDoFs(GetBoneFromName(child, armature), armature);
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
                    if (!configurations.Contains(b))
                    {
                        configurations.Add(b);
                    }
                }
            }
            return configurations;

        }

        public static Bone GetBoneFromName(string boneName, List<Bone> armature)
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

        public static List<Bone> GetOneDofBones(Bone currentBone, List<Bone> components, Dictionary<string, List<List<char>>> dictionary)
        {
            List<Bone> bones = new List<Bone>();

            if (currentBone.rot_DoF.Count == 3)
            {
                // list of position
                var list = new List<string>();
                var alternatives = new List<Bone[]>();
                var componentList = new List<Bone>();
                List<List<char>> threeDofsRepr = dictionary[Metrics.GetDofString(currentBone.rot_DoF)];

                for (int i = 0; i < components.Count; i++)
                {
                    list.Add(i.ToString());
                }

                var result = Combinatorics.GetDispositions(list, 3);

                int index = 0;
                foreach (var perm in result)
                {
                    Bone[] g = new Bone[3];
                    index = 0;
                    foreach (var c in perm)
                    {
                        g[index] = components[Convert.ToInt32(c)];
                        index++;
                    }
                    alternatives.Add(g);
                }

                int bestScore = int.MaxValue;
                foreach (Bone[] alt in alternatives)
                {
                    List<char> currentRepr = GetDofSequenceFromPartition(alt.ToList());
                    bool validRepr = false;

                    foreach (List<char> repr in threeDofsRepr)
                    {
                        bool isEqual = true;
                        for (int k = 0; k < repr.Count; k++)
                        {
                            if (currentRepr[k] != repr[k])
                            {
                                isEqual = false;
                                break;
                            }
                        }
                        if (isEqual)
                        {
                            validRepr = true;
                            break;
                        }
                    }

                    if (validRepr)
                    {
                        int score = 0;
                        for (int i = 0; i < alt.Length; i++)
                        {
                            score +=
                                Metrics.GetAnnoyanceCost("ROT(" + alt[i].rot_DoF[0].ToString() + ")", alt[i].name);
                        }
                        if (score < bestScore)
                        {
                            componentList.Clear();
                            componentList = alt.ToList();
                        }
                    }
                }

                foreach (Bone comp in componentList)
                {
                    Bone b = new Bone(currentBone.name);
                    b.level = currentBone.level;
                    b.children = currentBone.children.ToList();
                    b.parent = currentBone.parent;


                    if (comp.name.Contains("LOC"))
                    {
                        b.loc_DoF.Add(comp.name[comp.name.IndexOf(":LOC(") + 5]);
                        bones.Add(b);
                        continue;
                    }

                    if (comp.name.Contains("ROT"))
                    {
                        b.rot_DoF.Add(comp.name[comp.name.IndexOf(":ROT(") + 5]);
                        bones.Add(b);
                        continue;
                    }
                }
            }
            else
            {
                foreach (char c in currentBone.rot_DoF)
                {
                    Bone b = new Bone(currentBone.name);
                    b.level = currentBone.level;
                    b.children = currentBone.children.ToList();
                    b.parent = currentBone.parent;
                    b.rot_DoF = new List<char>() { c };
                    bones.Add(b);
                }

                foreach (char c in currentBone.loc_DoF)
                {
                    Bone b = new Bone(currentBone.name);
                    b.level = currentBone.level;
                    b.children = currentBone.children.ToList();
                    b.parent = currentBone.parent;
                    b.loc_DoF = new List<char>() { c };
                    bones.Add(b);
                }
            }
            return bones;
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

            // VIRTUAL_MOTOR
            //components.Add(DeviceType.LMotor.ToString() + "(PORT-Two)_TUI");
            //components.Add(DeviceType.LMotor.ToString() + "(PORT-Three)_TUI");
            //components.Add(DeviceType.MMotor.ToString() + "(PORT-D)_TUI");

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

        public static List<Bone> GetTuiArmature(List<Bone> controlledAramature, bool useSensor, Brick brick, Dictionary<string, List<List<char>>> dictionary)
        {
            List<Bone> armature = new List<Bone>();

            // Gets the dof[i] and the bone to which it belongs
            List<List<DofBoneAssociation>> dofBoneAss = GetDofsBoneAssociation(controlledAramature, dictionary);

            // Gets only the dof sequence of the controlled armature (Blender armature)
            List<List<string>> dofsAlternatives = new List<List<string>>();
            foreach (List<DofBoneAssociation> dofSequence in dofBoneAss)
            {
                List<string> dofs = new List<string>();
                for (int i = 0; i < dofSequence.Count; i++)
                {
                    dofs.Add(dofSequence[i].Dof);
                }
                dofsAlternatives.Add(dofs);
            }

            List<string> dofsAssigned = Metrics.AssignName(dofsAlternatives, useSensor, brick, true);
            int componentIndex = 0;
            for (int i = 0; i < controlledAramature.Count; i++)
            {
                Bone bone = new Bone("");
                bone.level = controlledAramature[i].level;
                bone.rot_DoF = controlledAramature[i].rot_DoF.ToList();
                bone.loc_DoF = controlledAramature[i].loc_DoF.ToList();
                bone.parent = controlledAramature.FindIndex(x => x.name.Equals(controlledAramature[i].parent)).ToString();
                foreach (string child in controlledAramature[i].children)
                {
                    controlledAramature[i].children.ToList();
                    bone.children.Add(controlledAramature.FindIndex(x => x.name.Equals(child)).ToString());
                }

                // Creates a name that contain the component name assigned
                for (int j = componentIndex; j < dofsAssigned.Count; j++)
                {
                    if (controlledAramature[i].name.Equals(dofBoneAss[0][j].ReferenceBone.name))
                    {
                        bone.name += dofsAssigned[componentIndex] + " | ";
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

        public static List<List<DofBoneAssociation>> GetDofsBoneAssociation(List<Bone> partition,
            Dictionary<string, List<List<char>>> dictionary)
        {
            List<List<DofBoneAssociation>> armatureDofs = new List<List<DofBoneAssociation>>();
            List<List<char>> alternatives = new List<List<char>>();

            foreach (Bone b in partition)
            {
                if (b.rot_DoF.Count > 0)
                {
                    alternatives = dictionary[Metrics.GetDofString(b.rot_DoF.ToList())];

                    if (armatureDofs.Count == 0)
                    {
                        //the armatureDofs List is empty
                        foreach (List<char> dofSequence in alternatives)
                        {
                            List<DofBoneAssociation> item = new List<DofBoneAssociation>();
                            foreach (char dof in dofSequence)
                            {
                                item.Add(new DofBoneAssociation(b, "ROT(" + dof + ")"));
                            }
                            armatureDofs.Add(item);
                        }
                    }
                    else
                    {
                        int armatureDofsCount = armatureDofs.Count;
                        for (int i = 0; i < armatureDofsCount; i++)
                        {
                            foreach (List<char> dofSequence in alternatives)
                            {
                                List<DofBoneAssociation> newItem = armatureDofs[0].ToList();

                                foreach (char dof in dofSequence)
                                {
                                    newItem.Add(new DofBoneAssociation(b, "ROT(" + dof + ")"));
                                }
                                armatureDofs.Add(newItem);
                            }
                            armatureDofs.RemoveAt(0);
                        }
                    }
                }
                if (b.loc_DoF.Count > 0)
                {
                    if (armatureDofs.Count == 0)
                    {
                        List<DofBoneAssociation> item = new List<DofBoneAssociation>();
                        foreach (char dof in b.loc_DoF)
                        {
                            item.Add(new DofBoneAssociation(b, "LOC(" + dof + ")"));
                        }
                        armatureDofs.Add(item);

                    }
                    else
                    {
                        foreach (List<DofBoneAssociation> item in armatureDofs)
                        {
                            foreach (char dof in b.loc_DoF)
                            {
                                item.Add(new DofBoneAssociation(b, "LOC(" + dof + ")"));
                            }
                        }
                    }
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

        public static bool IsSplittedArmature(List<Bone> armature)
        {
            bool splitterFound = false;
            foreach (Bone b in armature)
            {
                if (b.children.Count > 1)
                {
                    splitterFound = true;
                    break;
                }

            }

            return splitterFound;
        }

        public static List<Bone> GetRotBones(List<Bone> armature)
        {
            List<Bone> result = new List<Bone>();
            int minLevelBone = AutomaticMapping.GetMinLengthChain(armature);
            foreach (Bone b in armature)
            {
                if (b.rot_DoF.Count > 0)
                {
                    Bone boneToAdd = new Bone(b.name);
                    boneToAdd.rot_DoF = b.rot_DoF.ToList();
                    boneToAdd.level = b.level - minLevelBone;
                    boneToAdd.parent = b.parent;
                    boneToAdd.children = b.children.ToList();
                    result.Add(boneToAdd);
                }
            }

            return result;
        }

        public static List<char> GetDofSequenceFromPartition(List<Bone> partition)
        {
            List<char> comb = new List<char>();
            foreach (Bone b in partition)
            {
                foreach (char c in b.rot_DoF)
                {
                    comb.Add(c);
                }
            }
            return comb;
        }

        public static DecompositionAssignment GetDecompositionAssignment(List<PartitionAssignment> partAssign, char decType)
        {
            float totalCost = 0;

            foreach (PartitionAssignment partition in partAssign)
                totalCost += partition.Score;

            return new DecompositionAssignment(partAssign, totalCost, decType);
        }


        public static int GetMaxPartitionCount(List<List<List<Bone>>> graphPartitions)
        {
            int maxPartitionCount = 0;
            foreach (List<List<Bone>> decomposition in graphPartitions)
            {
                if (decomposition.Count > maxPartitionCount) 
                {
                    maxPartitionCount = decomposition.Count;
                }
            }
            return maxPartitionCount;
        }

        
    }

}