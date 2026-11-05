namespace Sample;

internal sealed class DisposalNumbers
{
    public DateTimeOffset Cutoffs(DateTimeOffset now)
    {
        DateTimeOffset a = now - TimeSpan.FromDays(400);
        DateTimeOffset b = now.AddMonths(-6);
        DateTimeOffset c = now.AddYears(-10);
        DateTimeOffset d = now.AddDays( -30);
        TimeSpan e = new TimeSpan(3, 0, 0);
        DateTimeOffset f = now - TimeSpan.FromMinutes(15); // TimeSpan.FromDays(1) in a comment is not code
        DateTimeOffset g = now - TimeSpan.FromDays(ProductConstants.LongSessionDays);
        DateTimeOffset h = now.AddMonths(-ProductConstants.SystemLogRetentionMonths);
        /* TimeSpan.FromHours(2) in a block comment */
        return a;
    }
}
