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
            
            float rotCost = 0;
            float locCost = 0;

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
/*

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

       
        public static KinectPieces ChainSimilarityScore(List<Bone> subPartition, List<Bone> kinectPartition, int maxLength)
        {

            // solves assignment problem with Hungarian Algorithm                                        
            float[,] costsMatrix = new float[subPartition.Count, kinectPartition.Count];

            // initialize costMatrix
            for (int row = 0; row < subPartition.Count; row++)
            {
                for (int col = 0; col < kinectPartition.Count; col++)
                {
                    costsMatrix[row, col] =
                        RotDofSimilarityScore(subPartition[row], kinectPartition[col]) +
                        SymmetryScore(subPartition[row], kinectPartition[col]) +
                        DofAnalysisScore(subPartition[row], kinectPartition[col], maxLength);
                    /*Math.Abs(subPartition[row].level - kinectPartition[col].level) + 
                    GetCostRange(kinectPartition[col]);*/
                }
            }


            int[] assignment = HungarianAlgorithm.FindAssignments(costsMatrix);

            Bone kinectBone = new Bone("");
            float totalCost = 0;

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

        public static int RotDofDifferenceScore(List<Bone> partition, List<char> motorDecomposition, Dictionary<string, List<List<char>>> dictionary)
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

            //return minScore / (partitionDoFPadded.Count*2) * MAX_COST;
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

            float totalCost = 0;
            // computes cost for this assignment
            for (int ass = 0; ass < assignment.Length; ass++)
            {
                totalCost += costsMatrix[ass, assignment[ass]];
            }

            return totalCost / (motorDecomposition.Count * MAX_COST) * MAX_COST;
        }        

        public static float[,] NodeSimilarityScore(List<Bone> partition, List<Bone> virtualArmature)
        {
            var graphVirtualArmature = AutomaticMapping.CreateUndirectedGraph(virtualArmature);
            var graphControlledArmature = AutomaticMapping.CreateUndirectedGraph(partition);

            int[,] A = Matrix.GetAdjacencyMatrix(partition, graphControlledArmature);
            int[,] B = Matrix.GetAdjacencyMatrix(virtualArmature, graphVirtualArmature);

            int[,] AT = Matrix.TransposeMatrix(A);
            int[,] BT = Matrix.TransposeMatrix(B);

            int[,] DAs = Matrix.GetSourceDiagonalMatrix(partition, graphControlledArmature);
            int[,] DAt = Matrix.GetTerminalDiagonalMatrix(partition, graphControlledArmature);
            
            int[,] DBs = Matrix.GetSourceDiagonalMatrix(virtualArmature, graphVirtualArmature);
            int[,] DBt = Matrix.GetTerminalDiagonalMatrix(virtualArmature, graphVirtualArmature);

            /*
            
            Matrices taken from article "Graph similarity scoring and matching" 
            http://www.sciencedirect.com/science/article/pii/S0893965907001012
             
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

            //Matrix.PrintMatrix(A, "A");
            //Matrix.PrintMatrix(B, "B");
            //Matrix.PrintMatrix(AT, "AT");
            //Matrix.PrintMatrix(BT, "BT");
            //Matrix.PrintMatrix(DAs, "DAs");
            //Matrix.PrintMatrix(DAt, "DAt");
            //Matrix.PrintMatrix(DBs, "DBs");
            //Matrix.PrintMatrix(DBt, "DBt");

            int[,] kroneckerAxB = Matrix.KroneckerProduct(A, B);
            //Matrix.PrintMatrix(kroneckerAxB, "kroneckerAxB");

            int[,] kroneckerATxBT = Matrix.KroneckerProduct(AT, BT);
            //Matrix.PrintMatrix(kroneckerATxBT, "kroneckerATxBT");

            int[,] kroneckerDAsxDBs = Matrix.KroneckerProduct(DAs, DBs);
            //Matrix.PrintMatrix(kroneckerDAsxDBs, "kroneckerDAsxDBs");

            int[,] kroneckerDAtxDBt = Matrix.KroneckerProduct(DAt, DBt);
            //Matrix.PrintMatrix(kroneckerDAtxDBt, "kroneckerDAtxDBt");

            int[,] MatricesSummation = Matrix.ComputeMatricesSummation
                (new List<int[,]>() { kroneckerAxB, kroneckerATxBT/*, kroneckerDAsxDBs, kroneckerDAtxDBt*/ });
            //Matrix.PrintMatrix(MatricesSummation, "MatricesSummation");


            // Start iterating procedure
            double[] costVector = Matrix.GetAllOneVector(MatricesSummation.GetLength(0));


            for (int step = 0; step < 30; step++)
            {
                costVector = Matrix.Product(MatricesSummation, costVector);
                costVector = Matrix.NormalizeVector(costVector);
                //Console.WriteLine(" ===========================================");
                //Console.WriteLine("  STEP n." + step);
                // DEBUG TEST : LOG
                // Matrix.VectorToCostMatrix(costVector, B.GetLength(0), A.GetLength(0), 10);   
            }

            /*
            Console.WriteLine("VIRTUAL ARMATURE COMPONENT");
            for (int i = 0; i < virtualArmature.Count; i++)
            {
                Console.Write("[" + i + "] = " + virtualArmature[i].name + "; ");
            }
            Console.WriteLine("\nBLENDER BONE");
            for (int i = 0; i < partition.Count; i++)
            {
                Console.Write("[" + i + "] = " + partition[i].name + "; ");
            }             
            */

            float[,] costMatrix = Matrix.VectorToCostMatrix(costVector, B.GetLength(0), A.GetLength(0), 10);
            return costMatrix;
            
        }

        public static float DofAnalysisScore(Bone bone, Bone handler, int maxLengthChain)
        {
            float rotCost = 0;
            float locCost = 0;
            if (bone.rot_DoF.Count > 0)
            {
                foreach (Bone oneDofHandler in AutomaticMapping.GetOneDofBones(new List<Bone>() { handler }, true, false))
                {
                    rotCost += /*ComponentRangeCost(oneDofHandler) / 2 + */ChainLengthScore(bone, oneDofHandler, maxLengthChain) / 2;
                }
            }
            if (bone.loc_DoF.Count > 0)
            {
                foreach (Bone oneDofHandler in AutomaticMapping.GetOneDofBones(new List<Bone>() { handler }, false, true))
                {
                    locCost +=/* ComponentRangeCost(oneDofHandler) / 2 +*/ ChainLengthScore(bone, oneDofHandler, maxLengthChain) / 2;
                }
            }
            float rotWorseCase = MAX_COST * 3;
            float locWorseCase = MAX_COST * 3;
            return (rotCost / rotWorseCase + locCost / locWorseCase) / 2 * MAX_COST;
        }



        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        // UTILITY FUNCTIONS

        public static List<string> AssignName(List<string> boneDoF, bool useSensor, Brick brick, bool useHipJoint)
        {
            List<string> result = new List<string>();
            List<string> tuiPieces = AutomaticMapping.GetTuiComponentList(useSensor, brick);
            if (useHipJoint)
            {
                tuiPieces.Add("Hip_NUI");
                tuiPieces.Add("Hip_NUI");
                tuiPieces.Add("Hip_NUI");
            }

            float[,] costsMatrix = new float[boneDoF.Count, tuiPieces.Count];
            // initialize costsMatrix
            for (int row = 0; row < boneDoF.Count; row++)
            {
                for (int col = 0; col < tuiPieces.Count; col++)
                {
                    costsMatrix[row, col] =
                        GetAnnoyanceCost(boneDoF[row], tuiPieces[col]) +
                        GetComponentRangeCost(boneDoF[row], tuiPieces[col]);
                }
            }

            int[] assignment = HungarianAlgorithm.FindAssignments(costsMatrix);

            for (int ass = 0; ass < assignment.Length; ass++)
            {
                result.Add(tuiPieces[assignment[ass]] + ":" + boneDoF[ass]);
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

                case "Head":
                    cost = 3;
                    break;

                case "Hip":
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
                return MAX_COST / 5 * 2;
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
                return MAX_COST / 5 * 2;
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
                    List<List<Bone>> kinectSkeleton = AutomaticMapping.GetKinectPartition();

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
                    cost = MAX_COST / 4 * (4 - 2.8f);

                if (DoF.Contains("ROT("))
                    cost = MAX_COST / 5 * GetKinectRotRangeCost(ComponentType);
            }
            return cost;
        }
        
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =







        public static PartitionAssignment ComputeKinectAssignmentScore(Dictionary<string, List<List<char>>> dictionary, List<List<Bone>> currentPartition, int maxLengthChain)
        {

            List<List<Bone>> kinectSkeleton = AutomaticMapping.GetKinectPartition();
            List<Bone> motorDecomposition = new List<Bone>();

            // solves assignment problem with Hungarian Algorithm                                        
            float[,] costsMatrix = new float[currentPartition.Count, kinectSkeleton.Count * currentPartition.Count];
            // initialize costMatrix
            for (int row = 0; row < currentPartition.Count; row++)
            {
                for (int col = 0; col < kinectSkeleton.Count * currentPartition.Count;
                    col += currentPartition.Count)
                {
                    KinectPieces kinectBones = ChainSimilarityScore(currentPartition[row], kinectSkeleton[col / currentPartition.Count], maxLengthChain);
                    float cost = kinectBones.Score /*+ currentPartition.Count*/;
                    motorDecomposition.Add(kinectBones.Bones);

                    for (int index = 0; index < currentPartition.Count; index++)
                    {
                        costsMatrix[row, col + index] = cost;
                    }

                }
            }


            int[] assignment = HungarianAlgorithm.FindAssignments(costsMatrix);

            float totalCost = 0;
            // computes cost for this assignment
            for (int ass = 0; ass < assignment.Length; ass++)
            {
                totalCost += costsMatrix[ass, assignment[ass]];
                assignment[ass] = assignment[ass] % currentPartition.Count + currentPartition.Count * ass;
            }

            return null/*new PartitionAssignment("Kinect_Configuration", assignment, currentPartition, motorDecomposition, totalCost)*/;
        }

        public static AxisArrangement GetBestAxisArrangement(int motors, Dictionary<string, List<List<char>>> dictionary, List<List<Bone>> currentPartition, char[] comb, bool useSensor, Brick brick)
        {
            // creates possible configurations 
            List<Bone> motorDecomposition = AutomaticMapping.DecomposeMotorCombination(motors, comb, useSensor, brick);

            // solves assignment problem with Hungarian Algorithm                                        
            float[,] costsMatrix = new float[currentPartition.Count, motorDecomposition.Count * currentPartition.Count];

            // initialize costMatrix
            for (int row = 0; row < currentPartition.Count; row++)
            {
                for (int col = 0; col < motorDecomposition.Count * currentPartition.Count;
                    col += currentPartition.Count)
                {
                    // currentPartition[row] -> 
                    // motorDecomposition[col/currentPartition.Count].rot_DoF ->
                    //computes cost from :
                    //  DoF difference +
                    int compIndex = col / currentPartition.Count;

                    float cost =
                        RotDofDifferenceScore(currentPartition[row], motorDecomposition[compIndex].rot_DoF, dictionary) +
                        ComponentRequiredScore(motorDecomposition[compIndex].rot_DoF, useSensor, brick);

                    for (int index = 0; index < currentPartition.Count; index++)
                    {
                        costsMatrix[row, col + index] = cost;
                    }

                }
            }


            int[] assignment = HungarianAlgorithm.FindAssignments(costsMatrix);

            float totalCost = 0;
            // computes cost for this assignment
            for (int ass = 0; ass < assignment.Length; ass++)
            {
                totalCost += costsMatrix[ass, assignment[ass]];
            }

            return new AxisArrangement(GetDofString(comb.ToList()), comb, totalCost);
                
            /* return 
             * new PartitionAssignment(GetDofString(comb.ToList()), assignment, currentPartition, motorDecomposition, totalCost)*/
        }

        public static List<PartitionAssignment> ComputeLocAssignment(List<Bone> locBones, List<Bone> handler, int maxLengthChain/*, bool leftHanded*/)
        {
            List<PartitionAssignment> arrangements = new List<PartitionAssignment>();
            foreach (Bone bone in locBones)
            {
                List<Bone> locBoneOneDof = AutomaticMapping.GetOneDofBones(locBones, false, true);

                // solves assignment problem with Hungarian Algorithm                                        
                float[,] costsMatrix = new float[locBoneOneDof.Count, handler.Count];
                // initialize costMatrix
                for (int row = 0; row < locBoneOneDof.Count; row++)
                {
                    for (int col = 0; col < handler.Count; col += locBones.Count)
                    {
                        costsMatrix[row, col] = Metrics.LocDoFSimilarityScore(locBoneOneDof[row], handler[col]) /*+
                            Metrics.ComponentRangeCost(handler[col])*/ +
                            Metrics.SymmetryScore(locBoneOneDof[row], handler[col / locBones.Count]) +
                            Metrics.ChainLengthScore(locBoneOneDof[row], handler[col], maxLengthChain)/* +
                            UserPreferenceScore(handler[col], leftHanded)*/;
                    }
                }

                int[] assignment = HungarianAlgorithm.FindAssignments(costsMatrix);

                float score = 0;
                for (int ass = 0; ass < assignment.Length; ass++)
                {
                    score += costsMatrix[ass, assignment[ass]];
                }

                List<List<Bone>> partition = new List<List<Bone>>();
                partition.Add(locBoneOneDof);
                arrangements.Add(null/*new PartitionAssignment(bone.name, assignment, partition, handler, score)*/);
            }
            return arrangements;
        }

        public static PartitionAssignment ComputeLocRotKinectAssignement(List<Bone> uniquePartition, int maxLengthChain)
        {
            List<Bone> kinectSkeleton = new List<Bone>();
            foreach (List<Bone> lb in AutomaticMapping.GetKinectPartition())
                foreach (Bone b in lb)
                    kinectSkeleton.Add(b);

            // solves assignment problem with Hungarian Algorithm                                        
            //float[,] costsMatrix = NodeEdgeSimilarity(uniquePartition, kinectSkeleton);
            float[,] costsMatrix = new float[uniquePartition.Count, kinectSkeleton.Count];

            // initialize costMatrix
            for (int row = 0; row < uniquePartition.Count; row++)
            {
                for (int col = 0; col < kinectSkeleton.Count; col++)
                {
                    costsMatrix[row, col] +=
                        Metrics.RotDofSimilarityScore(uniquePartition[row], kinectSkeleton[col]) +
                        Metrics.LocDoFSimilarityScore(uniquePartition[row], kinectSkeleton[col]) +
                        Metrics.DofAnalysisScore(uniquePartition[row], kinectSkeleton[col], maxLengthChain);

                    if (uniquePartition[row].name.Contains(".R") || uniquePartition[row].name.Contains(".L"))
                        costsMatrix[row, col] += Metrics.SymmetryScore(uniquePartition[row], kinectSkeleton[col]);
                }
            }


            int[] assignment = HungarianAlgorithm.FindAssignments(costsMatrix);

            float score = 0;
            for (int ass = 0; ass < assignment.Length; ass++)
            {
                score += costsMatrix[ass, assignment[ass]];
            }

            List<List<Bone>> partition = new List<List<Bone>>();
            partition.Add(uniquePartition);
            PartitionAssignment bestArrangement = null
                /*new PartitionAssignment("Kinect_Configuration", assignment, partition, kinectSkeleton, score)*/;


            // Gets oneBone-mode mapping representation

            List<Bone> oneDofBone = AutomaticMapping.GetOneDofBones(uniquePartition, true, true);
            List<Bone> oneDofHandler = AutomaticMapping.GetOneDofBones(bestArrangement.Handler, true, true);
            List<int> dofAssignament = new List<int>();

            for (int handlerIndex = 0; handlerIndex < bestArrangement.Assignment.Length; handlerIndex++)
            {
                foreach (char dof in uniquePartition[handlerIndex].rot_DoF)
                {
                    string boneToFind =
                        bestArrangement.Handler[bestArrangement.Assignment[handlerIndex]].name +
                        "_ROT(" + dof + ")";

                    dofAssignament.Add(oneDofHandler.FindIndex(x => x.name.Equals(boneToFind)));

                }
                foreach (char dof in uniquePartition[handlerIndex].loc_DoF)
                {
                    string boneToFind =
                        bestArrangement.Handler[bestArrangement.Assignment[handlerIndex]].name +
                        "_LOC(" + dof + ")";

                    dofAssignament.Add(oneDofHandler.FindIndex(x => x.name.Equals(boneToFind)));
                }

            }

            List<List<Bone>> oneDofPartition = new List<List<Bone>>();
            oneDofPartition.Add(oneDofBone);
            return null /*new PartitionAssignment("Kinect_Configuration", dofAssignament.ToArray(), oneDofPartition, oneDofHandler, bestArrangement.Score)*/;/* arrangements[0];*/

        }

        public static PartitionAssignment ComputeLocRotTuiAssignement(List<Bone> uniquePartition, Brick brick, Dictionary<string, List<List<char>>> dictionary, int maxLenghtChain)
        {

            List<Bone> handler = AutomaticMapping.GetTuiHandler(AutomaticMapping.GetTuiComponentList(true, brick));
            Bone hipCenter = new Bone("Hip");
            hipCenter.loc_DoF = new List<char>() { 'x', 'y', 'z' };
            foreach (Bone bone in AutomaticMapping.GetOneDofBones(new List<Bone>() { hipCenter }, false, true))
            {
                handler.Add(bone);
            }
            List<Bone> boneOneDof = AutomaticMapping.GetOneDofBones(uniquePartition, true, true);


            // solves assignment problem with Hungarian Algorithm                                        
            float[,] costsMatrix = new float[boneOneDof.Count, handler.Count];
            // initialize costMatrix
            for (int row = 0; row < boneOneDof.Count; row++)
            {
                for (int col = 0; col < handler.Count; col++)
                {
                    costsMatrix[row, col] =
                        Metrics.RotDofSimilarityScore(boneOneDof[row], handler[col]) +
                        Metrics.LocDoFSimilarityScore(boneOneDof[row], handler[col]) +
                        /*Metrics.ComponentRangeCost(handler[col]) +*/
                        Metrics.SymmetryScore(boneOneDof[row], handler[col]) +
                        Metrics.ChainLengthScore(boneOneDof[row], handler[col], maxLenghtChain);
                }
            }

            int[] assignment = HungarianAlgorithm.FindAssignments(costsMatrix);

            float score = 0;
            for (int ass = 0; ass < assignment.Length; ass++)
            {
                score += costsMatrix[ass, assignment[ass]];
            }

            List<List<Bone>> partition = new List<List<Bone>>();
            partition.Add(boneOneDof);
            PartitionAssignment result = null
                /*new PartitionAssignment("TuiHip_Configuration", assignment, partition, handler, score)*/;
            if (AutomaticMapping.KinectAssignmentConsistency(result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        
    }
}
