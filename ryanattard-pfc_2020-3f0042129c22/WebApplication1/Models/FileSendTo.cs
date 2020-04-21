using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class FileSendTo
    {
        public string Link { get; set; }
        public string Name { get; set; }
        public string OwnerFk { get; set; }
        public string Message { get; set; }
        public string Email { get; set; }
        
    }
}