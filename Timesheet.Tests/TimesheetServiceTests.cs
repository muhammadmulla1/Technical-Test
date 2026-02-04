using System;
using System.Collections.Generic;
using System.Linq;
using Timesheet.Core.Models;
using Timesheet.Core.Services;
using Xunit;


namespace Timesheet.Tests
{
    public class TimesheetServiceTests
    {
        private static DateTime WeekStart => DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);

        [Fact]
        public void Can_Add_And_Get_Entry()
        {
            var service = new TimesheetService();
            var entry = new TimesheetEntry
            {
                UserId = 1,
                ProjectId = 101,
                Date = DateTime.Today,
                HoursWorked = 5
            };

            service.Add(entry);

            var entries = service.GetEntries(1, WeekStart);
            Assert.Single(entries);
            Assert.Equal(5, entries.First().HoursWorked);
            Assert.Equal(101, entries.First().ProjectId);
        }

        [Fact]
        public void Cannot_Add_Duplicate_Entry()
        {
            var service = new TimesheetService();
            var entry1 = new TimesheetEntry
            {
                UserId = 1,
                ProjectId = 101,
                Date = DateTime.Today,
                HoursWorked = 5
            };
            var entry2 = new TimesheetEntry
            {
                UserId = 1,
                ProjectId = 101,
                Date = DateTime.Today,
                HoursWorked = 6
            };

            service.Add(entry1);
            var ex = Assert.Throws<InvalidOperationException>(() => service.Add(entry2));
            Assert.Equal("Duplicate entry for this user, project, and date.", ex.Message);
        }

        [Fact]
        public void Can_Update_Entry()
        {
            var service = new TimesheetService();
            var entry = new TimesheetEntry
            {
                UserId = 1,
                ProjectId = 101,
                Date = DateTime.Today,
                HoursWorked = 5
            };
            service.Add(entry);

            var updated = new TimesheetEntry
            {
                UserId = 1,
                ProjectId = 101,
                Date = DateTime.Today,
                HoursWorked = 7,
                Description = "Updated work"
            };

            service.Update(updated);

            var result = service.GetEntries(1, WeekStart).First();
            Assert.Equal(7, result.HoursWorked);
            Assert.Equal("Updated work", result.Description);
        }

        [Fact]
        public void Can_Delete_Entry()
        {
            var service = new TimesheetService();
            var entry = new TimesheetEntry
            {
                UserId = 1,
                ProjectId = 101,
                Date = DateTime.Today,
                HoursWorked = 5
            };
            service.Add(entry);

            service.Delete(entry.UserId, entry.ProjectId, entry.Date);

            var entries = service.GetEntries(1, WeekStart);
            Assert.Empty(entries);
        }

        [Fact]
        public void Can_Get_Total_Hours_By_Project()
        {
            var service = new TimesheetService();
            var entries = new List<TimesheetEntry>
            {
                new() { UserId = 1, ProjectId = 101, Date = DateTime.Today, HoursWorked = 5 },
                new() { UserId = 1, ProjectId = 102, Date = DateTime.Today, HoursWorked = 3 },
                new() { UserId = 1, ProjectId = 101, Date = DateTime.Today.AddDays(-1), HoursWorked = 2 },
            };

            foreach (var e in entries)
                service.Add(e);

            var weekEntries = service.GetEntries(1, WeekStart).ToList();
            var totalsByProject = weekEntries
                .GroupBy(e => e.ProjectId)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.HoursWorked));

            Assert.Equal(2, totalsByProject.Count);
            Assert.Equal(7, totalsByProject[101]);
            Assert.Equal(3, totalsByProject[102]);
        }
    }
}
