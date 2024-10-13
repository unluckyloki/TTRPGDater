using Godot;
using System;
using System.Text.RegularExpressions;

public partial class Timeline : MarginContainer
{
	private const string DATE_FORMAT = @"([0-9]{1,2})\.([0-9]{1,2})\.([0-9]{1,4})";
	private const string TIME_FORMAT = @"(\d{1,2}):(\d{1,2})";

	#region Getters/Setters
	private bool selected = false;
	public bool Selected{
		get{ return selected; }
		set{ 
			selected = value;
			_selection.Visible = value;
			_select.ButtonPressed = value;
		}
	}


	private DateTime dateTime = new();
	public DateTime DateTime {
		get { return dateTime; }
		set {
			dateTime = value;
			if(_time != null) _time.Text = value.ToString("t");
			if(_date != null) _date.Text = value.ToString("D");
			_programManager?.OnTimelineChanged();
		}
	}

	private string name = "New Timeline";
	public string TimelineName {
		get { return name; }
		set { 
			name = value;
			if(_name != null) _name.Text = value;
			_programManager?.OnTimelineChanged();
		}
	}

	private bool isHidden = false;
	public bool IsHidden {
		get { return isHidden; }
		set { 
			isHidden = value;
			if(_hidden != null) _hidden.ButtonPressed = value;
			_programManager?.OnTimelineChanged();
		}
	}

	#endregion Getters/Setters

	#region Gui
	[Export] private ColorRect _selection;
	[Export] private CheckBox _select;
	[Export] private CheckButton _hidden;
	[Export] private LineEdit _name;
	[Export] private LineEdit _time;
	[Export] private LineEdit _date;
	[Export] private Button _delete;
	#endregion Gui

	#region Gui events
	private ProgramManager _programManager;
	public override void _Ready()
	{
		_programManager = GetNode<ProgramManager>("/root/ProgramManager");

		_name.Text = TimelineName;
		_time.Text = DateTime.ToString("t");
		_date.Text = DateTime.ToString("D");
		_hidden.ButtonPressed = IsHidden;

		_hidden.Toggled += HiddenToggled;
		_select.Toggled += SelectToggled;
		_delete.Pressed += DeletePressed;

		_name.GuiInput += NameGuiInput;
		_time.GuiInput += TimeGuiInput;
		_date.GuiInput += DateGuiInput;

		_name.TextSubmitted += NameTextSubmitted;
		_time.TextSubmitted += TimeTextSubmitted;
		_date.TextSubmitted += DateTextSubmitted;
	}

    private void HiddenToggled(bool toggledOn) =>
		IsHidden = toggledOn;

    private void SelectToggled(bool toggledOn) =>
    	Selected = toggledOn;

    private void DeletePressed()
    {
		GetParent().RemoveChild(this);
        QueueFree();
		_programManager.OnTimelineChanged();
    }

    //* MARK: ChangeSubmited

    private void NameTextSubmitted(string newText)
    {
		if (newText != TimelineName)
        	TimelineName = newText;
		_name.Flat = true; _name.Editable = false;
    }


    private void TimeTextSubmitted(string newText)
    {
        if (
			Regex.IsMatch(newText, TIME_FORMAT) &&
			!(int.Parse(Regex.Match(newText, TIME_FORMAT).Groups[1].Value) > 23) &&
			!(int.Parse(Regex.Match(newText, TIME_FORMAT).Groups[2].Value) > 59)
		)
		{
			try{
				DateTime newDateTime = new(
					DateTime.Year,
					DateTime.Month,
					DateTime.Day,
					int.Parse(Regex.Match(newText, TIME_FORMAT).Groups[1].Value),
					int.Parse(Regex.Match(newText, TIME_FORMAT).Groups[2].Value),
					0
				);
				DateTime = newDateTime;
			}
			catch(Exception){ DateTime = DateTime; };
		}
		else DateTime = DateTime;

		//_time.Text = DateTime.ToString("t");
        _time.Flat = true; _time.Editable = false;
    }


    private void DateTextSubmitted(string newText)
    {
		if (
			Regex.IsMatch(newText, DATE_FORMAT) &&
			!(int.Parse(Regex.Match(newText, DATE_FORMAT).Groups[1].Value) > 31) &&
			!(int.Parse(Regex.Match(newText, DATE_FORMAT).Groups[2].Value) > 12)
		)
		{
			try{
				DateTime newDateTime = new(
					int.Parse(Regex.Match(newText, DATE_FORMAT).Groups[3].Value),
					int.Parse(Regex.Match(newText, DATE_FORMAT).Groups[2].Value),
					int.Parse(Regex.Match(newText, DATE_FORMAT).Groups[1].Value)
				);

				DateTime = newDateTime + DateTime.TimeOfDay;
			}
			catch(Exception){ DateTime = DateTime; };
		}
		else DateTime = DateTime;

		//_date.Text = DateTime.ToString("D");
        _date.Flat = true; _date.Editable = false;
    }

	//* MARK: Gui input
    private void NameGuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton)
			if (
				mouseButton.ButtonIndex == MouseButton.Left &&
				mouseButton.DoubleClick &&
				!_name.Editable
			) {
				if(_time.Editable) {
					_time.Text = DateTime.ToString("t");
					_time.Flat = true; _time.Editable = false;
				}
				if(_date.Editable) {
					_date.Text = DateTime.ToString("D");
					_date.Flat = true; _date.Editable = false;
				}

				_name.Flat = false; _name.Editable = true;
			}
		if (@event is InputEventKey eventKey)
			if (
				eventKey.Keycode == Key.Escape &&
				_name.Editable
			) {
				_name.Text = TimelineName;
				_name.Flat = true; _name.Editable = false;
			}
    }

	private void TimeGuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton)
			if (
				mouseButton.ButtonIndex == MouseButton.Left &&
				mouseButton.DoubleClick &&
				!_time.Editable
			) {
				if(_name.Editable) {
					_name.Text = TimelineName;
					_name.Flat = true; _name.Editable = false;
				}
				if(_date.Editable) {
					_date.Text = DateTime.ToString("D");
					_date.Flat = true; _date.Editable = false;
				}

				_time.Flat = false; _time.Editable = true;
			}
		if (@event is InputEventKey eventKey)
			if (
				eventKey.Keycode == Key.Escape &&
				_time.Editable
			) {
				_time.Text = DateTime.ToString("t");
				_time.Flat = true; _time.Editable = false;
			}
    }

    private void DateGuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton)
			if (
				mouseButton.ButtonIndex == MouseButton.Left &&
				mouseButton.DoubleClick &&
				!_date.Editable
			) {
				if(_time.Editable) {
					_time.Text = DateTime.ToString("t");
					_time.Flat = true; _time.Editable = false;
				}
				if(_name.Editable) {
					_name.Text = TimelineName;
					_name.Flat = true; _name.Editable = false;
				}

				_date.Text = DateTime.ToString("d");
				_date.Flat = false; _date.Editable = true;
			}
		
		if (@event is InputEventKey eventKey)
			if (
				eventKey.Keycode == Key.Escape &&
				_date.Editable
			) {
				_date.Text = DateTime.ToString("D");
				_date.Flat = true; _date.Editable = false;
			}
    }

    #endregion Gui events
}
