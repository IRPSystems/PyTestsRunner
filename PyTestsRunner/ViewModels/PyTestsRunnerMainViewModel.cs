
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using PyTestsRunner.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace PyTestsRunner.ViewModels
{
	public class PyTestsRunnerMainViewModel: ObservableObject
	{
		#region Properties

		public string Version { get; set; }

		public string ScriptsDirectory { get; set; }
		public ObservableCollection<TestData> TestsList { get; set; }

		public bool IsEnableRun { get; set; }

		public string OutputText { get; set; }
		public string ErrorText { get; set; }

		#endregion Properties

		#region Fields

		private PyTestsRunnerUserData _pyTestsRunnerUserData;

		private bool _isAllSelected;

		private int _classIndex;
		private int _functionIndex;

		#endregion Fields

		#region Constructor

		public PyTestsRunnerMainViewModel() 
		{
			Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

			_isAllSelected = false;
			IsEnableRun = false;

			BrowseScriptsDirectoryCommand = new RelayCommand(BrowseScriptsDirectory);
			LoadScriptsDirectoryCommand = new RelayCommand(LoadScriptsDirectory);
			SelectAllCommand = new RelayCommand(SelectAll);
			StartAllCommand = new RelayCommand(StartAll);
			ClosingCommand = new RelayCommand<CancelEventArgs>(Closing);
			ScriptExpandAllCommand = new RelayCommand(ScriptExpandAll);
			ScriptCollapseAllCommand = new RelayCommand(ScriptCollapseAll);
			TestSelect_CheckedCommand = new RelayCommand<RoutedEventArgs>(TestSelect_Checked);
			TestSelect_UncheckedCommand = new RelayCommand<RoutedEventArgs>(TestSelect_Checked);


			_pyTestsRunnerUserData = PyTestsRunnerUserData.LoadPyTestsRunnerUserData("PyTestsRunner");
			ScriptsDirectory = _pyTestsRunnerUserData.ScriptsDirectory;
		}

		#endregion Constructor

		#region Methods

		private void Closing(CancelEventArgs e)
		{
			PyTestsRunnerUserData.SavePyTestsRunnerUserData(
				"PyTestsRunner", 
				_pyTestsRunnerUserData);
		}

		private void BrowseScriptsDirectory()
		{
			string initDir = _pyTestsRunnerUserData.ScriptsDirectory;
			if (Directory.Exists(initDir) == false)
				initDir = "";
			CommonOpenFileDialog commonOpenFile = new CommonOpenFileDialog();
			commonOpenFile.IsFolderPicker = true;
			commonOpenFile.InitialDirectory = initDir;
			CommonFileDialogResult results = commonOpenFile.ShowDialog();
			if (results != CommonFileDialogResult.Ok)
				return;

			_pyTestsRunnerUserData.ScriptsDirectory =
				commonOpenFile.FileName;
			ScriptsDirectory = commonOpenFile.FileName;
		}

		#region Load scripts

		private void LoadScriptsDirectory()
		{
			List<string> testsList = GetTests(ScriptsDirectory);

			TestsList = new ObservableCollection<TestData>();

			foreach (string test in testsList)
            {
				string fileName = Path.GetFileName(test);
				TestData data = new TestData()
				{ 
					Name = fileName,
					Path = test,
					Type = TestTypes.Class,
					SubTestsList = new ObservableCollection<TestData>(),
					ParentClass = null,
				};

				TestsList.Add(data);
				AddSubTests(data);
			}

			if(TestsList.Count > 0)
				IsEnableRun = true;

		}

		private List<string> GetTests(string directory)
		{ 
			string[] dirFiles = Directory.GetFiles(directory);
			string[] dirDirs = Directory.GetDirectories(directory);

			List<string> tests = new List<string>();
			foreach (string file in dirFiles)
			{
				string fileName = Path.GetFileName(file);
				if(fileName.StartsWith("test_"))
					tests.Add(file);
			}

			foreach (string dir in dirDirs)
			{
				int index = dir.LastIndexOf("\\");
				string dirName = dir.Substring(index + 1);
				if (dirName == ".pytest_cache" ||
					dirName == "__pycache__")
					continue;

				List<string> subTests = GetTests(dir);
				tests.AddRange(subTests);
			}

			return tests;
		}

		private void AddSubTests(TestData parentTest)
		{
			string fileData = string.Empty;
			using(StreamReader sr = new StreamReader(parentTest.Path))
			{ 
				fileData = sr.ReadToEnd(); 
			}

			if (string.IsNullOrEmpty(fileData))
				return;

			int classStartIndex = fileData.IndexOf("class ");
			classStartIndex += "class ".Length;
			int classEndIndex = fileData.IndexOf(":", classStartIndex);
			string className = fileData.Substring(classStartIndex, classEndIndex - classStartIndex);
			parentTest.Name = className;

			int currentIndex = 0;
			while(currentIndex < fileData.Length)
			{
				int index = fileData.IndexOf("def ", currentIndex);
				if (index < 0)
					return;

                int endIndex = fileData.IndexOf("\n", index);

				string funcHeaderLine = fileData.Substring(index, endIndex - index);

				string funcName = funcHeaderLine.Replace("def ", string.Empty);
				int funcNameEnd = funcName.IndexOf("(");
				funcName = funcName.Substring(0, funcNameEnd);

				currentIndex = endIndex;
				if (funcName.StartsWith("test_") == false)
					continue;

				TestData data = new TestData()
				{
					Name = funcName,
					Path = null,
					Type = TestTypes.Function,
					ParentClass = parentTest,
				};

				parentTest.SubTestsList.Add(data);
			}
		}

		#endregion Load scripts

		private void TestSelect_Checked(RoutedEventArgs e)
		{
			if (!(e.OriginalSource is CheckBox checkBox))
				return;

			if(!(checkBox.DataContext is TestData data)) 
				return;

			if(data.Type != TestTypes.Class)
				return;

			foreach(TestData testData in data.SubTestsList) 
				testData.IsRun = data.IsRun;
		}

		#region Expand/Collapse

		private void ScriptExpandAll()
		{
			foreach (TestData testData in TestsList)
				testData.IsExpanded = true;
		}

		private void ScriptCollapseAll()
		{
			foreach (TestData testData in TestsList)
				testData.IsExpanded = false;
		}

		#endregion Expand/Collapse

		private void SelectAll()
		{
			_isAllSelected = !_isAllSelected;

			foreach (TestData testData in TestsList)
			{
				testData.IsRun = _isAllSelected;

				foreach (TestData subTestData in testData.SubTestsList)
				{
					subTestData.IsRun = _isAllSelected;
				}
			}
		}

		private void StartAll()
		{
			IsEnableRun = false;
			OutputText = string.Empty;
			ErrorText = string.Empty;

			foreach (TestData testData in TestsList)
			{
				testData.State = StateTypes.None;
				testData.IsEnabled = false;

				foreach (TestData subTestData in testData.SubTestsList)
				{
					subTestData.State = StateTypes.None;
					subTestData.IsEnabled = false;
				}
			}

			_classIndex = 0;
			_functionIndex = 0;

			RunSingle();
		}

		private void RunSingle()
		{ 
			if(_classIndex < 0 || _classIndex >= TestsList.Count)
			{
				EndRun();
				return;
			}

			TestData classData = TestsList[_classIndex];
			if (!classData.IsRun)
			{
				_classIndex++;
				return;
			}

			TestData testData = classData.SubTestsList[_functionIndex];
			if (!testData.IsRun)
			{
				_functionIndex++;
				return;
			}

			if (testData.Type!= TestTypes.Function ||
				testData.ParentClass == null)
			{
				return;
			}

			testData.State = StateTypes.Runing;

			string arguments = $"-s {testData.ParentClass.Path}::{testData.ParentClass.Name}::{testData.Name}";
			

			Process process = new Process(); 
			process.StartInfo.FileName = "pytest";
			process.StartInfo.Arguments = arguments;
			process.StartInfo.WorkingDirectory = ScriptsDirectory;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardError = true;


			process.EnableRaisingEvents = true;
			process.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
			process.ErrorDataReceived += new DataReceivedEventHandler(ErrorHandler);
			process.Exited += Process_Exited;

			bool isOK = process.Start();
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();

			
		}

		private void EndRun()
		{
			IsEnableRun = true;

			foreach (TestData testData in TestsList)
			{
				testData.State = StateTypes.None;
				testData.IsEnabled = true;

				foreach (TestData subTestData in testData.SubTestsList)
				{
					subTestData.State = StateTypes.None;
					subTestData.IsEnabled = true;
				}
			}
		}

		private void Process_Exited(object sender, EventArgs e)
		{
			TestsList[_classIndex].SubTestsList[_functionIndex].State = StateTypes.Ended;

			_functionIndex++;
			if (_functionIndex < 0 || _functionIndex >= TestsList[_classIndex].SubTestsList.Count)
			{
				_functionIndex = 0;
				_classIndex++;

				while (_classIndex < TestsList.Count && TestsList[_classIndex].SubTestsList.Count == 0)
					_classIndex++;
			}

			if (_classIndex < 0 || _classIndex >= TestsList.Count)
			{
				EndRun();
				return;
			}


			RunSingle();
		}

		private void OutputHandler(object sender, DataReceivedEventArgs e)
		{
			Application.Current.Dispatcher.Invoke(() =>
			{
				OutputText += e.Data + "\r\n";
			});
		}

		private void ErrorHandler(object sender, DataReceivedEventArgs e)
		{
			Application.Current.Dispatcher.Invoke(() =>
			{
				ErrorText += e.Data + "\r\n";
			});
		}

		#endregion Methods

		#region Commands

		public RelayCommand BrowseScriptsDirectoryCommand { get; private set; }
		public RelayCommand LoadScriptsDirectoryCommand { get; private set; }
		public RelayCommand SelectAllCommand { get; private set; }
		public RelayCommand StartAllCommand { get; private set; }
		public RelayCommand<CancelEventArgs> ClosingCommand { get; private set; }


		public RelayCommand ScriptExpandAllCommand { get; private set; }
		public RelayCommand ScriptCollapseAllCommand { get; private set; }

		public RelayCommand<RoutedEventArgs> TestSelect_CheckedCommand { get; private set; }
		public RelayCommand<RoutedEventArgs> TestSelect_UncheckedCommand { get; private set; }

		#endregion Commands
	}
}
