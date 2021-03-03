using System;
using System.Collections.Generic;
using System.Text;

namespace AutoMLDataReader.WPF
{
    public class DelegateProgress<T> : IProgress<T>
    {
        private Action<T> a;
        public DelegateProgress(Action<T> report)
        {
            a = report;
        }
        public void Report(T value)
        {
            a(value);
        }
    }
}
