/*
 *  FuzzyLogic: FuzzyLogic Library for C#
 *  Copyright (C) 2017 Луферов Виктор (lyferov@yandex.ru)
 */

using System;
using System.Collections.Generic;

namespace FuzzyLogic
{

    //Псевдоним для нечеткого вывода для Мамдани
    using FuzzyConclusion = SingleCondition<FuzzyVariable, FuzzyTerm>;
    //Псевдоним для нечеткого вывода для Сугено
    using SugenoConclusion = SingleCondition<SugenoVariable, ISugenoFunction>;
    
    /// <summary>
    /// Состояние нечеткого правила для систем Мамдани и Сугено
    /// </summary>
    public class FuzzyCondition : SingleCondition<FuzzyVariable, FuzzyTerm>
    {
        HedgeType _hedge = HedgeType.None;

        /// <summary>
        /// Модификатор
        /// </summary>
        public HedgeType Hedge {
            get { return _hedge; }
            set { _hedge = value; }
        }
        internal FuzzyCondition(FuzzyVariable var, FuzzyTerm term) : this(var, term, false) { }
        internal FuzzyCondition(FuzzyVariable var, FuzzyTerm term, bool not) : this(var, term, not, HedgeType.None) { }
        internal FuzzyCondition(FuzzyVariable var, FuzzyTerm term, bool not, HedgeType hedge)
            : base(var, term, not)
        {
            _hedge = hedge;
        }

    }

    #region Переключатели модификаторов и операторов
    public enum OperatorType {  //Тип оператора
        And,    //Оператор "и"
        Or      //Оператор "или"
    }

    public enum HedgeType { //Hedge модификатор
        None,       //Нет
        Slightly,   //Корень кубический
        Somewhat,   //Корень квадратный
        Very,       //Квадрат
        Extremely   //Куб

    }
    #endregion

    /// <summary>
    /// Интерфейс условий используемый в if выражениях
    /// </summary>
    public interface ICondition { }

    public class SingleCondition<VariableType, ValueType> : ICondition
        where ValueType : class, INamedValue
        where VariableType : class, INamedVariable
    {
        VariableType _var = null;
        ValueType _term = null;
        bool _not = false;

        /// <summary>
        /// Лингвистическая переменная с которой связанно условие
        /// </summary>
        public VariableType Var {
            get { return _var; }
            set { _var = value; }
        }
        /// <summary>
        /// Термин выражения "переменная является термом"
        /// </summary>
        public ValueType Term {
            get { return _term; }
            set { _term = value; }
        }
        /// <summary>
        /// Содержит ли условие "not"
        /// </summary>
        public bool Not {
            get { return _not; }
            set { _not = value; }
        }

        internal SingleCondition() { }
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="var">Лингвистическая переменная с которой связанно условие</param>
        /// <param name="term">Термин выражения "переменная является термом"</param>
        internal SingleCondition(VariableType var, ValueType term)
        {
            _var = var;
            _term = term;
        }
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="var">Лингвистическая переменная с которой связанно условие</param>
        /// <param name="term">Термин выражения "переменная является термом"</param>
        /// <param name="not">Содержит ли условие "not"</param>
        internal SingleCondition(VariableType var, ValueType term, bool not) : this(var, term)
        {
            _not = not;
        }
    }

