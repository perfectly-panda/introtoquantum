using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace demo
{
    public class Number : TableEntity
    {
        public string State { get; set; }
        public string Numbers { get; set; }

    }
}
