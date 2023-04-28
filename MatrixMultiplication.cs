using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Problem
{
    // *****************************************
    // DON'T CHANGE CLASS OR FUNCTION NAME
    // YOU CAN ADD FUNCTIONS IF YOU NEED TO
    // *****************************************
    public static class MatrixMultiplication
    {

        #region 

        //Your Code is Here:
        //==================
        /// <summary>
        /// Multiply 2 square matrices in an efficient way [Strassen's Method]
        /// </summary>
        /// <param name="M1">First square matrix</param>
        /// <param name="M2">Second square matrix</param>
        /// <param name="N">Dimension (power of 2)</param>
        /// <returns>Resulting square matrix</returns>
        static public int[,] MatrixMultiply(int[,] M1, int[,] M2, int N)
        {
            if (N <= 256 && N != 2) return base_case1(M1, M2, N);
            if (N == 2) return base_case2(M1, M2, N);
            int newSize = N / 2;
            // Dividing the matrices into 4 submatrices
            int[,] A11 = new int[newSize, newSize];
            int[,] A12 = new int[newSize, newSize];
            int[,] A21 = new int[newSize, newSize];
            int[,] A22 = new int[newSize, newSize];

            int[,] B11 = new int[newSize, newSize];
            int[,] B12 = new int[newSize, newSize];
            int[,] B21 = new int[newSize, newSize];
            int[,] B22 = new int[newSize, newSize];

            DivideMatrix(M1, A11, A12, A21, A22, newSize);
            DivideMatrix(M2, B11, B12, B21, B22, newSize);

            var tasks = new Task<int[,]>[]
            {
                Task.Run(() => MatrixMultiply(A11, MatrixSub(B12, B22, newSize), newSize)), // P1
                Task.Run(() => MatrixMultiply(MatrixAdd(A11, A12, newSize), B22, newSize)), // P2
                Task.Run(() => MatrixMultiply(MatrixAdd(A21, A22, newSize), B11, newSize)), // P3
                Task.Run(() => MatrixMultiply(A22, MatrixSub(B21, B11, newSize), newSize)), // P4
                Task.Run(() => MatrixMultiply(MatrixAdd(A11, A22, newSize), MatrixAdd(B11, B22, newSize), newSize)), // P5
                Task.Run(() => MatrixMultiply(MatrixSub(A12, A22, newSize), MatrixAdd(B21, B22, newSize), newSize)), // P6
                Task.Run(() => MatrixMultiply(MatrixSub(A11, A21, newSize), MatrixAdd(B11, B12, newSize), newSize)) // P7
            };

            // Wait for all tasks to complete
            Task.WaitAll(tasks);

            // Extract the sub-matrices P1 through P7 from their tasks
            int[,] P1 = tasks[0].Result;
            int[,] P2 = tasks[1].Result;
            int[,] P3 = tasks[2].Result;
            int[,] P4 = tasks[3].Result;
            int[,] P5 = tasks[4].Result;
            int[,] P6 = tasks[5].Result;
            int[,] P7 = tasks[6].Result;

            // Computing the resulting submatrices
            int[,] sub_matrix_11A = MatrixAdd(P5, P4, newSize);
            int[,] sub_matrix_11B = MatrixAdd(sub_matrix_11A, P6, newSize);
            int[,] C11 = MatrixSub(sub_matrix_11B, P2, newSize);

            int[,] C12 = MatrixAdd(P1, P2, newSize);
            int[,] C21 = MatrixAdd(P3, P4, newSize);

            int[,] sub_matrix_22A = MatrixAdd(P5, P1, newSize);
            int[,] sub_matrix_22B = MatrixSub(sub_matrix_22A, P7, newSize);
            int[,] C22 = MatrixSub(sub_matrix_22B, P3, newSize);

            // Combining the resulting submatrices into the resulting matrix
            return CombineMatrix(C11, C12, C21, C22, newSize, N);
        }
        static int[,] base_case1(int[,] M1, int[,] M2, int N)
        {
            int[,] c = new int[N, N];

            Parallel.ForEach(Enumerable.Range(0, N), i =>
            {
                for (int k = 0; k < N; k += 4)
                {
                    int temp1 = M1[i, k];
                    int temp2 = M1[i, k + 1];
                    int temp3 = M1[i, k + 2];
                    int temp4 = M1[i, k + 3];
                    for (int j = 0; j < N; j += 4)
                    {
                        c[i, j] += temp1 * M2[k, j] + temp2 * M2[k + 1, j] + temp3 * M2[k + 2, j] + temp4 * M2[k + 3, j];
                        c[i, j + 1] += temp1 * M2[k, j + 1] + temp2 * M2[k + 1, j + 1] + temp3 * M2[k + 2, j + 1] + temp4 * M2[k + 3, j + 1];
                        c[i, j + 2] += temp1 * M2[k, j + 2] + temp2 * M2[k + 1, j + 2] + temp3 * M2[k + 2, j + 2] + temp4 * M2[k + 3, j + 2];
                        c[i, j + 3] += temp1 * M2[k, j + 3] + temp2 * M2[k + 1, j + 3] + temp3 * M2[k + 2, j + 3] + temp4 * M2[k + 3, j + 3];
                    }
                }
            });

            return c;
        }

        static int[,] base_case2(int[,] M1, int[,] M2, int N)
        {
            int[,] c = new int[N, N];
            c[0, 0] = M1[0, 0] * M2[0, 0] + M1[0, 1] * M2[1, 0];
            c[0, 1] = M1[0, 0] * M2[0, 1] + M1[0, 1] * M2[1, 1];
            c[1, 0] = M1[1, 0] * M2[0, 0] + M1[1, 1] * M2[1, 0];
            c[1, 1] = M1[1, 0] * M2[0, 1] + M1[1, 1] * M2[1, 1];
            return c;
        }

        static void DivideMatrix(int[,] M, int[,] M11, int[,] M12, int[,] M21, int[,] M22, int Newsize)
        {
            for (int i = 0; i < Newsize; i++)
            {
                for (int j = 0; j < Newsize; j++)
                {
                    M11[i, j] = M[i, j];
                    M12[i, j] = M[i, j + Newsize];
                    M21[i, j] = M[i + Newsize, j];
                    M22[i, j] = M[i + Newsize, j + Newsize];
                }
            }
        }

        static int[,] CombineMatrix(int[,] M11, int[,] M12, int[,] M21, int[,] M22, int Newsize, int N)
        {
            int[,] M = new int[N, N];
            for (int i = 0; i < Newsize; i++)
                for (int j = 0; j < Newsize; j++)
                {
                    M[i, j] = M11[i, j];
                    M[i, j + Newsize] = M12[i, j];
                    M[i + Newsize, j] = M21[i, j];
                    M[i + Newsize, j + Newsize] = M22[i, j];
                }
            return M;
        }

        static int[,] MatrixAdd(int[,] A, int[,] B, int N)
        {
            int[,] C = new int[N, N];
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    C[i, j] = A[i, j] + B[i, j];
                }
            }
            return C;
        }
        static int[,] MatrixSub(int[,] A, int[,] B, int N)
        {
            int[,] C = new int[N, N];
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    C[i, j] = A[i, j] - B[i, j];
                }
            }
            return C;
        }
        #endregion
    }
}
