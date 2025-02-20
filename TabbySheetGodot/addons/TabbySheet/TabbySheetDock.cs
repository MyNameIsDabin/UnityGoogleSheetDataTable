using Godot;
using System;

[Tool]
public partial class TabbySheetDock : Control
{
	[Export]
	public LineEdit lineEditGoogleSheetUrl;
	[Export]
	public LineEdit lineEditCredentialJsonPath;
	[Export]
	public LineEdit lineEditExcelDownloadPath;
	[Export]
	public LineEdit lineEditGenerateClassPath;
	[Export]
	public Button downloadButton;
	[Export]
	public Button generateClassesButton;
	[Export]
	public Button exportBinaryButton;
	
	private TabbySheetSettings tabbySheetSettings;
	
	private const string SettingResourcePath = "res://addons/TabbySheet/TabbySheetSettings.tres";

	public override void _Ready()
	{
		tabbySheetSettings = GD.Load<TabbySheetSettings>(SettingResourcePath);
		
		lineEditGoogleSheetUrl.Text = tabbySheetSettings.GoogleSheetURL;
		
		downloadButton.Pressed += DownloadButtonPressed;
		generateClassesButton.Pressed += GenerateClassesButtonPressed;
		exportBinaryButton.Pressed += ExportBinaryButtonPressed;
		
		lineEditGoogleSheetUrl.TextChanged += SheetUrlChanged;
		lineEditCredentialJsonPath.TextChanged += CredentialJsonPathChanged;
		lineEditExcelDownloadPath.TextChanged += ExcelDownloadPathChanged;
		lineEditGenerateClassPath.TextChanged += GenerateClassPathChanged;
	}
	
	public void SaveSettings()
	{
		ResourceSaver.Save(tabbySheetSettings, SettingResourcePath);
	}

	public override void _Process(double delta)
	{
	}
	
	private void DownloadButtonPressed()
	{
		GD.Print("DownloadButtonPressed!");
	}
	
	private void GenerateClassesButtonPressed()
	{
		GD.Print("GenerateClassesButtonPressed!");
	}
	
	private void ExportBinaryButtonPressed()
	{
		GD.Print("ExportBinaryButtonPressed!");
	}
	
	private void SheetUrlChanged(string input)
	{
		tabbySheetSettings.GoogleSheetURL = input;
		SaveSettings();
	}
	
	private void CredentialJsonPathChanged(string input)
	{
		tabbySheetSettings.CredentialJsonPath = input;
		SaveSettings();
	}
	
	private void ExcelDownloadPathChanged(string input)
	{
		tabbySheetSettings.ExcelDownloadPath = input;
		SaveSettings();
	}
	
	private void GenerateClassPathChanged(string input)
	{
		tabbySheetSettings.ClassDirectoryPath = input;
		SaveSettings();
	}
}
