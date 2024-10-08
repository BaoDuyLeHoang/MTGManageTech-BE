﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MartyrGraveManagement_BAL.ModelViews.MartyrGraveDTOs
{
    public class MartyrGraveSearchDtoRequest
    {
        public string? Name { get; set; }
        public int? YearOfBirth { get; set; }  // Chỉ nhận năm
        public int? YearOfSacrifice { get; set; }  // Chỉ nhận năm
        public string? HomeTown { get; set; }
    }

}