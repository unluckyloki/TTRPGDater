using Godot;
using System;

public partial class Inspector : MarginContainer
{
	[Export] private Button _deselectAll;
	[Export] private Button _newTimeline;
	[Export] private VBoxContainer _timelineList;

	private ProgramManager _programManager;

	public override void _Ready()
	{
		_programManager = GetNode<ProgramManager>("/root/ProgramManager");
		_programManager.TimelineList = _timelineList;

		if (_newTimeline == null || _timelineList == null)
			throw new Exception("Inspector nodes not selected");
		
		_deselectAll.Pressed += OnDeselectAllPressed;
		_newTimeline.Pressed += OnNewTimelinePressed;
	}

    private void OnDeselectAllPressed()
    {
		int timelinesCount = _timelineList.GetChildCount();
		for (int i = 0; i < timelinesCount; i++)
		{
			Timeline timeline = _timelineList.GetChild<Timeline>(i);
			if (timeline.Selected)
				timeline.Selected = false;
		}
    }

    private void OnNewTimelinePressed()
    {
        var scene = GD.Load<PackedScene>("res://scenes/Timeline.tscn");
		var instance = scene.Instantiate();
		_timelineList.AddChild(instance);
		_programManager.OnTimelineChanged();
    }
}
