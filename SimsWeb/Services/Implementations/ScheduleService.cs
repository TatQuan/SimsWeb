using Microsoft.EntityFrameworkCore;
using SimsWeb.Data;
using SimsWeb.Helpers;
using SimsWeb.Models;
using SimsWeb.Services.Interfaces;
using SimsWeb.ViewModels;

namespace SimsWeb.Services.Implementations
{
    public class ScheduleService : IScheduleService
    {
        private readonly AppDbContext _context;

        public ScheduleService(AppDbContext context)
        {
            _context = context;
        }

        // =============== SCHEDULE CHO GIẢNG VIÊN ===============
        public async Task<List<ScheduleItemViewModel>> GetScheduleForTeacherAsync(string userId)
        {
            var query = _context.ClassSchedules
                .Where(s => !s.IsDeleted
                            && !s.ClassSection.IsDeleted
                            && !s.ClassSection.Teacher.IsDeleted
                            && s.ClassSection.Teacher.UserId == userId)
                .Include(s => s.ClassSection)
                    .ThenInclude(cs => cs.Course)
                .Include(s => s.ClassSection)
                    .ThenInclude(cs => cs.Teacher)
                        .ThenInclude(f => f.User);

            return await query
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .Select(s => new ScheduleItemViewModel
                {
                    ClassSectionId = s.ClassSectionId,
                    ClassCode = s.ClassSection.Code,
                    ClassName = s.ClassSection.Name,
                    CourseName = s.ClassSection.Course.Name,
                    TeacherName = s.ClassSection.Teacher.User.FullName,
                    DayOfWeek = s.DayOfWeek,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    Room = s.Room
                })
                .ToListAsync();
        }

        // =============== SCHEDULE CHO SINH VIÊN ===============
        public async Task<List<ScheduleItemViewModel>> GetScheduleForStudentAsync(string userId)
        {
            var query = _context.ClassSchedules
                .Where(s => !s.IsDeleted
                            && !s.ClassSection.IsDeleted
                            && s.ClassSection.Enrollments.Any(e =>
                                   !e.IsDeleted
                                   && !e.Student.IsDeleted
                                   && e.Student.UserId == userId))
                .Include(s => s.ClassSection)
                    .ThenInclude(cs => cs.Course)
                .Include(s => s.ClassSection)
                    .ThenInclude(cs => cs.Teacher)
                        .ThenInclude(f => f.User);

            return await query
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .Select(s => new ScheduleItemViewModel
                {
                    ClassSectionId = s.ClassSectionId,
                    ClassCode = s.ClassSection.Code,
                    ClassName = s.ClassSection.Name,
                    CourseName = s.ClassSection.Course.Name,
                    TeacherName = s.ClassSection.Teacher.User.FullName,
                    DayOfWeek = s.DayOfWeek,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    Room = s.Room
                })
                .ToListAsync();
        }

        // =============== CRUD + RECYCLE BIN ===============

        public async Task<List<ClassSchedule>> GetActiveSchedulesAsync()
        {
            return await _context.ClassSchedules
                .Where(s => !s.IsDeleted)
                .Include(s => s.ClassSection)
                    .ThenInclude(cs => cs.Course)
                .Include(s => s.ClassSection)
                    .ThenInclude(cs => cs.Teacher)
                        .ThenInclude(f => f.User)
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<List<ClassSchedule>> GetDeletedSchedulesAsync()
        {
            return await _context.ClassSchedules
                .Where(s => s.IsDeleted)
                .Include(s => s.ClassSection)
                    .ThenInclude(cs => cs.Course)
                .Include(s => s.ClassSection)
                    .ThenInclude(cs => cs.Teacher)
                        .ThenInclude(f => f.User)
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<ClassScheduleViewModel?> BuildCreateViewModelAsync()
        {
            var sections = await _context.ClassSections
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Code)
                .ToListAsync();

            return new ClassScheduleViewModel
            {
                ClassSections = sections
            };
        }

        public async Task<ClassScheduleViewModel?> BuildEditViewModelAsync(int id)
        {
            var schedule = await _context.ClassSchedules
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

            if (schedule == null) return null;

            var sections = await _context.ClassSections
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Code)
                .ToListAsync();

            var periodIndex = ScheduleTimeHelper.GetPeriodIndex(schedule.StartTime) ?? 1;

            return new ClassScheduleViewModel
            {
                Id = schedule.Id,
                ClassSectionId = schedule.ClassSectionId,
                DayOfWeek = schedule.DayOfWeek,
                Period = periodIndex,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime,
                Room = schedule.Room,
                ClassSections = sections
            };
        }

        public async Task<bool> CreateAsync(ClassScheduleViewModel model)
        {
            var entity = new ClassSchedule
            {
                ClassSectionId = model.ClassSectionId,
                DayOfWeek = model.DayOfWeek,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                Room = model.Room,
                IsDeleted = false
            };

            _context.ClassSchedules.Add(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateAsync(ClassScheduleViewModel model)
        {
            var schedule = await _context.ClassSchedules
                .FirstOrDefaultAsync(s => s.Id == model.Id && !s.IsDeleted);

            if (schedule == null) return false;

            schedule.ClassSectionId = model.ClassSectionId;
            schedule.DayOfWeek = model.DayOfWeek;
            schedule.StartTime = model.StartTime;
            schedule.EndTime = model.EndTime;
            schedule.Room = model.Room;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            var schedule = await _context.ClassSchedules
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

            if (schedule == null) return false;

            schedule.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreAsync(int id)
        {
            var schedule = await _context.ClassSchedules
                .FirstOrDefaultAsync(s => s.Id == id && s.IsDeleted);

            if (schedule == null) return false;

            schedule.IsDeleted = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HardDeleteAsync(int id)
        {
            var schedule = await _context.ClassSchedules
                .FirstOrDefaultAsync(s => s.Id == id);

            if (schedule == null) return false;

            _context.ClassSchedules.Remove(schedule);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
