/*
 *  FuzzyLogic: FuzzyLogic Library for C#
 *  Copyright (C) 2017 Луферов Виктор (lyferov@yandex.ru)
 */
using System;

namespace FuzzyLogic
{
    /// <summary>
    /// Нечеткое преобразование
    /// Предложено Перфиловой
    /// Разбивает ряд на трендовую и остаточную составляющую
    /// </summary>
    public class FuzzyTransform
    {
        private double[] timeSeries;            // Исходное значение временного ряда
        private double[] timeSeriesDirect;      // Значения после прямого FT преобразования
        private double[] timeSeriesInverse;     // Значение после обратного FT преобразования
        private double[] timeSeriesRemainder;   // Остаток временного ряда
        private double[] A;                     // Значения узлов
        private int uLeft;                      // Левая граница разбиения
        private int uRight;                     // Правая граница разбиения
        private int step;                       // Шаг разбиения
        private int n;                          // Количество узлов

        #region Свойства
        /// <summary>
        /// Исходный временной ряд
        /// </summary>
        public double[] TimeSeries
        {
            get { return timeSeries; }
        }
        /// <summary>
        /// Временной ряд, после нечеткого преобразования
        /// </summary>
        public double[] TimeSeriesDirect
        {
            get { return timeSeriesDirect; }
        }
        /// <summary>
        /// Трендовая составляющая временного ряда
        /// </summary>
        public double[] TimeSeriesInverse
        {
            get { return timeSeriesInverse; }
        }
        /// <summary>
        /// Остаток временного ряда
        /// </summary>
        public double[] TimeSeriesRemainder
        {
            get { return timeSeriesRemainder; }
        }
        #endregion
        #region Конструктор
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="ts">Временной ряд</param>
        /// <param name="_n">Количество шагов разбиения</param>
        public FuzzyTransform(double[] ts, int _n = 3)
        {
            timeSeries = ts;
            n = timeSeries.Length / _n;
            FuzzySplit(1, timeSeries.Length);
        }
        #endregion
        #region Нечеткое разбиение
        /// <summary>
        /// Определяем границы и шаг нечеткого разбиения
        /// </summary>
        /// <param name="_uleft">Левая граница разбиения</param>
        /// <param name="_uright">Правая граница разбиения</param>
        private void FuzzySplit(int _uleft, int _uright)
        {
            uLeft = _uleft;
            uRight = _uright;
            if (uLeft >= uRight)
            {
                throw new Exception("Левая граница не может быть больше, чем правая");
            }
            if (n - 1 == 0) n = 2;  // Защита от деления на 0
            step = (uRight - uLeft) / (n - 1);
            FuzzyAk();
        }

        /// <summary>
        /// Нечеткое разбиение
        /// </summary>
        private void FuzzyAk()
        {
            while (uRight > uLeft + step * (n - 1)) n++;    // Чтобы разбиение было до конца
            A = new double[n];
            for (int i = 0; i < n; i++)
            {
                A[i] = uLeft + step * i;
            }
        }
        /// <summary>
        /// Базисная функция принадлежности
        /// </summary>
        /// <param name="i">Шаг</param>
        /// <param name="x">Значение</param>
        /// <returns></returns>
        private double Ak(int i, double x)
        {
            x++;    //Двигаем функцию правее на 1 позицию
            // В начале 
            if (i == 0)
            {
                if ((x >= A[i]) && (x < A[i + 1]))
                {
                    return (1 - ((x - A[0]) / step));
                }
            }
            // В конце
            if (i == n - 1)
            {
                if ((x >= A[i - 1]) && (x <= A[i]))
                {
                    return (x - A[i - 1]) / (double)step;
                }
            }
            // промежуток
            if ((i > 0) && (i < n - 1))
            {
                if ((x >= A[i]) && (x < A[i + 1]))
                {
                    return 1 - ((x - A[i]) / (double)step);
                }
                if (x >= A[i - 1] && x < A[i])
                {
                    return (x - A[i - 1]) / (double)step;
                }
            }
            return 0;
        }

        /// <summary>
        /// Прямое преобразование
        /// </summary>
        public void FuzzyDirect()
        {
            timeSeriesDirect = new double[n];
            for (int i = 0; i< n; i++)
            {
                double s1 = 0, s2 = 0;
                for (int j = 0; j < timeSeries.Length; j++)     // Пробегаемся по ряду
                {
                    s1 += timeSeries[j] * Ak(i, j);
                    s2 += Ak(i, j);
                }
                timeSeriesDirect[i] = s1 / s2;                  // Находим вектор [F1,...,Fk]
            }
        }
        /// <summary>
        /// Обратное преобразование
        /// </summary>
        public void FuzzyInverse()
        {
            timeSeriesInverse = new double[timeSeries.Length];
            timeSeriesRemainder = new double[timeSeries.Length];
            for (int i = 0; i < timeSeries.Length; i++)
            {
                double t = 0;
                for (int j = 0; j < n; j++)
                {
                    t += timeSeriesDirect[j] * Ak(j, i);
                }
                timeSeriesInverse[i] = t;                   // Тренд
                timeSeriesRemainder[i] = timeSeries[i] - t; // Остаточная компонента
            }
        }
        /// <summary>
        /// Прямое и обратное преобразование
        /// </summary>
        public void Run()
        {
            FuzzyDirect();
            FuzzyInverse();
        }
        #endregion
    }
}
