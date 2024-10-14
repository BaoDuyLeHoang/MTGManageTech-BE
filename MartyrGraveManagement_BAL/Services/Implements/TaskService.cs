﻿using AutoMapper;
using MartyrGraveManagement_BAL.ModelViews.TaskDTOs;
using MartyrGraveManagement_BAL.Services.Interfaces;
using MartyrGraveManagement_DAL.Entities;
using MartyrGraveManagement_DAL.UnitOfWorks.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MartyrGraveManagement_BAL.Services.Implements
{
    public class TaskService : ITaskService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TaskService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TaskDtoResponse>> GetAllTasksAsync()
        {
            var tasks = await _unitOfWork.TaskRepository.GetAllAsync();

            if (!tasks.Any())
            {
                throw new KeyNotFoundException("No tasks found.");
            }

            // Lấy thông tin FullName từ Account và ánh xạ sang TaskDtoResponse
            var taskResponses = new List<TaskDtoResponse>();
            foreach (var task in tasks)
            {
                var account = await _unitOfWork.AccountRepository.GetByIDAsync(task.AccountId);
                var taskDto = _mapper.Map<TaskDtoResponse>(task);
                taskDto.Fullname = account?.FullName;  // Ánh xạ FullName từ Account
                taskResponses.Add(taskDto);
            }

            return taskResponses;
        }





        public async Task<IEnumerable<TaskDtoResponse>> GetTasksByAccountIdAsync(int accountId)
        {
            // Kiểm tra xem AccountId có tồn tại không
            var account = await _unitOfWork.AccountRepository.GetByIDAsync(accountId);
            if (account == null)
            {
                throw new KeyNotFoundException("Account not found.");
            }

            // Lấy danh sách các Task thuộc về account
            var tasks = await _unitOfWork.TaskRepository.GetAsync(t => t.AccountId == accountId);

            if (!tasks.Any())
            {
                throw new InvalidOperationException("This account does not have any tasks.");
            }

            // Ánh xạ FullName từ Account
            var taskResponses = new List<TaskDtoResponse>();
            foreach (var task in tasks)
            {
                var taskDto = _mapper.Map<TaskDtoResponse>(task);
                taskDto.Fullname = account?.FullName;  // Ánh xạ FullName từ Account
                taskResponses.Add(taskDto);
            }

            return taskResponses;
        }





        public async Task<TaskDtoResponse> GetTaskByIdAsync(int taskId)
        {
            // Lấy thông tin Task theo taskId
            var task = await _unitOfWork.TaskRepository.GetByIDAsync(taskId);

            if (task == null)
            {
                throw new KeyNotFoundException("Task not found.");
            }

            // Lấy thông tin FullName từ Account và ánh xạ vào TaskDtoResponse
            var account = await _unitOfWork.AccountRepository.GetByIDAsync(task.AccountId);
            var taskDto = _mapper.Map<TaskDtoResponse>(task);
            taskDto.Fullname = account?.FullName;  // Ánh xạ FullName từ Account

            return taskDto;
        }





        //public async Task<TaskDtoResponse> CreateTaskAsync(TaskDtoRequest newTask)
        //{
        //    // Kiểm tra xem AccountId có tồn tại không
        //    var account = await _unitOfWork.AccountRepository.GetByIDAsync(newTask.AccountId);
        //    if (account == null)
        //    {
        //        throw new KeyNotFoundException("AccountId does not exist.");
        //    }

        //    // Kiểm tra Role của account phải là Role 3 (Staff)
        //    if (account.RoleId != 3)
        //    {
        //        throw new UnauthorizedAccessException("The account is not authorized to perform this task (not a staff account).");
        //    }

        //    // Kiểm tra xem OrderId có tồn tại không
        //    var order = await _unitOfWork.OrderRepository.GetByIDAsync(newTask.OrderId);
        //    if (order == null)
        //    {
        //        throw new KeyNotFoundException("OrderId does not exist.");
        //    }

        //    // Kiểm tra trạng thái của Order phải là 1 (đã thanh toán)
        //    if (order.Status != 1)
        //    {
        //        throw new InvalidOperationException("Order has not been paid (status must be 1).");
        //    }

        //    // Kiểm tra nếu StartDate của nhiệm vụ (thời điểm hiện tại) lớn hơn EndDate của Order
        //    if (DateTime.Now > order.EndDate)
        //    {
        //        throw new InvalidOperationException("Cannot create a task because the start date is after the order's end date.");
        //    }

        //    // Lấy tất cả các chi tiết của Order để kiểm tra khu vực của tất cả các mộ
        //    var orderDetails = await _unitOfWork.OrderDetailRepository
        //        .GetAsync(od => od.OrderId == newTask.OrderId, includeProperties: "MartyrGrave");

        //    if (orderDetails == null || !orderDetails.Any())
        //    {
        //        throw new InvalidOperationException("Order details are not associated with any martyr grave.");
        //    }

        //    foreach (var detail in orderDetails)
        //    {
        //        var martyrGrave = detail.MartyrGrave;
        //        if (martyrGrave == null)
        //        {
        //            throw new InvalidOperationException("Order details are not associated with any martyr grave.");
        //        }

        //        // Kiểm tra nếu nhân viên chỉ được làm việc trong khu vực của họ
        //        if (account.AreaId != martyrGrave.AreaId)
        //        {
        //            throw new UnauthorizedAccessException("Staff can only work in their assigned area.");
        //        }
        //    }

        //    // Nếu tất cả các mộ đều nằm trong khu vực của nhân viên, tạo Task mới
        //    var taskEntity = new StaffTask
        //    {
        //        AccountId = newTask.AccountId,
        //        OrderId = newTask.OrderId,
        //        NameOfWork = newTask.NameOfWork,
        //        TypeOfWork = newTask.TypeOfWork,
        //        StartDate = DateTime.Now,  // Gán StartDate là thời gian hiện tại
        //        EndDate = order.EndDate,   // Lấy EndDate từ Order
        //        Description = newTask.Description,
        //        Status = 0
        //    };

        //    // Thêm Task vào cơ sở dữ liệu
        //    await _unitOfWork.TaskRepository.AddAsync(taskEntity);
        //    await _unitOfWork.SaveAsync();

        //    return _mapper.Map<TaskDtoResponse>(taskEntity);
        //}


        public async Task<TaskDtoResponse> CreateTaskAsync(TaskDtoRequest newTask, int managerId)
        {
            // Kiểm tra xem AccountId (Manager) có tồn tại không từ context của Manager đang đăng nhập
            var managerAccount = await _unitOfWork.AccountRepository.GetByIDAsync(managerId);
            if (managerAccount == null)
            {
                throw new KeyNotFoundException("ManagerId does not exist.");
            }

            // Kiểm tra Role của account phải là Role 2 (Manager)
            if (managerAccount.RoleId != 2)
            {
                throw new UnauthorizedAccessException("The account is not authorized to create this task (not a manager account).");
            }

            // Kiểm tra xem OrderId có tồn tại không
            var order = await _unitOfWork.OrderRepository.GetByIDAsync(newTask.OrderId);
            if (order == null)
            {
                throw new KeyNotFoundException("OrderId does not exist.");
            }

            // Kiểm tra trạng thái của Order phải là 1 (đã thanh toán)
            if (order.Status != 1)
            {
                throw new InvalidOperationException("Order has not been paid (status must be 1).");
            }

            // Kiểm tra nếu StartDate của nhiệm vụ (thời điểm hiện tại) lớn hơn EndDate của Order
            if (DateTime.Now > order.EndDate)
            {
                throw new InvalidOperationException("Cannot create a task because the start date is after the order's end date.");
            }

            // Nếu tất cả điều kiện hợp lệ, tạo Task mới với ManagerId được gán tạm cho AccountId
            var taskEntity = new StaffTask
            {
                AccountId = managerId,  // Gán ManagerId vào AccountId
                OrderId = newTask.OrderId,
                NameOfWork = newTask.NameOfWork,
                TypeOfWork = newTask.TypeOfWork,
                StartDate = DateTime.Now,  // Gán StartDate là thời gian hiện tại
                EndDate = order.EndDate,   // Lấy EndDate từ Order
                Description = newTask.Description,
                Status = 0  // Status ban đầu là 0
            };

            // Thêm Task vào cơ sở dữ liệu
            await _unitOfWork.TaskRepository.AddAsync(taskEntity);
            await _unitOfWork.SaveAsync();

            return _mapper.Map<TaskDtoResponse>(taskEntity);
        }


        public async Task<TaskDtoResponse> AssignTaskAsync(int taskId, int staffId)
        {
            // 1. Lấy Task cần gán từ cơ sở dữ liệu
            var task = await _unitOfWork.TaskRepository.GetByIDAsync(taskId);
            if (task == null)
            {
                throw new KeyNotFoundException("TaskId does not exist.");
            }

            // 2. Kiểm tra trạng thái của Task phải là 0 (chưa gán)
            if (task.Status != 0)
            {
                throw new InvalidOperationException("Task has already been assigned or is in progress.");
            }

            // 3. Lấy thông tin nhân viên (Staff) để gán Task
            var staffAccount = await _unitOfWork.AccountRepository.GetByIDAsync(staffId);
            if (staffAccount == null)
            {
                throw new KeyNotFoundException("StaffId does not exist.");
            }

            // 4. Kiểm tra Role của account phải là Role 3 (Staff)
            if (staffAccount.RoleId != 3)
            {
                throw new UnauthorizedAccessException("The account is not a staff account.");
            }

            // 5. Kiểm tra nếu nhân viên chỉ được làm việc trong khu vực của họ
            var orderDetails = await _unitOfWork.OrderDetailRepository
                .GetAsync(od => od.OrderId == task.OrderId, includeProperties: "MartyrGrave");

            if (orderDetails == null || !orderDetails.Any())
            {
                throw new InvalidOperationException("No details found for this order.");
            }

            foreach (var detail in orderDetails)
            {
                if (detail.MartyrGrave == null || detail.MartyrGrave.AreaId != staffAccount.AreaId)
                {
                    throw new UnauthorizedAccessException("Staff can only be assigned to work in their area.");
                }
            }

            // 6. Gán Task cho Staff và cập nhật trạng thái sang 1 (đã nhận)
            task.AccountId = staffId;
            task.Status = 1; // Cập nhật trạng thái sang 1 khi Staff nhận task

            // 7. Lưu thay đổi
            await _unitOfWork.TaskRepository.UpdateAsync(task);
            await _unitOfWork.SaveAsync();

            return _mapper.Map<TaskDtoResponse>(task);
        }




        //public async Task<TaskDtoResponse> UpdateTaskStatusAsync(int taskId, int accountId, int newStatus, string? urlImage = null, string? reason = null)
        //{
        //    using (var transaction = await _unitOfWork.BeginTransactionAsync()) // Bắt đầu transaction
        //    {
        //        try
        //        {
        //            // 1. Kiểm tra xem TaskId có tồn tại không
        //            var task = await _unitOfWork.TaskRepository.GetByIDAsync(taskId);
        //            if (task == null)
        //            {
        //                throw new KeyNotFoundException("TaskId does not exist.");
        //            }

        //            // 2. Kiểm tra xem AccountId có tồn tại không
        //            var account = await _unitOfWork.AccountRepository.GetByIDAsync(accountId);
        //            if (account == null)
        //            {
        //                throw new KeyNotFoundException("AccountId does not exist.");
        //            }

        //            // 3. Kiểm tra Role của account phải là Role 3 (Staff)
        //            if (account.RoleId != 3)
        //            {
        //                throw new UnauthorizedAccessException("The account is not authorized to perform this task (not a staff account).");
        //            }

        //            // 4. Lấy các chi tiết đơn hàng (OrderDetails) từ đơn hàng liên kết với Task
        //            var orderDetails = await _unitOfWork.OrderDetailRepository
        //                .GetAsync(od => od.OrderId == task.OrderId, includeProperties: "MartyrGrave");

        //            if (orderDetails == null || !orderDetails.Any())
        //            {
        //                throw new InvalidOperationException("No order details found for this task.");
        //            }

        //            // Kiểm tra nếu nhân viên chỉ được làm việc trong khu vực của họ
        //            foreach (var detail in orderDetails)
        //            {
        //                if (detail.MartyrGrave?.AreaId != account.AreaId)
        //                {
        //                    throw new UnauthorizedAccessException("Staff can only work in their assigned area.");
        //                }
        //            }

        //            // 5. Kiểm tra trạng thái hiện tại của Task và áp dụng logic chuyển đổi trạng thái
        //            if (task.Status == 0)
        //            {
        //                // Nếu task đang ở trạng thái 0 (chưa nhận), có thể chuyển lên 1 hoặc 2
        //                if (newStatus == 1)
        //                {
        //                    task.Status = 1;  // Staff nhận task
        //                }
        //                else if (newStatus == 2)
        //                {
        //                    task.Status = 2;  // Staff từ chối task, không thể thay đổi tiếp
        //                }
        //                else
        //                {
        //                    throw new InvalidOperationException("You can only update status to 1 (accept) or 2 (reject) from status 0.");
        //                }
        //            }
        //            else if (task.Status == 1)
        //            {
        //                // Nếu task đang ở trạng thái 1 (đã nhận), có thể chuyển sang 3 hoặc 4
        //                if (newStatus == 3)
        //                {
        //                    task.Status = 3;  // Task chuyển sang hoàn thành
        //                }
        //                else if (newStatus == 4)
        //                {
        //                    task.Status = 4;  // Task chuyển sang thất bại
        //                }
        //                else
        //                {
        //                    throw new InvalidOperationException("You can only update status to 3 (complete) or 4 (fail) from status 1.");
        //                }
        //            }
        //            else if (task.Status == 2)
        //            {
        //                // Task đã bị từ chối, không thể thay đổi trạng thái nữa
        //                throw new InvalidOperationException("Task has been rejected and cannot be updated further.");
        //            }
        //            else if (task.Status == 3 || task.Status == 4)
        //            {
        //                // Nếu task đã hoàn thành hoặc hủy bỏ, không thể thay đổi trạng thái nữa
        //                throw new InvalidOperationException("Task has been completed or canceled and cannot be updated further.");
        //            }
        //            else
        //            {
        //                throw new InvalidOperationException("Invalid status transition.");
        //            }

        //            // 6. Nếu có hình ảnh check-in, cập nhật UrlImage
        //            if (!string.IsNullOrEmpty(urlImage))
        //            {
        //                task.UrlImage = urlImage;
        //            }

        //            // 7. Nếu có lý do, cập nhật lý do
        //            if (!string.IsNullOrEmpty(reason))
        //            {
        //                task.Reason = reason;
        //            }

        //            // 8. Lưu thay đổi vào cơ sở dữ liệu
        //            await _unitOfWork.TaskRepository.UpdateAsync(task);
        //            await _unitOfWork.SaveAsync();

        //            // 9. Commit transaction nếu không có lỗi
        //            await transaction.CommitAsync();

        //            return _mapper.Map<TaskDtoResponse>(task);
        //        }
        //        catch (Exception ex)
        //        {
        //            // Rollback transaction nếu có lỗi
        //            await transaction.RollbackAsync();
        //            throw new Exception($"Failed to update task: {ex.Message}");
        //        }
        //    }
        //}


        public async Task<TaskDtoResponse> UpdateTaskStatusAsync(int taskId, int accountId, int newStatus, string? urlImage = null, string? reason = null)
        {
            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    // 1. Kiểm tra TaskId có tồn tại không
                    var task = await _unitOfWork.TaskRepository.GetByIDAsync(taskId);
                    if (task == null)
                    {
                        throw new KeyNotFoundException("TaskId does not exist.");
                    }

                    // 2. Kiểm tra AccountId có tồn tại không
                    var account = await _unitOfWork.AccountRepository.GetByIDAsync(accountId);
                    if (account == null)
                    {
                        throw new KeyNotFoundException("AccountId does not exist.");
                    }

                    // 3. Kiểm tra Role của account phải là Role 3 (Staff)
                    if (account.RoleId != 3)
                    {
                        throw new UnauthorizedAccessException("The account is not authorized to perform this task (not a staff account).");
                    }

                    // 4. Kiểm tra khu vực làm việc của Staff
                    var orderDetails = await _unitOfWork.OrderDetailRepository
                        .GetAsync(od => od.OrderId == task.OrderId, includeProperties: "MartyrGrave");

                    if (!orderDetails.Any())
                    {
                        throw new InvalidOperationException("No order details found for this task.");
                    }

                    foreach (var detail in orderDetails)
                    {
                        if (detail.MartyrGrave?.AreaId != account.AreaId)
                        {
                            throw new UnauthorizedAccessException("Staff can only work in their assigned area.");
                        }
                    }

                    // 5. Cập nhật trạng thái của Task
                    if (task.Status == 1)
                    {
                        // Staff có thể từ chối (lên 2) hoặc nhận task (lên 3)
                        if (newStatus == 2)
                        {
                            task.Status = 2;  // Từ chối task, không thể thay đổi tiếp
                        }
                        else if (newStatus == 3)
                        {
                            task.Status = 3;  // Nhận task, chuyển sang trạng thái đang thực hiện

                            // Cập nhật trạng thái của Order sang "đang thực hiện"
                            var order = await _unitOfWork.OrderRepository.GetByIDAsync(task.OrderId);
                            if (order != null)
                            {
                                order.Status = 3;  // Order chuyển sang trạng thái "đang thực hiện"
                                await _unitOfWork.OrderRepository.UpdateAsync(order);
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException("You can only update status to 2 (reject) or 3 (in progress) from status 1.");
                        }
                    }
                    else if (task.Status == 3)
                    {
                        // Task đang ở trạng thái "đang thực hiện", có thể hoàn thành (lên 4)
                        if (newStatus == 4)
                        {
                            if (string.IsNullOrEmpty(urlImage))
                            {
                                throw new InvalidOperationException("You must provide an image when completing the task.");
                            }

                            task.Status = 4;  // Hoàn thành task
                            task.UrlImage = urlImage;  // Cập nhật UrlImage

                            // Cập nhật trạng thái của Order sang "hoàn thành"
                            var order = await _unitOfWork.OrderRepository.GetByIDAsync(task.OrderId);
                            if (order != null)
                            {
                                order.Status = 4;  // Order chuyển sang trạng thái "hoàn thành"
                                await _unitOfWork.OrderRepository.UpdateAsync(order);
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException("You can only update status to 4 (completed) from status 3.");
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Invalid status transition.");
                    }

                    // 6. Nếu có lý do, cập nhật lý do
                    if (!string.IsNullOrEmpty(reason))
                    {
                        task.Reason = reason;
                    }

                    // 7. Lưu thay đổi vào cơ sở dữ liệu
                    await _unitOfWork.TaskRepository.UpdateAsync(task);
                    await _unitOfWork.SaveAsync();

                    // 8. Commit transaction nếu không có lỗi
                    await transaction.CommitAsync();

                    return _mapper.Map<TaskDtoResponse>(task);
                }
                catch (Exception ex)
                {
                    // Rollback transaction nếu có lỗi
                    await transaction.RollbackAsync();
                    throw new Exception($"Failed to update task: {ex.Message}");
                }
            }
        }



        public async Task<TaskDtoResponse> ReassignTaskAsync(int taskId, int newAccountId)
        {
            using (var transaction = await _unitOfWork.BeginTransactionAsync()) // Bắt đầu transaction
            {
                try
                {
                    // 1. Kiểm tra xem TaskId có tồn tại không
                    var task = await _unitOfWork.TaskRepository.GetByIDAsync(taskId);
                    if (task == null)
                    {
                        throw new KeyNotFoundException("TaskId does not exist.");
                    }

                    // 2. Kiểm tra trạng thái hiện tại của Task
                    if (task.Status != 2)
                    {
                        throw new InvalidOperationException("Task can only be reassigned if it is in 2 (rejected).");
                    }

                    // 3. Kiểm tra xem AccountId mới có tồn tại không
                    var newAccount = await _unitOfWork.AccountRepository.GetByIDAsync(newAccountId);
                    if (newAccount == null)
                    {
                        throw new KeyNotFoundException("New AccountId does not exist.");
                    }

                    // 4. Kiểm tra Role của account mới phải là Role 3 (Staff)
                    if (newAccount.RoleId != 3)
                    {
                        throw new UnauthorizedAccessException("The new account is not authorized to perform this task (not a staff account).");
                    }

                    // 5. Lấy các chi tiết đơn hàng (OrderDetails) từ đơn hàng liên kết với Task
                    var orderDetails = await _unitOfWork.OrderDetailRepository
                        .GetAsync(od => od.OrderId == task.OrderId, includeProperties: "MartyrGrave");

                    if (orderDetails == null || !orderDetails.Any())
                    {
                        throw new InvalidOperationException("No order details found for this task.");
                    }

                    // 6. Kiểm tra nếu nhân viên chỉ được làm việc trong khu vực của họ
                    foreach (var detail in orderDetails)
                    {
                        if (detail.MartyrGrave?.AreaId != newAccount.AreaId)
                        {
                            throw new UnauthorizedAccessException("Staff can only work in their assigned area.");
                        }
                    }

                    // 7. Cập nhật AccountId mới và Status của task
                    task.AccountId = newAccountId;
                    task.Status = 1; // Task đã được bàn giao

                    // 8. Lưu thay đổi vào cơ sở dữ liệu
                    await _unitOfWork.TaskRepository.UpdateAsync(task);
                    await _unitOfWork.SaveAsync();

                    // 9. Commit transaction nếu không có lỗi
                    await transaction.CommitAsync();

                    return _mapper.Map<TaskDtoResponse>(task);
                }
                catch (Exception ex)
                {
                    // Rollback transaction nếu có lỗi
                    await transaction.RollbackAsync();
                    throw new Exception($"Failed to reassign task: {ex.Message}");
                }
            }
        }



        public async Task<bool> DeleteTaskAsync(int taskId)
        {
            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    // 1. Kiểm tra xem TaskId có tồn tại không
                    var task = await _unitOfWork.TaskRepository.GetByIDAsync(taskId);
                    if (task == null)
                    {
                        throw new KeyNotFoundException("TaskId does not exist.");
                    }

                    // 2. Kiểm tra trạng thái của Task
                    if (task.Status != 0)
                    {
                        throw new InvalidOperationException("Only tasks with status 0 (not assigned) can be deleted.");
                    }

                    // 3. Xóa Task khỏi cơ sở dữ liệu
                    await _unitOfWork.TaskRepository.DeleteAsync(task);
                    await _unitOfWork.SaveAsync();

                    // Commit transaction
                    await transaction.CommitAsync();

                    return true; // Task đã xóa thành công
                }
                catch (Exception ex)
                {
                    // Rollback transaction nếu có lỗi
                    await transaction.RollbackAsync();
                    throw new Exception(ex.Message);
                }
            }
        }

    }
}
