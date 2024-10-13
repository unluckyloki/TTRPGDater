using Godot;
using System;

public partial class WebhookSettings : HBoxContainer
{
	[Export] private LineEdit _webhookLE;
	[Export] private Button _saveWebhook;
	[Export] private LineEdit _messageIDLE;
	[Export] private Button _saveMessageID;

	private ProgramManager _programManager;

	public override void _Ready()
	{
		_programManager = GetNode<ProgramManager>("/root/ProgramManager");

		_webhookLE.Text = _programManager.WebhookLink;
		_messageIDLE.Text = _programManager.MessageID;
		_programManager.ActiveProfileChanged += OnActiveProfileChanged;
		_programManager.MessageIDChanged += () => { _messageIDLE.Text = _programManager.MessageID; };

		_webhookLE.TextChanged += OnWebhookLETextChanged;
		_messageIDLE.TextChanged += OnMessageIDTextChanged;

		_saveWebhook.Pressed += OnSaveWebhookPressed;
		_saveMessageID.Pressed += OnSaveMessageIDPressed;
	}

    private void OnActiveProfileChanged()
    {
        _webhookLE.Text = _programManager.WebhookLink;
		_messageIDLE.Text = _programManager.MessageID;
		_saveWebhook.Visible = _webhookLE.Text != _programManager.WebhookLink;
		_saveMessageID.Visible = _messageIDLE.Text != _programManager.MessageID;
    }


    private void OnSaveWebhookPressed()
    {
		if (_programManager.WebhookLink != _webhookLE.Text)
        	_programManager.WebhookLink = _webhookLE.Text;
		_saveWebhook.Visible = _webhookLE.Text != _programManager.WebhookLink;
    }


    private void OnSaveMessageIDPressed()
    {
        if (_programManager.MessageID != _messageIDLE.Text)
        	_programManager.MessageID = _messageIDLE.Text;
		_saveMessageID.Visible = _messageIDLE.Text != _programManager.MessageID;
    }


    private void OnWebhookLETextChanged(string newText) => 
		_saveWebhook.Visible = newText != _programManager.WebhookLink;

    private void OnMessageIDTextChanged(string newText) => 
		_saveMessageID.Visible = newText != _programManager.MessageID;
}
