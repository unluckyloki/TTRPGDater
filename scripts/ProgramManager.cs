using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Discord.Net;
using Discord.Webhook;
using System.Threading.Tasks;
using System.Threading;
using Discord;

public partial class ProgramManager : Node
{
	public const string PROFILE_FOLDER = ".\\Profiles";
	public const string PROFILE_EXT = "daterprof";
	public const string PROFILE_LAST = ".\\LastProfile.cfg";

	private VBoxContainer timelineList;
	public VBoxContainer TimelineList {
		get {return timelineList;}
	 	set {
			timelineList = value;
			if (!File.Exists(PROFILE_LAST))
				LoadProfile(Profiles[0].Name);
			else
				LoadProfile(File.ReadAllText(PROFILE_LAST));
		}
	}

    #region Profiles
	public event Action ProfilesUpdated;
	private List<Profile> profiles = new();
	public List<Profile> Profiles{
		get { return profiles; }
		private set {
			profiles = value;
			ProfilesUpdated?.Invoke();
		}
	}

    public event Action ActiveProfileChanged;
    private string activeProfile = "";
	public string ActiveProfile {
		get { return activeProfile; }
		set {
			activeProfile = value;
			File.Create(PROFILE_LAST).Close();
			using (var file = new StreamWriter(PROFILE_LAST))
				file.Write(value);
			ActiveProfileChanged?.Invoke();
		}
	}

	public string WebhookLink
	{
		get { return Profiles.Find(x => x.Name == ActiveProfile).WebhookLink; }
		set {
			Profiles.Find(x => x.Name == ActiveProfile).WebhookLink = value;
			if(value != "") try {discordWebhookClient = new(value); } catch (Exception){};
			SaveProfiles();
			RefreshProfliesList();
			UpdateWebhook();
		}
	}

	public event Action MessageIDChanged;
	public string MessageID
	{
		get { return Profiles.Find(x => x.Name == ActiveProfile).MesaageID; }
		set { 
			Profiles.Find(x => x.Name == ActiveProfile).MesaageID = value;
			SaveProfiles();
			RefreshProfliesList();
			UpdateWebhook();
			MessageIDChanged?.Invoke();
		}
	}
	#endregion Profiles

	private DiscordWebhookClient discordWebhookClient;

	private Thread mainThread;

	public override void _Ready()
	{
		GetWindow().MinSize = new Vector2I(695, 510);

		mainThread = Thread.CurrentThread;

		ProfilesUpdated += () => { SaveProfiles(); };

		RefreshProfliesList();
	}

	#region Profile modules

	public void RenameProfile(string newName)
	{
		string oldName = ActiveProfile;
		if(Profiles.Any(x =>  x.Name == newName) || newName == "")
			return;
		Profiles.Find(x => x.Name == ActiveProfile).Name = newName;
		ActiveProfile = newName;

		SaveProfiles();
		try { File.Delete(Path.Combine(PROFILE_FOLDER, $"{oldName}.{PROFILE_EXT}")); } catch (Exception) {}
		//ProfilesUpdated?.Invoke();
		RefreshProfliesList();
	}
	
	public void CreateProfile()
	{
		string namePat = "New Profile";
		
		if(Profiles.Any(x => x.Name == namePat))
		{
			int i = 1;
			while(Profiles.Any(x => x.Name == $"{namePat} {i}"))
				i++;
			
			namePat = $"{namePat} {i}";
		}

		Profiles.Add(new Profile(namePat));
		ProfilesUpdated?.Invoke();
		LoadProfile(namePat);
	}

	public void SaveProfiles()
	{
		foreach (var profile in Profiles)
		{
			File.Create(Path.Combine(PROFILE_FOLDER, $"{profile.Name}.{PROFILE_EXT}")).Close();
			using (var file = new StreamWriter(Path.Combine(PROFILE_FOLDER, $"{profile.Name}.{PROFILE_EXT}")))
				file.Write(JsonSerializer.Serialize(profile));
		}
	}

	public void LoadProfile(string name) // дописать если не найден такой профиль, сохранять активный профиль
	{
		RefreshProfliesList();
		ActiveProfile = name;
		if (!Profiles.Any(x => x.Name == ActiveProfile))
			ActiveProfile = Profiles[0].Name;

		int timelinesCount = TimelineList.GetChildCount(true);
		GD.Print($"Deleting {timelinesCount} timelines");
		//if (timelinesCount > 0)
		for (int i = timelinesCount-1; i >= 0; i--)
		{
			var child = TimelineList.GetChild(i);
			TimelineList.RemoveChild(child);
			child.QueueFree();
		}
		
		foreach (var timelineSave in Profiles.Find(x => x.Name == ActiveProfile).Timelines)
		{
			GD.Print(timelineSave.Name);
			var scene = GD.Load<PackedScene>("res://scenes/Timeline.tscn");
			var instance = scene.Instantiate<Timeline>();
			instance.TimelineName = timelineSave.Name;
			instance.DateTime = DateTime.Parse(timelineSave.Date);
			instance.IsHidden = timelineSave.IsHidden;
			TimelineList.AddChild(instance);
		}
		OnTimelineChanged(); // надо нахуй, потому что без в инстансе оно не хочет работать нормально
	}

