using Godot;
using System;

public partial class ProfileSettings : HBoxContainer
{
	[Export] OptionButton _profile;
	[Export] LineEdit _renameLE;
	[Export] Button _rename;
	[Export] Button _refresh;
	[Export] Button _delete;
	
	private ProgramManager _programManager;
	public override void _Ready()
	{
		_programManager = GetNode<ProgramManager>("/root/ProgramManager");

		_renameLE.TextSubmitted += OnRenameSubmitted;
		_rename.Pressed += OnRenamePressed;
		_refresh.Pressed += () => { _programManager.RefreshProfliesList(); };
		_delete.Pressed += () => { _programManager.DeleteProfile(); };
		_profile.ItemSelected += OnProfileItemSelected;

		OnProfilesUpdated();
		_programManager.ProfilesUpdated += OnProfilesUpdated;
		_programManager.ActiveProfileChanged += OnProfilesUpdated;
	}

    private void OnProfileItemSelected(long index)
    {
        if(index == _profile.ItemCount-1)
			_programManager.CreateProfile();
		else
			_programManager.LoadProfile(_profile.Text);
    }


    private void OnProfilesUpdated()
    {
		_profile.Clear();
        foreach (var profile in _programManager.Profiles)
			_profile.AddItem(profile.Name);

		for (int i = 0; i < _profile.ItemCount; i++)
			if (_profile.GetItemText(i) == _programManager.ActiveProfile) _profile.Selected = i;
		
		_profile.AddItem("Create new");
    }


    private void OnRenameSubmitted(string newText)
    {
		if(newText != "")
			_programManager.RenameProfile(newText);

		_renameLE.Clear();
        _renameLE.Visible = false;
		_delete.Visible = true;
		_refresh.Visible = true;
		_profile.Visible = true;
		_rename.Visible = true;
    }


    private void OnRenamePressed()
    {
        _renameLE.Visible = true;
		_delete.Visible = false;
		_refresh.Visible = false;
		_profile.Visible = false;
		_rename.Visible = false;
    }
}
