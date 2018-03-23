/*
 *  FuzzyLogic: FuzzyLogic Library for C#
 *  Copyright (C) 2017 Луферов Виктор (lyferov@yandex.ru)
 */
 
namespace FuzzyLogic
{

    /// <summary>
    /// Лингвистический терм
    /// </summary>
    public class FuzzyTerm : NamedValueImpl
    {
        IMembershipFunction _mf;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="name">Имя терма</param>
        /// <param name="mf">Функция принадлежности терма</param>
        public FuzzyTerm(string name, IMembershipFunction mf) : base(name)
        {
            _mf = mf;
        }

        /// <summary>
        /// Функция принадлежности терма
        /// </summary>
        public IMembershipFunction MembershipFunction
        {
            get { return _mf; }
            set { _mf = value; }
        }
    }
}
