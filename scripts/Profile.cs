using System;
using System.Collections.Generic;

[Serializable]
public class Profile
{
    public string Name { get; set; } = "New Profile";
    public List<TimelineSave> Timelines { get; set; } = new();
    public string WebhookLink { get; set; } = "";
    public string MesaageID { get; set; } = "";

    public Profile(){}
    public Profile(string name) =>
        Name = name;
}

[Serializable]
public class TimelineSave
{
    public bool IsHidden { get; set; }
    public string Name { get; set; }
    public string Date { get; set; }
    public static TimelineSave FromTimeline(Timeline timeline) => new()
    {
        IsHidden = timeline.IsHidden,
        Name = timeline.TimelineName,
        Date = timeline.DateTime.ToString("dd/MM/yyyy HH:mm:ss")
    };

    public static Timeline ToTimeline(TimelineSave timelineSave) => new()
    {
        IsHidden = timelineSave.IsHidden,
        TimelineName = timelineSave.Name,
        DateTime = DateTime.Parse(timelineSave.Date)
    };
}
