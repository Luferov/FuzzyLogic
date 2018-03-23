/*
 *  FuzzyLogic: FuzzyLogic Library for C#
 *  Copyright (C) 2017 Луферов Виктор (lyferov@yandex.ru)
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace FuzzyLogic
{
    /// <summary>
    /// Адаптивная нейронечеткая система сугено
    /// </summary>
    public class Anfis : SugenoFuzzySystem
    {

        public delegate void anfisTrain(int epoch, int epochs, double nu, double error);
        
        /// <summary>
        /// Параметры обучения
        /// </summary>
        private int epochs = 10;                        // Количество эпох обучения 
        private double error = 0.0;                     // Желательная ошибка при обучении 
        private double[] errorTrain;                    // Ошибки при обучении на каждом шаге
        double nu = 0.1;                                // Коэффициент при обучении
        double nuStep = 0.9;                            // Изменение nu с каждым шагом
        const string nameInput = "input";               // Имена выходных переменных
        const string nameOutput = "output1";            // Имя выходной переменной
        const string nameMf = "mf";                     // Имена термов
        private int countInput;                         // Количество входных переменных
        private string[] rulesText;                     // Текстовой представление правил

        /// <summary>
        /// Параметры кластеризации
        /// </summary>
        private double radii = 0.5;                     // Параметр кластеризации
        private double sqshFactor = 1.25;               // Коэффициент подавления
        private double acceptRatio = 0.5;               // Коэффициент принятия
        private double rejectRatio = 0.15;              // Коэффициент отторжения

        /// <summary>
        /// Массив входных данных для обучения
        /// </summary>
        private double[][] xin;                         // Массив входных переменных для обучения
        private double[] xout;                          // Массив выходных переменных для обучения

        #region Свойства
        /// <summary>
        /// Желательная ошибка, по умолчанию 0
        /// </summary>
        public double Error
        {
            get { return error; }
            set { error = value; }

        }
        /// <summary>
        /// Коэффициент при обучении, по умолчанию 0,1
        /// </summary>
        public double Nu
        {
            get { return nu; }
            set {
                if (0 <= value && value <= 1)   //Если в диапазоне от [0,1]
                {
                    nu = value;
                }
            }
        }
        /// <summary>
        /// Имя выходной переменной по умолчанию
        /// </summary>
        public static string NameOutput
        {
            get { return nameOutput; }
        }
        /// <summary>
        /// Имена входных переменных
        /// </summary>
        public static string NameInput
        {
            get { return nameInput; }
        }
        /// <summary>
        /// Количество эпох обучения, по умолчанию 10
        /// </summary>
        public int Epochs
        {
            get { return epochs; }
            set
            {
                if (value > 0)
                {
                    epochs = value;
                }
            }
        }
        /// <summary>
        /// Ошибки при обучении
        /// </summary>
        public double[] ErrorTrain
        {
            get { return errorTrain; }
        }
        /// <summary>
        /// Радиус кластеров
        /// </summary>
        public double Radii
        {
            get { return radii; }
            set { radii = value; }
        }
        /// <summary>
        /// Коэффициент подавления
        /// </summary>
        public double SqshFactor
        {
            get { return sqshFactor; }
            set { sqshFactor = value; }
        }
        /// <summary>
        /// Коэффициент принятия
        /// </summary>
        public double AcceptRatio
        {
            get { return acceptRatio; }
            set { acceptRatio = value; }
        }
        /// <summary>
        /// Коэффициент отторжения
        /// </summary>
        public double RejectRatio
        {
            get { return rejectRatio; }
            set { rejectRatio = value; }
        }
        /// <summary>
        /// Имена функций принадлежности
        /// </summary>
        public static string NameMf
        {
            get { return nameMf; }
        }
        /// <summary>
        /// Изменение nu с каждым шагом, если NuStep = 1, то nu не изменяется
        /// </summary>
        public double NuStep
        {
            get { return nuStep; }
            set { nuStep = value; }
        }
        /// <summary>
        /// Текстовое представление правил
        /// </summary>
        public string[] RulesText
        {
            get { return rulesText; }
        }
        /// <summary>
        /// Количество входных переменных нечеткой системы сугено
        /// </summary>
        public int CountInput
        {
            get { return countInput; }
        }
        /// <summary>
        /// Количество выходных переменных
        /// </summary>
        public int CountOutput
        {
            get { return xout.Length; }
        }
        #endregion

        #region Конструктор
        public Anfis(double[][] xin, double[] xout) {
            if (xin[0].Length != xout.Length) {
                throw new ArgumentException("Параметры длины входных данных не соответствуют друг другу");
            }
            double[] xin1 = xin[0];                 //Смотрим по первому элементу
            for (int i = 1; i< xin.Length; i++)
            {
                if (xin1.Length != xin[i].Length) {
                    throw new ArgumentException("Входные данные не согласованы");
                }
            }
            this.xin = xin;
            this.xout = xout;

            // Настройка Fis по умолчанию
            OrMethod = OrMethod.Max;
            AndMethod = AndMethod.Production;
            countInput = xin.Length;

        }
        public Anfis(double[][] xin, double[] xout, double radii, double sf = 1.25, double ar = 0.5, double rr = 0.15)
            : this(xin, xout)
        {
            Radii = radii;
            SqshFactor = sf;
            AcceptRatio = ar;
            RejectRatio = rr;
        }
        #endregion

        #region Вычисление значения функции
        /// <summary>
        /// Вычисляем значение
        /// </summary>
        /// <param name="x">Вектор входных данных</param>
        /// <returns></returns>
        public double Calculate(double[] x) {
            double result = 0.0;
            if (x.Length != CountInput) {
                throw new Exception(
                    string.Format("Количество входных fis и вектора не совпадают: {0}/{1}", x.Length, CountInput)
                );
            }
            Dictionary<FuzzyVariable, double> input = new Dictionary<FuzzyVariable, double>();
            for (int i = 0; i < Input.Count; i++) {
                input.Add(InputByName(NameInput + (i + 1).ToString()), x[i]);
            }
            result = Calculate(input)[OutputByName(NameOutput)];
            return result;
        }
        #endregion

        #region Обучение нечетконейронной сети
        /// <summary>
        /// Обучение Fis
        /// </summary>
        /// <param name="anfisTrain">Функция обучения</param>
        public void Train(anfisTrain anfisTrain = null) {
            //
            // Генерация Fis
            //
            Genfis();
            //
            // Обучение Fis
            //
            int epoch = 1;
            double epochError;

            if (Rules.Count == 0) {
                throw new Exception("Должно быть хотя бы одно правило.");
            }

            int k = xout.Length;                            // Количество параметров обучающей выборки
            int l = (xin.Length + 1) * Rules.Count;         // (m+1)n - m - количество входных переменных n - количество правил

            Matrix C = new Matrix(l,1);                     // Столбец весовых коэффициентов
            Matrix Y = new Matrix(k, 1);                    // Вектор столбец выходных данных
            for (int i = 0; i < k; i++) {
                Y[i, 0] = xout[i];
            }

            errorTrain = new double[Epochs];
            while (epoch <= Epochs) {
                epochError = 0.0;                           // Ошибка на i эпохе
                //
                // Формирование матрицы коэффициентов
                //
                Matrix W = new Matrix(k, l);
                Matrix ew = new Matrix(k, Rules.Count);
                for (int i = 0; i < k; i++)
                {
                    Dictionary<FuzzyVariable, double> inputValues = new Dictionary<FuzzyVariable, double>();
                    // По количеству переменных
                    for (int j = 0; j < xin.Length; j++)
                    {
                        inputValues.Add(InputByName(NameInput + (j + 1).ToString()), xin[j][i]);
                    }
                    // Посылки правил
                    Dictionary<FuzzyVariable, Dictionary<FuzzyTerm, double>> fuzzyInput = Fuzzify(inputValues);
                    // Агрегирование подусловий
                    Dictionary<SugenoFuzzyRule, double> rulesWeight = EvaluateConditions(fuzzyInput);
                    // Заключение правил
                    double Stau = 0.0;
                    List<double> tau = new List<double>();
                    foreach (double _tau in rulesWeight.Values) {
                        Stau += _tau;
                        tau.Add(_tau);
                    }
                    double[] b = new double[tau.Count];
                    for (int j = 0; j < b.Length; j++)
                    {
                        b[j] = tau[j] / Stau;
                        ew[i, j] = tau[j];
                    }
                    // Формирование входных переменных
                    double[] x = new double[xin.Length + 1];        // +1, т.к. x0 = 1
                    x[0] = 1;                                       // x0 = 1
                    for (int j = 1; j < x.Length; j++)
                    {
                        x[j] = xin[j - 1][i];                       // заполняем x1, x2,...,x3
                    }
                    int columnW = 0;
                    for (int g = 0; g < b.Length; g++) {            // перебираем по заключениям
                        for (int d = 0; d < x.Length; d++)          // перебираем по входным данным
                        {
                            W[i, columnW] = b[g] * x[d];            // переменожаем коэффициенты на переменные
                            columnW++;                              // увеличиваем строку на 1
                        }
                    }
                }

                Matrix Winverse = W.PInverse();     // W+ псевдоинверстная матрица

                //
                // Если псевдоинверстная матрица не найдена
                //
                bool breakTrain = false;
                for (int i = 0; i < Winverse.N; i++)
                {
                    for (int j = 0; j < Winverse.M; j++)
                    {
                        if (double.IsNaN(Winverse[i, j]))
                        {
                            breakTrain = true;
                            break;
                        }
                    }
                    if (breakTrain) break;
                }
                if (breakTrain) break;  // Прерываем обучение

                C = Winverse * Y;            // Находим коэффициенты 
                //
                // Нахождение вектора фактического выхода сети
                //
                Matrix Ystrich = W * C;
                //
                // Правим коэффициенты
                //
                for (int i = 0; i < Input.Count; i++)
                {
                    FuzzyVariable fv = Input[i];                // Забираем переменную
                    for (int j = 0; j < fv.Terms.Count; j++)
                    {
                        FuzzyTerm term = fv.Terms[j];               // Выбираем j терм i входной переменной
                        //
                        // Меняем в случае если функция принадлежности колоколообразная
                        //
                        IMembershipFunction mf = term.MembershipFunction;
                        if (mf is NormalMembershipFunction) {
                            NormalMembershipFunction mfterm = (NormalMembershipFunction)mf;  // Приводим тип
                            //
                            // Перебираем все переменные k - количество выходных параметров
                            //
                            
                            for (int g = 0; g < k; g++)
                            {
                                double b = mfterm.B;
                                double sigma = mfterm.Sigma;
                                double xa = xin[i][g] - b;   // xin - b

                                double yyStrih = Ystrich[g,0] - xout[g]; // y' - y
                                double p = ew[g, j];
                                double sp = 0.0;
                                for (int q = 0; q < Rules.Count; q++)
                                {
                                    sp += ew[g, q];
                                }
                                double pb = p / (sp / Math.Pow(sigma, 2));   // (t/summt) / sigma^2
                                //
                                // Инициализируем матрицы для нахождения c
                                //
                                Matrix x = new Matrix(1, xin.Length + 1);
                                Matrix c = new Matrix(xin.Length + 1, 1);
                                x[0,0] = 1;                                 //x0 = 1
                                for (int q = 1; q < x.M; q++) {
                                    x[0, q] = xin[q - 1][g];
                                }
                                // Заполняем коэффициенты
                                int start = j * x.M;
                                for (int q = start; q < start + x.M; q++)
                                {
                                    c[q - start, 0] = C[q, 0];
                                }

                                // Перемножаем матрици
                                double cy = ((x*c)[0] - Ystrich[g,0]);
                                //
                                // Корректируем B
                                //

                                b -= 2 * Nu * xa * yyStrih * cy * pb;

                                //
                                // Корректируем Sigma
                                //

                                sigma -= 2 * Nu * Math.Pow(xa, 2) * yyStrih * cy * pb;
                                //
                                // Условия переобучения и недопустимости применения условий
                                //

                                if (double.IsNaN(mfterm.Sigma) || double.IsNaN(mfterm.B))
                                {
                                    continue;   // Идем на следующую итерацию
                                }
                                else {
                                    mfterm.B = b;
                                    mfterm.Sigma = sigma;
                                }
                            }
                            //
                            // Загоняем терм обратно
                            //



                            fv.Terms[j].MembershipFunction = mfterm;
                        }
                        //
                        // TODO: Настройка остальных функций принадлежности не реализовано
                        //
                    }

                    Input[i] = fv;                      // Загоняем переменную обратно
                }
                //
                // Находим ошибку выхода 
                //
                epochError = 0.0;
                for (int i = 0; i < Ystrich.N; i++)
                {
                    epochError += 0.5 * Math.Pow(Ystrich[i, 0] - xout[i], 2);
                }
                errorTrain[epoch - 1] = epochError;
                anfisTrain?.Invoke(epoch, Epochs, nu, epochError);
                Nu *= NuStep;
                epoch++;

            }   // Конец эпох обучения

            //
            // Применяем параметры коэффициентов y = c0 + c1 * x1 + c2 * x2 + ... + ci * xi
            //
            SetCoefficient(C);
            //
            // Перезаписываем правила в силу архитектуры сети
            //
            Rules.Clear();
            for (int i = 0; i < rulesText.Length; i++)
            {
                Rules.Add(ParseRule(rulesText[i]));
            }

            Console.WriteLine(Rules[0].ToString());
        }
        /// <summary>
        /// Генерация базы правил на основе субстратной кластеризации
        /// </summary>
        private void Genfis() {
            //
            // Подготовка данных для кластеризации
            //
            int len = xin.Length + 1;                       // + 1, т.к. входные данне
            double[][] x = new double[len][];               // Создаем массив данных
            for (int i = 0; i < x.Length - 1; i++) {        // Перебираем массивы
                x[i] = xin[i];
            }
            x[x.Length - 1] = xout;

            //
            // Кластеризация данных
            //

            SubtractClustesing subclust = new SubtractClustesing(x, Radii, SqshFactor, AcceptRatio, RejectRatio);
            subclust.SubClust();
            double[][] centers = subclust.Centers;          // Центы кластеров
            double[] sigmas = subclust.Sigmas;              // Радиусы кластеров

            //
            // Ращипление центров кластеров
            //

            // Формирование центров кластеров
            double[][] centersIn = new double[centers.Length - 1][];
            double[] centersOut = centers[centers.Length - 1];
            for (int i = 0; i < centersIn.Length; i++) {
                centersIn[i] = centers[i];
            }
            // Формирование радиусов кластера
            double[] sigmaIn = new double[sigmas.Length - 1];
            for (int i = 0; i < sigmaIn.Length; i++)
            {
                sigmaIn[i] = sigmas[i];
            }
            double sigmaOut = sigmas[sigmas.Length - 1];

            //
            // Формируем входные переменные
            //

            for (int i = 0; i < centersIn.Length; i++)
            {
                // Создаем переменную
                double min, max;
                MinMax(xin[i], out max, out min);
                FuzzyVariable input = new FuzzyVariable(NameInput + (i + 1).ToString(), min, max);
                // Формируем термы / функции принадлежности
                for (int j = 0; j < centersIn[i].Length; j++)
                {
                    input.Terms.Add(
                        new FuzzyTerm(
                            NameMf + (j + 1).ToString(),
                            new NormalMembershipFunction(centersIn[i][j], sigmaIn[i])
                        )
                    );
                }
                // Заносим переменную в список
                Input.Add(input);
            }

            //
            // Формирование выходных переменных
            //

            SugenoVariable output = new SugenoVariable(NameOutput);
            for (int i = 0; i < centersOut.Length; i++)
            {
                Dictionary<FuzzyVariable, double> mf = new Dictionary<FuzzyVariable, double>();
                for (int j = 0; j< centersIn.Length; j++)                           // Перебираем входные переменные
                {
                    mf.Add(InputByName(NameInput + (j + 1).ToString()), 0.0);   // По умолчанию 0,0
                }
                // Константа тоже по умолчанию 0.0
                output.Functions.Add(CreateSugenoFunction(NameMf + (i + 1).ToString(), mf, 0.0));
            }
            Output.Add(output);

            //
            // Формировние базы правил типа
            // if (input1 is mf1) and (input2 is mf1) then (output1 is mf1)
            //
            rulesText = new string[centersOut.Length];          // Текстовое представление правил
            for (int i = 0; i < centersOut.Length; i++) {
                StringBuilder sb = new StringBuilder();
                sb.Append("if");
                for (int j = 0; j < centersIn.Length; j++) {
                    sb.Append(string.Format(" ({0} is {1}) ", NameInput + (j + 1).ToString(), NameMf + (i + 1).ToString()));
                    if (j != centersIn.Length - 1) {
                        sb.Append("and");
                    }
                }
                sb.Append("then");
                sb.Append(string.Format(" ({0} is {1})", NameOutput, NameMf + (i + 1).ToString()));
                rulesText[i] = sb.ToString();
                Rules.Add(ParseRule(sb.ToString()));                    // Парсим правило
            }
        }
        /// <summary>
        /// Установление коэффициентов выходной переменной NameOutput
        /// </summary>
        /// <param name="C"></param>
        private void SetCoefficient(Matrix C) {
            for (int i = 0; i < OutputByName(NameOutput).Functions.Count; i++)
            {
                ISugenoFunction mf = OutputByName(NameOutput).Functions[i];
                //
                // Настройка функции принадлежности y = c0 + c1 * x1 + c2 * x2 + ... + ci * xi
                //
                if (mf is LinearSugenoFunction)
                {                           // Если линейная функция
                    LinearSugenoFunction smf = (LinearSugenoFunction)mf;    // Преобразовываем функцию к линейной
                    int m = xin.Length + 1;
                    double[] c = new double[m];                             // т.к. c0
                    int start = i * m;
                    // Выдергиваем свои коэффициенты
                    for (int j = start; j < start + m; j++)
                    {
                        c[j - start] = C[j, 0];
                    }
                    Dictionary<FuzzyVariable, double> mfs = new Dictionary<FuzzyVariable, double>();
                    for (int j = 1; j < Input.Count + 1; j++)
                    {
                        mfs.Add(Input[j - 1], c[j]);    // Каждый коэффициент к своей переменной
                    }

                    // Достаем имя функции коэффициентов
                    string nameMfs = OutputByName(NameOutput).Functions[i].Name;
                    OutputByName(NameOutput).Functions[i] = CreateSugenoFunction(nameMfs, mfs, c[0]);
                }
            }
        }
        #endregion

        #region Вспомогательные функции
        private void MinMax(double[] x, out double max, out double min) {
            max = x[0];
            min = x[0];
            for (int i = 0; i < x.Length; i++) {
                max = Math.Max(max, x[i]);
                min = Math.Min(min, x[i]);
            }
        }
        #endregion
    }
}
