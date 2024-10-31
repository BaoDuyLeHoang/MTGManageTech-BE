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
            var tasks = await _unitOfWork.TaskRepository.GetAsync(includeProperties: "OrderDetail.Service,OrderDetail.MartyrGrave");

            if (!tasks.Any())
            {
                throw new KeyNotFoundException("No tasks found.");
            }

            // Lấy thông tin FullName từ Account và ánh xạ sang TaskDtoResponse
            var taskResponses = new List<TaskDtoResponse>();
            foreach (var task in tasks)
            {
                // Lấy thông tin Account để ánh xạ Fullname
                var account = await _unitOfWork.AccountRepository.GetByIDAsync(task.AccountId);

                // Ánh xạ task sang TaskDtoResponse
                var taskDto = _mapper.Map<TaskDtoResponse>(task);
                taskDto.Fullname = account?.FullName;  // Ánh xạ FullName từ Account

                // Lấy thông tin từ OrderDetail, Service, và MartyrGrave
                taskDto.ServiceName = task.OrderDetail?.Service?.ServiceName;
                taskDto.ServiceDescription = task.OrderDetail?.Service?.Description;

                // Ghép vị trí mộ từ AreaNumber, RowNumber, và MartyrNumber
                var martyrGrave = task.OrderDetail?.MartyrGrave;
                if (martyrGrave != null)
                {
                    var location = await _unitOfWork.LocationRepository.GetByIDAsync(martyrGrave.LocationId);
                    taskDto.GraveLocation = $"K{location.AreaNumber}-R{location.RowNumber}-{location.MartyrNumber}";
                }

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

            // Lấy danh sách các Task thuộc về account, bao gồm các bảng liên quan
            var tasks = await _unitOfWork.TaskRepository.GetAsync(t => t.AccountId == accountId, includeProperties: "OrderDetail.Service,OrderDetail.MartyrGrave");

            if (!tasks.Any())
            {
                throw new InvalidOperationException("This account does not have any tasks.");
            }

            // Ánh xạ FullName từ Account và các thông tin khác
            var taskResponses = new List<TaskDtoResponse>();
            foreach (var task in tasks)
            {
                var taskDto = _mapper.Map<TaskDtoResponse>(task);
                taskDto.Fullname = account?.FullName;  // Ánh xạ FullName từ Account

                // Lấy thông tin từ OrderDetail, Service, và MartyrGrave
                taskDto.ServiceName = task.OrderDetail?.Service?.ServiceName;
                taskDto.ServiceDescription = task.OrderDetail?.Service?.Description;

                // Ghép vị trí mộ từ AreaNumber, RowNumber, và MartyrNumber
                var martyrGrave = task.OrderDetail?.MartyrGrave;
                if (martyrGrave != null)
                {
                    var location = await _unitOfWork.LocationRepository.GetByIDAsync(martyrGrave.LocationId);
                    taskDto.GraveLocation = $"K{location.AreaNumber}-R{location.RowNumber}-{location.MartyrNumber}";
                }

                taskResponses.Add(taskDto);
            }

            return taskResponses;
        }






        public async Task<TaskDtoResponse> GetTaskByIdAsync(int taskId)
        {
            // Lấy thông tin Task theo taskId, bao gồm các bảng liên quan
            var task = await _unitOfWork.TaskRepository.GetAsync(t => t.TaskId == taskId, includeProperties: "OrderDetail.Service,OrderDetail.MartyrGrave");

            // Đảm bảo task trả về là một thực thể duy nhất
            var singleTask = task.FirstOrDefault(); // Lấy task đầu tiên (hoặc null nếu không có task nào)

            if (singleTask == null)
            {
                throw new KeyNotFoundException("Task not found.");
            }

            // Lấy thông tin Account để ánh xạ Fullname
            var account = await _unitOfWork.AccountRepository.GetByIDAsync(singleTask.AccountId);

            // Ánh xạ task sang TaskDtoResponse
            var taskDto = _mapper.Map<TaskDtoResponse>(singleTask);
            taskDto.Fullname = account?.FullName;  // Ánh xạ FullName từ Account

            // Lấy thông tin từ OrderDetail, Service, và MartyrGrave
            taskDto.ServiceName = singleTask.OrderDetail?.Service?.ServiceName;
            taskDto.ServiceDescription = singleTask.OrderDetail?.Service?.Description;

            // Ghép vị trí mộ từ AreaNumber, RowNumber, và MartyrNumber
            var martyrGrave = singleTask.OrderDetail?.MartyrGrave;
            if (martyrGrave != null)
            {
                var location = await _unitOfWork.LocationRepository.GetByIDAsync(martyrGrave.LocationId);
                taskDto.GraveLocation = $"K{location.AreaNumber}-R{location.RowNumber}-{location.MartyrNumber}";
            }

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


        //public async Task<TaskDtoResponse> CreateTaskAsync(TaskDtoRequest newTask, int managerId)
        //{
        //    // 1. Kiểm tra xem AccountId (Manager) có tồn tại không từ context của Manager đang đăng nhập
        //    var managerAccount = await _unitOfWork.AccountRepository.GetByIDAsync(managerId);
        //    if (managerAccount == null) 
        //    {
        //        throw new KeyNotFoundException("ManagerId does not exist.");
        //    }

        //    // 2. Kiểm tra Role của account phải là Role 2 (Manager)
        //    if (managerAccount.RoleId != 2)
        //    {
        //        throw new UnauthorizedAccessException("The account is not authorized to create this task (not a manager account).");
        //    }

        //    // 3. Kiểm tra xem AccountId của staff có tồn tại không
        //    var staffAccount = await _unitOfWork.AccountRepository.GetByIDAsync(newTask.AccountId);
        //    if (staffAccount == null)
        //    {
        //        throw new KeyNotFoundException("Staff AccountId does not exist.");
        //    }

        //    // 4. Kiểm tra Role của account phải là Role 3 (Staff)
        //    if (staffAccount.RoleId != 3)
        //    {
        //        throw new UnauthorizedAccessException("The specified account is not a staff account.");
        //    }

        //    // 5. Kiểm tra xem OrderId có tồn tại không
        //    var order = await _unitOfWork.OrderRepository.GetByIDAsync(newTask.OrderId);
        //    if (order == null)
        //    {
        //        throw new KeyNotFoundException("OrderId does not exist.");
        //    }

        //    // 6. Kiểm tra trạng thái của Order phải là 1 (đã thanh toán)
        //    if (order.Status != 1)
        //    {
        //        throw new InvalidOperationException("Order has not been paid (status must be 1).");
        //    }

        //    // 7. Kiểm tra nếu StartDate của nhiệm vụ (thời điểm hiện tại) lớn hơn EndDate của Order
        //    if (DateTime.Now > order.EndDate)
        //    {
        //        throw new InvalidOperationException("Cannot create a task because the start date is after the order's end date.");
        //    }

        //    // 8. Kiểm tra xem OrderDetail có tồn tại không
        //    var orderDetail = await _unitOfWork.OrderDetailRepository.GetByIDAsync(newTask.DetailId);
        //    if (orderDetail == null)
        //    {
        //        throw new KeyNotFoundException("OrderDetailId does not exist.");
        //    }

        //    // 9. Kiểm tra nếu OrderDetail có liên quan đến OrderId được truyền vào
        //    if (orderDetail.OrderId != newTask.OrderId)
        //    {
        //        throw new InvalidOperationException("The specified OrderDetail does not belong to the given Order.");
        //    }

        //    // 10. Lấy thông tin MartyrGrave từ MartyrId
        //    var martyrGrave = await _unitOfWork.MartyrGraveRepository.GetByIDAsync(orderDetail.MartyrId);
        //    if (martyrGrave == null)
        //    {
        //        throw new KeyNotFoundException("MartyrGrave does not exist.");
        //    }

        //    // 11. Kiểm tra khu vực làm việc của Staff
        //    if (martyrGrave.AreaId != staffAccount.AreaId)
        //    {
        //        throw new UnauthorizedAccessException("Staff can only be assigned tasks in their assigned area.");
        //    }

        //    // 12. Kiểm tra xem Task đã tồn tại với OrderDetail này chưa
        //    if (orderDetail.StaffTask != null)
        //    {
        //        throw new InvalidOperationException("A task has already been created for this OrderDetail.");
        //    }

        //    // 13. Tự động điều chỉnh EndDate của Task không được vượt quá EndDate của Order
        //    DateTime taskEndDate = order.EndDate;  // Mặc định lấy EndDate của Order

        //    // Nếu có EndDate từ phía request và nhỏ hơn hoặc bằng EndDate của Order thì sử dụng
        //    if (newTask.EndDate <= order.EndDate)
        //    {
        //        taskEndDate = newTask.EndDate;
        //    }

        //    // 14. Nếu tất cả điều kiện hợp lệ, tạo Task mới với AccountId của Staff
        //    var taskEntity = new StaffTask
        //    {
        //        AccountId = newTask.AccountId,  // Gán AccountId của staff từ request
        //        OrderId = newTask.OrderId,
        //        DetailId = newTask.DetailId,  // Gán OrderDetailId
        //        Description = newTask.Description,
        //        StartDate = DateTime.Now,  // Gán StartDate là thời gian hiện tại
        //        EndDate = taskEndDate,   // EndDate là giá trị đã điều chỉnh
        //        Status = 1,  // Status ban đầu là 1 (đã bàn giao)
        //        ImagePath1 = newTask.ImagePath1,
        //        ImagePath2 = newTask.ImagePath2,
        //        ImagePath3 = newTask.ImagePath3
        //    };

        //    // 15. Liên kết Task với OrderDetail
        //    orderDetail.StaffTask = taskEntity;

        //    // 16. Thêm Task vào cơ sở dữ liệu
        //    await _unitOfWork.TaskRepository.AddAsync(taskEntity);
        //    await _unitOfWork.SaveAsync();

        //    // 17. Trả về DTO của Task đã tạo
        //    return _mapper.Map<TaskDtoResponse>(taskEntity);
        //}


        //public async Task<List<TaskDtoResponse>> CreateTaskAsync(TaskBatchCreateRequest newTaskBatch, int managerId)
        //{
        //    // 1. Kiểm tra xem AccountId (Manager) có tồn tại không từ context của Manager đang đăng nhập
        //    var managerAccount = await _unitOfWork.AccountRepository.GetByIDAsync(managerId);
        //    if (managerAccount == null)
        //    {
        //        throw new KeyNotFoundException("ManagerId does not exist.");
        //    }

        //    // 2. Kiểm tra Role của account phải là Role 2 (Manager)
        //    if (managerAccount.RoleId != 2)
        //    {
        //        throw new UnauthorizedAccessException("The account is not authorized to create this task (not a manager account).");
        //    }

        //    // 3. Kiểm tra xem OrderId có tồn tại không
        //    var order = await _unitOfWork.OrderRepository.GetByIDAsync(newTaskBatch.OrderId);
        //    if (order == null)
        //    {
        //        throw new KeyNotFoundException("OrderId does not exist.");
        //    }

        //    // 4. Kiểm tra trạng thái của Order phải là 1 (đã thanh toán)
        //    if (order.Status != 1)
        //    {
        //        throw new InvalidOperationException("Order has not been paid (status must be 1).");
        //    }

        //    // 5. Kiểm tra nếu StartDate của nhiệm vụ (thời điểm hiện tại) lớn hơn EndDate của Order
        //    if (DateTime.Now > order.EndDate)
        //    {
        //        throw new InvalidOperationException("Cannot create a task because the start date is after the order's end date.");
        //    }

        //    // 6. Lấy tất cả OrderDetail liên quan đến OrderId
        //    var orderDetails = await _unitOfWork.OrderDetailRepository.GetAsync(od => od.OrderId == newTaskBatch.OrderId);

        //    if (!orderDetails.Any())
        //    {
        //        throw new KeyNotFoundException("No order details found for the given OrderId.");
        //    }

        //    if (orderDetails.Count() != newTaskBatch.TaskDetails.Count)
        //    {
        //        throw new InvalidOperationException("The number of task details does not match the number of order details.");
        //    }

        //    var taskResponses = new List<TaskDtoResponse>();

        //    // 7. Tạo task cho từng OrderDetail
        //    for (int i = 0; i < orderDetails.Count(); i++)
        //    {
        //        var orderDetail = orderDetails.ElementAt(i);
        //        var taskDetail = newTaskBatch.TaskDetails[i];



        //        // Lấy thông tin nhân viên (staff)
        //        var staffAccount = await _unitOfWork.AccountRepository.GetByIDAsync(taskDetail.AccountId);
        //        if (staffAccount == null || staffAccount.RoleId != 3)
        //        {
        //            throw new KeyNotFoundException("Staff AccountId does not exist or is not a valid staff account.");
        //        }

        //        // Lấy thông tin MartyrGrave từ MartyrId
        //        var martyrGrave = await _unitOfWork.MartyrGraveRepository.GetByIDAsync(orderDetail.MartyrId);
        //        if (martyrGrave == null)
        //        {
        //            throw new KeyNotFoundException("MartyrGrave does not exist for OrderDetailId: " + orderDetail.DetailId);
        //        }

        //        // Kiểm tra khu vực làm việc của Staff
        //        if (martyrGrave.AreaId != staffAccount.AreaId)
        //        {
        //            throw new UnauthorizedAccessException("Staff can only be assigned tasks in their assigned area for OrderDetailId: " + orderDetail.DetailId);
        //        }

        //        // Tự động điều chỉnh EndDate của Task không được vượt quá EndDate của Order
        //        DateTime taskEndDate = order.EndDate;

        //        if (taskDetail.EndDate <= order.EndDate)
        //        {
        //            taskEndDate = taskDetail.EndDate;
        //        }

        //        // Tạo task mới
        //        var taskEntity = new StaffTask
        //        {
        //            AccountId = taskDetail.AccountId,
        //            OrderId = newTaskBatch.OrderId,
        //            DetailId = orderDetail.DetailId,
        //            StartDate = DateTime.Now,
        //            EndDate = taskEndDate,
        //            Status = 1  // Trạng thái ban đầu là đã bàn giao
        //        };

        //        // Gán task cho order detail
        //        orderDetail.StaffTask = taskEntity;

        //        await _unitOfWork.TaskRepository.AddAsync(taskEntity);

        //        taskResponses.Add(_mapper.Map<TaskDtoResponse>(taskEntity));
        //    }

        //    await _unitOfWork.SaveAsync();

        //    return taskResponses;
        //}



        public async Task<List<TaskDtoResponse>> CreateTasksAsync(List<TaskDtoRequest> taskDtos)
        {//
            var taskResponses = new List<TaskDtoResponse>();

            foreach (var taskDto in taskDtos)
            {
                // Kiểm tra xem AccountId (nhân viên) có tồn tại không
                var staffAccount = await _unitOfWork.AccountRepository.GetByIDAsync(taskDto.AccountId);
                if (staffAccount == null || staffAccount.RoleId != 3)
                {
                    throw new KeyNotFoundException("Staff AccountId does not exist or is not a valid staff account.");
                }

                // Kiểm tra xem OrderId có tồn tại không
                var order = await _unitOfWork.OrderRepository.GetByIDAsync(taskDto.OrderId);
                if (order == null)
                {
                    throw new KeyNotFoundException("OrderId does not exist.");
                }

                // Kiểm tra xem DetailId có tồn tại không
                var orderDetail = await _unitOfWork.OrderDetailRepository.GetByIDAsync(taskDto.DetailId);
                if (orderDetail == null || orderDetail.OrderId != taskDto.OrderId)
                {
                    throw new InvalidOperationException("Invalid DetailId or does not belong to the given OrderId.");
                }


                // Tự động điều chỉnh EndDate của Task không được vượt quá EndDate của Order
                DateTime taskEndDate = order.ExpectedCompletionDate;
                if (taskDto.EndDate <= order.ExpectedCompletionDate)
                {
                    taskEndDate = taskDto.EndDate;
                }

                // Tạo task mới
                var taskEntity = new StaffTask
                {
                    AccountId = taskDto.AccountId,
                    OrderId = taskDto.OrderId,
                    DetailId = taskDto.DetailId,
                    StartDate = DateTime.Now,
                    EndDate = taskEndDate,
                    Status = 1  // Trạng thái ban đầu là đã bàn giao
                };

                // Gán task cho order detail
                //orderDetail.StaffTask = taskEntity;

                // Thêm Task vào cơ sở dữ liệu
                await _unitOfWork.TaskRepository.AddAsync(taskEntity);

                // Thêm vào danh sách kết quả
                taskResponses.Add(_mapper.Map<TaskDtoResponse>(taskEntity));
            }

            await _unitOfWork.SaveAsync();

            return taskResponses;
        }






        //public async Task<TaskDtoResponse> UpdateTaskStatusAsync(int taskId, int accountId, int newStatus, List<string>? urlImages = null, string? reason = null)
        //{
        //    using (var transaction = await _unitOfWork.BeginTransactionAsync())
        //    {
        //        try
        //        {
        //            // 1. Kiểm tra TaskId có tồn tại không
        //            var task = await _unitOfWork.TaskRepository.GetByIDAsync(taskId);
        //            if (task == null)
        //            {
        //                throw new KeyNotFoundException("TaskId does not exist.");
        //            }

        //            // 2. Kiểm tra AccountId có tồn tại không
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

        //            // 4. Kiểm tra khu vực làm việc của Staff
        //            var orderDetail = await _unitOfWork.OrderDetailRepository.GetAsync(
        //            od => od.DetailId == task.DetailId,
        //            includeProperties: "MartyrGrave"
        //            );
        //            var detailEntity = orderDetail.FirstOrDefault();
        //            if (detailEntity == null || detailEntity.MartyrGrave?.AreaId != account.AreaId)
        //            {
        //                throw new UnauthorizedAccessException("Staff can only work in their assigned area.");
        //            }

        //            // 5. Cập nhật trạng thái của Task
        //            if (task.Status == 1)
        //            {
        //                if (newStatus == 2)
        //                {
        //                    task.Status = 2;  // Từ chối task
        //                }
        //                else if (newStatus == 3)
        //                {
        //                    task.Status = 3;  // Nhận task

        //                    // Cập nhật trạng thái của Order sang "đang thực hiện"
        //                    var order = await _unitOfWork.OrderRepository.GetByIDAsync(task.OrderId);
        //                    if (order != null)
        //                    {
        //                        order.Status = 3;  // Order chuyển sang trạng thái "đang thực hiện"
        //                        await _unitOfWork.OrderRepository.UpdateAsync(order);
        //                    }
        //                }
        //                else
        //                {
        //                    throw new InvalidOperationException("You can only update status to 2 (reject) or 3 (in progress) from status 1.");
        //                }
        //            }
        //            else if (task.Status == 3)
        //            {
        //                // Task đang ở trạng thái "đang thực hiện", có thể hoàn thành (lên 4)
        //                if (newStatus == 4)
        //                {
        //                    if (urlImages == null || !urlImages.Any())
        //                    {
        //                        throw new InvalidOperationException("You must provide at least one image when completing the task.");
        //                    }

        //                    // Cập nhật 3 ảnh nếu có
        //                    task.ImagePath1 = urlImages.ElementAtOrDefault(0);  // Ảnh 1
        //                    task.ImagePath2 = urlImages.ElementAtOrDefault(1);  // Ảnh 2 (nếu có)
        //                    task.ImagePath3 = urlImages.ElementAtOrDefault(2);  // Ảnh 3 (nếu có)

        //                    task.Status = 4;  // Hoàn thành task

        //                    // Cập nhật trạng thái của Order sang "hoàn thành"
        //                    var order = await _unitOfWork.OrderRepository.GetByIDAsync(task.OrderId);
        //                    if (order != null)
        //                    {
        //                        order.Status = 4;  // Order chuyển sang trạng thái "hoàn thành"
        //                        await _unitOfWork.OrderRepository.UpdateAsync(order);
        //                    }
        //                }
        //                else
        //                {
        //                    throw new InvalidOperationException("You can only update status to 4 (completed) from status 3.");
        //                }
        //            }
        //            else
        //            {
        //                throw new InvalidOperationException("Invalid status transition.");
        //            }

        //            // 6. Nếu có lý do, cập nhật lý do
        //            if (!string.IsNullOrEmpty(reason))
        //            {
        //                task.Reason = reason;
        //            }

        //            // 7. Lưu thay đổi vào cơ sở dữ liệu
        //            await _unitOfWork.TaskRepository.UpdateAsync(task);
        //            await _unitOfWork.SaveAsync();

        //            // 8. Commit transaction nếu không có lỗi
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


        public async Task<TaskDtoResponse> UpdateTaskStatusAsync(int taskId, int newStatus)
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

                    // 2. Cập nhật trạng thái của Task
                    if (task.Status == 1)
                    {
                        if (newStatus == 2)
                        {
                            task.Status = 2;  // Từ chối task
                        }
                        else if (newStatus == 3)
                        {
                            task.Status = 3;  // Nhận task

                            // Kiểm tra nếu tất cả các Task của Order đều ở trạng thái "đang thực hiện"
                            var allTasksForOrder = await _unitOfWork.TaskRepository.GetAsync(t => t.OrderId == task.OrderId);

                            if (allTasksForOrder.All(t => t.Status == 3)) // Kiểm tra tất cả task có status là 3
                            {
                                // Cập nhật trạng thái của Order sang "đang thực hiện" nếu tất cả các Task đang thực hiện
                                var order = await _unitOfWork.OrderRepository.GetByIDAsync(task.OrderId);
                                if (order != null)
                                {
                                    order.Status = 3;  // Order chuyển sang trạng thái "đang thực hiện"
                                    await _unitOfWork.OrderRepository.UpdateAsync(order);
                                }
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException("You can only update status to 2 (reject) or 3 (in progress) from status 1.");
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Invalid status transition.");
                    }

                    // 3. Lưu thay đổi vào cơ sở dữ liệu
                    await _unitOfWork.TaskRepository.UpdateAsync(task);
                    await _unitOfWork.SaveAsync();

                    // 4. Commit transaction nếu không có lỗi
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





        public async Task<TaskDtoResponse> UpdateTaskImagesAsync(int taskId, TaskImageUpdateDTO imageUpdateDto)
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

                    // 2. Kiểm tra nếu task có thể cập nhật hình ảnh (task phải ở trạng thái "đang thực hiện")
                    if (task.Status != 3)
                    {
                        throw new InvalidOperationException("Task is not in a state that allows image updates.");
                    }

                    // 3. Cập nhật hình ảnh
                    task.ImagePath1 = imageUpdateDto.UrlImages.ElementAtOrDefault(0);  // Ảnh 1
                    task.ImagePath2 = imageUpdateDto.UrlImages.ElementAtOrDefault(1);  // Ảnh 2 (nếu có)
                    task.ImagePath3 = imageUpdateDto.UrlImages.ElementAtOrDefault(2);  // Ảnh 3 (nếu có)

                    // 4. Cập nhật trạng thái task lên 4
                    task.Status = 4;  // Task hoàn thành
                    await _unitOfWork.TaskRepository.UpdateAsync(task);

                    // 5. Kiểm tra nếu tất cả các task của Order này đều đã hoàn thành
                    var allTasksForOrder = await _unitOfWork.TaskRepository.GetAsync(t => t.OrderId == task.OrderId);
                    if (allTasksForOrder.All(t => t.Status == 4)) // Kiểm tra tất cả task có status là 4
                    {
                        var order = await _unitOfWork.OrderRepository.GetByIDAsync(task.OrderId);
                        if (order != null)
                        {
                            // Cập nhật trạng thái của Order sang 4 nếu tất cả các Task đã hoàn thành
                            order.Status = 4;  // Order hoàn thành
                            await _unitOfWork.OrderRepository.UpdateAsync(order);
                        }
                    }

                    // 6. Lưu thay đổi
                    await _unitOfWork.SaveAsync();

                    // 7. Commit transaction nếu không có lỗi
                    await transaction.CommitAsync();

                    return _mapper.Map<TaskDtoResponse>(task);
                }
                catch (Exception ex)
                {
                    // Rollback transaction nếu có lỗi
                    await transaction.RollbackAsync();
                    throw new Exception($"Failed to update task images: {ex.Message}");
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
                        throw new InvalidOperationException("Task can only be reassigned if it is in status 2 (rejected).");
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

                    // 5. Kiểm tra `OrderDetail` liên quan đến Task và nạp cả MartyrGrave
                    var orderDetail = await _unitOfWork.OrderDetailRepository.GetAsync(
                        od => od.DetailId == task.DetailId,
                        includeProperties: "MartyrGrave"
                    );
                    var detailEntity = orderDetail.FirstOrDefault();
                    if (detailEntity == null)
                    {
                        throw new InvalidOperationException("No order detail found for this task.");
                    }

                    // 6. Kiểm tra nếu nhân viên chỉ được làm việc trong khu vực của họ
                    if (detailEntity.MartyrGrave?.AreaId != newAccount.AreaId)
                    {
                        throw new UnauthorizedAccessException("Staff can only work in their assigned area.");
                    }

                    // 7. Cập nhật AccountId mới và Status của task
                    task.AccountId = newAccountId; // Bàn giao task cho Account mới
                    task.Status = 1; // Trạng thái về 1 (đã bàn giao)

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
        {//
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

                    // 3. Kiểm tra quan hệ với OrderDetail
                    var orderDetail = await _unitOfWork.OrderDetailRepository.GetByIDAsync(task.DetailId);
                    if (orderDetail == null)
                    {
                        throw new InvalidOperationException("The order detail associated with this task does not exist.");
                    }

                    // 4. Nếu task được xóa, có thể cập nhật OrderDetail (nếu cần)
                    //orderDetail.StaffTask = null; // Gỡ liên kết task khỏi OrderDetail

                    // 5. Xóa Task khỏi cơ sở dữ liệu
                    await _unitOfWork.TaskRepository.DeleteAsync(task);
                    await _unitOfWork.SaveAsync();

                    // 6. Lưu thay đổi vào OrderDetail (nếu có thay đổi)
                    await _unitOfWork.OrderDetailRepository.UpdateAsync(orderDetail);
                    await _unitOfWork.SaveAsync();

                    // Commit transaction
                    await transaction.CommitAsync();

                    return true; // Task đã xóa thành công
                }
                catch (Exception ex)
                {
                    // Rollback transaction nếu có lỗi
                    await transaction.RollbackAsync();
                    throw new Exception($"Failed to delete task: {ex.Message}");
                }
            }
        }


    }
}
