﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MartyrGraveManagement_DAL.Entities
{
    public class ReportGrave
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReportId { get; set; }
        public int RequestId { get; set; }
        public int StaffId { get; set; }
        public string? VideoFile { get; set; }
        [Column(TypeName = "nvarchar(MAX)")]
        public string? Description { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime UpdateAt { get; set; }
        public int Status { get; set; } // 1 đang chờ, 2 là từ chối, 3 là đang làm, 4 hoàn thành, 5 thất bại (quá hạn deadline)

        public RequestCustomer? RequestCustomer { get; set; }
        public IEnumerable<ReportImage>? ReportImages { get; set; }
    }

}
