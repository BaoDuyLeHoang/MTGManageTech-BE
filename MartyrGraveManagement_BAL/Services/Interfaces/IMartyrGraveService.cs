﻿using MartyrGraveManagement_BAL.ModelViews.MartyrGraveDTOs;
using MartyrGraveManagement_DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MartyrGraveManagement_BAL.Services.Interfaces
{
    public interface IMartyrGraveService
    {
        Task<IEnumerable<MartyrGraveDtoResponse>> GetAllMartyrGravesAsync();
        Task<MartyrGraveDtoResponse> GetMartyrGraveByIdAsync(int id);
        Task<MartyrGraveDtoResponse> CreateMartyrGraveAsync(MartyrGraveDtoRequest martyrGraveDto);
        Task<(bool status, string result, string? accountName, string? password)> CreateMartyrGraveAsyncV2(MartyrGraveDtoRequest martyrGraveDto);
        Task<MartyrGraveDtoResponse> UpdateMartyrGraveAsync(int id, MartyrGraveDtoRequest martyrGraveDto);
        Task<bool> DeleteMartyrGraveAsync(int id);
        Task<List<MartyrGraveGetAllDtoResponse>> GetAllMartyrGravesForManagerAsync();

    }
}
