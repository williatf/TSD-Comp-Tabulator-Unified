public static class AwardPlaceNames
{
    public static string ToPlaceName(int place)
    {
        return place switch
        {
            1 => "Winner",
            2 => "1st Runner Up",
            3 => "2nd Runner Up",
            4 => "3rd Runner Up",
            5 => "4th Runner Up",
            _ => $"{place - 1}th Runner Up"
        };
    }
}