    public class Conditions : ICondition
    {
        bool _not = false;
        OperatorType _op = OperatorType.And;
        List<ICondition> _Conditions = new List<ICondition>();
        /// <summary>
        /// Содержит ли условие "not"
        /// </summary>
        public bool Not {
            get { return _not; }
            set { _not = value; }
        }
        /// <summary>
        /// Оператор, который связывает выражение (or/and)
        /// </summary>
        public OperatorType Op {
            get { return _op; }
            set { _op = value; }
        }
        /// <summary>
        /// Список условий (единственное или множество)
        /// </summary>
        public List<ICondition> ConditionsList {
            get { return _Conditions; }
        }
    }
    /// <summary>
    /// Интерфейс используемый парсером правил
    /// </summary>
    /// <typeparam name="OutputVariableType"></typeparam>
    /// <typeparam name="OutputValueType"></typeparam>
    interface IParsableRule<OutputVariableType, OutputValueType>
        where OutputVariableType : class, INamedVariable
        where OutputValueType : class, INamedValue
    {
        /// <summary>
        /// Условие (IF) части правила
        /// </summary>
        Conditions Condition { get; set; }
        /// <summary>
        /// Вывод (THEN) части правиала
        /// </summary>
        SingleCondition<OutputVariableType, OutputValueType> Conclusion { get; set; }
    }
    /// <summary>
    /// Общая функциональность нечетких правил
    /// </summary>
    public abstract class GenericFuzzyRule
    {
        Conditions _condition = new Conditions();
        /// <summary>
        /// Условие (IF), часть правила
        /// </summary>
        public Conditions Condition {
            get { return _condition; }
            set { _condition = value; }
        }
        /// <summary>
        /// Создается единственное состояние
        /// </summary>
        /// <param name="var">Лингвистическая переменная к которой привязано условие</param>
        /// <param name="term">Терм в выражении "Переменная есть терм"</param>
        /// <returns>Созданное состояние</returns>
        public FuzzyCondition CreateCondition(FuzzyVariable var, FuzzyTerm term)
        {
            return new FuzzyCondition(var, term);
        }
        /// <summary>
        /// Создается единственное состояние
        /// </summary>
        /// <param name="var">Лингвистическая переменная к которой привязано условие</param>
        /// <param name="term">Терм в выражении "Переменная есть терм"</param>
        /// <param name="not">Содержит ли состояние отрицание</param>
        /// <returns>Созданное состояние</returns>
        public FuzzyCondition CreateCondition(FuzzyVariable var, FuzzyTerm term, bool not)
        {
            return new FuzzyCondition(var, term, not);
        }
        /// <summary>
        /// Создается единственное состояние
        /// </summary>
        /// <param name="var">Лингвистическая переменная к которой привязано условие</param>
        /// <param name="term">Терм в выражении "Переменная есть терм"</param>
        /// <param name="not">Содержит ли состояние отрицание</param>
        /// <param name="type">Модификатор</param>
        /// <returns>Созданное состояние</returns>
        public FuzzyCondition CreateCondition(FuzzyVariable var, FuzzyTerm term, bool not, HedgeType type)
        {
            return new FuzzyCondition(var, term, not, type);
        }
    }
    /// <summary>
    /// Нечеткое праивло для нечеткой системы мамдани
    /// </summary>
    public class MamdaniFuzzyRule : GenericFuzzyRule, IParsableRule<FuzzyVariable, FuzzyTerm>
    {
        FuzzyConclusion _conclusion = new FuzzyConclusion();
        double _weight = 1.0;
        /// <summary>
        /// Конструктор. Примечание: правило не может быть создано напрямую, только через EmptyRule или ParseRule
        /// </summary>
        internal MamdaniFuzzyRule() { }

        /// <summary>
        /// Вывод (THEN), часть правила
        /// </summary>
        public FuzzyConclusion Conclusion
        {
            get { return _conclusion; }
            set { _conclusion = value; }
        }
        /// <summary>
        /// Вес правила
        /// </summary>
        public double Weight {
            get { return _weight; }
            set { _weight = value; }
        }
    }


    public class SugenoFuzzyRule : GenericFuzzyRule, IParsableRule<SugenoVariable, ISugenoFunction>
    {
        SugenoConclusion _conclusion = new SugenoConclusion();
        /// <summary>
        /// Конструктор. Примечание: правило не может быть создано напрямую, только через EmptyRule или ParseRule
        /// </summary>
        internal SugenoFuzzyRule() { }
        /// <summary>
        /// Вывод (THEN), часть правила 
        /// </summary>
        public SugenoConclusion Conclusion {
            get { return _conclusion; }
            set { _conclusion = value; }
        }
    }


}
