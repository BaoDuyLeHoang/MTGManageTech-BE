﻿using MartyrGraveManagement_DAL.Entities;
using MartyrGraveManagement_DAL.Repositories.Implements;
using MartyrGraveManagement_DAL.Repositories.Interfaces;
using MartyrGraveManagement_DAL.UnitOfWorks.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MartyrGraveManagement_DAL.UnitOfWorks.Implements
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _context;
        private GenericRepository<Account> _accountRepository;
        private GenericRepository<Role> _roleRepository;
        private GenericRepository<Area> _areaRepository;
        private GenericRepository<MartyrGrave> _martyrGraveRepository;
        private GenericRepository<GraveImage> _graveImageRepository;
        private GenericRepository<MartyrGraveInformation> _martyrGraveInformationRepository;
        private GenericRepository<ServiceCategory> _serviceCategoryRepository;
        private GenericRepository<Service> _serviceRepository;
        private GenericRepository<CartItem> _cartItemRepository;
        private GenericRepository<StaffTask> _taskRepository;
        private GenericRepository<StaffJob> _jobRepository;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IGenericRepository<Account> AccountRepository => _accountRepository ??= new GenericRepository<Account>(_context);
        public IGenericRepository<Role> RoleRepository => _roleRepository ??= new GenericRepository<Role>(_context);
        public IGenericRepository<Area> AreaRepository => _areaRepository ??= new GenericRepository<Area>(_context);
        public IGenericRepository<MartyrGrave> MartyrGraveRepository => _martyrGraveRepository ??= new GenericRepository<MartyrGrave>(_context);
        public IGenericRepository<GraveImage> GraveImageRepository => _graveImageRepository ??= new GenericRepository<GraveImage>(_context);
        public IGenericRepository<MartyrGraveInformation> MartyrGraveInformationRepository => _martyrGraveInformationRepository ??= new GenericRepository<MartyrGraveInformation>(_context);
        public IGenericRepository<ServiceCategory> ServiceCategoryRepository => _serviceCategoryRepository ??= new GenericRepository<ServiceCategory>(_context);
        public IGenericRepository<Service> ServiceRepository => _serviceRepository ??= new GenericRepository<Service>(_context);

        public IGenericRepository<CartItem> CartItemRepository => _cartItemRepository ??= new GenericRepository<CartItem>(_context);
        public IGenericRepository<StaffTask> TaskRepository => _taskRepository ??= new GenericRepository<StaffTask>(_context);
        public IGenericRepository<StaffJob> JobRepository => _jobRepository ??= new GenericRepository<StaffJob>(_context);

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
