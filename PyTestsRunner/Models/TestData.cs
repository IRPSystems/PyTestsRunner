
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows;

namespace PyTestsRunner.Models
{
	public enum TestTypes { Class, Function}
	public enum StateTypes { None, Runing, Ended }
	public class TestData: ObservableObject
	{
		public string Name { get; set; }
		public string Path { get; set; }
		public TestTypes Type { get; set; }
		public ObservableCollection<TestData> SubTestsList { get; set; }
		public TestData ParentClass { get; set; }

		public bool IsRun { get; set; }
		public bool IsExpanded { get; set; }
		public Visibility Visibility { get; set; }
		public bool IsEnabled { get; set; }

		public StateTypes State { get; set; }

	}
}
