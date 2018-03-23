/*
 *  FuzzyLogic: FuzzyLogic Library for C#
 *  Copyright (C) 2017 Луферов Виктор (lyferov@yandex.ru)
 */

using System;
using System.Collections.Generic;
using System.Text;


namespace FuzzyLogic
{
    #region Вспомогательные классы

    /// <summary>
    /// Интерфейс должен быть реализован значениями в парсере правил
    /// </summary>
    public interface INamedValue
    {
        /// <summary>
        /// Имя значения
        /// </summary>
        string Name { get; set; }
    }

    /// <summary>
    /// Интерфейс должен быть реализован переменными в парсере правил
    /// </summary>
    public interface INamedVariable
    {
        /// <summary>
        /// Имя переменной
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Список переменных, которые принадлежат переменной
        /// </summary>
        List<INamedValue> Values { get; }
    }

    /// <summary>
    /// Именованная переменная
    /// </summary>
    public abstract class NamedVariableImpl : INamedVariable
    {
        string _name = string.Empty;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="name">Имя переменной</param>
        public NamedVariableImpl(string name)
        {
            if (NameHelper.IsValidName(name))
            {
                throw new ArgumentException("Недопустимое имя переменной.");
            }
            _name = name;
        }

        /// <summary>
        /// Имя переменной
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                if (NameHelper.IsValidName(value))
                {
                    throw new ArgumentException("Недопустимое имя переменной.");
                }
                _name = value;
            }
        }

        /// <summary>
        /// Именованные значения
        /// </summary>
        public abstract List<INamedValue> Values { get; }
    }

    /// <summary>
    /// Именованные значения переменной
    /// </summary>
    public abstract class NamedValueImpl : INamedValue
    {
        string _name;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="name">Имя значения</param>
        public NamedValueImpl(string name)
        {
            if (NameHelper.IsValidName(name))
            {
                throw new ArgumentException("Недопустимое имя значения.");
            }

            _name = name;
        }

        /// <summary>
        /// Имя терма
        /// </summary>
        public string Name
        {
            set
            {
                if (NameHelper.IsValidName(value))
                {
                    throw new ArgumentException("Недопустимое имя терма.");
                }
                _name = value;
            }
            get { return _name; }
        }
    }

    internal class NameHelper
    {
        static public string[] KEYWORDS = new string[] { "if", "then", "is", "and", "or", "not", "(", ")", "slightly", "somewhat", "very", "extremely" };

        /// <summary>
        /// Проверка имен переменных/термов.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        static public bool IsValidName(string name)
        {
            //
            // Пустое имя недопустимо
            //
            if (name.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < name.Length; i++)
            {
                //
                // Только буквы, цифры и _ допустимы
                //
                if (!System.Char.IsDigit(name, i) ||
                    !System.Char.IsDigit(name, i) ||
                    name[i] != '_')
                {
                    return false;
                }
            }

            //
            // Идентификатор не может быть ключевым словом
            //
            foreach (string keyword in KEYWORDS)
            {
                if (name == keyword)
                {
                    return false;
                }
            }

            return true;
        }
    }

    #endregion


    /// <summary>
    /// Класс отвечающий за разбор правил
    /// </summary>
    internal class RuleParser<RuleType, OutputVariableType, OutputValueType>
        where OutputVariableType : class, INamedVariable
        where OutputValueType : class, INamedValue
        where RuleType : class, IParsableRule<OutputVariableType, OutputValueType>
    {
        #region Иерархия выражений правил
        /// <summary>
        /// Интерфейс выражения
        /// </summary>
        interface IExpression
        {
            string Text { get; }
        }
        /// <summary>
        /// Интерфейс лексемы
        /// </summary>
        abstract class Lexem : IExpression
        {
            public abstract string Text { get; }
            public override string ToString()
            {
                return Text;
            }
        }
        /// <summary>
        /// Выражение состояний
        /// </summary>
        class ConditionExpression : IExpression
        {
            List<IExpression> _expressions = null;
            FuzzyCondition _condition = null;

            public ConditionExpression(List<IExpression> expressions, FuzzyCondition condition)
            {
                _expressions = expressions;
                _condition = condition;
            }

            public List<IExpression> Expressions
            {
                get { return _expressions; }
                set { _expressions = value; }
            }

            public FuzzyCondition Condition
            {
                get { return _condition; }
                set { _condition = value; }
            }

            public string Text
            {
                get
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (IExpression ex in _expressions)
                    {
                        sb.Append(ex.Text);
                    }
                    return sb.ToString();
                }
            }
        }
        /// <summary>
        /// Ключевые слова лексем
        /// </summary>
        class KeywordLexem : Lexem
        {
            string _name;

            public KeywordLexem(string name)
            {
                _name = name;
            }

            public override string Text
            {
                get { return _name; }
            }
        }

        class VarLexem<VariableType> : Lexem
            where VariableType : class, INamedVariable
        {
            VariableType _var = null;
            bool _input = true;

            public VarLexem(VariableType var, bool input)
            {
                _var = var;
                _input = input;
            }

            public VariableType Var
            {
                get { return _var; }
                set { _var = value; }
            }

            public override string Text
            {
                get { return _var.Name; }
            }

            public bool Input
            {
                get { return _input; }
                set { _input = value; }
            }
        }
        /// <summary>
        /// Интерфейс альтернативных лексем
        /// </summary>
        interface IAltLexem
        {
            IAltLexem Alternative { get; set; }
        }
        /// <summary>
        /// Лексическая терма
        /// </summary>
        /// <typeparam name="ValueType"></typeparam>
        class TermLexem<ValueType> : Lexem, IAltLexem
            where ValueType : class, INamedValue
        {
            ValueType _term = null;
            IAltLexem _alternative = null;
            bool _input = true;

            public TermLexem(ValueType term, bool input)
            {
                _term = term;
                _input = input;
            }

            public ValueType Term
            {
                get { return _term; }
                set { _term = value; }
            }

            public override string Text
            {
                get { return _term.Name; }
            }

            public IAltLexem Alternative
            {
                get { return _alternative; }
                set { _alternative = value; }
            }
        }
        #endregion

        static private Dictionary<string, Lexem> BuildLexemsList(List<FuzzyVariable> input, List<OutputVariableType> output)
        {
            Dictionary<string, Lexem> lexems = new Dictionary<string, Lexem>();
            // Построение ключевых лексем
            foreach (string keyword in NameHelper.KEYWORDS)
            {
                KeywordLexem keywordLexem = new KeywordLexem(keyword);
                lexems.Add(keywordLexem.Text, keywordLexem);
            }
            // Построение лексем входных переменных
            foreach (FuzzyVariable var in input)
            {
                BuildLexemsList<FuzzyVariable, FuzzyTerm>(var, true, lexems);
            }
            // Построение лексем выходных переменных
            foreach (OutputVariableType var in output)
            {
                BuildLexemsList<OutputVariableType, OutputValueType>(var, false, lexems);
            }
            return lexems;
        }

        static private void BuildLexemsList<VariableType, ValueType>(VariableType var, bool input, Dictionary<string, Lexem> lexems)
            where VariableType : class, INamedVariable
            where ValueType : class, INamedValue
        {
            VarLexem<VariableType> varLexem = new VarLexem<VariableType>(var, input);
            lexems.Add(varLexem.Text, varLexem);
            foreach (ValueType term in var.Values)
            {
                TermLexem<ValueType> termLexem = new TermLexem<ValueType>(term, input);

                Lexem foundLexem = null;
                if (!lexems.TryGetValue(termLexem.Text, out foundLexem))
                {
                    //
                    // Нет лексем с похожим текстом. Только добавление новых лексем
                    //
                    lexems.Add(termLexem.Text, termLexem);
                }
                else
                {
                    if (foundLexem is IAltLexem)
                    {
                        //
                        // Там может быть более одного условия с тем же именем.
                        // TODO: Но только если они принадлежат к разным переменных.
                        //
                        IAltLexem foundTermLexem = (IAltLexem)foundLexem;
                        while (foundTermLexem.Alternative != null)
                        {
                            foundTermLexem = foundTermLexem.Alternative;
                        }
                        foundTermLexem.Alternative = termLexem;
                    }
                    else
                    {
                        //
                        // Только термы различных переменных могут иметь похожие имена
                        //
                        throw new System.Exception(string.Format("Найден более чем одна лексема с похожим именем: {0}", termLexem.Text));
                    }
                }
            }
        }

        static private List<IExpression> ParseLexems(string rule, Dictionary<string, Lexem> lexems)
        {
            List<IExpression> expressions = new List<IExpression>();

            string[] words = rule.Split(' ');
            foreach (string word in words)
            {
                Lexem lexem;
                if (lexems.TryGetValue(word, out lexem))
                {
                    expressions.Add(lexem);
                }
                else
                {
                    throw new System.Exception(string.Format("Не известный идентификатор: {0}", word));
                }
            }

            return expressions;
        }

        static private List<IExpression> ExtractSingleCondidtions(List<IExpression> conditionExpression, List<FuzzyVariable> input, Dictionary<string, Lexem> lexems)
        {
            List<IExpression> copyExpressions = conditionExpression.GetRange(0, conditionExpression.Count);
            List<IExpression> expressions = new List<IExpression>();

            while (copyExpressions.Count > 0)
            {
                if (copyExpressions[0] is VarLexem<FuzzyVariable>)
                {
                    //
                    // Разбор переменной
                    //
                    VarLexem<FuzzyVariable> varLexem = (VarLexem<FuzzyVariable>)copyExpressions[0];
                    if (copyExpressions.Count < 3)
                    {
                        throw new Exception(string.Format("Состояние начинается с '{0}' не корректно.", varLexem.Text));
                    }

                    if (varLexem.Input == false)
                    {
                        throw new Exception("Переменная в состоянии должна быть входной переменной.");
                    }

                    //
                    // Разбор "is" лексемы
                    //
                    Lexem exprIs = (Lexem)copyExpressions[1];
                    if (exprIs != lexems["is"])
                    {
                        throw new Exception(string.Format("'is' ключевое слово должно идти после {0} идентификатора.", varLexem.Text));
                    }


                    //
                    // Разбор 'not' лексемы (если существует)
                    //
                    int cur = 2;
                    bool not = false;
                    if (copyExpressions[cur] == lexems["not"])
                    {
                        not = true;
                        cur++;

                        if (copyExpressions.Count <= cur)
                        {
                            throw new Exception("Ошибка около 'not' в состоянии части правила.");
                        }
                    }

                    //"slightly"    - немного
                    //"somewhat"    - в некотором роде
                    //"very"        - очень
                    //"extremely"   - чрезвычайно

                    //
                    // Разбор hedge модификатора (если существует)
                    //
                    HedgeType hedge = HedgeType.None;
                    if (copyExpressions[cur] == lexems["slightly"])
                    {
                        hedge = HedgeType.Slightly;
                    }
                    else if (copyExpressions[cur] == lexems["somewhat"])
                    {
                        hedge = HedgeType.Somewhat;
                    }
                    else if (copyExpressions[cur] == lexems["very"])
                    {
                        hedge = HedgeType.Very;
                    }
                    else if (copyExpressions[cur] == lexems["extremely"])
                    {
                        hedge = HedgeType.Extremely;
                    }

                    if (hedge != HedgeType.None)
                    {
                        cur++;

                        if (copyExpressions.Count <= cur)
                        {
                            throw new Exception(string.Format("Ошибка около '{0}' в состоянии части правила.", hedge.ToString().ToLower()));
                        }
                    }

                    //
                    // Разбор терма
                    //
                    Lexem exprTerm = (Lexem)copyExpressions[cur];
                    if (!(exprTerm is IAltLexem))
                    {
                        throw new Exception(string.Format("Неверный идентификатор '{0}' в стостоянии части правила.", exprTerm.Text));
                    }
                    IAltLexem altLexem = (IAltLexem)exprTerm;
                    TermLexem<FuzzyTerm> termLexem = null;
                    do
                    {
                        if (!(altLexem is TermLexem<FuzzyTerm>))
                        {
                            continue;
                        }

                        termLexem = (TermLexem<FuzzyTerm>)altLexem;
                        if (!varLexem.Var.Values.Contains(termLexem.Term))
                        {
                            termLexem = null;
                            continue;
                        }
                    }
                    while ((altLexem = altLexem.Alternative) != null && termLexem == null);

                    if (termLexem == null)
                    {
                        throw new Exception(string.Format("Неверный идентификатор '{0}' в стостоянии части правила.", exprTerm.Text));
                    }

                    //
                    // Добавление нового выражения состояния
                    //
                    FuzzyCondition condition = new FuzzyCondition(varLexem.Var, termLexem.Term, not, hedge);
                    expressions.Add(new ConditionExpression(copyExpressions.GetRange(0, cur + 1), condition));
                    copyExpressions.RemoveRange(0, cur + 1);
                }
                else
                {
                    IExpression expr = copyExpressions[0];
                    if (expr == lexems["and"] ||
                        expr == lexems["or"] ||
                        expr == lexems["("] ||
                        expr == lexems[")"])
                    {
                        expressions.Add(expr);
                        copyExpressions.RemoveAt(0);
                    }
                    else
                    {
                        Lexem unknownLexem = (Lexem)expr;
                        throw new Exception(string.Format("Лексема  '{0}' найдена в ошибочном месте в состоянии части правила.", unknownLexem.Text));
                    }
                }
            }

            return expressions;
        }

        static private Conditions ParseConditions(List<IExpression> conditionExpression, List<FuzzyVariable> input, Dictionary<string, Lexem> lexems)
        {
            //
            // Извлечение отдельных условий
            //
            List<IExpression> expressions = ExtractSingleCondidtions(conditionExpression, input, lexems);

            if (expressions.Count == 0)
            {
                throw new Exception("Нет действительных условий в условиях части правила.");
            }

            ICondition cond = ParseConditionsRecurse(expressions, lexems);

            //
            // Возвращение состояний
            //
            if (cond is Conditions)
            {
                return (Conditions)cond;
            }
            else
            {
                Conditions conditions = new Conditions();
                return conditions;
            }
        }

        static private int FindPairBracket(List<IExpression> expressions, Dictionary<string, Lexem> lexems)
        {
            //
            // Предположим, что '(' стоит на первом месте
            //

            int bracketsOpened = 1;
            int closeBracket = -1;
            for (int i = 1; i < expressions.Count; i++)
            {
                if (expressions[i] == lexems["("])
                {
                    bracketsOpened++;
                }
                else if (expressions[i] == lexems[")"])
                {
                    bracketsOpened--;
                    if (bracketsOpened == 0)
                    {
                        closeBracket = i;
                        break;
                    }
                }
            }
            return closeBracket;
        }

        static private ICondition ParseConditionsRecurse(List<IExpression> expressions, Dictionary<string, Lexem> lexems)
        {
            if (expressions.Count < 1)
            {
                throw new Exception("Пустое условие найдено.");
            }

            if (expressions[0] == lexems["("] && FindPairBracket(expressions, lexems) == expressions.Count)
            {
                //
                // 
                // Удаление лишних скобок
                //
                return ParseConditionsRecurse(expressions.GetRange(1, expressions.Count - 2), lexems);
            }
            else if (expressions.Count == 1 && expressions[0] is ConditionExpression)
            {
                //
                // Возвращение единственного условия
                //
                return ((ConditionExpression)expressions[0]).Condition;
            }
            else
            {
                //
                // Разбор списка из одного уровня состояний соединенных методом or/and
                //
                List<IExpression> copyExpressions = expressions.GetRange(0, expressions.Count);
                Conditions conds = new Conditions();
                bool setOrAnd = false;
                while (copyExpressions.Count > 0)
                {
                    ICondition cond = null;
                    if (copyExpressions[0] == lexems["("])
                    {
                        //
                        // Найти пару кронштейнов
                        //
                        int closeBracket = FindPairBracket(copyExpressions, lexems);
                        if (closeBracket == -1)
                        {
                            throw new Exception("Ошибка скобок.");
                        }

                        cond = ParseConditionsRecurse(copyExpressions.GetRange(1, closeBracket - 1), lexems);
                        copyExpressions.RemoveRange(0, closeBracket + 1);
                    }
                    else if (copyExpressions[0] is ConditionExpression)
                    {
                        cond = ((ConditionExpression)copyExpressions[0]).Condition;
                        copyExpressions.RemoveAt(0);
                    }
                    else
                    {
                        throw new ArgumentException(string.Format("Неверное выражение в в состояние части правил '{0}'"), copyExpressions[0].Text);
                    }

                    //
                    // И состояние к списку
                    //
                    conds.ConditionsList.Add(cond);

                    if (copyExpressions.Count > 0)
                    {
                        if (copyExpressions[0] == lexems["and"] || copyExpressions[0] == lexems["or"])
                        {
                            if (copyExpressions.Count < 2)
                            {
                                throw new Exception(string.Format("Error at {0} in condition part.", copyExpressions[0].Text));
                            }

                            //
                            // Установить или/и для списка состояний
                            //
                            OperatorType newOp = (copyExpressions[0] == lexems["and"]) ? OperatorType.And : OperatorType.Or;

                            if (setOrAnd)
                            {
                                if (conds.Op != newOp)
                                {
                                    throw new Exception("На одном уровне вложенности не могут быть смешаны и/или операции.");
                                }
                            }
                            else
                            {
                                conds.Op = newOp;
                                setOrAnd = true;
                            }
                            copyExpressions.RemoveAt(0);
                        }
                        else
                        {
                            throw new Exception(string.Format("{1} не может идти за {0}", copyExpressions[0].Text, copyExpressions[1].Text));
                        }
                    }
                }
                return conds;
            }
        }

        static private SingleCondition<VariableType, ValueType> ParseConclusion<VariableType, ValueType>(List<IExpression> conditionExpression, List<VariableType> output, Dictionary<string, Lexem> lexems)
            where VariableType : class, INamedVariable
            where ValueType : class, INamedValue
        {

            List<IExpression> copyExpression = conditionExpression.GetRange(0, conditionExpression.Count);

            //
            // Удаление лишних скобок
            //
            while (
                copyExpression.Count >= 2 &&
                (copyExpression[0] == lexems["("] && copyExpression[conditionExpression.Count - 1] == lexems[")"]))
            {
                copyExpression = copyExpression.GetRange(1, copyExpression.Count - 2);
            }

            if (copyExpression.Count != 3)
            {
                throw new Exception("Вывод часть правила должны быть в форме: 'переменная есть терм'");
            }

            //
            // Разбор переменной
            //
            Lexem exprVariable = (Lexem)copyExpression[0];
            if (!(exprVariable is VarLexem<VariableType>))
            {
                throw new Exception(string.Format("Неверный идентификатор '{0}' в состоянии части правила.", exprVariable.Text));
            }

            VarLexem<VariableType> varLexem = (VarLexem<VariableType>)exprVariable;
            if (varLexem.Input == true)
            {
                throw new Exception("Переменная в заключительной части должна быть выходной переменной.");
            }

            //
            // Разбор 'is' лексемы
            //
            Lexem exprIs = (Lexem)copyExpression[1];
            if (exprIs != lexems["is"])
            {
                throw new Exception(string.Format("'is' ключевое слово после {0} идентификатора.", varLexem.Text));
            }

            //
            // Parse term
            //
            Lexem exprTerm = (Lexem)copyExpression[2];
            if (!(exprTerm is IAltLexem))
            {
                throw new Exception(string.Format("Неверный идентификатор '{0}' в заключительной части правила.", exprTerm.Text));
            }

            IAltLexem altLexem = (IAltLexem)exprTerm;
            TermLexem<ValueType> termLexem = null;
            do
            {
                if (!(altLexem is TermLexem<ValueType>))
                {
                    continue;
                }

                termLexem = (TermLexem<ValueType>)altLexem;
                if (!varLexem.Var.Values.Contains(termLexem.Term))
                {
                    termLexem = null;
                    continue;
                }
            }
            while ((altLexem = altLexem.Alternative) != null && termLexem == null);

            if (termLexem == null)
            {
                throw new Exception(string.Format("Неверный идентификатор '{0}' в заключительной части правила.", exprTerm.Text));
            }

            //
            // Возвращение нечеткого правила заключения
            //
            return new SingleCondition<VariableType, ValueType>(varLexem.Var, termLexem.Term, false);
        }

        static internal RuleType Parse(string rule, RuleType emptyRule, List<FuzzyVariable> input, List<OutputVariableType> output)
        {
            if (rule.Length == 0)
            {
                throw new ArgumentException("Правило не может быть пустое.");
            }

            //
            // Окружение тормозов c пробелами, удаление двойных пробелов
            //
            System.Text.StringBuilder sb = new StringBuilder();
            foreach (char ch in rule)
            {
                if (ch == ')' || ch == '(')
                {
                    if (sb.Length > 0 && sb[sb.Length - 1] == ' ')
                    {
                        // Нет дублировать пробелы
                    }
                    else
                    {
                        sb.Append(' ');
                    }

                    sb.Append(ch);
                    sb.Append(' ');
                }
                else
                {
                    if (ch == ' ' && sb.Length > 0 && sb[sb.Length - 1] == ' ')
                    {
                        // Нет дублировать пробелы
                    }
                    else
                    {
                        sb.Append(ch);
                    }
                }
            }

            //
            // Удаление пробелов
            //
            string prepRule = sb.ToString().Trim();

            //
            // Построение словарей лексем
            //
            Dictionary<string, Lexem> lexemsDict = BuildLexemsList(input, output);

            //
            // Около первого разбираем лексемы
            //
            List<IExpression> expressions = ParseLexems(prepRule, lexemsDict);
            if (expressions.Count == 0)
            {
                throw new System.Exception("Не найдены допустимые идентификаторы.");
            }

            //
            // 
            // Найти состояние и вывод частей часть
            //
            if (expressions[0] != lexemsDict["if"])
            {
                throw new System.Exception("'if' должно быть первым идентификатором.");
            }

            int thenIndex = -1;
            for (int i = 1; i < expressions.Count; i++)
            {
                if (expressions[i] == lexemsDict["then"])
                {
                    thenIndex = i;
                    break;
                }
            }

            if (thenIndex == -1)
            {
                throw new System.Exception("'then' идентификатор не найден.");
            }

            int conditionLen = thenIndex - 1;
            if (conditionLen < 1)
            {
                throw new System.Exception("Состояние части правила не найдено.");
            }

            int conclusionLen = expressions.Count - thenIndex - 1;
            if (conclusionLen < 1)
            {
                throw new System.Exception("Состояние части правила не найдено.");
            }
            // Забираем условие
            List<IExpression> conditionExpressions = expressions.GetRange(1, conditionLen);
            // Забираем следствие
            List<IExpression> conclusionExpressions = expressions.GetRange(thenIndex + 1, conclusionLen);

            Conditions conditions = ParseConditions(conditionExpressions, input, lexemsDict);
            SingleCondition<OutputVariableType, OutputValueType> conclusion = ParseConclusion<OutputVariableType, OutputValueType>(conclusionExpressions, output, lexemsDict);

            emptyRule.Condition = conditions;
            emptyRule.Conclusion = conclusion;
            return emptyRule;
        }
    }
}
