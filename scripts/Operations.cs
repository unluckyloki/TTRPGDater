using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

public partial class Operations : MarginContainer
{
	[Export] private VBoxContainer _timelineList;
	[Export] private LineEdit _days;
	[Export] private LineEdit _months;
	[Export] private LineEdit _years;
	[Export] private LineEdit _hours;
	[Export] private LineEdit _minutes;
	[Export] private Button _add;
	[Export] private Button _substract;

	public override void _Ready()
	{
		_add.Pressed += AddPressed;
		_substract.Pressed += SubstractPressed;
	}
	
    private void AddPressed()
    {
		string[] _inputs = {_years.Text, _months.Text, _days.Text, _hours.Text, _minutes.Text};
		List<int> inputs = new();
		foreach (var input in _inputs)
			if (input == "" || !Regex.IsMatch(input, @"\d+"))
				inputs.Add(0);
			else
				inputs.Add(int.Parse(input));

		TimeSpan timeSpan = new(
			inputs[0]*365+inputs[1]*30+inputs[2],
			inputs[3],
			inputs[4],
			0
		);

		_years.Text = ""; _months.Text = ""; _days.Text = ""; _hours.Text = ""; _minutes.Text = "";

		int timelinesCount = _timelineList.GetChildCount();
		if (timelinesCount == 0) return;
		
		bool hasSelected = false;
		for (int i = 0; i < timelinesCount; i++)
			if (_timelineList.GetChild<Timeline>(i).Selected) hasSelected = true;
		
		for (int i = 0; i < timelinesCount; i++)
		{
			Timeline timeline = _timelineList.GetChild<Timeline>(i);
			if (!hasSelected || timeline.Selected) try { timeline.DateTime += timeSpan; } catch (ArgumentOutOfRangeException){}
		}
    }

    private void SubstractPressed()
    {
        string[] _inputs = {_years.Text, _months.Text, _days.Text, _hours.Text, _minutes.Text};
		List<int> inputs = new();
		foreach (var input in _inputs)
			if (input == "" || !Regex.IsMatch(input, @"\d+"))
				inputs.Add(0);
			else
				inputs.Add(int.Parse(input));

		TimeSpan timeSpan = new(
			inputs[0]*365+inputs[1]*30+inputs[2],
			inputs[3],
			inputs[4],
			0
		);

		_years.Text = ""; _months.Text = ""; _days.Text = ""; _hours.Text = ""; _minutes.Text = "";

		int timelinesCount = _timelineList.GetChildCount();
		if (timelinesCount == 0) return;
		
		bool hasSelected = false;
		for (int i = 0; i < timelinesCount; i++)
			if (_timelineList.GetChild<Timeline>(i).Selected) hasSelected = true;
		
		for (int i = 0; i < timelinesCount; i++)
		{
			Timeline timeline = _timelineList.GetChild<Timeline>(i);
			if (!hasSelected || timeline.Selected) try { timeline.DateTime -= timeSpan; } catch (ArgumentOutOfRangeException){}
		}
    }
}
