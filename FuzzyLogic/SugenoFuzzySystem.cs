/*
 *  FuzzyLogic: FuzzyLogic Library for C#
 *  Copyright (C) 2017 Луферов Виктор (lyferov@yandex.ru)
 */

using System;
using System.Collections.Generic;

namespace FuzzyLogic
{
    /// <summary>
    /// Нечетка система сугено
    /// </summary>
    public class SugenoFuzzySystem : GenericFuzzySystem
    {
        List<SugenoVariable> _output = new List<SugenoVariable>();
        List<SugenoFuzzyRule> _rules = new List<SugenoFuzzyRule>();

        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public SugenoFuzzySystem()
        { }

        /// <summary>
        /// Выходные переменные
        /// </summary>
        public List<SugenoVariable> Output
        {
            get { return _output; }
        }

        /// <summary>
        /// Список правил
        /// </summary>
        public List<SugenoFuzzyRule> Rules
        {
            get { return _rules; }
        }

        /// <summary>
        /// Получить выходную переменную по имени
        /// </summary>
        /// <param name="name">Имя переменноой</param>
        /// <returns>Найденная переменная</returns>
        public SugenoVariable OutputByName(string name)
        {
            foreach (SugenoVariable var in _output)
            {
                if (var.Name == name)
                {
                    return var;
                }
            }

            throw new KeyNotFoundException();
        }

        /// <summary>
        /// Использование этого метода для создания линейной функции для нечеткой системы Сугено
        /// </summary>
        /// <param name="name">Имя функции</param>
        /// <param name="coeffs">Список коэффициентов. Длина списка должна быть меньше или эквивалентна выходным переменным.</param>
        /// <param name="constValue"></param>
        /// <returns>Созданная функция</returns>
        public LinearSugenoFunction CreateSugenoFunction(string name, Dictionary<FuzzyVariable, double> coeffs, double constValue)
        {
            return new LinearSugenoFunction(name, Input, coeffs, constValue);
        }

        /// <summary>
        /// Использование этого метода для создания линейной функции для нечеткой системы Сугено
        /// </summary>
        /// <param name="name">Имя функции</param>
        /// <param name="coeffs">Список коэффициентов. Длина списка должна быть меньше или эквивалентна выходным переменным.</param>
        /// <returns>Созданная функция</returns>
        public LinearSugenoFunction CreateSugenoFunction(string name, double[] coeffs)
        {
            return new LinearSugenoFunction(name, this.Input, coeffs);
        }

        /// <summary>
        /// Использование метода для создания пустого правила
        /// </summary>
        /// <returns>Созданное правило</returns>
        public SugenoFuzzyRule EmptyRule()
        {
            return new SugenoFuzzyRule();
        }

        /// <summary>
        /// Метод для создания правила в текстовой фоме
        /// </summary>
        /// <param name="rule">Правило в текстовой форме</param>
        /// <returns>Созданное правило</returns>
        public SugenoFuzzyRule ParseRule(string rule)
        {
            return RuleParser<SugenoFuzzyRule, SugenoVariable, ISugenoFunction>.Parse(rule, EmptyRule(), Input, Output);
        }

        /// <summary>
        /// Вычисление состояний
        /// </summary>
        /// <param name="fuzzifiedInput">Входные переменные в фаззифицированной форме</param>
        /// <returns>Результат вычисления</returns>
        public Dictionary<SugenoFuzzyRule, double> EvaluateConditions(Dictionary<FuzzyVariable, Dictionary<FuzzyTerm, double>> fuzzifiedInput)
        {
            Dictionary<SugenoFuzzyRule, double> result = new Dictionary<SugenoFuzzyRule, double>();
            foreach (SugenoFuzzyRule rule in Rules)
            {
                result.Add(rule, EvaluateCondition(rule.Condition, fuzzifiedInput));
            }

            return result;
        }

        /// <summary>
        /// Вычисление результат функций
        /// </summary>
        /// <param name="inputValues">Входные значения</param>
        /// <returns>Результаты</returns>
        public Dictionary<SugenoVariable, Dictionary<ISugenoFunction, double>> EvaluateFunctions(Dictionary<FuzzyVariable, double> inputValues)
        {
            Dictionary<SugenoVariable, Dictionary<ISugenoFunction, double>> result = new Dictionary<SugenoVariable, Dictionary<ISugenoFunction, double>>();

            foreach (SugenoVariable var in Output)
            {
                Dictionary<ISugenoFunction, double> varResult = new Dictionary<ISugenoFunction, double>();

                foreach (ISugenoFunction func in var.Functions)
                {
                    varResult.Add(func, func.Evaluate(inputValues));
                }

                result.Add(var, varResult);
            }
            return result;
        }

        /// <summary>
        /// Объединение результатов функций и правил
        /// </summary>
        /// <param name="ruleWeights">Весовые правила (Результат вычислений)</param>
        /// <param name="functionResults">Результат вычисления функций</param>
        /// <returns>Результат вычислений</returns>
        public Dictionary<SugenoVariable, double> CombineResult(Dictionary<SugenoFuzzyRule, double> ruleWeights, Dictionary<SugenoVariable, Dictionary<ISugenoFunction, double>> functionResults)
        {
            Dictionary<SugenoVariable, double> numerators = new Dictionary<SugenoVariable, double>();
            Dictionary<SugenoVariable, double> denominators = new Dictionary<SugenoVariable, double>();
            Dictionary<SugenoVariable, double> results = new Dictionary<SugenoVariable, double>();

            //
            // Вычисление числителя и знаменателя для каждого входа
            //
            foreach (SugenoVariable var in Output)
            {
                numerators.Add(var, 0.0);
                denominators.Add(var, 0.0);
            }

            foreach (SugenoFuzzyRule rule in ruleWeights.Keys)
            {
                SugenoVariable var = rule.Conclusion.Var;
                double z = functionResults[var][rule.Conclusion.Term];
                double w = ruleWeights[rule];

                numerators[var] += z * w;
                denominators[var] += w;
            }

            //
            // Расчет фракций
            //
            foreach (SugenoVariable var in Output)
            {
                if (denominators[var] == 0.0)
                {
                    results[var] = 0.0;
                }
                else
                {
                    results[var] = numerators[var] / denominators[var];
                }
            }

            return results;
        }

        /// <summary>
        /// Вычисление выхода нечеткой системы Сугены
        /// </summary>
        /// <param name="inputValues">Входные значения</param>
        /// <returns>Выходные значения</returns>
        public Dictionary<SugenoVariable, double> Calculate(Dictionary<FuzzyVariable, double> inputValues)
        {
            //
            // Должно быть как минимум одно правило
            //
            if (_rules.Count == 0)
            {
                throw new Exception("Должно быть как минимум одно правило.");
            }

            //
            // Этап фаззификации
            //
            Dictionary<FuzzyVariable, Dictionary<FuzzyTerm, double>> fuzzifiedInput =
                Fuzzify(inputValues);

            //
            // Агрегация подусловий
            //
            Dictionary<SugenoFuzzyRule, double> ruleWeights = EvaluateConditions(fuzzifiedInput);

            //
            // Вычисление функций
            //
            Dictionary<SugenoVariable, Dictionary<ISugenoFunction, double>> functionsResult = EvaluateFunctions(inputValues);

            //
            // Объединение результатов
            //
            Dictionary<SugenoVariable, double> result = CombineResult(ruleWeights, functionsResult);

            return result;
        }
    }
}
