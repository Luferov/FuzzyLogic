/*
 *  FuzzyLogic: FuzzyLogic Library for C#
 *  Copyright (C) 2017 Луферов Виктор (lyferov@yandex.ru)
 */

namespace FuzzyLogic
{
    /// <summary>
    /// Метод вывода "И"
    /// </summary>
    public enum AndMethod {
        /// <summary>
        /// Минимум: min(a,b)
        /// </summary>
        Min,
        /// <summary>
        /// Произведение: a * b
        /// </summary>
        Production
    }
    /// <summary>
    /// Метод вывода "ИЛИ"
    /// </summary>
    public enum OrMethod {
        /// <summary>
        /// Максимум: max(a,b)
        /// </summary>
        Max,
        /// <summary>
        /// Ограниченная сумма: a + b - a * b
        /// </summary>
        Probabilistic
    }
    /// <summary>
    /// Метод нечеткой импликации
    /// </summary>
    public enum ImplicationMethod
    {
        /// <summary>
        /// Усечение вывода нечетких множеств
        /// </summary>
        Min,
        /// <summary>
        /// Масштабирование вывода нечетких множеств
        /// </summary>
        Production
    }
    /// <summary>
    /// Метод агрегации для функций принадлежностей
    /// </summary>
    public enum AggregationMethod
    {
        /// <summary>
        /// Максимум из выходных правил
        /// </summary>
        Max,
        /// <summary>
        /// Сумма выходных правил
        /// </summary>
        Sum
    }
    /// <summary>
    /// Метод дефаззификации
    /// </summary>
    public enum DefuzzificationMethod {
        /// <summary>
        /// Центр тяжести результирующей функции принадлежности
        /// </summary>
        Centroid,
        /// <summary>
        /// Биссектриса: Не реализовано! 
        /// </summary>
        Bisector,
        /// <summary>
        /// Средняя максимальная: Не реализовано!
        /// </summary>
        AverageMaximum
    }

}
