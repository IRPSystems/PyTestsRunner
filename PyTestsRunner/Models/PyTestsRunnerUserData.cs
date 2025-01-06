
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System.IO;

namespace PyTestsRunner.Models
{
	public class PyTestsRunnerUserData: ObservableObject
	{
		public string ScriptsDirectory { get; set; }

		public PyTestsRunnerUserData()
		{
		}

		public static PyTestsRunnerUserData LoadPyTestsRunnerUserData(string dirName)
		{

			string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			path = Path.Combine(path, dirName);
			if (Directory.Exists(path) == false)
			{
				return new PyTestsRunnerUserData();
			}
			path = Path.Combine(path, "EvvaUserData.json");
			if (File.Exists(path) == false)
			{
				return new PyTestsRunnerUserData();
			}


			string jsonString = File.ReadAllText(path);
			JsonSerializerSettings settings = new JsonSerializerSettings();
			settings.Formatting = Formatting.Indented;
			settings.TypeNameHandling = TypeNameHandling.All;
			PyTestsRunnerUserData pyTestsRunnerUserData = JsonConvert.DeserializeObject(jsonString, settings) as PyTestsRunnerUserData;
			if (pyTestsRunnerUserData == null)
				return pyTestsRunnerUserData;
			
			


			return pyTestsRunnerUserData;
		}



		public static void SavePyTestsRunnerUserData(
			string dirName,
			PyTestsRunnerUserData pyTestsRunnerUserData)
		{
			string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			path = Path.Combine(path, dirName);
			if (Directory.Exists(path) == false)
				Directory.CreateDirectory(path);
			path = Path.Combine(path, "EvvaUserData.json");

			JsonSerializerSettings settings = new JsonSerializerSettings();
			settings.Formatting = Formatting.Indented;
			settings.TypeNameHandling = TypeNameHandling.All;
			var sz = JsonConvert.SerializeObject(pyTestsRunnerUserData, settings);
			File.WriteAllText(path, sz);
		}
	}
}
