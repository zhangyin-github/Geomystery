﻿using Geomystery.Models.FMatrix;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeomysteryTest
{
    /// <summary>
    /// 测试矩阵类
    /// </summary>
    [TestClass]
    public class FMatrixText
    {
        /// <summary>
        /// 测试构造函数
        /// </summary>
        [TestMethod]
        public void TestMatrixConstructionFunction()
        {
            //public FMatrix(int row, int column, T initializeNumber)
            FMatrix<int> mat = new FMatrix<int>(3, 4, -1);  //row 3 column 4 初始化成 -1
            Assert.IsTrue(mat.row == 3);
            Assert.IsTrue(mat.column == 4);
            Assert.IsTrue(mat.matrix[2][3] == -1);

            FMatrix<int> mat2 = new FMatrix<int>(mat);          //复制构造函数
            Assert.IsTrue(mat2.row == 3);
            Assert.IsTrue(mat2.column == 4);
            Assert.IsTrue(mat2.matrix[2][3] == -1);
        }
        
        /// <summary>
        /// 测试初始化函数
        /// </summary>
        [TestMethod]
        public void TestInitialize()
        {
            FMatrix<int> mat = new FMatrix<int>();
            Assert.IsNull(mat.matrix);                      //未初始化

            Assert.IsTrue(mat.Initialize(4, 5, -2));        //初始化
            Assert.IsTrue(mat.row == 4);
            Assert.IsTrue(mat.column == 5);
            Assert.IsTrue(mat.matrix[3][4] == -2);

            Assert.IsFalse(mat.Initialize(3, 4, -3));       //二次初始化
            Assert.IsTrue(mat.row == 4);
            Assert.IsTrue(mat.column == 5);
            Assert.IsTrue(mat.matrix[3][4] == -2);
        }

        /// <summary>
        /// 测试矩阵乘数
        /// </summary>
        [TestMethod]
        public void TestMultiply()
        {
            FMatrix<int> mat = new FMatrix<int>(3, 3, -1);
            Assert.IsTrue(mat.row == 3);
            Assert.IsTrue(mat.column == 3);
            int number = 0;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    mat.matrix[i][j] = number;
                    number++;
                }
            }
            FMatrix<int> matMulti = FMatrix<int>.Multiply(mat, 2);
            Assert.IsTrue(matMulti.row == 3);
            Assert.IsTrue(matMulti.column == 3);
            Assert.IsTrue(matMulti.matrix[1][1] == 8);

            FMatrix<int> matMulti2 = mat * -3;
            Assert.AreEqual(matMulti2.matrix[1][1], -12);
        }

        /// <summary>
        /// 测试代数余子式
        /// </summary>
        [TestMethod]
        public void TestAij()
        {
            FMatrix<int> mat = new FMatrix<int>(3,3,-1);
            int number = 0;
            for(int i = 0; i < 3; i++)
            {
                for(int j = 0; j < 3; j++)
                {
                    mat.matrix[i][j] = number;
                    number++;
                }
            }
            FMatrix<int> matAij = FMatrix<int>.Aij(mat, 1, 1);
            Assert.IsTrue(matAij.row == 2);
            Assert.IsTrue(matAij.column == 2);
            Assert.IsTrue(matAij.matrix[0][0] == 0);
            Assert.IsTrue(matAij.matrix[0][1] == 2);
            Assert.IsTrue(matAij.matrix[1][0] == 6);
            Assert.IsTrue(matAij.matrix[1][1] == 8);

            FMatrix<int> matAij2 = FMatrix<int>.Aij(mat, 0, 1);
            Assert.IsTrue(matAij2.row == 2);
            Assert.IsTrue(matAij2.column == 2);
            Assert.IsTrue(matAij2.matrix[0][0] == -3);
            Assert.IsTrue(matAij2.matrix[0][1] == -5);
            Assert.IsTrue(matAij2.matrix[1][0] == -6);
            Assert.IsTrue(matAij2.matrix[1][1] == -8);
        }

        [TestMethod]
        public void TestAdd()
        {
            FMatrix<int> mat = new FMatrix<int>(3, 3, -2);
            FMatrix<int> mat2 = new FMatrix<int>(3, 3, 1);
            int number = 0;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    mat.matrix[i][j] = number;
                    number++;
                }
            }
            FMatrix<int> mat3 = mat + mat2;
            Assert.AreEqual(mat3[0][0], 1);
            Assert.AreEqual(mat3[0][1], 2);
            Assert.AreEqual(mat3[1][0], 4);
            Assert.AreEqual(mat3[1][1], 5);
            Assert.AreEqual(mat3[2][1], 8);
            Assert.AreEqual(mat3[2][2], 9);
        }

        /// <summary>
        /// 测试计算行列式
        /// </summary>
        [TestMethod]
        public void TestDeterminant()
        {
            // 10
            FMatrix<int> testOneMat = new FMatrix<int>(1, 1, 10);
            int? oneDet = FMatrix<int>.Determinant(testOneMat);
            Assert.IsTrue(oneDet.HasValue);
            Assert.AreEqual(oneDet.Value, 10);

            // 1 2
            // 3 4
            FMatrix<int> mat = new FMatrix<int>(2, 2, -1);
            int number = 1;
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    mat.matrix[i][j] = number;
                    number++;
                }
            }
            Assert.IsTrue(mat.matrix[0][0] == 1);
            Assert.IsTrue(mat.matrix[0][1] == 2);
            Assert.IsTrue(mat.matrix[1][0] == 3);
            Assert.IsTrue(mat.matrix[1][1] == 4);
            int? det = FMatrix<int>.Determinant(mat);
            Assert.IsTrue(det.HasValue);
            Assert.AreEqual(det.Value, -2);

            // -1 -2
            // -3 -4
            FMatrix<int> mat3 = new FMatrix<int>(2, 2, -1);
            number = 1;
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    mat3.matrix[i][j] = number;
                    number++;
                }
            }
            int? det3 = FMatrix<int>.Determinant(mat3);
            Assert.IsTrue(det3.HasValue);
            Assert.AreEqual(det3.Value, -2);

            // 1 2 5
            // 3 4 6
            // 9 8 7
            FMatrix<int> mat2 = new FMatrix<int>(3, 3, 0);
            mat2[0][0] = 1;
            mat2[0][1] = 2;
            mat2[0][2] = 5;
            mat2[1][0] = 3;
            mat2[1][1] = 4;
            mat2[1][2] = 6;
            mat2[2][0] = 9;
            mat2[2][1] = 8;
            mat2[2][2] = 7;
            int? det2 = FMatrix<int>.Determinant(mat2);
            Assert.IsTrue(det2.HasValue);
            Assert.AreEqual(det2.Value, -14);
        }

        /// <summary>
        /// 测试矩阵的转置
        /// </summary>
        public void TestTranspose()
        {
            FMatrix<int> mat = new FMatrix<int>(2, 3, 0);
            int number = 1;
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    mat.matrix[i][j] = number;
                    number++;
                }
            }
            FMatrix<int> matT = FMatrix<int>.Transpose(mat);
            Assert.AreEqual(matT.row, 3);
            Assert.AreEqual(matT.column, 2);
            Assert.AreEqual(matT[1, 0], 2);
            Assert.AreEqual(matT[2, 1], 6);
        }


    }
}
