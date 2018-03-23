/*
 *  FuzzyLogic: FuzzyLogic Library for C#
 *  Copyright (C) 2017 Луферов Виктор (lyferov@yandex.ru)
 */

using System;
using System.Collections.Generic;


namespace FuzzyLogic
{
    /// <summary>
    /// Лингвистическая переменная
    /// </summary>
    public class FuzzyVariable : NamedVariableImpl
    {
        double _min = 0.0, _max = 10.0;
        List<FuzzyTerm> _terms = new List<FuzzyTerm>();

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="name">Имя лингвистической переменной</param>
        /// <param name="min">Минимальное значение переменной</param>
        /// <param name="max">Максимальное значение переменной</param>
        public FuzzyVariable(string name, double min, double max) : base(name)
        {
            if (min > max)
            {
                throw new ArgumentException("Максимальное значение должно быть больше чем минимальное.");
            }

            _min = min;
            _max = max;
        }

        /// <summary>
        /// Список лингвистических термов
        /// </summary>
        public List<FuzzyTerm> Terms
        {
            get { return _terms; }
        }

        /// <summary>
        /// Именованные значения
        /// </summary>
        public override List<INamedValue> Values
        {
            get
            {
                List<INamedValue> result = new List<INamedValue>();
                foreach (FuzzyTerm term in _terms)
                {
                    result.Add(term);
                }
                return result;
            }
        }

        /// <summary>
        /// Получение функции принадлежности(терма) по имени
        /// </summary>
        /// <param name="name">Имя терма</param>
        /// <returns></returns>
        public FuzzyTerm GetTermByName(string name)
        {
            foreach (FuzzyTerm term in _terms)
            {
                if (term.Name == name)
                {
                    return term;
                }
            }

            throw new KeyNotFoundException();
        }

        /// <summary>
        /// Максимальное значение переменной
        /// </summary>
        public double Max
        {
            get { return _max; }
            set { _max = value; }
        }

        /// <summary>
        /// Минимальное значение переменной
        /// </summary>
        public double Min
        {
            get { return _min; }
            set { _min = value; }
        }
    }
}
