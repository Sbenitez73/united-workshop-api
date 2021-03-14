using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkshopAPI.Models
{
    public class ApiError
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }
}
