using System;
using System.Collections.Generic;
using System.Linq;
using Timesheet.Core.Models;

namespace Timesheet.Core.Services
{
    public class TimesheetService
    {
        private readonly List<TimesheetEntry> _entries = new();

        // Add a new entry (throws exception on duplicate)
        public void Add(TimesheetEntry entry)
        {
            if (_entries.Any(e =>
                e.UserId == entry.UserId &&
                e.ProjectId == entry.ProjectId &&
                e.Date.Date == entry.Date.Date))
            {
                throw new InvalidOperationException("Duplicate entry for this user, project, and date.");
            }

            _entries.Add(entry);
        }

        // Update an existing entry
        public void Update(TimesheetEntry entry)
        {
            var existing = _entries.FirstOrDefault(e =>
                e.UserId == entry.UserId &&
                e.ProjectId == entry.ProjectId &&
                e.Date.Date == entry.Date.Date);

            if (existing == null)
                throw new InvalidOperationException("Entry not found.");

            existing.HoursWorked = entry.HoursWorked;
            existing.Description = entry.Description;
        }

        // Delete an entry
        public void Delete(int userId, int projectId, DateTime date)
        {
            var entry = _entries.FirstOrDefault(e =>
                e.UserId == userId &&
                e.ProjectId == projectId &&
                e.Date.Date == date.Date);

            if (entry != null)
                _entries.Remove(entry);
        }

        // Get entries for a user (optionally filtered by week)
        public IEnumerable<TimesheetEntry> GetEntries(int userId, DateTime? weekStart = null)
        {
            if (weekStart == null)
            {
                return _entries
                    .Where(e => e.UserId == userId)
                    .OrderByDescending(e => e.Date);
            }

            var weekEnd = weekStart.Value.AddDays(7);
            return _entries
                .Where(e => e.UserId == userId && e.Date >= weekStart && e.Date < weekEnd)
                .OrderByDescending(e => e.Date);
        }
    }
}
