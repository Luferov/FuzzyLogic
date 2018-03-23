/*
 *  FuzzyLogic: FuzzyLogic Library for C#
 *  Copyright (C) 2017 Луферов Виктор (lyferov@yandex.ru)
 */
using System;
using System.Collections.Generic;

namespace FuzzyLogic
{
    /// <summary>
    /// Вынесенный класс сингулярного разложения матрицы
    /// </summary>
    internal sealed class SingularValueDecomposition
    {
        #region Поля матрицы
        private int _mRow;
        private int _nCol;
        #endregion
        #region Настройки

        // A = U * S * V'

        /// <summary>
        /// Исходная матрица
        /// </summary>
        public double[,] A { get; private set; }
        /// <summary>
        /// Матрица S
        /// </summary>
        public double[] S { get; private set; }
        /// <summary>
        /// Матрица V
        /// </summary>
        public double[,] V { get; private set; }
        /// <summary>
        /// Матрица U
        /// </summary>
        public double[,] U { get; private set; }
        #endregion
        #region Конструктор
        /// <summary>
        /// Конструктор сингулярного преобразования
        /// </summary>
        /// <param name="value"></param>
        public SingularValueDecomposition(double[,] value)
        {
            Run(value, true, true);     //Запускаем сингулярное преобразование
        }
        #endregion
        #region Вспомогательный метод
        /// <summary>
        /// Гипотенуза
        /// </summary>
        /// <param name="a">a</param>
        /// <param name="b">b</param>
        /// <returns></returns>
        public double Hypotenuse(double a, double b)
        {
            var r = 0.0;

            if (Math.Abs(a) > Math.Abs(b))
            {
                r = b / a;
                r = Math.Abs(a) * Math.Sqrt(1 + r * r);
            }
            else if (b != 0)
            {
                r = a / b;
                r = Math.Abs(b) * Math.Sqrt(1 + r * r);
            }

            return r;
        }

        #endregion
        #region Методы

        private void Run(double[,] value, bool computeLeftSingularVectors, bool computeRightSingularVectors)
        {
            #region Значения
            A = value;
            _mRow = value.GetLength(0);                     // Строки
            _nCol = value.GetLength(1);                     // Колонки
            var nu = _mRow;
            S = new double[Math.Min(_mRow + 1, _nCol)];
            U = new double[_mRow, nu];
            V = new double[_nCol, _nCol];
            var e = new double[_nCol];
            var work = new double[_mRow];
            var wantU = computeLeftSingularVectors;
            var wantV = computeRightSingularVectors;

            #endregion
            #region Cокращение к двухдиагональной форме, хранение диагональных элементов в S и супер-диагональные элементы е.
            int nct;
            int nrt;
            ReduceA(A, e, work, wantU, wantV, out nct, out nrt);
            #endregion
            #region Настройка окончательного двухдиагональной матрицы или порядок р.

            var p = Math.Min(_nCol, _mRow + 1);

            if (nct < _nCol)
                S[nct] = A[nct, nct];

            if (_mRow < p)
                S[p - 1] = 0.0;

            if (nrt + 1 < p)
                e[nrt] = A[nrt, p - 1];

            e[p - 1] = 0.0;

            #endregion
            #region Если необходимо, вычисляем U

            if (wantU)
            {
                GenerateU(nu, nct);
            }

            #endregion
            #region Если необходимо вычисляем V

            if (wantV)
            {
                GenerateV(e, nrt);
            }

            #endregion
            #region Главный итерационный цикл для сингулярных значений
            GenerateS(e, wantU, wantV, p);
            #endregion
        }

