/*
 *  FuzzyLogic: FuzzyLogic Library for C#
 *  Copyright (C) 2017 Луферов Виктор (lyferov@yandex.ru)
 */

using System;
using System.Collections.Generic;

namespace FuzzyLogic
{
    /// <summary>
    /// Интерфейс, который должен быть реализован классом использующий выходной функционал в Сугено
    /// </summary>
    public interface ISugenoFunction : INamedValue
    {
        /// <summary>
        /// Подсчет результатов функции
        /// </summary>
        /// <param name="inputValues">Входные значения</param>
        /// <returns>Результат вычислений</returns>
        double Evaluate(Dictionary<FuzzyVariable, double> inputValues);
    }

    /// <summary>
    /// Линейная функция для нечеткой системы сугено (может быть создан как Sugeno FuzzySystem::CreateSugenoFunction methods)
    /// </summary>
    public class LinearSugenoFunction : NamedValueImpl, ISugenoFunction
    {
        List<FuzzyVariable> _input = null;
        Dictionary<FuzzyVariable, double> _coeffs = new Dictionary<FuzzyVariable, double>();
        double _constValue = 0.0;

        /// <summary>
        /// Получить или установить константу / чаще всего 0
        /// </summary>
        public double ConstValue
        {
            get { return _constValue; }
            set { _constValue = value; }
        }

        /// <summary>
        /// Получает коэффициент нечеткой переменной
        /// </summary>
        /// <param name="var">нечеткая переменная</param>
        /// <returns>Значение коэффициента</returns>
        public double GetCoefficient(FuzzyVariable var)
        {
            if (var == null)
            {
                return _constValue;
            }
            else
            {
                return _coeffs[var];
            }
        }

        /// <summary>
        /// Установить коэффициент нечеткой переменной
        /// </summary>
        /// <param name="var">Нечеткая переменная</param>
        /// <param name="coeff">Новое значение коэффициента</param>
        public void SetCoefficient(FuzzyVariable var, double coeff)
        {
            if (var == null)
            {
                _constValue = coeff;
            }
            else
            {
                _coeffs[var] = coeff;
            }
        }

        internal LinearSugenoFunction(string name, List<FuzzyVariable> input) : base(name)
        {
            _input = input;
        }

        internal LinearSugenoFunction(string name, List<FuzzyVariable> input, Dictionary<FuzzyVariable, double> coeffs, double constValue)
            : this(name, input)
        {
            //
            // Убедиться, что все коэффициенты связаны с переменной от входа
            //
            foreach (FuzzyVariable var in coeffs.Keys)
            {
                if (!_input.Contains(var))
                {
                    throw new ArgumentException(string.Format(
                        "Вход нечеткой системы не содержит '{0}' переменную.",
                        var.Name));
                }
            }

            //
            // Инициализация коэффициентов
            //
            _coeffs = coeffs;
            _constValue = constValue;
        }

        internal LinearSugenoFunction(string name, List<FuzzyVariable> input, double[] coeffs)
            : this(name, input)
        {
            //
            // Проверка входных значений
            //
            if (coeffs.Length != input.Count && coeffs.Length != input.Count + 1)
            {
                throw new ArgumentException("Неверная длинна массива коэффициентов и входных переменных");
            }

            //
            // Заполнить список коэффициентов
            //
            for (int i = 0; i < input.Count; i++)
            {
                _coeffs.Add(input[i], coeffs[i]);
            }

            if (coeffs.Length == input.Count + 1)
            {
                _constValue = coeffs[coeffs.Length - 1];
            }
        }

        /// <summary>
        /// Вычисление результатов линейной функции
        /// </summary>
        /// <param name="inputValues">Входные значения</param>
        /// <returns>Result of the calculation</returns>
        public double Evaluate(Dictionary<FuzzyVariable, double> inputValues)
        {
            //NOTE: Входные значения должны быть пройти валидацию тут
            double result = 0.0;

            foreach (FuzzyVariable var in _coeffs.Keys)
            {
                result += _coeffs[var] * inputValues[var];
            }
            result += _constValue;

            return result;
        }
    }

    /// <summary>
    /// Использование как выходные переменные в системе нечеткого вывода Сугено
    /// </summary>
    public class SugenoVariable : NamedVariableImpl
    {
        List<ISugenoFunction> _functions = new List<ISugenoFunction>();

        /// <summary>
        /// Cnstructor
        /// </summary>
        /// <param name="name">Имя переменной</param>
        public SugenoVariable(string name) : base(name)
        {
        }

        /// <summary>
        /// Список переменных, которые принадлежат переменной
        /// </summary>
        public List<ISugenoFunction> Functions
        {
            get { return _functions; }
        }

        /// <summary>
        /// Список функций, которые принадлежат переменной (имплементированно от INamedVariable)
        /// </summary>
        public override List<INamedValue> Values
        {
            get
            {
                List<INamedValue> values = new List<INamedValue>();
                foreach (ISugenoFunction val in _functions)
                {
                    values.Add(val);
                }
                return values;
            }
        }

        /// <summary>
        /// Найти функцию по имени
        /// </summary>
        /// <param name="name">Имя функции</param>
        /// <returns>Найденная функция</returns>
        public ISugenoFunction GetFuncByName(string name)
        {
            foreach (NamedValueImpl func in Values)
            {
                if (func.Name == name)
                {
                    return (ISugenoFunction)func;
                }
            }

            throw new KeyNotFoundException();
        }
    }
}
