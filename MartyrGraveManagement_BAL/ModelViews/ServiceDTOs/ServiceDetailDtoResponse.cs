﻿using MartyrGraveManagement_BAL.ModelViews.MaterialDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MartyrGraveManagement_BAL.ModelViews.ServiceDTOs
{
    public class ServiceDetailDtoResponse
    {
        public int ServiceId { get; set; }
        public int CategoryId { get; set; }
        public string ServiceName { get; set; }
        public string? Description { get; set; }
        public double Price { get; set; }
        public bool Status { get; set; }

        public List<MaterialDtoResponse> Materials { get; set; } = new List<MaterialDtoResponse>();
    }
}
