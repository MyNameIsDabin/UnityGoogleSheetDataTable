using Godot;
using System;

[Tool]
[GlobalClass]
public partial class TabbySheetSettings : Resource
{
	[Export]
	public string GoogleSheetURL;
	[Export]
	public string CredentialJsonPath;
	[Export]
	public string ExcelDownloadPath;
	[Export]
	public string ClassDirectoryPath;
}
