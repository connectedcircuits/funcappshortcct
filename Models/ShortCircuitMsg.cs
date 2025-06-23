using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuncTriggerManagerSvc.Models
{
    internal class ShortCircuitMsg
    {
        public string FunctionAppName { get; set; }
        public string FunctionName { get; set; }

        public string RessourceGroupName { get; set; }
        public bool DisableFunction { get; set; }

        public int DisablePeriodMinutes { get; set; }
    }
}
