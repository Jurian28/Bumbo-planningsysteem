﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using BumboDB.Models; // Ensure this using directive is present

namespace BumboDB.Models
{
    public class BumboContext : IdentityDbContext<IdentityUser>
    {
        public BumboContext()
        {
        }

        public BumboContext(DbContextOptions<BumboContext> options) : base(options)
        {
        }

        public virtual DbSet<Availability> Availabilities { get; set; }
        public virtual DbSet<AvailableShift> AvailableShifts { get; set; }
        public virtual DbSet<Chapter> Chapters { get; set; }
        public virtual DbSet<Department> Departments { get; set; }
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<EmployeeRole> EmployeeRoles { get; set; }
        public virtual DbSet<Experience> Experiences { get; set; }
        public virtual DbSet<History> Histories { get; set; }
        public virtual DbSet<Norm> Norms { get; set; }
        public virtual DbSet<OpeningHour> OpeningHours { get; set; }
        public virtual DbSet<PrognosesWeek> PrognosesWeeks { get; set; }
        public virtual DbSet<Prognosis> Prognoses { get; set; }
        public virtual DbSet<Shift> Shifts { get; set; }
        public virtual DbSet<Template> Templates { get; set; }
        public virtual DbSet<TimeOffRequest> TimeOffRequests { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https: //go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
            => optionsBuilder.UseSqlServer(
                "Data Source=Localhost;Initial Catalog=Bumbo;Integrated Security=True;Encrypt=True;Trust Server Certificate=True");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Availability>(entity =>
            {
                entity.HasKey(e => e.AvailabilityId);
                entity.ToTable("Availability");
                entity.Property(e => e.AvailabilityId)
                    .HasColumnName("Availability_ID")
                    .ValueGeneratedOnAdd(); // Zorgt ervoor dat de waarde automatisch wordt gegenereerd
                entity.Property(e => e.Day)
                    .HasColumnName("Day")
                    .IsRequired();
                entity.Property(e => e.IsAvailable)
                    .HasColumnName("Is_Available")
                    .IsRequired();
                entity.Property(e => e.Employee)
                    .HasColumnName("Employee")
                    .IsRequired();
                entity.Property(e => e.StartTime)
                    .HasColumnName("Start_Time");
                entity.Property(e => e.EndTime)
                    .HasColumnName("End_Time");
                entity.Property(e => e.HoursWorkedSchool)
                    .HasColumnName("Hours_Worked_School");
                entity.HasOne(e => e.EmployeeNavigation)
                    .WithMany(e => e.Availabilities)
                    .HasForeignKey(e => e.Employee)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            
            modelBuilder.Entity<OpeningHour>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("OpeningHour");
                entity.Property(e => e.Id).HasColumnName("OpeningHour_ID");
                entity.Property(e => e.Day).HasColumnName("Day").IsRequired();
                entity.Property(e => e.OpenTime).HasColumnName("Open_Time").IsRequired();
                entity.Property(e => e.CloseTime).HasColumnName("Close_Time").IsRequired();
            });

            modelBuilder.Entity<Chapter>(entity =>
            {
                entity.HasKey(e => e.ChapterId);
                entity.ToTable("Chapter");
                entity.Property(e => e.ChapterId).HasColumnName("Chapter_ID");
                entity.Property(e => e.Name).HasColumnName("Name");
                entity.HasOne(d => d.ManagerNavigation).WithMany(p => p.Chapters)
                    .HasForeignKey(d => d.Manager)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(e => e.DepartmentId);
                entity.ToTable("Department");
                entity.Property(e => e.DepartmentId).HasColumnName("Department_ID");
                entity.Property(e => e.Name).HasMaxLength(45);
                entity.HasOne(d => d.ChapterNavigation).WithMany(p => p.Departments)
                    .HasForeignKey(d => d.Chapter)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Employee>(entity =>
            {
                var dateOnlyConverter = new ValueConverter<DateOnly, string>(
                    d => d.ToString("yyyy-MM-dd"),
                    d => DateOnly.Parse(d));

                entity.Property(e => e.EmployedSince)
                    .HasConversion(dateOnlyConverter)
                    .HasColumnType("varchar(10)")
                    .IsRequired()
                    .HasDefaultValue(DateOnly.Parse("1900-01-01"));

                entity.Property(e => e.HouseNumber).HasMaxLength(45).HasColumnName("House_Number");
                entity.Property(e => e.Name).HasMaxLength(45);
                entity.Property(e => e.PayScale);
                entity.Property(e => e.PhoneNumber).HasMaxLength(45);
                entity.Property(e => e.Zipcode).HasMaxLength(45);
                entity.Property(e => e.IsAvailable).HasColumnName("Is_Available").IsRequired(); // Add this line
                entity.Property(e => e.Status).HasColumnName("Status");

                entity.Property(e => e.FiredDate)
                    .HasConversion(dateOnlyConverter)
                    .HasColumnType("varchar(10)");
            });

            modelBuilder.Entity<EmployeeRole>(entity =>
            {
                entity.HasKey(e => new { e.Employee, e.Department });
                entity.ToTable("Employee_Role");
                entity.HasOne(d => d.DepartmentNavigation).WithMany(p => p.EmployeeRoles)
                    .HasForeignKey(d => d.Department)
                    .OnDelete(DeleteBehavior.ClientSetNull);
                entity.HasOne(d => d.EmployeeNavigation).WithMany(p => p.EmployeeRoles)
                    .HasForeignKey(d => d.Employee)
                    .OnDelete(DeleteBehavior.ClientSetNull);
                entity.HasOne(d => d.ExperienceNavigation).WithMany(p => p.EmployeeRoles)
                    .HasForeignKey(d => d.Experience)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Experience>(entity =>
            {
                entity.HasKey(e => e.ExperienceId);
                entity.ToTable("Experience");
                entity.Property(e => e.ExperienceId).HasColumnName("Experience_ID");
                entity.Property(e => e.Name).HasMaxLength(45);
            });

            modelBuilder.Entity<History>(entity =>
            {
                entity.HasKey(e => new { e.Chapter, e.Day });
                entity.ToTable("History");
                entity.Property(e => e.CargoCrates).HasColumnName("Cargo_Crates");
                entity.Property(e => e.Holiday).HasMaxLength(45);
                entity.HasOne(d => d.ChapterNavigation).WithMany(p => p.Histories)
                    .HasForeignKey(d => d.Chapter)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Norm>(entity =>
            {
                entity.HasKey(e => e.NormId);
                entity.ToTable("Norm");
                entity.Property(e => e.NormId).HasColumnName("Norm_ID");
                entity.Property(e => e.Date).HasColumnType("date");
                entity.Property(e => e.FreshCustomersPerHour).HasColumnName("Fresh_Customers_Per_Hour");
                entity.Property(e => e.ShelfFillingSeconds).HasColumnName("Shelf_Filling_Seconds");
                entity.Property(e => e.FacingSecondsPerMeter).HasColumnName("Facing_Seconds_Per_Meter");
                entity.Property(e => e.CashierCustomersPerHour).HasColumnName("Cashier_Customers_Per_Hour");
                entity.Property(e => e.UnloadingSeconds).HasColumnName("Unloading_Seconds");
            });

            modelBuilder.Entity<Prognosis>()
                .HasKey(p => new { p.Department, p.Day });

            modelBuilder.Entity<Template>(entity =>
            {
                entity.HasKey(e => new { e.TemplateId, e.ChapterId }); 
                entity.ToTable("Template");
                entity.Property(e => e.TemplateId).HasColumnName("Template_ID");
                entity.Property(e => e.Name).HasMaxLength(45);
                entity.Property(e => e.PredictedCargo).HasColumnName("Predicted_Cargo");
                entity.Property(e => e.PredictedCustomers).HasColumnName("Predicted_Customers");
                entity.HasOne(e => e.Chapter)
                      .WithMany(c => c.Templates)
                      .HasForeignKey(e => e.ChapterId);
            });

            modelBuilder.Entity<Shift>(entity =>
            {
                entity.HasKey(e => e.ShiftId);
                entity.ToTable("Shift");
                entity.Property(e => e.ShiftId).HasColumnName("Shift_ID");
                entity.Property(e => e.StartTime).HasColumnName("Start_Time");
                entity.Property(e => e.EndTime).HasColumnName("End_Time");
                entity.Property(e => e.EmployeeId).HasColumnName("Employee_ID");
                entity.Property(e => e.ChapterId).HasColumnName("Chapter_ID");
                entity.Property(e => e.DepartmentId).HasColumnName("Department_ID");
                entity.Property(e => e.IsPublished).HasColumnName("IsPublished");
                entity.HasOne(d => d.Employee).WithMany(p => p.Shifts).HasForeignKey(d => d.EmployeeId);
            });

            modelBuilder.Entity<TimeOffRequest>(entity =>
            {
                entity.HasKey(e => e.TimeOffRequestId);
                entity.ToTable("TimeOffRequest");
                entity.Property(e => e.TimeOffRequestId).HasColumnName("TimeOffRequest_ID");
                entity.Property(e => e.StartDate).HasColumnName("Start_Date");
                entity.Property(e => e.EndDate).HasColumnName("End_Date");
                entity.Property(e => e.Reason).HasMaxLength(255);
                entity.Property(e => e.IsApproved).HasColumnName("Is_Approved");
            });

            modelBuilder.Entity<AvailableShift>(entity =>
            {
                entity.HasKey(e => e.AvailableShiftId);
                entity.ToTable("AvailableShift");
                entity.Property(e => e.AvailableShiftId).HasColumnName("AvailableShift_ID");
                entity.Property(e => e.StartTime).HasColumnName("Start_Time");
                entity.Property(e => e.EndTime).HasColumnName("End_Time");
                entity.Property(e => e.IsFilled).HasColumnName("Is_Filled");
            });

            modelBuilder.Entity<PrognosesWeek>(entity =>
            {
                entity.HasKey(e => new { e.Chapter, e.Week, e.Year });
                entity.ToTable("PrognosesWeek");
                entity.Property(e => e.BeginDate).IsRequired();
                entity.Property(e => e.EndDate).IsRequired();
            });

            // Seed data for users
            var employeeUser = new Employee
            {
                Id = "2",
                UserName = "Koray Yilmaz",
                NormalizedUserName = "EMPLOYEE@EXAMPLE.COM",
                Email = "employee@example.com",
                NormalizedEmail = "EMPLOYEE@EXAMPLE.COM",
                Birthday = DateOnly.Parse("2009-01-01"),
                EmailConfirmed = true,
                Name = "Koray Yilmaz",
                EmployedSince = DateOnly.MinValue,
                HouseNumber = "2",
                PayScale = 5,
                PhoneNumber = "123-456-7891",
                Zipcode = "12346",
                SecurityStamp = Guid.NewGuid().ToString(),
                Chapter = 1 
            };

            var managerUser = new Employee
            {
                Id = "3",
                UserName = "Stefan",
                NormalizedUserName = "MANAGER@EXAMPLE.COM",
                Email = "manager@example.com",
                NormalizedEmail = "MANAGER@EXAMPLE.COM",
                EmailConfirmed = true,
                Name = "Stefan",
                EmployedSince = DateOnly.MinValue,
                HouseNumber = "3",
                PayScale = 8,
                PhoneNumber = "123-456-7892",
                Zipcode = "12347",
                SecurityStamp = Guid.NewGuid().ToString(),
                Chapter = 1 
            };

            var newEmployeeUser = new Employee
            {
                Id = "4",
                UserName = "Jonne Vliem",
                NormalizedUserName = "NEWEMPLOYEE@EXAMPLE.COM",
                Email = "newemployee@example.com",
                NormalizedEmail = "NEWEMPLOYEE@EXAMPLE.COM",
                Birthday = DateOnly.Parse("1999-01-01"),
                EmailConfirmed = true,
                Name = "Jonne Vliem",
                EmployedSince = DateOnly.MinValue,
                HouseNumber = "4",
                PayScale = 5,
                PhoneNumber = "123-456-7893",
                Zipcode = "12348",
                SecurityStamp = Guid.NewGuid().ToString(),
                Chapter = 1 
            };

            var newManagerUser = new Employee
            {
                Id = "5",
                UserName = "Caron",
                NormalizedUserName = "NEWMANAGER@EXAMPLE.COM",
                Email = "newmanager@example.com",
                NormalizedEmail = "NEWMANAGER@EXAMPLE.COM",
                EmailConfirmed = true,
                Name = "Caron",
                EmployedSince = DateOnly.MinValue,
                HouseNumber = "5",
                PayScale = 8,
                PhoneNumber = "123-456-7894",
                Zipcode = "12349",
                SecurityStamp = Guid.NewGuid().ToString(),
                Chapter = 2 
            };

            var plasemployeeUser = new Employee
            {
                Id = "6",
                UserName = "Noud Lange",
                NormalizedUserName = "EMPLOYEEPLAS@EXAMPLE.COM",
                Email = "employeeplas@example.com",
                NormalizedEmail = "EMPLOYEEPLAS@EXAMPLE.COM",
                Birthday = DateOnly.Parse("2008-01-01"),
                EmailConfirmed = true,
                Name = "Noud Lange",
                EmployedSince = DateOnly.MinValue,
                HouseNumber = "2",
                PayScale = 5,
                PhoneNumber = "123-456-7891",
                Zipcode = "12346",
                SecurityStamp = Guid.NewGuid().ToString(),
                Chapter = 2 
            };

            var hasher = new PasswordHasher<Employee>();
            employeeUser.PasswordHash = hasher.HashPassword(employeeUser, "a");
            managerUser.PasswordHash = hasher.HashPassword(managerUser, "a");
                newEmployeeUser.PasswordHash = hasher.HashPassword(newEmployeeUser, "a");
            newManagerUser.PasswordHash = hasher.HashPassword(newManagerUser, "a");
            plasemployeeUser.PasswordHash = hasher.HashPassword(plasemployeeUser, "a");

            // Seed data for roles
            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = "2", Name = "Employee", NormalizedName = "EMPLOYEE" },
                new IdentityRole { Id = "3", Name = "Manager", NormalizedName = "MANAGER" }
            );

            modelBuilder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string> { UserId = "2", RoleId = "2" },
                new IdentityUserRole<string> { UserId = "3", RoleId = "3" },
                new IdentityUserRole<string> { UserId = "4", RoleId = "2" },
                new IdentityUserRole<string> { UserId = "5", RoleId = "3" },
                new IdentityUserRole<string> { UserId = "6", RoleId = "2" } 
            );

            // Add the new users to the model
            modelBuilder.Entity<Employee>().HasData(employeeUser, managerUser, newEmployeeUser, newManagerUser, plasemployeeUser);

            modelBuilder.Entity<Chapter>().HasData(
                new Chapter { ChapterId = 1, Manager = "3", Name = "Waalwijk", Meters = 400},
                new Chapter { ChapterId = 2, Manager = "5", Name = "Kaatsheuvel", Meters =  600}
            );

            modelBuilder.Entity<Department>().HasData(
                new Department { DepartmentId = 1, Name = "Vers", Description = "Vers", Chapter = 1 },
                new Department { DepartmentId = 2, Name = "Vulploeg", Description = "Vulploeg", Chapter = 1 },
                new Department { DepartmentId = 3, Name = "Kassa", Description = "Kassa", Chapter = 1 }
            );

            modelBuilder.Entity<Norm>().HasData(
                new Norm
                {
                    NormId = 1,
                    Date = DateTime.Parse("2023-01-01"),
                    FreshCustomersPerHour = 100,
                    ShelfFillingSeconds = 30,
                    FacingSecondsPerMeter = 15,
                    CashierCustomersPerHour = 10,
                    UnloadingSeconds = 5,
                }
            );

            modelBuilder.Entity<Template>().HasData(
                 // Templates for Chapter 1
                 new Template { TemplateId = 1, Name = "Gem. Ma", PredictedCargo = 100, PredictedCustomers = 200, ChapterId = 1 },
                 new Template { TemplateId = 2, Name = "Gem. Di", PredictedCargo = 150, PredictedCustomers = 300, ChapterId = 1 },
                 new Template { TemplateId = 3, Name = "Gem. Wo", PredictedCargo = 200, PredictedCustomers = 400, ChapterId = 1 },
                 new Template { TemplateId = 4, Name = "Gem. Do", PredictedCargo = 250, PredictedCustomers = 500, ChapterId = 1 },
                 new Template { TemplateId = 5, Name = "Gem. Vr", PredictedCargo = 300, PredictedCustomers = 600, ChapterId = 1 },
                 new Template { TemplateId = 6, Name = "Gem. Za", PredictedCargo = 350, PredictedCustomers = 700, ChapterId = 1 },
                 new Template { TemplateId = 7, Name = "Gem. Zo", PredictedCargo = 400, PredictedCustomers = 800, ChapterId = 1 },
                 new Template { TemplateId = 8, Name = "Sample Template", PredictedCargo = 10000000, PredictedCustomers = 200000000, ChapterId = 1 },

                 // Templates for Chapter 2
                 new Template { TemplateId = 9, Name = "Gem. Ma", PredictedCargo = 100, PredictedCustomers = 200, ChapterId = 2 },
                 new Template { TemplateId = 10, Name = "Gem. Di", PredictedCargo = 150, PredictedCustomers = 300, ChapterId = 2 },
                 new Template { TemplateId = 11, Name = "Gem. Wo", PredictedCargo = 200, PredictedCustomers = 400, ChapterId = 2 },
                 new Template { TemplateId = 12, Name = "Gem. Do", PredictedCargo = 250, PredictedCustomers = 500, ChapterId = 2 },
                 new Template { TemplateId = 13, Name = "Gem. Vr", PredictedCargo = 300, PredictedCustomers = 600, ChapterId = 2 },
                 new Template { TemplateId = 14, Name = "Gem. Za", PredictedCargo = 350, PredictedCustomers = 700, ChapterId = 2 },
                 new Template { TemplateId = 15, Name = "Gem. Zo", PredictedCargo = 400, PredictedCustomers = 800, ChapterId = 2 },
                 new Template { TemplateId = 16, Name = "Sample Template", PredictedCargo = 10000000, PredictedCustomers = 200000000, ChapterId = 2 }
             );
            
            modelBuilder.Entity<Shift>().HasData(
                new Shift
                {
                    ShiftId = 1,
                    StartTime = TimeOnly.Parse("08:00:00"),
                    EndTime = TimeOnly.Parse("16:00:00"),
                    Date = DateOnly.Parse("2024-11-26"),
                    EmployeeId = "2",
                    ChapterId = 1,
                    DepartmentId = 1,
                    IsPublished = true
                },
                 new Shift
                 {
                     ShiftId = 2,
                     StartTime = TimeOnly.Parse("08:00:00"),
                     EndTime = TimeOnly.Parse("16:00:00"),
                     Date = DateOnly.Parse("2024-11-27"),
                     EmployeeId = "2",
                     ChapterId = 1,
                     DepartmentId = 2,
                     IsPublished = true
                 },
                   new Shift
                   {
                       ShiftId = 3,
                       StartTime = TimeOnly.Parse("08:00:00"),
                       EndTime = TimeOnly.Parse("16:00:00"),
                       Date = DateOnly.Parse("2024-11-27"),
                       EmployeeId = "3",
                       ChapterId = 1,
                       DepartmentId = 1,
                       IsPublished = true
                   }

            );
            modelBuilder.Entity<Availability>().HasData(
                //25
                new Availability { AvailabilityId = 1, Day = DateOnly.Parse("2024-11-25"), Employee = "2", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 2, Day = DateOnly.Parse("2024-11-25"), Employee = "3", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 3, Day = DateOnly.Parse("2024-11-25"), Employee = "4", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 4, Day = DateOnly.Parse("2024-11-25"), Employee = "5", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 5, Day = DateOnly.Parse("2024-11-25"), Employee = "6", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                //26
                new Availability { AvailabilityId = 6, Day = DateOnly.Parse("2024-11-26"), Employee = "2", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 7, Day = DateOnly.Parse("2024-11-26"), Employee = "3", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 8, Day = DateOnly.Parse("2024-11-26"), Employee = "4", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 9, Day = DateOnly.Parse("2024-11-26"), Employee = "5", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 10, Day = DateOnly.Parse("2024-11-26"), Employee = "6", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                //27
                new Availability { AvailabilityId = 11, Day = DateOnly.Parse("2024-11-27"), Employee = "2", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 12, Day = DateOnly.Parse("2024-11-27"), Employee = "3", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 13, Day = DateOnly.Parse("2024-11-27"), Employee = "4", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 14, Day = DateOnly.Parse("2024-11-27"), Employee = "5", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 15, Day = DateOnly.Parse("2024-11-27"), Employee = "6", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                //28
                new Availability { AvailabilityId = 16, Day = DateOnly.Parse("2024-11-28"), Employee = "2", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 17, Day = DateOnly.Parse("2024-11-28"), Employee = "3", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 18, Day = DateOnly.Parse("2024-11-28"), Employee = "4", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 19, Day = DateOnly.Parse("2024-11-28"), Employee = "5", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 20, Day = DateOnly.Parse("2024-11-28"), Employee = "6", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                //29
                new Availability { AvailabilityId = 21, Day = DateOnly.Parse("2024-11-29"), Employee = "2", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 22, Day = DateOnly.Parse("2024-11-29"), Employee = "3", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 23, Day = DateOnly.Parse("2024-11-29"), Employee = "4", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 24, Day = DateOnly.Parse("2024-11-29"), Employee = "5", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 25, Day = DateOnly.Parse("2024-11-29"), Employee = "6", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                //30
                new Availability { AvailabilityId = 26, Day = DateOnly.Parse("2024-11-30"), Employee = "2", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 27, Day = DateOnly.Parse("2024-11-30"), Employee = "3", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 28, Day = DateOnly.Parse("2024-11-30"), Employee = "4", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 29, Day = DateOnly.Parse("2024-11-30"), Employee = "5", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 30, Day = DateOnly.Parse("2024-11-30"), Employee = "6", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                //1 december
                new Availability { AvailabilityId = 31, Day = DateOnly.Parse("2024-12-01"), Employee = "2", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 32, Day = DateOnly.Parse("2024-12-01"), Employee = "3", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 33, Day = DateOnly.Parse("2024-12-01"), Employee = "4", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 34, Day = DateOnly.Parse("2024-12-01"), Employee = "5", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 35, Day = DateOnly.Parse("2024-12-01"), Employee = "6", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                //2 december
                new Availability { AvailabilityId = 36, Day = DateOnly.Parse("2024-12-02"), Employee = "2", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 37, Day = DateOnly.Parse("2024-12-02"), Employee = "3", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 38, Day = DateOnly.Parse("2024-12-02"), Employee = "4", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 39, Day = DateOnly.Parse("2024-12-02"), Employee = "5", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 40, Day = DateOnly.Parse("2024-12-02"), Employee = "6", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                //3 december
                new Availability { AvailabilityId = 41, Day = DateOnly.Parse("2024-12-03"), Employee = "2", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 42, Day = DateOnly.Parse("2024-12-03"), Employee = "3", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 43, Day = DateOnly.Parse("2024-12-03"), Employee = "4", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 44, Day = DateOnly.Parse("2024-12-03"), Employee = "5", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 45, Day = DateOnly.Parse("2024-12-03"), Employee = "6", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                //4
                new Availability { AvailabilityId = 46, Day = DateOnly.Parse("2024-12-04"), Employee = "2", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 47, Day = DateOnly.Parse("2024-12-04"), Employee = "3", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 48, Day = DateOnly.Parse("2024-12-04"), Employee = "4", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 49, Day = DateOnly.Parse("2024-12-04"), Employee = "5", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 50, Day = DateOnly.Parse("2024-12-04"), Employee = "6", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                //5
                new Availability { AvailabilityId = 51, Day = DateOnly.Parse("2024-12-05"), Employee = "2", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 52, Day = DateOnly.Parse("2024-12-05"), Employee = "3", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 53, Day = DateOnly.Parse("2024-12-05"), Employee = "4", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 54, Day = DateOnly.Parse("2024-12-05"), Employee = "5", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 55, Day = DateOnly.Parse("2024-12-05"), Employee = "6", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                //6 december
                new Availability { AvailabilityId = 56, Day = DateOnly.Parse("2024-12-06"), Employee = "2", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 57, Day = DateOnly.Parse("2024-12-06"), Employee = "3", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 58, Day = DateOnly.Parse("2024-12-06"), Employee = "4", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 59, Day = DateOnly.Parse("2024-12-06"), Employee = "5", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 60, Day = DateOnly.Parse("2024-12-06"), Employee = "6", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                //7 december
                new Availability { AvailabilityId = 61, Day = DateOnly.Parse("2024-12-07"), Employee = "2", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 62, Day = DateOnly.Parse("2024-12-07"), Employee = "3", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 63, Day = DateOnly.Parse("2024-12-07"), Employee = "4", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 64, Day = DateOnly.Parse("2024-12-07"), Employee = "5", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 65, Day = DateOnly.Parse("2024-12-07"), Employee = "6", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                //8 december
                new Availability { AvailabilityId = 66, Day = DateOnly.Parse("2024-12-08"), Employee = "2", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 67, Day = DateOnly.Parse("2024-12-08"), Employee = "3", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 68, Day = DateOnly.Parse("2024-12-08"), Employee = "4", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 69, Day = DateOnly.Parse("2024-12-08"), Employee = "5", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 70, Day = DateOnly.Parse("2024-12-08"), Employee = "6", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                //9 december
                new Availability { AvailabilityId = 71, Day = DateOnly.Parse("2024-12-09"), Employee = "2", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 72, Day = DateOnly.Parse("2024-12-09"), Employee = "3", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 73, Day = DateOnly.Parse("2024-12-09"), Employee = "4", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 74, Day = DateOnly.Parse("2024-12-09"), Employee = "5", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 75, Day = DateOnly.Parse("2024-12-09"), Employee = "6", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                //10 december
                new Availability { AvailabilityId = 76, Day = DateOnly.Parse("2024-12-10"), Employee = "2", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 77, Day = DateOnly.Parse("2024-12-10"), Employee = "3", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 78, Day = DateOnly.Parse("2024-12-10"), Employee = "4", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 79, Day = DateOnly.Parse("2024-12-10"), Employee = "5", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 80, Day = DateOnly.Parse("2024-12-10"), Employee = "6", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                //11 t/m 15 december (zelfde patroon)
                new Availability { AvailabilityId = 81, Day = DateOnly.Parse("2024-12-11"), Employee = "2", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 82, Day = DateOnly.Parse("2024-12-11"), Employee = "3", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 83, Day = DateOnly.Parse("2024-12-11"), Employee = "4", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 84, Day = DateOnly.Parse("2024-12-11"), Employee = "5", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 85, Day = DateOnly.Parse("2024-12-11"), Employee = "6", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                //12 december
                new Availability { AvailabilityId = 86, Day = DateOnly.Parse("2024-12-12"), Employee = "2", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 87, Day = DateOnly.Parse("2024-12-12"), Employee = "3", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 88, Day = DateOnly.Parse("2024-12-12"), Employee = "4", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 89, Day = DateOnly.Parse("2024-12-12"), Employee = "5", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 90, Day = DateOnly.Parse("2024-12-12"), Employee = "6", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                //13 december
                new Availability { AvailabilityId = 91, Day = DateOnly.Parse("2024-12-13"), Employee = "2", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 92, Day = DateOnly.Parse("2024-12-13"), Employee = "3", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 93, Day = DateOnly.Parse("2024-12-13"), Employee = "4", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 94, Day = DateOnly.Parse("2024-12-13"), Employee = "5", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 95, Day = DateOnly.Parse("2024-12-13"), Employee = "6", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                //14 december
                new Availability { AvailabilityId = 96, Day = DateOnly.Parse("2024-12-14"), Employee = "2", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 97, Day = DateOnly.Parse("2024-12-14"), Employee = "3", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 98, Day = DateOnly.Parse("2024-12-14"), Employee = "4", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 99, Day = DateOnly.Parse("2024-12-14"), Employee = "5", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 100, Day = DateOnly.Parse("2024-12-14"), Employee = "6", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                //15 december
                new Availability { AvailabilityId = 101, Day = DateOnly.Parse("2024-12-15"), Employee = "2", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 102, Day = DateOnly.Parse("2024-12-15"), Employee = "3", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 103, Day = DateOnly.Parse("2024-12-15"), Employee = "4", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 104, Day = DateOnly.Parse("2024-12-15"), Employee = "5", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 },
                new Availability { AvailabilityId = 105, Day = DateOnly.Parse("2024-12-15"), Employee = "6", IsAvailable = true, StartTime = TimeOnly.Parse("00:00:00"), EndTime = TimeOnly.Parse("23:59:59"), HoursWorkedSchool = 0 }
            );
        }
    }
}