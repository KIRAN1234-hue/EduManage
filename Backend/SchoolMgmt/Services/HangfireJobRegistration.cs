using Hangfire;
using SchoolMgmt.Services.Interfaces;

namespace SchoolMgmt.Services;

public static class HangfireJobRegistration
{
    // CHANGE: Accept IRecurringJobManager instead of using static RecurringJob
    public static void RegisterRecurringJobs(IRecurringJobManager jobManager)
    {
        jobManager.AddOrUpdate<IBackgroundJobService>(
            "attendance-check-daily",
            job => job.CheckAttendanceAndAlertParentsAsync(),
            "0 18 * * *"
        );

        jobManager.AddOrUpdate<IBackgroundJobService>(
            "exam-reminders-daily",
            job => job.SendExamRemindersAsync(),
            "0 7 * * *"
        );

        jobManager.AddOrUpdate<IBackgroundJobService>(
            "library-reminders-daily",
            job => job.SendLibraryReturnRemindersAsync(),
            "0 8 * * *"
        );

        jobManager.AddOrUpdate<IBackgroundJobService>(
            "fee-reminders-monthly",
            job => job.SendOverdueFeeRemindersAsync(),
            "0 9 1 * *"
        );
    }
}