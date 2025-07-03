using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuncTriggerManagerSvc.Models
{
    internal class FuncTriggerMsg
    {
        public string FunctionAppName { get; set; } = default!;
        public string FunctionName { get; set; } = default!;

        public string RessourceGroupName { get; set; } = default!;
        public bool DisableFunction { get; set; }

        public int DisablePeriodMinutes { get; set; }
    }
}
