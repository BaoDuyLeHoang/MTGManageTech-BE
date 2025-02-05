﻿using System.ComponentModel.DataAnnotations.Schema;

namespace MartyrGraveManagement_BAL.ModelViews.ReportGraveDTOs
{
    public class ReportGraveDtoResponse
    {
        public int ReportId { get; set; }
        public int RequestId { get; set; }
        public int StaffId { get; set; }
        public string? VideoFile { get; set; }
        [Column(TypeName = "nvarchar(MAX)")]
        public string? Description { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime UpdateAt { get; set; }
        public int Status { get; set; } // 1 đang chờ, 2 là từ chối, 3 là đang làm, 4 hoàn thành, 5 thất bại (quá hạn deadline)
        public byte[]? VideoContent { get; set; }

        //Ngày kết thúc
        public DateOnly? EndDate { get; set; }

        //Staff
        public string? StaffName { get; set; }
        public string? StaffPhone { get; set; }

        //MartyrGrave
        public string? MartyrCode { get; set; }

        //Customer
        public string? CustomerName { get; set; }  // Tên khách hàng
        public string? CustomerPhone { get; set; }
    }
}
