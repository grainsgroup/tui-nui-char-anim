using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lego.Ev3.Core;

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    public static class Metrics
    {
        public const int MAX_COST = 100;

        
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        // METRICS
        
        /// <summary>
        /// Cost = Abs(bone.level - handler.level)
        /// </summary>
        /// <param name="bone"></param>
        /// <param name="handler"></param>
        /// <param name="MaxLenghtChain"></param>
        /// <returns></returns>
        public static float ChainLengthScore(Bone bone, Bone handler, int MaxLenghtChain)
        {
            float cost = Math.Abs(bone.level - handler.level) / (float)MaxLenghtChain * MAX_COST;            
            return cost;
        }

        /*
        public static float ChainLengthScore(Bone bone, Bone handler, int MaxLenghtChain)
        {
            float cost = MAX_COST;

            if (handler.name.Contains("(x)") || handler.name.Contains("(y)") || handler.name.Contains("(z)"))
            {
                cost = Math.Abs(bone.level - handler.level) / MaxLenghtChain * MAX_COST;
            }

            if (handler.name.Contains("(PORT-"))
            {
                string componentName = handler.name.Substring(0, handler.name.IndexOf("(PORT-"));
                if (bone.loc_DoF.Count > 0)
                {
                    switch (componentName)
                    {
                        case "Ultrasonic":
                            cost = 0;
                            //cost = bone.level;
                            break;

                        case "LMotor":
                        case "MMotor":
                        case "Gyroscope":
                            //cost = 5 + bone.level;
                            cost = MAX_COST;
                            break;
                    }
                }
                if (bone.rot_DoF.Count > 0)
                {
                    cost = ComponentAnnoyanceScore("ROT(" + bone.rot_DoF[0].ToString() + ")", componentName);
                }
            }

            return cost;
        }
*/

        public static float ComponentRangeScore(Bone bone, Bone handler)
        {

            List<Bone> components = new List<Bone>();
            int dofCount = 0;
            float cost = 0;

            if (handler.name.Contains(" | "))
            {
                components = DecomposeHandler(handler);
            }
            else
            {
                components.Add(handler);
            }


            if (handler.name.Contains("ROT") || handler.name.Contains("LOC"))
            {
                // Handler name contains the informations of dofs assigned
                foreach (Bone boneHandler in components)
                {
                    foreach (char dof in boneHandler.rot_DoF)
                    {
                        cost += GetComponentRangeCost("ROT(" + dof + ")", boneHandler.name);
                        dofCount++;
                    }
                    foreach (char dof in boneHandler.loc_DoF)
                    {
                        cost += GetComponentRangeCost("LOC(" + dof + ")", boneHandler.name);
                        dofCount++;
                    }
                }
            }
            else 
            {
                foreach (Bone boneHandler in components)
                {
                    foreach (char dof in bone.rot_DoF)
                    {
                        cost += GetComponentRangeCost("ROT(" + dof + ")", boneHandler.name);
                        dofCount++;
                    }
                    foreach (char dof in bone.loc_DoF)
                    {
                        cost += GetComponentRangeCost("LOC(" + dof + ")", boneHandler.name);
                        dofCount++;
                    }
                }
            }
            
            return cost/dofCount;
        }

        public static float ComponentAnnoyanceScore(Bone bone, Bone handler)
        {
            List<Bone> components = new List<Bone>();
            int dofCount = 0;
            float cost = 0;

            if (handler.name.Contains(" | "))
            {
                components = DecomposeHandler(handler);
            }
            else
            {
                components.Add(handler);
            }

            if (handler.name.Contains("ROT") || handler.name.Contains("LOC"))
            {
                foreach (Bone boneHandler in components)
                {
                    foreach (char dof in boneHandler.rot_DoF)
                    {
                        cost += GetAnnoyanceCost("ROT(" + dof + ")", boneHandler.name);
                        dofCount++;
                    }
                    foreach (char dof in boneHandler.loc_DoF)
                    {
                        cost += GetAnnoyanceCost("LOC(" + dof + ")", boneHandler.name);
                        dofCount++;
                    }
                }
            }
            else 
            {
                foreach (Bone boneHandler in components)
                {
                    foreach (char dof in bone.rot_DoF)
                    {
                        cost += GetAnnoyanceCost("ROT(" + dof + ")", boneHandler.name);
                        dofCount++;
                    }
                    foreach (char dof in bone.loc_DoF)
                    {
                        cost += GetAnnoyanceCost("LOC(" + dof + ")", boneHandler.name);
                        dofCount++;
                    }
                }
            }

            return cost / dofCount;
        }

        public static float DofCoverageScore(Bone bone, Bone handler, Dictionary<string, List<List<char>>> dictionary)
        {
            // Vers 3.0
            float rotCost = 0;
            float locCost = 0;

            List<char> handlerDof = handler.rot_DoF;

            if (bone.rot_DoF.Count > 0)
            {
                // Degrees of fredom of bones which belong to the actual partition
                List<List<char>> boneDoF = new List<List<char>>();
                foreach (List<char> alternative in dictionary[GetDofString(bone.rot_DoF)])
                {
                    boneDoF.Add(alternative);
                }

                // Adds padding 
                List<List<char>> boneDoFPadded = new List<List<char>>();

                if (boneDoF[0].Count == handlerDof.Count)
                {
                    // padding not needed
                    boneDoFPadded = boneDoF;
                }
                else if (boneDoF[0].Count < handlerDof.Count)
                {
                    // Adds padding for a better comparison
                    foreach (List<char> alternativeRepr in boneDoF)
                    {
                        // list of possible position
                        var list = new List<string>();
                        for (int i = 0; i < handlerDof.Count; i++)
                        {
                            list.Add(i.ToString());
                        }
                        // Calculates permutation of list to identify padding position
                        var result = Combinatorics.GetDispositions(list, boneDoF[0].Count);

                        int index = 0;
                        foreach (var perm in result)
                        {
                            char[] g = new char[handlerDof.Count];
                            index = 0;
                            foreach (var c in perm)
                            {
                                g[Convert.ToInt32(c)] = alternativeRepr[index];
                                //Console.Write(c + " ");
                                index++;
                            }
                            boneDoFPadded.Add(g.ToList());
                        }
                    }
                }
                else
                {
                    // this motorDisposition is not able to control this partition
                    return MAX_COST;
                }


                // compute min score for all alterantives representation of this partition
                float minScore = float.MaxValue;

                foreach (List<char> item in boneDoFPadded)
                {
                    float tempScore = 0;
                    for (int j = 0; j < item.Count; j++)
                    {
                        if (item[j] == 0) { tempScore+=0.2f; continue; }
                        if (item[j] == 'x' && handlerDof[j] != 'x') { tempScore++; continue; }
                        if (item[j] == 'y' && handlerDof[j] != 'y') { tempScore++; continue; }
                        if (item[j] == 'z' && handlerDof[j] != 'z') { tempScore++; continue; }
                    }

                    if (tempScore < minScore)
                    {
                        minScore = tempScore;
                    }
                }

                rotCost = minScore / boneDoFPadded[0].Count * MAX_COST;
            }

            if (bone.loc_DoF.Count > 0)
            {
                foreach (char dof in bone.loc_DoF)
                {
                    if (!handler.loc_DoF.Contains(dof))
                    {
                        locCost++;
                    }
                }

                locCost = locCost / bone.loc_DoF.Count * MAX_COST; 
            }

            return rotCost + locCost;

/* 
// Version 1/2
            float rotCost = 0;
            float locCost = 0;
*/
            
/* 
// Version 1
 

            if (bone.rot_DoF.Count > 0)
            {
                if (bone.rot_DoF.Count > handler.rot_DoF.Count)
                {
                    rotCost = bone.rot_DoF.Count;
                }
                else
                {
                    rotCost = MAX_COST;
                    foreach (List<char> boneRotDof in dictionary[GetDofString(bone.rot_DoF)])
                    {
                        float cost = 0;
                        List<char> handlerDof = handler.rot_DoF.ToList();

                        foreach (char dof in boneRotDof)
                        {
                            if (!handlerDof.Contains(dof))
                            {
                                cost++;
                            }
                            else
                            {
                                handlerDof.Remove(dof);
                            }
                        }

                        if (cost < rotCost)
                            rotCost = cost;
                    }
                }
            }


           foreach (char dof in bone.loc_DoF)
           {
               if (!handler.loc_DoF.Contains(dof))
               {
                   locCost++;
               }
                
           }

           return (rotCost + locCost) / (bone.rot_DoF.Count + bone.loc_DoF.Count) * MAX_COST;
*/

/* 
// Version 2

            if (bone.rot_DoF.Count > 0)
            {           
                rotCost = bone.rot_DoF.Count + handler.rot_DoF.Count;
                
                if (bone.rot_DoF.Count <= handler.rot_DoF.Count)
                {                    
                    foreach (List<char> alternativeDof in dictionary[GetDofString(bone.rot_DoF)])
                    {
                        int cost = 0;

                        List<char> handlerDof = handler.rot_DoF.ToList();
                        List<char> boneDof = alternativeDof.ToList();

                        foreach (char dof in alternativeDof)
                        {
                            if (handlerDof.Contains(dof))
                            {
                                handlerDof.Remove(dof);
                                boneDof.Remove(dof);
                            }
                        }

                        cost = handlerDof.Count + boneDof.Count;
                        
                        if (cost < rotCost)
                            rotCost = cost;
                    }                                    
                }

                rotCost = rotCost / (bone.rot_DoF.Count + handler.rot_DoF.Count) * MAX_COST;
            }

            if (bone.loc_DoF.Count > 0)
            {                
                locCost = bone.loc_DoF.Count + handler.loc_DoF.Count;
                if (bone.loc_DoF.Count <= handler.loc_DoF.Count)
                {
                    int cost = 0;
                    List<char> handlerDof = handler.loc_DoF.ToList();
                    List<char> boneDof = bone.loc_DoF.ToList();

                    foreach (char dof in bone.loc_DoF)
                    {
                        if (handlerDof.Contains(dof))
                        {
                            handlerDof.Remove(dof);
                            boneDof.Remove(dof);
                        }
                    }

                    cost = handlerDof.Count + boneDof.Count;

                    if (cost < locCost)
                        locCost = cost;                    
                }

                locCost = locCost / (bone.loc_DoF.Count + handler.loc_DoF.Count) * MAX_COST;
            }

            return (rotCost + locCost) / 2;
*/
        }


        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        



        public static float LocDoFSimilarityScore(Bone locBone, Bone handler)
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

        public static float SymmetryScore(Bone bone, Bone handler)
        {
            if (bone.name.Contains(".R") || bone.name.Contains(".L"))
            {
                float cost = 0;
                if (bone.name.Contains(".R"))
                    cost += MAX_COST / 2;
                if (handler.name.Contains(".R"))
                    cost -= MAX_COST / 2;

                if (bone.name.Contains(".L"))
                    cost -= MAX_COST;
                if (handler.name.Contains(".L"))
                    cost += MAX_COST;
                return Math.Abs(cost);
            }
            else return 0;
        }

        public static float RotDofSimilarityScore(Bone bone, Bone handler)
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

        public static int RotDofDifferenceScore(List<Bone> partition, List<char> motorDecomposition, Dictionary<string, List<List<char>>> dictionary, ref bool[] dofUsed)
        {
            // Degrees of fredom of bones which belong to the current partition
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
                // padding not needed
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
                    var result = Combinatorics.GetDispositions(list, partitionDoF[0].Count);

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
            bool[] currentDofUsed = new bool[motorDecomposition.Count];
            bool[] bestDofUsed = new bool[motorDecomposition.Count];

            foreach (List<char> item in partitionDoFPadded)
            {
                //Initializes curreDofUsed
                for (int i = 0; i < currentDofUsed.Length; i++)
                    currentDofUsed[i] = false;

                int tempScore = 0;
                int dofCovered = 0;
                for (int j = 0; j < item.Count; j++)
                {
                    // Estimates dof proximity
                    if (item[j] == 0) 
                    {
                        if (dofCovered > 0 && dofCovered < partitionDoF[0].Count)
                        { 
                            tempScore += 1; continue;
                        }
                    }
                    if (item[j] == 'x' && motorDecomposition[j] == 'x') { dofCovered++; currentDofUsed[j] = true; continue; }
                    if (item[j] == 'x' && motorDecomposition[j] == 'y') { tempScore += 2; dofCovered++; currentDofUsed[j] = true; continue; }
                    if (item[j] == 'x' && motorDecomposition[j] == 'z') { tempScore += 1; dofCovered++; currentDofUsed[j] = true; continue; }
                    if (item[j] == 'y' && motorDecomposition[j] == 'y') { dofCovered++; currentDofUsed[j] = true; continue; }
                    if (item[j] == 'y' && motorDecomposition[j] == 'x') { tempScore += 2; dofCovered++; currentDofUsed[j] = true; continue; }
                    if (item[j] == 'y' && motorDecomposition[j] == 'z') { tempScore += 2; dofCovered++; currentDofUsed[j] = true; continue; }
                    if (item[j] == 'z' && motorDecomposition[j] == 'z') { dofCovered++; currentDofUsed[j] = true; continue; }
                    if (item[j] == 'z' && motorDecomposition[j] == 'x') { tempScore += 1; dofCovered++; currentDofUsed[j] = true; continue; }
                    if (item[j] == 'z' && motorDecomposition[j] == 'y') { tempScore += 2; dofCovered++; currentDofUsed[j] = true; continue; }

                }

                if (tempScore < minScore)
                {
                    minScore = tempScore;
                    for (int index = 0; index < bestDofUsed.Length; index++)
                    {
                        bestDofUsed[index] = currentDofUsed[index];
                    }

                }
            }


            for (int i = 0; i < bestDofUsed.Length; i++)
                if (bestDofUsed[i])
                    dofUsed[i] = true;
            
            return minScore;
        }

        public static float ComponentRequiredScore(List<char> motorDecomposition, bool useSensor, Brick brick)
        {
            List<string> tuiPieces = AutomaticMapping.GetTuiComponentList(useSensor, brick);
            float[,] costsMatrix = new float[motorDecomposition.Count, tuiPieces.Count];
            // initialize costsMatrix
            for (int row = 0; row < motorDecomposition.Count; row++)
            {
                for (int col = 0; col < tuiPieces.Count; col++)
                {
                    costsMatrix[row, col] = GetAnnoyanceCost("ROT(" + motorDecomposition[row].ToString() + ")", tuiPieces[col]);
                }
            }

            int[] assignment = HungarianAlgorithm.FindAssignments(costsMatrix);

            float totalCost = ComputeCostAssignment(costsMatrix, assignment);

            return totalCost / (motorDecomposition.Count * MAX_COST) * MAX_COST;
        }        

        public static float[,] NodeSimilarityScore(List<Bone> partition, List<Bone> virtualArmature)
        {                        
            var graphControlledArmature = AutomaticMapping.CreateUndirectedGraph(partition);
            var graphVirtualArmature = AutomaticMapping.CreateUndirectedGraph(virtualArmature);

            int[,] A = Matrix.GetAdjacencyMatrix(partition, graphControlledArmature);
            int[,] B = Matrix.GetAdjacencyMatrix(virtualArmature, graphVirtualArmature);                

            int[,] AT = Matrix.TransposeMatrix(A);
            int[,] BT = Matrix.TransposeMatrix(B);

            int[,] DAs = Matrix.GetSourceDiagonalMatrix(partition, graphControlledArmature);
            int[,] DAt = Matrix.GetTerminalDiagonalMatrix(partition, graphControlledArmature);

            int[,] DBs = Matrix.GetSourceDiagonalMatrix(virtualArmature, graphVirtualArmature);
            int[,] DBt = Matrix.GetTerminalDiagonalMatrix(virtualArmature, graphVirtualArmature);

            // Matrices taken from article: "Graph similarity scoring and matching" 
            // http://www.sciencedirect.com/science/article/pii/S0893965907001012
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

            /*
            Matrix.PrintMatrix(A, "A");
            Matrix.PrintMatrix(B, "B");
            Matrix.PrintMatrix(AT, "AT");
            Matrix.PrintMatrix(BT, "BT");
            Matrix.PrintMatrix(DAs, "DAs");
            Matrix.PrintMatrix(DAt, "DAt");
            Matrix.PrintMatrix(DBs, "DBs");
            Matrix.PrintMatrix(DBt, "DBt");
            */

            int[,] kroneckerAxB = Matrix.KroneckerProduct(A, B);
            //Matrix.PrintMatrix(kroneckerAxB, "kroneckerAxB");

            int[,] kroneckerATxBT = Matrix.KroneckerProduct(AT, BT);
            //Matrix.PrintMatrix(kroneckerATxBT, "kroneckerATxBT");

            int[,] kroneckerDAsxDBs = Matrix.KroneckerProduct(DAs, DBs);
            //Matrix.PrintMatrix(kroneckerDAsxDBs, "kroneckerDAsxDBs");

            int[,] kroneckerDAtxDBt = Matrix.KroneckerProduct(DAt, DBt);
            //Matrix.PrintMatrix(kroneckerDAtxDBt, "kroneckerDAtxDBt");

            int[,] MatricesSummation = Matrix.ComputeMatricesSummation
                (new List<int[,]>() { kroneckerAxB, kroneckerATxBT, kroneckerDAsxDBs, kroneckerDAtxDBt });
            //Matrix.PrintMatrix(MatricesSummation, "MatricesSummation");


            // Start iterating procedure
            double[] costVector = Matrix.GetAllOneVector(MatricesSummation.GetLength(0));


            for (int step = 0; step < 15; step++)
            {
                costVector = Matrix.Product(MatricesSummation, costVector);
                costVector = Matrix.NormalizeVector(costVector);
                //Console.WriteLine(" ===========================================");
                //Console.WriteLine("  STEP n." + step);
                // DEBUG TEST : LOG
                // Matrix.VectorToCostMatrix(costVector, B.GetLength(0), A.GetLength(0), 10);   
            }

            /*
            Console.WriteLine("VIRTUAL ARMATURE COMPONENTS");
            for (int i = 0; i < virtualArmature.Count; i++)
            {
                Console.Write("[" + i + "] = " + virtualArmature[i].name + "; ");
            }
            Console.WriteLine("\nBLENDER BONES");
            for (int i = 0; i < partition.Count; i++)
            {
                Console.Write("[" + i + "] = " + partition[i].name + "; ");
            }             
            */

            float[,] costMatrix = Matrix.VectorToCostMatrix(costVector, B.GetLength(0), A.GetLength(0), 10);

            if (A.GetLength(0) == B.GetLength(0) &&  A.GetLength(1) == B.GetLength(1) && 
                graphControlledArmature.EdgeCount == graphVirtualArmature.EdgeCount)

                GraphComparison(costMatrix, partition, virtualArmature);

                
            return costMatrix;
            
        }

        private static void GraphComparison(float[,] costMatrix, List<Bone> partition, List<Bone> virtualArmature)
        {

            for (int row = 0; row < costMatrix.GetLength(0); row++)
            {
                float[] scores = Matrix.GetRow(costMatrix, row);
                float min = scores.Min();

                for (int col = 0; col < scores.Length; col++)
                {
                    if (scores[col] <= min &&
                        partition[row].level == virtualArmature[col].level &&
                        partition[row].children.Count == virtualArmature[col].children.Count)
                    {
                        costMatrix[row, col] = 0;
                    }
                }
            }
        }
       

        

        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        // UTILITY FUNCTIONS

        public static List<string> AssignName(List<List<string>> boneDoF, bool useSensor, Brick brick, bool useHipJoint)
        {
            List<string> result = new List<string>();
            float bestCost = float.MaxValue;
            
            List<string> tuiPieces = AutomaticMapping.GetTuiComponentList(useSensor, brick);
            if (useHipJoint)
            {
                tuiPieces.Add("Hip_NUI");
                tuiPieces.Add("Hip_NUI");
                tuiPieces.Add("Hip_NUI");
            }

            foreach (List<string> item in boneDoF)
            {
                float[,] costsMatrix = new float[item.Count, tuiPieces.Count];
                // initialize costsMatrix
                for (int row = 0; row < item.Count; row++)
                {
                    for (int col = 0; col < tuiPieces.Count; col++)
                    {
                        costsMatrix[row, col] =
                            GetAnnoyanceCost(item[row], tuiPieces[col]) +
                            GetComponentRangeCost(item[row], tuiPieces[col]);
                    }
                }

                int[] assignment = HungarianAlgorithm.FindAssignments(costsMatrix);

                float score = ComputeCostAssignment(costsMatrix, assignment);

                if (score < bestCost)
                {
                    bestCost = score;
                    result.Clear();
                    for (int ass = 0; ass < assignment.Length; ass++)
                    {
                        result.Add(tuiPieces[assignment[ass]] + ":" + item[ass]);
                    }
                }
            }            
            return result;
        }

        public static string GetDofString(List<char> rot_DoF)
        {
            string DoF = "";
            foreach (char c in rot_DoF)
            {
                DoF += c;
            }
            return DoF;
        }       

        public static int GetKinectRotRangeCost(string boneName)
        {
            if (boneName.Contains("_ROT"))
                boneName = boneName.Remove(boneName.IndexOf("_ROT"));

            if (boneName.Contains("_NUI"))
                boneName = boneName.Remove(boneName.IndexOf("_NUI"));

            int cost = 0;
            
            switch (boneName)
            {
                case "Elbow.R":
                case "Elbow.L":
                case "Wrist.R":
                case "Wrist.L":
                    cost = 0;
                    break;                
                

                case "Head":
                case "Hip":
                    cost = 2;
                    break;

                case "Shoulder.R":
                case "Shoulder.L":                    
                case "Hand.R":
                case "Hand.L":
                case "Knee.R":
                case "Knee.L":
                case "Ankle.R":
                case "Ankle.L":
                    cost = 3;
                    break;               

                case "Hip.R":
                case "Hip.L":                
                case "Foot.R":
                case "Foot.L":
                    cost = 4;
                break;

            }
            
            return cost;
        }

        public static List<Bone> DecomposeHandler(Bone handler)
        {
            List<Bone> result = new List<Bone>();
            List<string> componentsName =
                handler.name.Split(new string[] { " | " }, StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (string name in componentsName)
            {
                Bone comp = new Bone(name);
                comp.level = handler.level;
                comp.parent = handler.parent;
                comp.children = handler.children.ToList();
                if (name.Contains(":ROT("))
                {
                    comp.rot_DoF.Add(name[name.IndexOf(":ROT(") + 5]);
                }
                if (name.Contains(":LOC("))
                {
                    comp.loc_DoF.Add(name[name.IndexOf(":LOC(") + 5]);
                }
                if (comp.loc_DoF.Count + comp.rot_DoF.Count == 0)
                {
                    comp.rot_DoF = handler.rot_DoF.ToList();
                    comp.loc_DoF = handler.loc_DoF.ToList();
                }
                result.Add(comp);
            }

            return result;
        }

        public static int GetAnnoyanceCost(string DoF, string ComponentType)
        {

            // Rot mapped on Tui component
            if ((DoF.Equals("ROT(x)") || DoF.Equals("ROT(z)")) &&
                ComponentType.Contains(DeviceType.LMotor.ToString()))
                //return 0;
                return 0;
            if ((DoF.Equals("ROT(x)") || DoF.Equals("ROT(z)")) &&
                ComponentType.Contains(DeviceType.MMotor.ToString()))
                //return 1;
                return MAX_COST / 5;
            if ((DoF.Equals("ROT(x)") || DoF.Equals("ROT(z)")) &&
                ComponentType.Contains(DeviceType.Gyroscope.ToString()))
                //return 2;
                return MAX_COST / 5 * 4;
            if ((DoF.Equals("ROT(x)") || DoF.Equals("ROT(z)")) &&
                ComponentType.Contains(DeviceType.Ultrasonic.ToString()))
                //return 5;
                return MAX_COST;

            // Rot mapped on Tui component
            if (DoF.Equals("ROT(y)") && ComponentType.Contains(DeviceType.MMotor.ToString()))
                //return 0;
                return 0;
            if (DoF.Equals("ROT(y)") && ComponentType.Contains(DeviceType.LMotor.ToString()))
                //return 1;
                return MAX_COST / 5;
            if (DoF.Equals("ROT(y)") && ComponentType.Contains(DeviceType.Gyroscope.ToString()))
                //return 2;
                return MAX_COST / 5 * 4;
            if (DoF.Equals("ROT(y)") && ComponentType.Contains(DeviceType.Ultrasonic.ToString()))
                //return 5;
                return MAX_COST;

            // Loc mapped on Tui component
            if ((DoF.Equals("LOC(x)") || DoF.Equals("LOC(y)") || DoF.Equals("LOC(z)")) &&
                ComponentType.Contains(DeviceType.LMotor.ToString()))
                return MAX_COST;

            if ((DoF.Equals("LOC(x)") || DoF.Equals("LOC(y)") || DoF.Equals("LOC(z)")) &&
                ComponentType.Contains(DeviceType.MMotor.ToString()))
                return MAX_COST;

            if ((DoF.Equals("LOC(x)") || DoF.Equals("LOC(y)") || DoF.Equals("LOC(z)")) &&
                ComponentType.Contains(DeviceType.Gyroscope.ToString()))
                return MAX_COST;

            if ((DoF.Equals("LOC(x)") || DoF.Equals("LOC(y)") || DoF.Equals("LOC(z)")) &&
                ComponentType.Contains(DeviceType.Ultrasonic.ToString()))
                return 0;


            // Rot mapped on Kinect joints
            if ((DoF.Equals("ROT(x)") || DoF.Equals("ROT(y)") || DoF.Equals("ROT(z)")) &&
                ComponentType.Contains("_NUI"))
                return MAX_COST;

            // Loc mapped on Kinect joints
            if ((DoF.Equals("LOC(x)") || DoF.Equals("LOC(y)") || DoF.Equals("LOC(z)")) &&
                ComponentType.Contains("_NUI"))
                return 0;

            return MAX_COST;
        }

        /// <summary>
        /// cost = 4m(Max Distance read from Kinect specification) - range of the sensor
        /// range is calculated considering that the animator is at a distance of 3.5 meters from the kinect 
        /// Kinect spec : https://msdn.microsoft.com/en-us/library/hh973074.aspx
        /// Kinect spec : https://msdn.microsoft.com/en-us/library/jj131033.aspx
        /// Lego spec : Lego User Manual 
        /// </summary>
        /// 
        /// <param name="handler"> Bone representive component: 
        /// 1. TUI => Gyroscope(PORT-ONE); 
        /// 2. NUI => Hip_LOC(X) 
        /// </param>
        /// 
        /// <returns></returns>
        public static float GetComponentRangeCost(string DoF, string ComponentType)
        {
            float cost = MAX_COST;

            if (ComponentType.Contains("_TUI"))
            {
                string componentName = ComponentType.Substring(0, ComponentType.IndexOf("(PORT-"));
                switch (componentName)
                {
                    case "Gyroscope":
                    case "MMotor":
                    case "LMotor":
                        cost = 0;
                        break;

                    case "Ultrasonic":
                        cost = MAX_COST / 4 * (4 - 2.55f);
                        break;
                }
            }
            else if(ComponentType.Contains("_NUI"))
            {
                if (DoF.Contains("LOC(x)"))
                    cost = MAX_COST / 4 * (4 - 3.80f);

                if (DoF.Contains("LOC(y)"))
                {
                    List<List<Bone>> kinectSkeleton = KinectSkeleton.GetKinectPartition();

                    if (kinectSkeleton[0].Contains(new Bone(ComponentType)) ||
                        kinectSkeleton[1].Contains(new Bone(ComponentType)))
                        cost = MAX_COST / 4 * (4 - 2.79f);
                    if (kinectSkeleton[2].Contains(new Bone(ComponentType)))
                        cost = MAX_COST / 4 * (4 - 0.90f); // average distance between feets and hip
                    if (kinectSkeleton[3].Contains(new Bone(ComponentType)) ||
                        kinectSkeleton[4].Contains(new Bone(ComponentType)))
                        cost = MAX_COST / 4 * (4 - 0.45f);
                }
                if (DoF.Contains("LOC(z)"))
                    cost = MAX_COST / 4 * (4 - 3.2f);

                if (DoF.Contains("ROT("))
                    cost = MAX_COST / 4 * GetKinectRotRangeCost(ComponentType);
            }
            return cost;
        }
        
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
      
        //public static AxisArrangement GetBestAxisArrangement(int motors, Dictionary<string, List<List<char>>> dictionary, List<List<Bone>> currentPartition, char[] comb, bool useSensor, Brick brick)
        //{
            
        //    // creates possible configurations 
        //    List<Bone> motorDecomposition = AutomaticMapping.DecomposeMotorCombination(motors, comb, useSensor, brick);
            
        //    // solves assignment problem with Hungarian Algorithm                                        
        //    float[,] costsMatrix = new float[currentPartition.Count, motorDecomposition.Count * currentPartition.Count];

        //    // initialize costMatrix
        //    for (int row = 0; row < currentPartition.Count; row++)
        //    {
        //        for (int col = 0; col < motorDecomposition.Count * currentPartition.Count;
        //            col += currentPartition.Count)
        //        {
        //            // currentPartition[row] -> 
        //            // motorDecomposition[col/currentPartition.Count].rot_DoF ->
        //            //computes cost from :
        //            //  DoF difference +
        //            int compIndex = col / currentPartition.Count;

        //            float cost =
        //                RotDofDifferenceScore(currentPartition[row], motorDecomposition[compIndex].rot_DoF, dictionary) +
        //                ComponentRequiredScore(motorDecomposition[compIndex].rot_DoF, useSensor, brick);
                        

        //            for (int index = 0; index < currentPartition.Count; index++)
        //            {
        //                costsMatrix[row, col + index] = cost;
        //            }

        //        }
        //    }


        //    int[] assignment = HungarianAlgorithm.FindAssignments(costsMatrix);

        //    float totalCost = 0;
        //    // computes cost for this assignment
        //    for (int ass = 0; ass < assignment.Length; ass++)
        //    {
        //        totalCost += costsMatrix[ass, assignment[ass]];
        //    }

        //    return new AxisArrangement(GetDofString(comb.ToList()), comb, totalCost);
                
        //    /* return 
        //     * new PartitionAssignment(GetDofString(comb.ToList()), assignment, currentPartition, motorDecomposition, totalCost)*/
        //}

        public static AxisArrangement GetBestAxisArrangement(int motors, Dictionary<string, List<List<char>>> dictionary, List<List<Bone>> currentPartition, char[] comb, bool useSensor, Brick brick)
        {                                                               

            List<char> combToReturn = new List<char>();           
            bool[] dofUsed = new bool[comb.Length];
        
            float cost = 0;
            // initialize costMatrix
            for (int i = 0; i < currentPartition.Count; i++)
            {
                cost += RotDofDifferenceScore(currentPartition[i], comb.ToList(), dictionary, ref dofUsed);                
            }

            for (int i = 0; i < dofUsed.Length; i++)
            {
                if (dofUsed[i])
                    combToReturn.Add(comb[i]);
            }


            // DEBUG
            return new AxisArrangement(combToReturn.ToArray(), cost);            
        }

        public static PartitionAssignment ComputeAssignement(List<Bone> partition, List<Bone> virtualArmature, int maxLenghtChain, Dictionary<string, List<List<char>>> dictionary, string configurationName)
        {

            // solves assignment problem with Hungarian Algorithm                                        
            //float[,] costsMatrix = new float[partition.Count, virtualArmature.Count];

            // Computes node similarity
            float[,] costsMatrix = Metrics.NodeSimilarityScore(partition, virtualArmature);

            // Defines the costs
            for (int row = 0; row < partition.Count; row++)
            {
                for (int col = 0; col < virtualArmature.Count; col++)
                {
                    costsMatrix[row, col] +=
                        Metrics.DofCoverageScore(partition[row], virtualArmature[col], dictionary) +
                        Metrics.ComponentRangeScore(partition[row], virtualArmature[col]) +
                        Metrics.ComponentAnnoyanceScore(partition[row], virtualArmature[col]) +
                        Metrics.ChainLengthScore(partition[row], virtualArmature[col], maxLenghtChain) +
                        Metrics.SymmetryScore(partition[row], virtualArmature[col]);
                }
            }

            int[] assignment = HungarianAlgorithm.FindAssignments(costsMatrix);

            float score = ComputeCostAssignment(costsMatrix, assignment);

            PartitionAssignment result =
                new PartitionAssignment(configurationName, assignment, partition, virtualArmature, score);

            /*
            if (AutomaticMapping.KinectAssignmentConsistency(result))
            {
                return result;
            }
            else
            {
                return null;
            }
            */
            return result;
        }

        public static float ComputeCostAssignment(float[,] costsMatrix, int[] assignment)
        {
            float score = 0;
            for (int ass = 0; ass < assignment.Length; ass++)
            {
                score += costsMatrix[ass, assignment[ass]];
            }
            return score;
        }
        
    }
}