        private void ReduceA(double[,] a, IList<double> e, IList<double> work, bool wantu, bool wantv, out int nct, out int nrt)
        {
            // Reduce A to bi-diagonal form, storing the diagonal elements in s and the super-diagonal elements in e.
            nct = Math.Min(_mRow - 1, _nCol);
            nrt = Math.Max(0, Math.Min(_nCol - 2, _mRow));
            for (var k = 0; k < Math.Max(nct, nrt); k++)
            {
                if (k < nct)
                {
                    // Compute the transformation for the k-th column and place the k-th diagonal in s[k].
                    // Compute 2-norm of k-th column without under/overflow.
                    S[k] = 0;
                    for (var i = k; i < _mRow; i++)
                    {
                        S[k] = Hypotenuse(S[k], a[i, k]);
                    }

                    if (S[k] != 0.0)
                    {
                        if (a[k, k] < 0.0)
                        {
                            S[k] = -S[k];
                        }

                        for (var i = k; i < _mRow; i++)
                        {
                            a[i, k] /= S[k];
                        }

                        a[k, k] += 1.0;
                    }

                    S[k] = -S[k];
                }

                for (var j = k + 1; j < _nCol; j++)
                {
                    if ((k < nct) & (S[k] != 0.0))
                    {
                        // Apply the transformation.
                        double t = 0;
                        for (var i = k; i < _mRow; i++)
                            t += a[i, k] * a[i, j];
                        t = -t / a[k, k];
                        for (var i = k; i < _mRow; i++)
                            a[i, j] += t * a[i, k];
                    }

                    // Place the k-th row of A into e for the subsequent calculation of the row transformation.
                    e[j] = a[k, j];
                }

                if (wantu & (k < nct))
                {
                    // Place the transformation in U for subsequent back
                    // multiplication.
                    for (var i = k; i < _mRow; i++)
                        U[i, k] = a[i, k];
                }

                if (k < nrt)
                {
                    // Com  pute the k-th row transformation and place the k-th super-diagonal in e[k].
                    // Compute 2-norm without under/overflow.
                    e[k] = 0;
                    for (var i = k + 1; i < _nCol; i++)
                    {
                        e[k] = Hypotenuse(e[k], e[i]);
                    }

                    if (e[k] != 0.0)
                    {
                        if (e[k + 1] < 0.0)
                            e[k] = -e[k];

                        for (var i = k + 1; i < _nCol; i++)
                            e[i] /= e[k];

                        e[k + 1] += 1.0;
                    }

                    e[k] = -e[k];
                    if ((k + 1 < _mRow) & (e[k] != 0.0))
                    {
                        // Apply the transformation.
                        for (var i = k + 1; i < _mRow; i++)
                            work[i] = 0.0;

                        for (var j = k + 1; j < _nCol; j++)
                            for (var i = k + 1; i < _mRow; i++)
                                work[i] += e[j] * a[i, j];

                        for (var j = k + 1; j < _nCol; j++)
                        {
                            var t = -e[j] / e[k + 1];
                            for (var i = k + 1; i < _mRow; i++)
                                a[i, j] += t * work[i];
                        }
                    }

                    if (wantv)
                    {
                        // Place the transformation in V for subsequent back multiplication.
                        for (var i = k + 1; i < _nCol; i++)
                            V[i, k] = e[i];
                    }
                }
            }
        }

        private void GenerateU(int nu, int nct)
        {
            for (var j = nct; j < nu; j++)
            {
                for (var i = 0; i < _mRow; i++)
                    U[i, j] = 0.0;

                U[j, j] = 1.0;
            }

            for (var k = nct - 1; k >= 0; k--)
            {
                if (S[k] != 0.0)
                {
                    for (var j = k + 1; j < nu; j++)
                    {
                        double t = 0;

                        for (var i = k; i < _mRow; i++)
                            t += U[i, k] * U[i, j];

                        t = -t / U[k, k];

                        for (var i = k; i < _mRow; i++)
                            U[i, j] += t * U[i, k];
                    }

                    for (var i = k; i < _mRow; i++)
                        U[i, k] = -U[i, k];

                    U[k, k] = 1.0 + U[k, k];

                    for (var i = 0; i < k - 1; i++)
                        U[i, k] = 0.0;
                }
                else
                {
                    for (var i = 0; i < _mRow; i++)
                        U[i, k] = 0.0;

                    U[k, k] = 1.0;
                }
            }
        }

