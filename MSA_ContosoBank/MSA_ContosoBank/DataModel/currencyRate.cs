using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MSA_ContosoBank.DataModel
{
    public class currencyRate
    {
        public class Rootobject
        {
            public string _base { get; set; }
            public string date { get; set; }
            public Rates rates { get; set; }
        }
        public class Rates
        {
            public double AUD { get; set; }
            public double BGN { get; set; }
            public double CAD { get; set; }
            public double CNY { get; set; }
            public double BRL { get; set; }
            public double HKD { get; set; }
            public double IDR { get; set; }
            public double ILS { get; set; }
            public double INR { get; set; }
            public double JPY { get; set; }
            public double KRW { get; set; }
            public double MXN { get; set; }
            public double USD { get; set; }
            public double EUR { get; set; }

        }
    }
}