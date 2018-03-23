/*
 *  FuzzyLogic: FuzzyLogic Library for C#
 *  Copyright (C) 2017 Луферов Виктор (lyferov@yandex.ru)
 */

using System;
using System.Collections.Generic;


namespace FuzzyLogic
{
    /// <summary>
    /// Тип композиции функций принадлежности
    /// </summary>
    public enum MfCompositionType
    {
        /// <summary>
        /// Минимум функций
        /// </summary>
        Min,
        /// <summary>
        /// Максимум функций
        /// </summary>
        Max,
        /// <summary>
        /// Произведение функций
        /// </summary>
        Prod,
        /// <summary>
        /// Сумма функций
        /// </summary>
        Sum
    }

    /// <summary>
    /// Интерфейс функций пирнадлежности
    /// </summary>
    public interface IMembershipFunction
    {
        /// <summary>
        /// Подсчет значения функции принадлежности
        /// </summary>
        /// <param name="x">Аргумент функции принадлежности</param>
        /// <returns></returns>
        double GetValue(double x);
    }

    /// <summary>
    /// Треугольная функция принадлежности
    /// </summary>
    public class TriangularMembershipFunction : IMembershipFunction
    {
        double _x1, _x2, _x3;

        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public TriangularMembershipFunction()
        { }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="x1">Первая точка</param>
        /// <param name="x2">Вторая точка</param>
        /// <param name="x3">Третья точка</param>
        public TriangularMembershipFunction(double x1, double x2, double x3)
        {
            if (!(x1 <= x2 && x2 <= x3))
            {
                throw new ArgumentException();
            }

            _x1 = x1;
            _x2 = x2;
            _x3 = x3;
        }

        /// <summary>
        /// Первая точка
        /// </summary>
        public double X1
        {
            get { return _x1; }
            set { _x1 = value; }
        }

        /// <summary>
        /// Вторая точка
        /// </summary>
        public double X2
        {
            get { return _x2; }
            set { _x2 = value; }
        }

        /// <summary>
        /// Третья точка
        /// </summary>
        public double X3
        {
            get { return _x3; }
            set { _x3 = value; }
        }

        /// <summary>
        /// Вычисление значение функции принадлежности
        /// </summary>
        /// <param name="x">Аргумент функции</param>
        /// <returns></returns>
        public double GetValue(double x)
        {
            double result = 0;

            if (x == _x1 && x == _x2)
            {
                result = 1.0;
            }
            else if (x == _x2 && x == _x3)
            {
                result = 1.0;
            }
            else if (x <= _x1 || x >= _x3)
            {
                result = 0;
            }
            else if (x == _x2)
            {
                result = 1;
            }
            else if ((x > _x1) && (x < _x2))
            {

                result = (x / (_x2 - _x1)) - (_x1 / (_x2 - _x1));
            }
            else
            {
                result = (-x / (_x3 - _x2)) + (_x3 / (_x3 - _x2));
            }

            return result;
        }

        /// <summary>
        /// Аппроксимационная конвертация к нормальной функции принадлежности
        /// </summary>
        /// <returns></returns>
        public NormalMembershipFunction ToNormalMF()
        {
            double b = _x2;
            double sigma25 = (_x3 - _x1) / 2.0;
            double sigma = sigma25 / 2.5;
            return new NormalMembershipFunction(b, sigma);
        }
    }


    /// <summary>
    /// Трапециевидная функция принадлежности
    /// </summary>
    public class TrapezoidMembershipFunction : IMembershipFunction
    {
        double _x1, _x2, _x3, _x4;


        /// <summary>
        /// Конструктор
        /// </summary>
        public TrapezoidMembershipFunction()
        { }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="x1">Первая точка</param>
        /// <param name="x2">Вторая точка</param>
        /// <param name="x3">Третья точка</param>
        /// <param name="x4">Четвертая точка</param>
        public TrapezoidMembershipFunction(double x1, double x2, double x3, double x4)
        {
            if (!(x1 <= x2 && x2 <= x3 && x3 <= x4))
            {
                throw new ArgumentException();
            }

            _x1 = x1;
            _x2 = x2;
            _x3 = x3;
            _x4 = x4;
        }

        /// <summary>
        /// Первая точка
        /// </summary>
        public double X1
        {
            get { return _x1; }
            set { _x1 = value; }
        }

        /// <summary>
        /// Вторая точка
        /// </summary>
        public double X2
        {
            get { return _x2; }
            set { _x2 = value; }
        }

        /// <summary>
        /// Третья точка
        /// </summary>
        public double X3
        {
            get { return _x3; }
            set { _x3 = value; }
        }

        /// <summary>
        /// Четвертая точка
        /// </summary>
        public double X4
        {
            get { return _x4; }
            set { _x4 = value; }
        }

