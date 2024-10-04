﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MartyrGraveManagement_BAL.ModelViews.AccountDTOs
{
    public class UserRegisterDtoRequest
    {
        [Required]
        [StringLength(64, ErrorMessage = "AccountName must be between 0 and 64 characters.")]
        public string AccountName { get; set; } = null!;
        [Required]
        public string Password { get; set; } = null!;
        [Required]
        public string ConfirmPassword { get; set; } = null!;
        //[StringLength(50, ErrorMessage = "Username must be between 0 and 50 characters.")]
        //public string UserName { get; set; } = null!;


        //[StringLength(10, ErrorMessage = "Phone must be between 0 and 10 characters.")]
        //public string? Phone { get; set; }
        //[StringLength(30, ErrorMessage = "Address must be between 0 and 64 characters.")]
        //public string? Address { get; set; }
        //public DateTime? Dob { get; set; }
        public int RoleId { get; set; }
        public int? AreaId { get; set; }
    }
}
