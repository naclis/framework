﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AccordTestsMathCpp2;
using Accord.Math.Optimization;

namespace Accord.Tests.Math.Optimization
{
    public class Specification
    {
        public Specification(int n, Func<double[], double> function, 
            Func<double[], double[]> gradient, double[] start)
        {
            this.Variables = n;
            this.Function = function;
            this.Gradient = gradient;
            this.Start = start;

            if (start == null)
                this.Start = new double[n];
        }

        public int Variables;
        public Func<double[], double> Function;
        public Func<double[], double[]> Gradient;
        public double[] Start;
    }

    public class LBFGSComparer
    {
        private List<OptimizationProgressEventArgs> actual;

        public int m = 6;
        public double epsilon = 1e-5;
        public int past = 0;
        public double delta = 1e-5;
        public int max_iterations = 0;
        public LineSearch linesearch = LineSearch.Default;
        public int max_linesearch = 40;
        public double min_step = 1e-20;
        public double max_step = 1e20;
        public double ftol = 1e-4;
        public double wolfe = 0.9;
        public double gtol = 0.9;
        public double xtol = 1.0e-16;
        public double orthantwise_c = 0;
        public int orthantwise_start = 0;
        public int orthantwise_end = -1;

        public string ActualMessage;

        public LBFGSComparer()
        {
            actual = new List<OptimizationProgressEventArgs>();
        }

        public Info[] Expected(Specification problem)
        {
            Function function = problem.Function.Invoke;
            Gradient gradient = problem.Gradient.Invoke;

            Param param = new Param()
            {
                m = m,
                epsilon = epsilon,
                past = past,
                delta = delta,
                max_iterations = max_iterations,
                linesearch = (int)linesearch,
                max_linesearch = max_linesearch,
                min_step = min_step,
                max_step = max_step,
                ftol = ftol,
                wolfe = wolfe,
                gtol = gtol,
                xtol = xtol,
                orthantwise_c = orthantwise_c,
                orthantwise_start = orthantwise_start,
                orthantwise_end = orthantwise_end
            };

            NativeCode = Wrapper.Libbfgs((double[])problem.Start.Clone(), function, gradient, param);

            return Wrapper.list.ToArray();
        }

        public string NativeCode { get; private set; }

        public OptimizationProgressEventArgs[] Actual(Specification problem)
        {
            BroydenFletcherGoldfarbShanno target = new BroydenFletcherGoldfarbShanno(problem.Variables)
            {
                Corrections = m,
                Epsilon = epsilon,
                Past = past,
                Delta = delta,
                MaxIterations = max_iterations,
                LineSearch = (LineSearch)linesearch,
                MaxLineSearch = max_linesearch,
                MinStep = min_step,
                MaxStep = max_step,
                ParameterTolerance = ftol,
                Wolfe = wolfe,
                GradientTolerance = gtol,
                FunctionTolerance = xtol,
                OrthantwiseC = orthantwise_c,
                OrthantwiseStart = orthantwise_start,
                OrthantwiseEnd = orthantwise_end
            };

            target.Function = problem.Function;
            target.Gradient = problem.Gradient;

            actual.Clear();
            target.Progress += new EventHandler<OptimizationProgressEventArgs>(target_Progress);

            target.Minimize((double[])problem.Start.Clone());

            switch (target.Status)
            {
                case BroydenFletcherGoldfarbShannoCode.Success:
                    ActualMessage = "LBFGS_SUCCESS"; break;
                case BroydenFletcherGoldfarbShannoCode.MaximumIterations:
                    ActualMessage = "LBFGSERR_MAXIMUMITERATION"; break;
                case BroydenFletcherGoldfarbShannoCode.AlreadyMinimized:
                    ActualMessage = "LBFGS_ALREADY_MINIMIZED"; break;
                case BroydenFletcherGoldfarbShannoCode.RoundingError:
                    ActualMessage = "LBFGSERR_ROUNDING_ERROR"; break;
                case BroydenFletcherGoldfarbShannoCode.MaximumLineSearch:
                    ActualMessage = "LBFGSERR_MAXIMUMLINESEARCH"; break;
                case BroydenFletcherGoldfarbShannoCode.Stop:
                    ActualMessage = "LBFGS_STOP"; break;
                default:
                    break;
            }


           // actual.Add(new OptimizationProgressEventArgs(0, 0, null, 0, target.Solution, 0, v, 0, true));

            return actual.ToArray();
        }

        void target_Progress(object sender, OptimizationProgressEventArgs e)
        {
            actual.Add(e);
        }
    }
}
