using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoMLDataReader.Core
{
    public class AMLPredictResult
    {
        public List<double> forecast { get; set; }
        public List<Dictionary<string,object>> index { get; set; }
    }
}
