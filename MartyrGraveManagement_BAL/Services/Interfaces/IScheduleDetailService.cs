﻿using MartyrGraveManagement_BAL.ModelViews.ScheduleDetailDTOs;
using MartyrGraveManagement_BAL.ModelViews.ScheduleDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MartyrGraveManagement_BAL.Services.Interfaces
{
    public interface IScheduleDetailService
    {
        Task<List<string>> CreateScheduleDetail(List<ScheduleDetailDtoRequest> requests, int accountId);
        Task<string> UpdateScheduleDetail(int request, int accountId, int Id);
        Task<List<ScheduleDetailListDtoResponse>> GetScheduleDetailStaff(int accountId, int scheduleId);
        Task<List<ScheduleDetailListDtoResponse>> GetSchedulesStaff(int accountId, DateTime FromDate, DateTime ToDate);
    }
}
