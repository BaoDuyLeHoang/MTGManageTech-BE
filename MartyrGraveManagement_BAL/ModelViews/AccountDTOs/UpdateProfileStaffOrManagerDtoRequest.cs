﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MartyrGraveManagement_BAL.ModelViews.AccountDTOs
{
    public class UpdateProfileStaffOrManagerDtoRequest
    {
        public string? FullName { get; set; }
        public string? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? AvatarPath { get; set; }
        public string? EmailAddress { get; set; }
        public int? AreaId { get; set; } 
    }
}
