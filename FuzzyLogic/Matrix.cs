/*
 *  FuzzyLogic: FuzzyLogic Library for C#
 *  Copyright (C) 2017 Луферов Виктор (lyferov@yandex.ru)
 */

using System;

namespace FuzzyLogic
{
    public class Matrix
    {
        #region Свойства
        private double[,] _matrix;      //Матрица
        private int n, m;               //Размеры матрицы

        /// <summary>
        /// Количество строк
        /// </summary>
        public int N
        {
            get { return n; }
        }
        /// <summary>
        /// Количество столбцов
        /// </summary>
        public int M
        {
            get { return m; }
        }
        #endregion

        #region Конструкторы
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="n">Количество строк</param>
        /// <param name="m">Количество столбцов</param>
        public Matrix(int n, int m)
        {
            if (n < 1 || m < 1) {
                throw new Exception("Строки и столбцы матрицы должны быть положительные");
            }
            this.n = n;
            this.m = m;
            _matrix = new double[n,m];
        }
        /// <summary>
        /// Квадратная матрица
        /// </summary>
        /// <param name="n">Количество строк и столбцов</param>
        public Matrix(int n) {
            if (n < 1) {
                throw new Exception("Строки и столбцы матрицы не могут быть меньше нуля");
            }
            this.n = this.m = n;
            _matrix = new double[n,n];
        }
        /// <summary>
        /// Заполнение диагональных элементов матрицы
        /// </summary>
        /// <param name="matrix"></param>
        public Matrix(double[] matrix) {
            n = m = matrix.Length;
            _matrix = new double[n,m];
            for (int i = 0; i < n; i++)
            {
                this[i] = matrix[i];
            }
        }
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="matrix">Двухмерный массив</param>
        public Matrix(double[,] matrix)
        {
            _matrix = matrix;
            n = matrix.GetLength(0);
            m = matrix.GetLength(1);
        }
        #endregion

        #region Индексатор
        /// <summary>
        /// Значение ячейки
        /// </summary>
        /// <param name="i">Строка</param>
        /// <param name="j">Столбец</param>
        /// <returns></returns>
        public double this[int i, int j] {
            get { return _matrix[i, j]; }
            set { _matrix[i, j] = value; }
        }
        /// <summary>
        /// Диагональные значения матрицы
        /// </summary>
        /// <param name="i">Значение строки и столбца</param>
        /// <returns></returns>
        public double this[int i] {
            get { return _matrix[i, i]; }
            set { _matrix[i, i] = value; }
        }
        #endregion

        #region Операции над ячейками
        /// <summary>
        /// Получение элемента
        /// </summary>
        /// <param name="i">Колонка</param>
        /// <param name="j">Строка</param>
        /// <returns></returns>
        public double getElement(int i, int j) {
            return _matrix[i,j];
        }
        /// <summary>
        /// Установление элемента матрицы
        /// </summary>
        /// <param name="i">Столбец</param>
        /// <param name="j">Строка</param>
        /// <param name="val">Значение</param>
        public void setElement(int i, int j, double val) {
            _matrix[i, j] = val;
        }
        #endregion

        #region Операции над матрицами
        /// <summary>
        /// Умножаем на число
        /// </summary>
        /// <param name="a">Число, на которое умножаем</param>
        public Matrix multiply(double a) {
            Matrix ra = this;
            for (int i = 0; i < N; i++)                         // Перебираем по строкам
            {
                for (int j = 0; j < M; j++)                     // Перебираем по столбцам
                {
                    ra[i, j] = a * ra[i, j];                    //Умножаем каждый элемент на число
                }
            }
            return ra;
        }
        /// <summary>
        /// Перемножаем матрицы на матрицу почленно
        /// </summary>
        /// <param name="a">Матрица на которую перемножаем</param>
        /// <returns></returns>
        public Matrix multiply(Matrix a) {
            Matrix ra = this;
            // Три случая почленного переменожения матрицы на матрицу
            // 1 случай, когда A = [1 2 3; 4 5 6] * B = [1 2 3]
            // 2 случай, когда A = [1 2 3; 4 5 6] * B = [1 2 3; 4 5 6]
            // 3 случай, когда A - вектор столбец * B = вектор строка // не реализован

            if (ra.M == a.M && a.N == 1)       // Равны по строкам, и a - вектор строка
            {
                for (int j = 0; j < ra.M; j++) {
                    for (int i = 0; i < ra.N; i++) {
                        ra[i, j] *= a[0, j];
                    }
                }
                
            }
            else if (ra.M == a.M && ra.N == a.N)    //Простое переменожение элементов друг на друга
            {
                for (int i = 0; i < ra.N; i++) {
                    for (int j = 0; j < ra.M; j++) {
                        ra[i, j] *= a[i, j];
                    }
                }
            } else {
                throw new Exception("Параметры переменожения элементов матриц заданы неверно");
            }

            return ra;
        }
        
