using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnyType.Models
{
    public class TaskViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public double Value { get; set; }
        public string Type { get; set; }
    }
}