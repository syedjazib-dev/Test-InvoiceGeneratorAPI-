﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceGenerator.Model.Models.DTOs
{
    public class UserLoginResponseDTO
    {

        public string UserId { get; set; }
        public TokenDTO TokenDTO { get; set; }
    }
}
