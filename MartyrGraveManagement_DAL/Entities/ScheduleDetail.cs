﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MartyrGraveManagement_DAL.Entities
{
    [Table("ScheduleDetail")]
    public class ScheduleDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int AccountId { get; set; }
        public int TaskId { get; set; }
        public int ScheduleId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Status { get; set; }

        public Account? Account { get; set; }
        public StaffTask? StaffTask { get; set; }
        public Schedule? Schedule { get; set; }
    }
}