	public void RefreshProfliesList()
	{
		if (!Directory.Exists(PROFILE_FOLDER))
		{
			Directory.CreateDirectory(PROFILE_FOLDER);

			var emptyProfiles = new List<Profile> { new() };
			Profiles = emptyProfiles;
			return;
		}

		string[] profFiles = Directory.GetFiles(PROFILE_FOLDER, $"*.{PROFILE_EXT}");
		Profiles.Clear();
		foreach (string profFile in profFiles)
		{
			try { 
				Profiles.Add(
					JsonSerializer.Deserialize<Profile>(File.ReadAllText(profFile))
				);
			} catch(Exception e){ GD.PrintErr(e); }
		}
		//Profiles = newProfiles;

		if (Profiles.Count == 0)
			CreateProfile();
		
		// if (!Profiles.Any(x => x.Name == ActiveProfile))
		// 	LoadProfile(Profiles[0].Name);
	}

	public void DeleteProfile()
	{
		try { File.Delete(Path.Combine(PROFILE_FOLDER, $"{ActiveProfile}.{PROFILE_EXT}")); } catch (Exception e) { GD.Print(e); }
		RefreshProfliesList();
	}

    public void OnTimelineChanged()
    {
		int timelinesCount = TimelineList.GetChildCount();
		GD.Print($"Found {timelinesCount} timelines");
		Profiles.Find(x => x.Name == ActiveProfile).Timelines.Clear();
		for (int i = 0; i < timelinesCount; i++)
			Profiles.Find(x => x.Name == ActiveProfile).Timelines.Add(TimelineSave.FromTimeline(TimelineList.GetChild<Timeline>(i)));
		
		SaveProfiles();
		UpdateWebhook();
	}

	#endregion

	private void UpdateWebhook()
	{
		GD.Print($"Updating webhook... | Task Null: {updateTask == null} | Task Running: {updateTask?.Status == TaskStatus.Created}");
		
		if (discordWebhookClient == null) 
			try { discordWebhookClient = new(WebhookLink); } catch (Exception) { return; }
		

		webhookContent.Clear();
		int timelinesCount = TimelineList.GetChildCount();

		for (int i = 0; i < timelinesCount; i++)
		{
			Timeline timeline = TimelineList.GetChild<Timeline>(i);
			string name = timeline.TimelineName;
			string time = timeline.IsHidden ? "??:??" : timeline.DateTime.ToString("t");
			string date = timeline.IsHidden ? "??.??.??" : timeline.DateTime.ToString("d");

			string[] line = {name, time, date};
			webhookContent.Add(line);
		}
		
		if (updateTask == null /*|| updateTask.Status != TaskStatus.Running*/)
		{
			GD.Print($"Running new task");
			updateTask = UpdateWebhookTask();
		}
	}

	private List<string[]> webhookContent = new();
	private Task updateTask;
	private Task UpdateWebhookTask() => Task.Run(async () =>
	{
		Thread.Sleep(2000);
		if (discordWebhookClient == null) return;

		// string content = "";
		// int timelinesCount = TimelineList.GetChildCount();

		// for (int i = 0; i < timelinesCount; i++)
		// {
		// 	Timeline timeline = TimelineList.GetChild<Timeline>(i);
		// 	string name = timeline.TimelineName;
		// 	string time = timeline.IsHidden ? "??:??" : timeline.DateTime.ToString("t");
		// 	string date = timeline.IsHidden ? "??.??.??" : timeline.DateTime.ToString("d");
		// 	string newLine = i == timelinesCount - 1 ? "" : "\n";

		// 	string line = $"{name} | {time} | {date}{newLine}";
		// 	content += line;
		// }
			
		

		ulong messageID = 0;
		EmbedBuilder embed = new(){
			Title = "TTRPG Dater by Unlucky",
		};
		foreach (var tl in webhookContent)
			embed.AddField(tl[0], $"{tl[1]} | {tl[2]}");

		if (ulong.TryParse(MessageID, out messageID) && messageID != 0)
			try 
			{
				GD.Print($"Editing msg: {messageID}");
				await discordWebhookClient.ModifyMessageAsync(messageID, (p) => {p.Embeds = new List<Embed>{embed.Build()};});
			} catch (Exception e) { GD.Print(e); }
		else 
			try 
			{
				GD.Print($"Sending msg");
				string _messageID = (await discordWebhookClient.SendMessageAsync(embeds:new List<Embed>{embed.Build()})).ToString();
				DefferedManager.DefferedInvoke (() => {
					MessageID = _messageID;
				});
			} catch (Exception e) { GD.Print(e); }
		
		updateTask = null;
	});
}