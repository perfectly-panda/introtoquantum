using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace demo.Models
{
    public class Results
    {
        public List<string> Histogram { get; set; }
        public List<string> Values { get
            {
                if(Histogram == null)
                {
                    return null;
                } else
                {
                    return Histogram.Where((c, i) => i % 2 == 0).ToList();
                }
            }
        }
    }
}
