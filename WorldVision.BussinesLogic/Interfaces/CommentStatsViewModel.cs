namespace eUseControl.BusinessLogic.Interfaces
{
    public class CommentStatsViewModel
    {
        public int ThisMonthComments { get; internal set; }
        public int TodayComments { get; internal set; }
        public int ThisWeekComments { get; internal set; }
        public int PendingComments { get; internal set; }
        public int ApprovedComments { get; internal set; }
        public int TotalComments { get; internal set; }
    }
}