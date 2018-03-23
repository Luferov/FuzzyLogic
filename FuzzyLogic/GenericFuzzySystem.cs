/*
 *  FuzzyLogic: FuzzyLogic Library for C#
 *  Copyright (C) 2017 Луферов Виктор (lyferov@yandex.ru)
 */

using System;
using System.Collections.Generic;


namespace FuzzyLogic
{
    /// <summary>
    /// Функционал нечетких систем мамдани и сугено
    /// </summary>
    public class GenericFuzzySystem
    {
        List<FuzzyVariable> _input = new List<FuzzyVariable>();
        AndMethod _andMethod = AndMethod.Min;
        OrMethod _orMethod = OrMethod.Max;

        /// <summary>
        /// Входные лингвистические переменные
        /// </summary>
        public List<FuzzyVariable> Input
        {
            get { return _input; }
        }

        /// <summary>
        /// "И" метод
        /// </summary>
        public AndMethod AndMethod
        {
            get { return _andMethod; }
            set { _andMethod = value; }
        }

        /// <summary>
        /// "ИЛИ" метод
        /// </summary>
        public OrMethod OrMethod
        {
            get { return _orMethod; }
            set { _orMethod = value; }
        }

        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        protected GenericFuzzySystem(){}

        /// <summary>
        /// Получить входную лингвистическую переменную по имени
        /// </summary>
        /// <param name="name">Имя переменной</param>
        /// <returns>Найденная переменная</returns>
        public FuzzyVariable InputByName(string name)
        {
            foreach (FuzzyVariable var in Input)
            {
                if (var.Name == name)
                {
                    return var;
                }
            }
            throw new KeyNotFoundException();
        }

        #region Intermidiate calculations

        /// <summary>
        /// Fuzzify input
        /// </summary>
        /// <param name="inputValues"></param>
        /// <returns></returns>
        public Dictionary<FuzzyVariable, Dictionary<FuzzyTerm, double>> Fuzzify(Dictionary<FuzzyVariable, double> inputValues)
        {
            //
            // Validate input
            //
            string msg;
            if (!ValidateInputValues(inputValues, out msg))
            {
                throw new ArgumentException(msg);
            }

            //
            // Fill results list
            //
            Dictionary<FuzzyVariable, Dictionary<FuzzyTerm, double>> result = new Dictionary<FuzzyVariable, Dictionary<FuzzyTerm, double>>();
            foreach (FuzzyVariable var in Input)
            {
                Dictionary<FuzzyTerm, double> resultForVar = new Dictionary<FuzzyTerm, double>();
                foreach (FuzzyTerm term in var.Terms)
                {
                    double fv = term.MembershipFunction.GetValue(inputValues[var]);
                    if (double.IsNaN(fv)) fv = 0;
                    resultForVar.Add(term, fv);
                }
                result.Add(var, resultForVar);
            }

            return result;
        }

        #endregion


        #region Helpers

        /// <summary>
        /// Подсчет нечеткого состояние
        /// </summary>
        /// <param name="condition">Состояние которое должно быть подсчитано</param>
        /// <param name="fuzzifiedInput">Входы в нечеткой форме</param>
        /// <returns>Результат нечеткого состояния</returns>
        protected double EvaluateCondition(ICondition condition, Dictionary<FuzzyVariable, Dictionary<FuzzyTerm, double>> fuzzifiedInput)
        {
            if (condition is Conditions)                            //Если состояние
            {
                double result = 0.0;
                Conditions conds = (Conditions)condition;

                if (conds.ConditionsList.Count == 0)
                {
                    throw new Exception("Состояний нет.");
                }
                else if (conds.ConditionsList.Count == 1)
                {
                    result = EvaluateCondition(conds.ConditionsList[0], fuzzifiedInput);
                }
                else
                {
                    result = EvaluateCondition(conds.ConditionsList[0], fuzzifiedInput);
                    for (int i = 1; i < conds.ConditionsList.Count; i++)
                    {
                        result = EvaluateConditionPair(result, EvaluateCondition(conds.ConditionsList[i], fuzzifiedInput), conds.Op);
                    }
                }

                if (conds.Not)
                {
                    result = 1.0 - result;
                }

                return result;
            }
            else if (condition is FuzzyCondition)
            {
                FuzzyCondition cond = (FuzzyCondition)condition;
                double result = fuzzifiedInput[(FuzzyVariable)cond.Var][(FuzzyTerm)cond.Term];

                switch (cond.Hedge)
                {
                    case HedgeType.Slightly:
                        result = Math.Pow(result, 1.0 / 3.0); //Cube root
                        break;
                    case HedgeType.Somewhat:
                        result = Math.Sqrt(result);
                        break;
                    case HedgeType.Very:
                        result = result * result;
                        break;
                    case HedgeType.Extremely:
                        result = Math.Pow(result, 3);
                        break;
                    default:
                        break;
                }

                if (cond.Not)
                {
                    result = 1.0 - result;
                }
                return result;
            }
            else
            {
                throw new Exception("Типо состояния не найден.");
            }
        }

        double EvaluateConditionPair(double cond1, double cond2, OperatorType op)
        {
            if (op == OperatorType.And)
            {
                if (AndMethod == AndMethod.Min)
                {
                    return Math.Min(cond1, cond2);
                }
                else if (AndMethod == AndMethod.Production)
                {
                    return cond1 * cond2;
                }
                else
                {
                    throw new Exception("Оператор метода И не найден.");
                }
            }
            else if (op == OperatorType.Or)
            {
                if (OrMethod == OrMethod.Max)
                {
                    return Math.Max(cond1, cond2);
                }
                else if (OrMethod == OrMethod.Probabilistic)
                {
                    return cond1 + cond2 - cond1 * cond2;
                }
                else
                {
                    throw new Exception("Оператор метода ИЛИ не найден.");
                }
            }
            else
            {
                throw new Exception("Операторы композиции не найдены");
            }
        }

        private bool ValidateInputValues(Dictionary<FuzzyVariable, double> inputValues, out string msg)
        {
            msg = null;
            if (inputValues.Count != Input.Count)
            {
                msg = "Количество входных значений не верно";
                return false;
            }

            foreach (FuzzyVariable var in Input)
            {
                if (inputValues.ContainsKey(var))
                {
                    double val = inputValues[var];
                    // TODO: временно убрана проверка на диапазон
                    /*
                    if (val < var.Min || val > var.Max)
                    {
                        msg = string.Format("Значение для '{0}' переменной вне диапазона.", var.Name);
                        return false;
                    }*/
                }
                else
                {
                    msg = string.Format("Значение для переменной '{0}' не найдено.", var.Name);
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}
