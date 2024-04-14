namespace Api;

public static class DateTimeExtensions
{
    public static int CalculateAge(this DateOnly dob)
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Now);

        int age = today.Year - dob.Year;

        if(dob > today.AddYears(-age)) age--;

        return age;
    }
}
