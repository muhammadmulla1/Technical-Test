namespace Timesheet.Core.Models
{
    public class TimesheetEntry
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int UserId { get; set; }
        public int ProjectId { get; set; }
        public DateTime Date { get; set; }
        public decimal HoursWorked { get; set; } // e.g., 7.5
        public string? Description { get; set; }
    }
}
