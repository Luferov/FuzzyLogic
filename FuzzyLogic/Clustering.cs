/*
 *  FuzzyLogic: FuzzyLogic Library for C#
 *  Copyright (C) 2017 Луферов Виктор (lyferov@yandex.ru)
 */

using System;
using System.Collections.Generic;

namespace FuzzyLogic
{
    #region Горная кластеризация

    /// <summary>
    /// Делегат вывода текста при работе алгоритма горной кластеризации
    /// </summary>
    /// <param name="text"></param>
    public delegate void SubClustVerbose(string text);

    /// <summary>
    /// Горная кластеризация
    /// Все исходные данные проецируются на гиперкуб
    /// </summary>
    public class SubtractClustesing {


        private double[][] centers;             // Центры кластеров
        private double[] sigmas;                // Радиус кластеров
        private int numClusters;                // Количество кластеров
        private double[][] x;                   // Матрица входных данных
        private double[] radii;                 // Радиус кластера
        private double sqshFactor;              // Коэффициент подавления
        private double acceptRatio;             // Коэффициент принятия
        private double rejectRatio;             // Коэффициент отторжения


        #region Свойства класса
        /// <summary>
        /// Центры кластеров
        /// </summary>
        public double[][] Centers
        {
            get { return centers; }
        }
        /// <summary>
        /// Вектор радиуса кластеров
        /// </summary>
        public double[] Sigmas
        {
            get { return sigmas; }
        }

        /// <summary>
        /// Матрица, над которой будет произведен кластерный анализ
        /// </summary>
        public double[][] X
        {
            get { return x; }
            set { x = value; }
        }
        /// <summary>
        /// Размеры кластера по каждой координате
        /// </summary>
        public double[] Radii
        {
            get { return radii; }

            set
            {
                foreach (double r in value) {
                    if (r < 0 || r > 1) {
                        throw new Exception("Параметры размера не может быть больше 1 и меньше 0");
                    }
                }
                radii = value;
            }
        }
        /// <summary>
        /// Коэффициент подавления. По умолчанию 1.25
        /// </summary>
        public double SqshFactor
        {
            get { return sqshFactor; }
            set { sqshFactor = value; }
        }
        /// <summary>
        /// Коэффициент принятия. По умолчанию 0.5
        /// </summary>
        public double AcceptRatio
        {
            get { return acceptRatio; }
            set { acceptRatio = value; }
        }
        /// <summary>
        /// Коэффициент отторжения. По умолчанию 0.15
        /// </summary>
        public double RejectRatio
        {
            get { return rejectRatio; }
            set { rejectRatio = value; }
        }
        /// <summary>
        /// Количество кластеров
        /// </summary>
        public int NumClusters
        {
            get { return numClusters; }
        }

