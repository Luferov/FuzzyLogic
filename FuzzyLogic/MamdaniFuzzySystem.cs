/*
 *  FuzzyLogic: FuzzyLogic Library for C#
 *  Copyright (C) 2017 Луферов Виктор (lyferov@yandex.ru)
 */
using System;
using System.Collections.Generic;

namespace FuzzyLogic
{
    /// <summary>
    /// Нечеткая система вывода мамдани
    /// </summary>
    public class MamdaniFuzzySystem : GenericFuzzySystem
    {
        List<FuzzyVariable> _output = new List<FuzzyVariable>();
        List<MamdaniFuzzyRule> _rules = new List<MamdaniFuzzyRule>();

        ImplicationMethod _implMethod = ImplicationMethod.Min;
        AggregationMethod _aggrMethod = AggregationMethod.Max;
        DefuzzificationMethod _defuzzMethod = DefuzzificationMethod.Centroid;

        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public MamdaniFuzzySystem() {}

        /// <summary>
        /// Выходные лингвистические переменные
        /// </summary>
        public List<FuzzyVariable> Output
        {
            get { return _output; }
        }

        /// <summary>
        /// Нечеткие правила
        /// </summary>
        public List<MamdaniFuzzyRule> Rules
        {
            get { return _rules; }
        }

        /// <summary>
        /// Метод имликации
        /// </summary>
        public ImplicationMethod ImplicationMethod
        {
            get { return _implMethod; }
            set { _implMethod = value; }
        }

        /// <summary>
        /// Метод агрегации
        /// </summary>
        public AggregationMethod AggregationMethod
        {
            get { return _aggrMethod; }
            set { _aggrMethod = value; }
        }

        /// <summary>
        /// Метод дефаззификации
        /// </summary>
        public DefuzzificationMethod DefuzzificationMethod
        {
            get { return _defuzzMethod; }
            set { _defuzzMethod = value; }
        }

        /// <summary>
        /// Получение выходных переменных по их имени
        /// </summary>
        /// <param name="name">Имя переменной</param>
        /// <returns>Найденная переменная</returns>
        public FuzzyVariable OutputByName(string name)
        {
            foreach (FuzzyVariable var in _output)
            {
                if (var.Name == name)
                {
                    return var;
                }
            }

            throw new KeyNotFoundException();
        }

        /// <summary>
        /// Создание нового пустого правила
        /// </summary>
        /// <returns></returns>
        public MamdaniFuzzyRule EmptyRule()
        {
            return new MamdaniFuzzyRule();
        }

        /// <summary>
        /// Разбор правила из строки
        /// </summary>
        /// <param name="rule">Строка, содержащая правило</param>
        /// <returns></returns>
        public MamdaniFuzzyRule ParseRule(string rule)
        {
            return RuleParser<MamdaniFuzzyRule, FuzzyVariable, FuzzyTerm>.Parse(rule, EmptyRule(), Input, Output);
        }

        /// <summary>
        /// Вычисление выходного значения
        /// </summary>
        /// <param name="inputValues">Входные значения (формат: переменная - значение)</param>
        /// <returns>Выходные значения (формат: переменная - значение)</returns>
        public Dictionary<FuzzyVariable, double> Calculate(Dictionary<FuzzyVariable, double> inputValues)
        {
            //
            // Как минимум должно быть одной правило
            //
            if (_rules.Count == 0)
            {
                throw new Exception("Должно быть как минимум одно правило.");
            }

            //
            // Шаг фаззификации
            //
            Dictionary<FuzzyVariable, Dictionary<FuzzyTerm, double>> fuzzifiedInput =
                Fuzzify(inputValues);

            //
            // Вычисление состояний
            //
            Dictionary<MamdaniFuzzyRule, double> evaluatedConditions = EvaluateConditions(fuzzifiedInput);

            //
            // Последствия для каждого состояния
            //
            Dictionary<MamdaniFuzzyRule, IMembershipFunction> implicatedConclusions = Implicate(evaluatedConditions);

            //
            // Агрегация результатов
            //
            Dictionary<FuzzyVariable, IMembershipFunction> fuzzyResult = Aggregate(implicatedConclusions);

            //
            // Дефаззификации результатов
            //
            Dictionary<FuzzyVariable, double> result = Defuzzify(fuzzyResult);

            return result;
        }


        #region Промежуточные расчеты

        /// <summary>
        /// Вычисление состояний
        /// </summary>
        /// <param name="fuzzifiedInput">Входящие состояния в фаззифицированной форме</param>
        /// <returns>Результат вычислений</returns>
        public Dictionary<MamdaniFuzzyRule, double> EvaluateConditions(Dictionary<FuzzyVariable, Dictionary<FuzzyTerm, double>> fuzzifiedInput)
        {
            Dictionary<MamdaniFuzzyRule, double> result = new Dictionary<MamdaniFuzzyRule, double>();
            foreach (MamdaniFuzzyRule rule in Rules)
            {
                result.Add(rule, EvaluateCondition(rule.Condition, fuzzifiedInput));
            }

            return result;
        }


