﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceGenerator.Model.Models.DTOs
{
    public class TokenDTO
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
