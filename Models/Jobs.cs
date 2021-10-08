using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace demo
{
    public class Jobs : TableEntity
    {
        public string status { get; set; }
    }
}