        /// <summary>
        /// Импликация результатов правила
        /// </summary>
        /// <param name="conditions">Состояния правила</param>
        /// <returns>Импликативное заключение</returns>
        public Dictionary<MamdaniFuzzyRule, IMembershipFunction> Implicate(Dictionary<MamdaniFuzzyRule, double> conditions)
        {
            Dictionary<MamdaniFuzzyRule, IMembershipFunction> conclusions = new Dictionary<MamdaniFuzzyRule, IMembershipFunction>();
            foreach (MamdaniFuzzyRule rule in conditions.Keys)
            {
                MfCompositionType compType;
                switch (_implMethod)
                {
                    case ImplicationMethod.Min:
                        compType = MfCompositionType.Min;
                        break;
                    case ImplicationMethod.Production:
                        compType = MfCompositionType.Prod;
                        break;
                    default:
                        throw new Exception("Internal error.");
                }

                CompositeMembershipFunction resultMf = new CompositeMembershipFunction(
                    compType,
                    new ConstantMembershipFunction(conditions[rule]),
                    ((FuzzyTerm)rule.Conclusion.Term).MembershipFunction);
                conclusions.Add(rule, resultMf);
            }

            return conclusions;
        }


        /// <summary>
        /// Агрегация результатов
        /// </summary>
        /// <param name="conclusions">Результаты правил</param>
        /// <returns>Агрегированные нечеткие правила</returns>
        public Dictionary<FuzzyVariable, IMembershipFunction> Aggregate(Dictionary<MamdaniFuzzyRule, IMembershipFunction> conclusions)
        {
            Dictionary<FuzzyVariable, IMembershipFunction> fuzzyResult = new Dictionary<FuzzyVariable, IMembershipFunction>();
            foreach (FuzzyVariable var in Output)
            {
                List<IMembershipFunction> mfList = new List<IMembershipFunction>();
                foreach (MamdaniFuzzyRule rule in conclusions.Keys)
                {
                    if (rule.Conclusion.Var == var)
                    {
                        mfList.Add(conclusions[rule]);
                    }
                }

                MfCompositionType composType;
                switch (_aggrMethod)
                {
                    case AggregationMethod.Max:
                        composType = MfCompositionType.Max;
                        break;
                    case AggregationMethod.Sum:
                        composType = MfCompositionType.Sum;
                        break;
                    default:
                        throw new Exception("Метод агрегации не найден.");
                }
                fuzzyResult.Add(var, new CompositeMembershipFunction(composType, mfList));
            }

            return fuzzyResult;
        }

        /// <summary>
        /// Вычисление четкого результат для каждого правила
        /// </summary>
        /// <param name="fuzzyResult"></param>
        /// <returns></returns>
        public Dictionary<FuzzyVariable, double> Defuzzify(Dictionary<FuzzyVariable, IMembershipFunction> fuzzyResult)
        {
            Dictionary<FuzzyVariable, double> crispResult = new Dictionary<FuzzyVariable, double>();
            foreach (FuzzyVariable var in fuzzyResult.Keys)
            {
                crispResult.Add(var, Defuzzify(fuzzyResult[var], var.Min, var.Max));
            }

            return crispResult;
        }

        #endregion
        #region Вспомогательные функции

        double Defuzzify(IMembershipFunction mf, double min, double max)
        {
            if (_defuzzMethod == DefuzzificationMethod.Centroid)
            {
                int k = 50;
                double step = (max - min) / k;

                //
                // Вычисление центра гравитации как интеграла
                //
                double ptLeft = 0.0;
                double ptCenter = 0.0;
                double ptRight = 0.0;

                double valLeft = 0.0;
                double valCenter = 0.0;
                double valRight = 0.0;

                double val2Left = 0.0;
                double val2Center = 0.0;
                double val2Right = 0.0;

                double numerator = 0.0;
                double denominator = 0.0;
                for (int i = 0; i < k; i++)
                {
                    if (i == 0)
                    {
                        ptRight = min;
                        valRight = mf.GetValue(ptRight);
                        val2Right = ptRight * valRight;
                    }

                    ptLeft = ptRight;
                    ptCenter = min + step * ((double)i + 0.5);
                    ptRight = min + step * (i + 1);

                    valLeft = valRight;
                    valCenter = mf.GetValue(ptCenter);
                    valRight = mf.GetValue(ptRight);

                    val2Left = val2Right;
                    val2Center = ptCenter * valCenter;
                    val2Right = ptRight * valRight;

                    numerator += step * (val2Left + 4 * val2Center + val2Right) / 3.0;
                    denominator += step * (valLeft + 4 * valCenter + valRight) / 3.0;
                }

                return numerator / denominator;
            }
            else if (_defuzzMethod == DefuzzificationMethod.Bisector)
            {
                // TODO:
                throw new NotSupportedException();
            }
            else if (_defuzzMethod == DefuzzificationMethod.AverageMaximum)
            {
                // TODO:
                throw new NotSupportedException();
            }
            else
            {
                throw new Exception("Метод дефаззификации не найден.");
            }
        }

        #endregion
    }
}
