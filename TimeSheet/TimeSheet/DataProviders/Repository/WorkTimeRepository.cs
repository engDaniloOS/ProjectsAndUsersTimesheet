using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeSheet.Models;

namespace TimeSheet.DataProviders.Repository
{
    public class WorkTimeRepository : IWorkTimeRepository
    {
        private readonly Context _context;

        public WorkTimeRepository(Context context) => _context = context;

        public async Task<WorkTime> Save(WorkTime workTime)
        {
            await _context.WorkTime.AddAsync(workTime);
            await _context.SaveChangesAsync();

            return await _context.WorkTime.Where(wt => wt.Id == workTime.Id)
                                          .Include(wt => wt.Project)
                                          .Include(wt => wt.User)
                                          .ThenInclude(u => u.Credential)
                                          .FirstOrDefaultAsync();
        }

        public async Task<List<WorkTime>> GetWorkTimesByProjectId(int projectId)
            => await _context.WorkTime.Where(wt => wt.ProjectId == projectId)
                                      .Include(wt => wt.Project)
                                      .Include(wt => wt.User)
                                      .ThenInclude(u => u.Credential)
                                      .ToListAsync();

        public async Task<WorkTime> EditWorkTime(long workTimeId, WorkTime workTime)
        {
            using var transaction = _context.Database.BeginTransaction();

            var workTimeToEdit = await _context.WorkTime.Where(wt => wt.Id == workTimeId)
                                                        .Include(wt => wt.Project)
                                                        .Include(wt => wt.User)
                                                        .FirstOrDefaultAsync();

            if (workTimeToEdit == null || workTimeToEdit.Id == 0)
                return new WorkTime();

            if (workTimeToEdit.ProjectId != workTime.ProjectId)
            {
                workTimeToEdit.ProjectId = workTime.ProjectId;
                workTimeToEdit.Project = await _context.Project.Where(p => p.Id == workTime.ProjectId).FirstOrDefaultAsync();
            }

            if (workTimeToEdit.UserId != workTime.UserId)
            {
                workTimeToEdit.UserId = workTime.UserId;
                workTimeToEdit.User = await _context.User.Where(u => u.Id == workTime.UserId)
                                                         .Include(u => u.Credential)
                                                         .FirstOrDefaultAsync();
            }

            workTimeToEdit.Start = workTime.Start;
            workTimeToEdit.End = workTime.End;

            _context.Entry(workTimeToEdit).State = EntityState.Modified;
            _context.Update(workTimeToEdit);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return await _context.WorkTime.Where(wt => wt.Id == workTimeId)
                                           .Include(wt => wt.Project)
                                           .Include(wt => wt.User)
                                           .ThenInclude(u => u.Credential)
                                           .FirstOrDefaultAsync();
        }
    }
}
