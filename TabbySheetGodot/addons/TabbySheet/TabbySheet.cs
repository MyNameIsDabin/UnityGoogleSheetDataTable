#if TOOLS
using Godot;
using System;

[Tool]
public partial class TabbySheet : EditorPlugin
{
	private Control _dock;
	
	public override void _EnterTree()
	{
		_dock = GD.Load<PackedScene>("res://addons/TabbySheet/TabbySheetDock.tscn")
			.Instantiate<Control>();
			
		AddControlToDock(DockSlot.LeftBr, _dock);
	}

	public override void _ExitTree()
	{
		RemoveControlFromDocks(_dock);
		_dock.Free();
	}
}
#endif