        /// <summary>
        /// Делим на число
        /// </summary>
        /// <param name="b">Чисо на котрое делим</param>
        public Matrix divide(double b) {
            return multiply(1 / b);
        }
        /// <summary>
        /// Прибавляем число к матрице
        /// </summary>
        /// <param name="a">Число, которое прибавляем к матрице</param>
        public void add(double a) {
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < M; j++) {
                    setElement(i, j, a + getElement(i, j));
                }
            }
        }
        /// <summary>
        /// Транспонирование матрицы
        /// </summary>
        public Matrix transposing() {
            int n_new = m;
            int m_new = n;
            double[,] new_matrix = new double[n_new, m_new];
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < M; j++) {
                    new_matrix[j, i] = getElement(i, j);
                }
            }
            return new Matrix(new_matrix);
        }
        public double[] Diag() {
            int elems = Math.Min(N, M);
            double[] d = new double[elems];
            for (int i = 0; i < elems; i++)
            {
                d[i] = this[i];
            }
            return d;
        }
        /// <summary>
        /// Детерменант матрицы
        /// </summary>
        /// <returns></returns>
        public double determinant() {
            if (N != M) {
                throw new Exception("Матрица должна быть квадратная");
            }
            return det(this);
        }

        /// <summary>
        /// Получение инверстной матрицы
        /// </summary>
        /// <returns></returns>
        public Matrix Inverse()
        {
            if (N != M)
            {
                throw new Exception("Матрица должна быть квадратная");
            }
            double determinant = this.determinant();
            Matrix A = new Matrix(N);               // Алгеброическое дополнение
            int sign = 1;
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < M; j++)
                {
                    A.setElement(i, j, sign * Matrix.det(Matrix.getMinor(this, i, j)));
                    sign = -sign;
                }
            }
            A = A.transposing();
            A.multiply(1 / determinant);
            return A;
        }

        /// <summary>
        /// Получение псевдоинверстной матрицы
        /// </summary>
        /// <returns></returns>
        public Matrix PInverse() {
            SVD svd = SingularValueDecomposition();
            Matrix V = new Matrix(M,N);
            int _n = Math.Min(M, svd.V.N);
            int _m = Math.Min(N, svd.V.M);
            for (int i = 0; i < _n; i++) {
                for (int j = 0; j< _m; j++)
                {
                    V[i, j] = svd.V[i,j];
                }
            }
            double[] s = svd.S.Diag();
            Matrix S = new Matrix(1, svd.S.N);            //Вектор строка
            _m = Math.Min(S.M,s.Length);
            for (int i = 0; i < _m; i++) {         //Массив в вектор строку
                S[0, i] = 1 / s[i];
            }
            Matrix VS = V.multiply(S);
            Matrix matrixPInverse = VS * svd.U.transposing();

            return matrixPInverse;

        }
        #endregion

        #region Статические методы
        /// <summary>
        /// Нахождение детерментанта матрицы
        /// </summary>
        /// <param name="A">Матрица</param>
        /// <returns></returns>
        private static double det(Matrix A) {
            if (A.N != A.M) {
                throw new Exception("Матрица должна быть квадратная");
            }
            double det = 0;
            switch (A.N) {
                case 1:                 //Ячейка
                    det = A.getElement(0,0);
                    break;
                case 2:
                    det = A.getElement(0, 0) * A.getElement(1, 1);
                    det -= A.getElement(1, 0) * A.getElement(0, 1);
                    break;
                default:
                    double sign = 1;
                    for (int i = 0; i < A.N; i++)
                    {
                        Matrix B = Matrix.getMinor(A, 0, i);
                        det += sign * A.getElement(0, i) * Matrix.det(B);
                        sign = -sign;      //Меняем знак
                    }
                    break;
            }
            return det;

        }

        /// <summary>
        /// Нахождение минора матрицы
        /// </summary>
        /// <param name="A">Матрица</param>
        /// <param name="R">Вычеркиваем строку</param>
        /// <param name="W">Вычеркиваем столбец</param>
        /// <returns>Минор матрицы</returns>
        private static Matrix getMinor(Matrix A,int R,int W) {
            Matrix B = new Matrix(A.N - 1);
            for (int i = 0, row = 0; i < A.N; i++) {
                if (i == R) continue;
                for (int j = 0, col = 0; j < A.N; j++)
                {
                    if (j == W) continue;
                    B.setElement(row, col, A.getElement(i, j));
                    col++;
                }
                row++;
            }
            return B;                       //Возвращаем минор
        }
        #endregion

        #region Статические перегрузки
        /// <summary>
        /// Сложение матриц
        /// </summary>
        /// <param name="m1">Первая матрица</param>
        /// <param name="m2">Вторая матрица</param>
        /// <returns></returns>
        public static Matrix operator +(Matrix m1, Matrix m2) {
            if (m1.N != m2.N || m1.M != m2.M) {
                throw new Exception("Размеры матрицы не совпадают");
            }
            Matrix mr = new Matrix(m1.N, m1.M);
            for (int i = 0; i < m1.N; i++)
            {
                for (int j = 0; j < m1.M; j++) {
                    mr.setElement(i, j, m1.getElement(i, j) + m2.getElement(i, j));
                }
            }
            return mr;
        }
        /// <summary>
        /// Вычитание матриц
        /// </summary>
        /// <param name="m1">Первая матрица</param>
        /// <param name="m2">Вторая матрица</param>
        /// <returns></returns>
        public static Matrix operator -(Matrix m1, Matrix m2)
        {
            if (m1.N != m2.N || m1.M != m2.M)
            {
                throw new Exception("Матрицы должны быть согласованны");
            }
            Matrix mr = new Matrix(m1.N, m1.M);
            for (int i = 0; i < m1.N; i++)
            {
                for (int j = 0; j < m1.M; j++)
                {
                    mr.setElement(i, j, m1.getElement(i, j) - m2.getElement(i, j));
                }
            }
            return mr;
        }
        /// <summary>
        /// Умножение матриц
        /// </summary>
        /// <param name="m1">Первая матрица</param>
        /// <param name="m2">Вторая матрица</param>
        /// <returns></returns>
        public static Matrix operator *(Matrix m1, Matrix m2) {
            if (m1.M != m2.N) {
                throw new Exception("Матрицы должна быть внутренне согласованными");
            }
            Matrix nm = new Matrix(m1.N, m2.M);                 //Инициализируем новую матрицу
            for (int i = 0; i < m1.N; i++) {
                for (int j = 0; j < m2.M; j++)
                {
                    double elem = 0;
                    for (int k = 0; k < m2.N;k++) {
                        elem += m1.getElement(i, k) * m2.getElement(k, j);
                    }
                    nm.setElement(i, j, elem);
                }
            }

            return nm;
        }
        #endregion

        #region Преобразование матрицы
        /// <summary>
        /// Преобразование матрици в строку
        /// </summary>
        /// <returns></returns>
        public string ToString(bool nl = true, int digits = 4)
        {
            string ts = "[" + (nl ? Environment.NewLine : "");
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < M; j++)
                {
                
                    if (j == M - 1)
                    {
                        ts += Math.Round(this[i, j], digits).ToString();
                    }
                    else {
                        ts += Math.Round(this[i, j], digits).ToString() + " ";
                    }
                }
                if (i != N - 1)
                {
                    ts += (nl ? Environment.NewLine : "; ");
                }
            }
            ts += (nl ? Environment.NewLine : "")+"]";
            return ts;
        }
        /// <summary>
        /// Преобразование матрицы в массив
        /// </summary>
        /// <returns>double[,]</returns>
        public double[,] toDouble()
        {
            double[,] m = new double[N, M];
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < M; j++)
                {
                    m[i, j] = getElement(i, j);
                }
            }
            return m;
        }
        #endregion

        #region SingularValueDecomposition
        public SVD SingularValueDecomposition()
        {
            SingularValueDecomposition svd = new SingularValueDecomposition(toDouble());
            Matrix U = new Matrix(svd.U);       // Матрица U[N,N]
            Matrix S = new Matrix(N, M);        // Матрица S[N,M]
            Matrix s = new Matrix(svd.S);
            int _n = Math.Min(S.N, s.N);
            int _m = Math.Min(S.M, s.M);
            for (int i = 0; i < _n; i++) {
                for (int j = 0; j < _m; j++)
                {
                    S[i, j] = s[i, j];
                }
            }
            Matrix V = new Matrix(svd.V);       // Матрица V[M,M]
            SVD svdMatrix = new SVD(U, S, V);   //Загоняем во вспомогательный класс
            return svdMatrix;                   //Возвращаем
        }

        #endregion
    }

    #region Вспомогательный класс SVD преобразования
    /// <summary>
    /// Класс, содержащий сингулярное разложение
    /// </summary>
    public class SVD {
        private Matrix u;
        private Matrix s;
        private Matrix v;

        /// <summary>
        /// Матрица U
        /// </summary>
        public Matrix U
        {
            get { return u; }
        }
        /// <summary>
        /// Матрица S
        /// </summary>
        public Matrix S
        {
            get { return s; }
        }
        /// <summary>
        /// Матрица V
        /// </summary>
        public Matrix V
        {
            get { return v; }
        }
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="u">Матрица U</param>
        /// <param name="s">Матрица S</param>
        /// <param name="v">Матрица V</param>
        public SVD(Matrix u, Matrix s, Matrix v)
        {
            this.u = u;
            this.s = s;
            this.v = v;
        }
        /// <summary>
        /// Получение оригинальной матрицы матрицы
        /// </summary>
        /// <returns>U * S * V'</returns>
        public Matrix Original() {
            return U * S * V.transposing();
        }
    }
    #endregion
}
