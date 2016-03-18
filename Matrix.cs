using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickGraph;

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    public static class Matrix
    {
        public static void PrintMatrix(int[,] matrix, string text)
        {
            Console.WriteLine(text);
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    Console.Write("\t" + matrix[i, j]);
                }

                Console.Write("\n");
            }

        }

        public static int[,] GetTerminalDiagonalMatrix(List<Bone> uniquePartition, UndirectedGraph<Bone, Edge<Bone>> graph)
        {
            int[,] DiagonalMatrix = new int[uniquePartition.Count, uniquePartition.Count];

            foreach (var edge in graph.Edges)
            {
                int index = uniquePartition.FindIndex(b => b.name.Equals(edge.Target.name));
                DiagonalMatrix[index, index] += 1;
            }
            return DiagonalMatrix;

        }

        public static int[,] GetSourceDiagonalMatrix(List<Bone> uniquePartition, UndirectedGraph<Bone, Edge<Bone>> graph)
        {
            int[,] DiagonalMatrix = new int[uniquePartition.Count, uniquePartition.Count];

            foreach (var edge in graph.Edges)
            {
                int index = uniquePartition.FindIndex(b => b.name.Equals(edge.Source.name));
                DiagonalMatrix[index, index] += 1;
            }
            return DiagonalMatrix;
        }

        public static int[,] TransposeMatrix(int[,] matrix)
        {
            int size = matrix.GetLength(0);
            int[,] transposeMatrix = new int[size, size];
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    transposeMatrix[j, i] = matrix[i, j];

            return transposeMatrix;
        }

        public static double[,] TransposeMatrix(double[,] matrix)
        {

            double[,] transposeMatrix = new double[matrix.GetLength(1), matrix.GetLength(0)];
            for (int i = 0; i < matrix.GetLength(0); i++)
                for (int j = 0; j < matrix.GetLength(1); j++)
                    transposeMatrix[j, i] = matrix[i, j];

            return transposeMatrix;
        }

        public static int[,] GetAdjacencyMatrix(List<Bone> uniquePartition, UndirectedGraph<Bone, Edge<Bone>> graph)
        {
            int[,] AdjacencyMatrix = new int[uniquePartition.Count, uniquePartition.Count];

            foreach (var edge in graph.Edges)
            {
                int row = uniquePartition.FindIndex(b => b.name.Equals(edge.Source.name));
                int col = uniquePartition.FindIndex(b => b.name.Equals(edge.Target.name));
                AdjacencyMatrix[row, col] = 1;
            }
            return AdjacencyMatrix;
        }

        internal static int[,] KroneckerProduct(int[,] M, int[,] N)
        {
            int[,] kroneckerProductMatrix = 
                new int[M.GetLength(0) * N.GetLength(0), M.GetLength(1) * N.GetLength(1)];
            
            for (int i = 0; i < M.GetLength(0); i++) 
            {
                for (int j = 0; j < M.GetLength(1); j++) 
                {
                    for (int k = 0; k < N.GetLength(0); k++)
                    {
                        for (int l = 0; l < N.GetLength(1); l++)
                        {
                            int row = i * N.GetLength(0) + k;
                            int col = j*N.GetLength(1) + l;
                            kroneckerProductMatrix[row, col] = M[i, j] * N[k, l];
                        }
                    }
                }  
            }
            

            return kroneckerProductMatrix;
        }

        internal static int[,] ComputeMatricesSummation(List<int[,]> matricesToSum)
        {
            int[,] result = new int [matricesToSum[0].GetLength(0),matricesToSum[0].GetLength(1)];
            
            for(int i = 0; i < matricesToSum[0].GetLength(0); i++)
            {
                for(int j = 0; j < matricesToSum[0].GetLength(1); j++)
                {
                    for (int matrix = 0; matrix < matricesToSum.Count; matrix++)
                    {
                        int[,] currentMatrix = matricesToSum[matrix];
                        result[i, j] += currentMatrix[i, j];
                    }
                }   
            }
            return result;
        }

        internal static double[] Product(int[,] MatricesSummation, double[] costVector)
        {
            double[] result = new double[MatricesSummation.GetLength(0)];

            for (int i = 0; i < costVector.Length; i++)
            {
                result[i] = 0;
                for (int j = 0; j < costVector.Length; j++) 
                {
                    result[i] += MatricesSummation[i, j] * costVector[j];
                }
            }
            return result;
        }

        internal static double[] GetAllOneVector(int size)
        {
            double[] result = new double[size];
            //Initialize cost vector to all-ones vector 
            for (int i = 0; i < size; i++)
            {
                result[i] = 1;
            }
            return result;
        }


        internal static int[,] VectorToCostMatrix(double[] costVector, int controlledBones, int kinectBones, int range)
        {        
            // verifica che il vettore viene incolonnato bene nella matrice
            double max = costVector.Max();

            double[,] matrix = new double[controlledBones, kinectBones];
            //int[,] matrix = new int[controlledBones, kinectBones];
            
            int index = 0;
            for(int col = 0; col < kinectBones; col++)
            {
                for (int row = 0; row < controlledBones; row++)
                {
                    //matrix[row, col] = range - (int)Math.Round((costVector[index] / max) * range);
                    matrix[row, col] = Math.Round(costVector[index], 3);
                    //matrix[row, col] = (int)Math.Round(costVector[index]);
                    index++;
                }
            }


            PrintCostMatrix(matrix, "COST");
            // Print Matrix
            matrix = Matrix.TransposeMatrix(matrix);
            PrintCostMatrix(matrix, "COST_TRASPOSTO");
            
            //return matrix;
            return null;
        }

        
        public static void PrintCostMatrix(double[,] matrix, string text)
        {
            // Print Matrix
            Console.WriteLine(text);
            Console.WriteLine("\t0   \t1   \t2   \t3   \t4   \t5   \t6   \t7   \t8   \t9   \t10   \t11   ");
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                Console.Write(i);
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    if (matrix[i, j] == 0)
                        Console.Write("\t" + "0   ");
                    else
                        Console.Write("\t" + matrix[i, j]);
                }
                Console.Write("\n");
            }
        }


        internal static double[] NormalizeVector(double[] vector)
        {
            
            double[] normalizedVector = new double[vector.Length];
            double sum = 0;
            for (int i = 0; i < vector.Length; i++) 
            {
                sum += Math.Pow(vector[i], 2.0);
            }
            
            double squaredSum = Math.Sqrt(sum);

            for (int i = 0; i < vector.Length; i++)
            {
                normalizedVector[i] = vector[i] / squaredSum;
            }
            return normalizedVector;
            
            /*
            double[] normalizedVector = new double[vector.Length];
            for (int i = 0; i < vector.Length; i++)
            {
                normalizedVector[i] = vector[i] / vector.Max();
            }
            return normalizedVector;
             */
        }
    }
}
