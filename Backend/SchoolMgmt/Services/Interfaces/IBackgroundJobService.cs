namespace SchoolMgmt.Services.Interfaces;

public interface IBackgroundJobService
{
    Task CheckAttendanceAndAlertParentsAsync();
    Task SendExamRemindersAsync();
    Task SendLibraryReturnRemindersAsync();
    Task SendOverdueFeeRemindersAsync();
}