        #endregion
        #region Конструкторы класса
        /// <summary>
        /// Создание экземпляра класса 
        /// </summary>
        /// <param name="_x">Матрица входных данных [NxM]</param>
        /// <param name="_radii">Матрица радиусов [N]</param>
        /// <param name="sf">Коэффициент подавления</param>
        /// <param name="ar">Коэффициент принятия</param>
        /// <param name="rr">Коэффициент отторжения</param>
        public SubtractClustesing(double[][] _x, double[] _radii, double sf = 1.25, double ar = 0.5, double rr = 0.15) {
            X = _x;
            Radii = _radii;
            SqshFactor = sf;
            AcceptRatio = ar;
            RejectRatio = rr;
        }
        /// <summary>
        /// Создание экземпляра класса 
        /// </summary>
        /// <param name="_x">Матрица входных данных [NxM]</param>
        /// <param name="_radii">Матрица радиусов</param>
        /// <param name="sf">Коэффициент подавления</param>
        /// <param name="ar">Коэффициент принятия</param>
        /// <param name="rr">Коэффициент отторжения</param>
        public SubtractClustesing(double[][] _x, double _radii, double sf = 1.25, double ar = 0.5, double rr = 0.15)
        {
            X = _x;
            radii = new double[_x.Length];
            setValue(ref radii, _radii);
            SqshFactor = sf;
            AcceptRatio = ar;
            RejectRatio = rr;
        }
        #endregion
        #region Алгоритм кластеризации
        public void SubClust(SubClustVerbose verbose = null) {
            int numParams = X.Length;               //Количество столбцов
            int numPoints = X[0].Length;            //Количество строк
            double[] accumMultp = new double[numParams];
            double[] sqshMultp = new double[numParams];
            numClusters = 0;
            // Растояние мультипликаторы для накопления и подавления потенциала кластера
            for (int i =0; i < numParams; i++)
            {
                accumMultp[i] = 1.0 / Radii[i];
                sqshMultp[i] = 1.0 / (Radii[i] * SqshFactor);
            }
            verbose?.Invoke("Нормализация данных...");
            // Нахождение максимальных и минимальных значений
            double[] minX = new double[numParams];
            double[] maxX = new double[numParams];
            for (int i = 0; i < numParams; i++) {   //Пробегаемся по столбцам
                double max = X[i][0];
                double min = X[i][0];
                for (int j = 0; j < numPoints; j++) {
                    if (max < X[i][j]) max = X[i][j];
                    if (min > X[i][j]) min = X[i][j];
                }
                minX[i] = min;
                maxX[i] = max;
            }
            // Нормализация данных в блок гиперкуба с использованием MinX и MaxX
            for (int i = 0; i < numParams; i++) {
                for (int j = 0; j < numPoints; j++)
                {
                    X[i][j] = (X[i][j] - minX[i]) / (maxX[i]-minX[i]);

                    // Не больше единицы
                    if (X[i][j] > 1.0) X[i][j] = 1.0;
                    // Не меньше нуля
                    if (X[i][j] < 0.0) X[i][j] = 0.0;
                }
            }

            verbose?.Invoke("Подсчет потенциала для каждой точки");
            double[] potVal = new double[numPoints];

            for (int j = 0; j < numPoints; j++)                 //Перебираем точки
            {
                double[] thePoint = new double[numParams];      //Берем точку
                double[,] dx = new double[numParams, numPoints];

                for (int i = 0; i < numParams; i++) {           //Перебираем колонки
                    thePoint[i] = X[i][j];                      //Заполняем точку
                }
                for (int k = 0; k < numParams; k++) {
                    for (int l = 0; l < numPoints; l++) {
                        dx[k, l] = (thePoint[k] - X[k][l]) * accumMultp[k];
                    }
                }
                // Суммируем потенциал
                double sum = 0;
                for (int k = 0; k < numPoints; k++)
                {
                    double summk = 0;
                    for (int l = 0; l < numParams; l++)
                    {
                        summk += Math.Pow(dx[l, k], 2);
                    }
                    sum += Math.Exp(-4 * summk);    // Возводим в квадрат
                }
                potVal[j] = sum;
            }

            // Ищем точку с самым большим потенциалом
            int maxPotIndex = 0;                                        // Индекс максимального элемента
            double maxPotVal = max(potVal, out maxPotIndex);            // Находим максимум
            double refPotVal = maxPotVal;
            numClusters = 0;
            List<double[]> _centers = new List<double[]>();
            int findMore = 1;                                           // Флаг поиска
            verbose?.Invoke("Определение центров кластеров");

            while (findMore != 0 && maxPotVal != 0) {                   // Ищем если потенциал не нулевой и можно искать
                findMore = 0;
                double[] maxPoint = new double[numParams];
                for (int i = 0; i < numParams; i++) {
                    maxPoint[i] = X[i][maxPotIndex];
                }
                double maxPotRatio = maxPotVal / refPotVal;
                
                if (maxPotRatio > AcceptRatio)
                {
                    // Новое значение пика является значительным
                    findMore = 1;
                } else if (maxPotRatio > RejectRatio)
                {
                    // Принять точку тогда, когда хороший баланс и далеко от кластеров
                    double minDistSq = -1;
                    for (int i = 0; i < _centers.Count; i++) {
                        double[] c = _centers[i];
                        double dxSq = 0;
                        for (int j = 0; j < numParams; j++) {
                            double tmp = (maxPoint[j] - c[j]) * accumMultp[j];
                            dxSq += tmp * tmp;      //tmp^2
                        }
                        if (minDistSq < 0 || dxSq < minDistSq) {
                            minDistSq = dxSq;
                        }
                        double minDist = Math.Sqrt(minDistSq);
                        if (maxPotRatio + minDist >= 1)
                        {
                            findMore = 1;
                        }
                        else
                        {
                            findMore = 2;
                        }
                    }

                }

                if (findMore == 1)
                {
                    // Добавление точки в список кластеров
                    _centers.Add(maxPoint);                         //Добавляем кластер
                    numClusters++;                                  //Увеличиваем номер кластера на +1
                    verbose?.Invoke(string.Format("Найден кластер {0}, потенциал = {1} ", numClusters, maxPotRatio));
                    double[,] dx = new double[numParams, numPoints];
                    for (int i = 0; i < numParams; i++) {
                        for (int j = 0; j < numPoints; j++) {
                            dx[i, j] = (maxPoint[i] - X[i][j]) * sqshMultp[i];
                        }
                    }

                    double[] deduct = new double[numPoints];
                    for (int j = 0; j < numPoints; j++) {           //Берем строчку
                        double sum = 0;
                        for (int i = 0; i < numParams; i++) {       //Элементы строки
                            sum += Math.Pow(dx[i, j], 2);
                        }
                        deduct[j] = maxPotVal * Math.Exp(-4 * sum);
                    }
                    for (int i = 0; i < numPoints; i++) {
                        potVal[i] -= deduct[i];
                        if (potVal[i] < 0) potVal[i] = 0;           // Ограничение по 0 снизу
                    }
                    maxPotVal = max(potVal, out maxPotIndex);       // Находим максимум
                }
                else if (findMore == 2)
                {
                    potVal[maxPotIndex] = 0;                        //Обнуляем значение потенциала
                    maxPotVal = max(potVal, out maxPotIndex);       //Находим максимум
                }

            }

            verbose?.Invoke("Денормализация денных...");
            // Денормализация данных с использованием MinX и MaxX
            for (int i = 0; i < numParams; i++)
            {
                double r = maxX[i] - minX[i];
                for (int j = 0; j < numPoints; j++)
                {
                    X[i][j] = (X[i][j] * r) + minX[i];
                }
            }
            // Денормализация центров кластеров кластеров
            centers = new double[numParams][];
            int countCluster = _centers.Count;
            for (int i = 0; i < numParams; i++)
            {
                centers[i] = new double[numClusters];
                for (int j = 0; j < countCluster; j++) {
                    centers[i][j] = _centers[j][i];
                }
            }
            
            
            for (int i = 0; i < centers.Length; i++) {
                double r = maxX[i] - minX[i];
                for (int j = 0; j < centers[i].Length; j++)
                {
                    centers[i][j] = (centers[i][j] * r) + minX[i];
                }
            }

            verbose?.Invoke("Определение радиуса кластеров");

            sigmas = new double[numParams];
            for (int i = 0; i < numParams; i++) {
                sigmas[i] = (Radii[i] * (maxX[i] - minX[i])) / Math.Sqrt(8.0);
            }
            verbose?.Invoke("Кластеризация успешно завершена");
        }
        #endregion
        #region Вспомогательные функции
        private void setValue<T>(ref T[] arr, T val)
        {
            for (int i = 0; i < arr.Length; i++) {
                arr[i] = val;
            }
        }
        private double max(double[] arr, out int index) {
            index = 0;
            double result = arr[0];
            for (int i = 1; i < arr.Length; i++) {
                if (result < arr[i])
                {
                    index = i;
                    result = arr[i];
                }
            }
            return result;
        }
        #endregion

    }

    #endregion


}