        private void GenerateV(IList<double> e, int nrt)
        {
            for (var k = _nCol - 1; k >= 0; k--)
            {
                if ((k < nrt) & (e[k] != 0.0))
                {
                    //TODO: Check if this is a bug.
                    //   for (var j = k + 1; j < nu; j++)
                    // The correction would be:
                    for (var j = k + 1; j < _nCol; j++)
                    {
                        double t = 0;

                        for (var i = k + 1; i < _nCol; i++)
                            t += V[i, k] * V[i, j];

                        t = -t / V[k + 1, k];

                        for (var i = k + 1; i < _nCol; i++)
                            V[i, j] += t * V[i, k];
                    }
                }

                for (var i = 0; i < _nCol; i++)
                    V[i, k] = 0.0;

                V[k, k] = 1.0;
            }
        }

        private void GenerateS(IList<double> e, bool wantu, bool wantv, int p)
        {
            // Main iteration loop for the singular values.
            int pp = p - 1;
            int iter = 0;
            double eps = Math.Pow(2,-256);

            while (p > 0)
            {
                int k, kase;
                // Here is where a test for too many iterations would go.
                // This section of the program inspects for
                // negligible elements in the s and e arrays.  On
                // completion the variables kase and k are set as follows.
                // kase = 1     if s(p) and e[k-1] are negligible and k<p
                // kase = 2     if s(k) is negligible and k<p
                // kase = 3     if e[k-1] is negligible, k<p, and s(k), ..., s(p) are not negligible (qr step).
                // kase = 4     if e(p-1) is negligible (convergence).
                for (k = p - 2; k >= -1; k--)
                {
                    if (k == -1)
                        break;

                    if (Math.Abs(e[k]) <= eps * (Math.Abs(S[k]) + Math.Abs(S[k + 1])))
                    {
                        e[k] = 0.0;
                        break;
                    }
                }

                if (k == p - 2)
                {
                    kase = 4;
                }
                else
                {
                    int ks;
                    for (ks = p - 1; ks >= k; ks--)
                    {
                        if (ks == k)
                            break;

                        double t = (ks != p ? Math.Abs(e[ks]) : 0.0) + (ks != k + 1 ? Math.Abs(e[ks - 1]) : 0.0);
                        if (Math.Abs(S[ks]) <= eps * t)
                        {
                            S[ks] = 0.0;
                            break;
                        }
                    }

                    if (ks == k)
                        kase = 3;
                    else if (ks == p - 1)
                        kase = 1;
                    else
                    {
                        kase = 2;
                        k = ks;
                    }
                }

                k++;

                // Perform the task indicated by kase.
                switch (kase)
                {
                    // Deflate negligible s(p).
                    case 1:
                        {
                            var f = e[p - 2];
                            e[p - 2] = 0.0;
                            for (var j = p - 2; j >= k; j--)
                            {
                                var t = Hypotenuse(S[j], f);
                                var cs = S[j] / t;
                                var sn = f / t;

                                S[j] = t;

                                if (j != k)
                                {
                                    f = -sn * e[j - 1];
                                    e[j - 1] = cs * e[j - 1];
                                }

                                if (wantv)
                                {
                                    for (var i = 0; i < _nCol; i++)
                                    {
                                        t = cs * V[i, j] + sn * V[i, p - 1];
                                        V[i, p - 1] = -sn * V[i, j] + cs * V[i, p - 1];
                                        V[i, j] = t;
                                    }
                                }
                            }
                        }
                        break;

                    // Split at negligible s(k).
                    case 2:
                        {
                            var f = e[k - 1];
                            e[k - 1] = 0.0;
                            for (var j = k; j < p; j++)
                            {
                                var t = Hypotenuse(S[j], f);
                                var cs = S[j] / t;
                                var sn = f / t;
                                S[j] = t;
                                f = -sn * e[j];
                                e[j] = cs * e[j];

                                if (wantu)
                                {
                                    for (var i = 0; i < _mRow; i++)
                                    {
                                        t = cs * U[i, j] + sn * U[i, k - 1];
                                        U[i, k - 1] = -sn * U[i, j] + cs * U[i, k - 1];
                                        U[i, j] = t;
                                    }
                                }
                            }
                        }
                        break;

                    // Perform one qr step.
                    case 3:
                        {
                            // Calculate the shift.
                            var scale = Math.Max(Math.Max(Math.Max(Math.Max(Math.Abs(S[p - 1]), Math.Abs(S[p - 2])), Math.Abs(e[p - 2])), Math.Abs(S[k])), Math.Abs(e[k]));
                            var sp = S[p - 1] / scale;
                            var spm1 = S[p - 2] / scale;
                            var epm1 = e[p - 2] / scale;
                            var sk = S[k] / scale;
                            var ek = e[k] / scale;
                            var b = ((spm1 + sp) * (spm1 - sp) + epm1 * epm1) / 2.0;
                            var c = (sp * epm1) * (sp * epm1);
                            var shift = 0.0;

                            if ((b != 0.0) | (c != 0.0))
                            {
                                shift = Math.Sqrt(b * b + c);

                                if (b < 0.0)
                                    shift = -shift;

                                shift = c / (b + shift);
                            }

                            var f = (sk + sp) * (sk - sp) + shift;
                            var g = sk * ek;

                            // Chase zeros.
                            for (var j = k; j < p - 1; j++)
                            {
                                var t = Hypotenuse(f, g);
                                var cs = f / t;
                                var sn = g / t;

                                if (j != k)
                                    e[j - 1] = t;

                                f = cs * S[j] + sn * e[j];
                                e[j] = cs * e[j] - sn * S[j];
                                g = sn * S[j + 1];
                                S[j + 1] = cs * S[j + 1];

                                if (wantv)
                                {
                                    for (var i = 0; i < _nCol; i++)
                                    {
                                        t = cs * V[i, j] + sn * V[i, j + 1];
                                        V[i, j + 1] = -sn * V[i, j] + cs * V[i, j + 1];
                                        V[i, j] = t;
                                    }
                                }

                                t = Hypotenuse(f, g);
                                cs = f / t;
                                sn = g / t;
                                S[j] = t;
                                f = cs * e[j] + sn * S[j + 1];
                                S[j + 1] = -sn * e[j] + cs * S[j + 1];
                                g = sn * e[j + 1];
                                e[j + 1] = cs * e[j + 1];

                                if (wantu && (j < _mRow - 1))
                                {
                                    for (var i = 0; i < _mRow; i++)
                                    {
                                        t = cs * U[i, j] + sn * U[i, j + 1];
                                        U[i, j + 1] = -sn * U[i, j] + cs * U[i, j + 1];
                                        U[i, j] = t;
                                    }
                                }
                            }

                            e[p - 2] = f;
                            iter = iter + 1;
                        }
                        break;

                    // Convergence.
                    case 4:
                        {
                            // Make the singular values positive.
                            if (S[k] <= 0.0)
                            {
                                S[k] = (S[k] < 0.0 ? -S[k] : 0.0);
                                if (wantv)
                                    for (var i = 0; i <= pp; i++)
                                        V[i, k] = -V[i, k];
                            }

                            // Order the singular values.
                            while (k < pp)
                            {
                                if (S[k] >= S[k + 1])
                                    break;

                                var t = S[k];
                                S[k] = S[k + 1];
                                S[k + 1] = t;

                                if (wantv && (k < _nCol - 1))
                                {
                                    for (var i = 0; i < _nCol; i++)
                                    {
                                        t = V[i, k + 1];
                                        V[i, k + 1] = V[i, k];
                                        V[i, k] = t;
                                    }
                                }

                                if (wantu && (k < _mRow - 1))
                                {
                                    for (var i = 0; i < _mRow; i++)
                                    {
                                        t = U[i, k + 1];
                                        U[i, k + 1] = U[i, k];
                                        U[i, k] = t;
                                    }
                                }

                                k++;
                            }

                            iter = 0;
                            p--;
                        }
                        break;
                }
            }
        }

        #endregion
    }
}
