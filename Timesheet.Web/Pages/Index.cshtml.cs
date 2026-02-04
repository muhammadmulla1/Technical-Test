using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Timesheet.Core.Models;
using Timesheet.Core.Services;
using System.Linq;
using System.Collections.Generic;
using System;

namespace Timesheet.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly TimesheetService _service;

        public IndexModel(TimesheetService service)
        {
            _service = service;
        }

        [BindProperty]
        public TimesheetEntry NewEntry { get; set; } = new();

        public List<TimesheetEntry> Entries { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int UserId { get; set; } = 1;

        public bool IsEditing { get; set; } = false;

        // Phase 3: totals per project
        public Dictionary<int, decimal> TotalsByProject { get; set; } = new();

        public void OnGet()
        {
            LoadEntries();
        }

        public void LoadEntries()
        {
            var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            Entries = _service.GetEntries(UserId, startOfWeek).ToList();

            // Calculate totals by project
            TotalsByProject = Entries
                .GroupBy(e => e.ProjectId)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.HoursWorked));
        }

        // Add or Update Entry
        public IActionResult OnPostAddOrUpdateEntry()
        {
            if (NewEntry.HoursWorked <= 0)
            {
                TempData["ErrorMessage"] = "⚠️ Hours must be greater than 0.";
                LoadEntries();
                return Page();
            }

            if (NewEntry.Date.Date > DateTime.Today)
            {
                TempData["ErrorMessage"] = "⚠️ Date cannot be in the future.";
                LoadEntries();
                return Page();
            }

            try
            {
                // Remove old entry if editing
                var existing = _service.GetEntries(NewEntry.UserId)
                    .FirstOrDefault(e => e.ProjectId == NewEntry.ProjectId && e.Date.Date == NewEntry.Date.Date);
                if (existing != null)
                    _service.Delete(existing.UserId, existing.ProjectId, existing.Date);

                _service.Add(NewEntry);
                TempData["SuccessMessage"] = "✅ Entry added/updated successfully!";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            LoadEntries();
            ModelState.Clear();
            NewEntry = new TimesheetEntry();
            IsEditing = false;
            return Page();
        }

        // Load entry into form for editing
        public IActionResult OnPostEditEntry(int userId, int projectId, DateTime date)
        {
            var entry = _service.GetEntries(userId)
                .FirstOrDefault(e => e.ProjectId == projectId && e.Date.Date == date.Date);

            if (entry != null)
            {
                NewEntry = new TimesheetEntry
                {
                    UserId = entry.UserId,
                    ProjectId = entry.ProjectId,
                    Date = entry.Date,
                    HoursWorked = entry.HoursWorked,
                    Description = entry.Description
                };
                IsEditing = true;
            }

            LoadEntries();
            return Page();
        }

        // Delete entry
        public IActionResult OnPostDeleteEntry(int userId, int projectId, DateTime date)
        {
            _service.Delete(userId, projectId, date);
            TempData["SuccessMessage"] = "🗑️ Entry deleted successfully!";
            LoadEntries();
            return Page();
        }
    }
}