        /// <summary>
        /// Вычисление значения функции принадлежности
        /// </summary>
        /// <param name="x">Аргумент функции</param>
        /// <returns></returns>
        public double GetValue(double x)
        {
            double result = 0;

            if (x == _x1 && x == _x2)
            {
                result = 1.0;
            }
            else if (x == _x3 && x == _x4)
            {
                result = 1.0;
            }
            else if (x <= _x1 || x >= _x4)
            {
                result = 0;
            }
            else if ((x >= _x2) && (x <= _x3))
            {
                result = 1;
            }
            else if ((x > _x1) && (x < _x2))
            {
                result = (x / (_x2 - _x1)) - (_x1 / (_x2 - _x1));
            }
            else
            {
                result = (-x / (_x4 - _x3)) + (_x4 / (_x4 - _x3));
            }

            return result;
        }
    }

    /// <summary>
    /// Нормально распределенная функция принадлежности
    /// </summary>
    public class NormalMembershipFunction : IMembershipFunction
    {
        double _b = 0.0, _sigma = 1.0;

        /// <summary>
        /// Конструктор
        /// </summary>
        public NormalMembershipFunction()
        { }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="b">Параметр b (центр функции принадлежности)</param>
        /// <param name="sigma">Разброс</param>
        public NormalMembershipFunction(double b, double sigma)
        {
            _b = b;
            _sigma = sigma;
        }

        /// <summary>
        /// Параметр b (центр функции принадлежности)
        /// </summary>
        public double B
        {
            get { return _b; }
            set { _b = value; }
        }

        /// <summary>
        /// Разброс
        /// </summary>
        public double Sigma
        {
            get { return _sigma; }
            set { _sigma = value; }
        }

        /// <summary>
        /// Вычисление значения функции принадлежности
        /// </summary>
        /// <param name="x">Аргумент функции</param>
        /// <returns></returns>
        public double GetValue(double x)
        {
            return Math.Exp(-(x - _b) * (x - _b) / (2.0 * _sigma * _sigma));
        }
    }

    /// <summary>
    /// Синглтон
    /// </summary>
    public class ConstantMembershipFunction : IMembershipFunction
    {
        double _constValue;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="constValue">Константная значения</param>
        public ConstantMembershipFunction(double constValue)
        {
            if (constValue < 0.0 || constValue > 1.0)
            {
                throw new ArgumentException("Диапазон значений должен находиться в диапазоне [0,1]");
            }

            _constValue = constValue;
        }

        /// <summary>
        /// Вычисление значения функции принадлежности
        /// </summary>
        /// <param name="x">Агрумент функции</param>
        /// <returns></returns>
        public double GetValue(double x)
        {
            return _constValue;
        }
    }


    /// <summary>
    /// Составная функция принадлежности как составная из единичных функций принадлежности
    /// </summary>
    internal class CompositeMembershipFunction : IMembershipFunction
    {
        List<IMembershipFunction> _mfs = new List<IMembershipFunction>();
        MfCompositionType _composType;                                      // Метод композиции функций принадлежности

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="composType">Метод композиции функций принадлежности</param>
        public CompositeMembershipFunction(MfCompositionType composType)
        {
            _composType = composType;
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="composType">Метод композиции функций принадлежности</param>
        /// <param name="mf1">Первая функция принадлежности</param>
        /// <param name="mf2">Вторая функция принадлежности</param>
        public CompositeMembershipFunction(
            MfCompositionType composType,
            IMembershipFunction mf1,
            IMembershipFunction mf2) : this(composType)
        {
            _mfs.Add(mf1);
            _mfs.Add(mf2);
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="composType">Метод композиции функций принадлежности</param>
        /// <param name="mfs">Список функций принадлежности</param>
        public CompositeMembershipFunction(
                MfCompositionType composType,
                List<IMembershipFunction> mfs)
            : this(composType)
        {
            _mfs = mfs;
        }

        /// <summary>
        /// Список функций принадлежности
        /// </summary>
        public List<IMembershipFunction> MembershipFunctions
        {
            get { return _mfs; }
        }

        /// <summary>
        /// Метод композиции функций принадлежности
        /// </summary>
        public MfCompositionType CompositionType
        {
            get { return _composType; }
            set { _composType = value; }
        }

        /// <summary>
        /// Вычисление значения составной функции принадлежности
        /// </summary>
        /// <param name="x">Аргумент функции</param>
        /// <returns></returns>
        public double GetValue(double x)
        {
            if (_mfs.Count == 0)
            {
                return 0.0;
            }
            else if (_mfs.Count == 1)
            {
                return _mfs[0].GetValue(x);
            }
            else
            {
                double result = _mfs[0].GetValue(x);
                for (int i = 1; i < _mfs.Count; i++)
                {
                    result = Compose(result, _mfs[i].GetValue(x));
                }
                return result;
            }
        }
        /// <summary>
        /// Вспомогательный метод композиции функций принадлежности
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        double Compose(double val1, double val2)
        {
            switch (_composType)
            {
                case MfCompositionType.Max:
                    return Math.Max(val1, val2);
                case MfCompositionType.Min:
                    return Math.Min(val1, val2);
                case MfCompositionType.Prod:
                    return val1 * val2;
                case MfCompositionType.Sum:
                    return val1 + val2;
                default:
                    throw new Exception("Internal exception.");
            }
        }
    }

  
}
