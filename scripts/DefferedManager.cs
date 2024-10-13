using Godot;
using System;
using System.Collections.Concurrent;

public partial class DefferedManager : Node
{
	public static ConcurrentQueue<Action> DeferredActionQueue { get; set; } = new();

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		while (!DeferredActionQueue.IsEmpty)
		{
		    bool gotAction = DeferredActionQueue.TryDequeue(out Action result);
		    if (gotAction && result != null)
		        result.Invoke();
		}
	}

	public static void DefferedInvoke(Action action)
	{
		DeferredActionQueue.Enqueue(action);
	}
}
