using SpolecenskaDeskovaHraSUkoly.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace SpolecenskaDeskovaHraSUkoly.Services
{
    public class TaskService
    {
        private readonly string _tasksPath;

        public TaskService()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var dataDir = Path.Combine(baseDir, "Data");

            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }

            _tasksPath = Path.Combine(dataDir, "tasks.json");
        }
        public List<TaskItem> LoadTasks()
        {
            if (!File.Exists(_tasksPath))
            {
                return new List<TaskItem>();
            }

            try
            {
                string json = File.ReadAllText(_tasksPath);
                List<TaskItem> vysledek = JsonSerializer.Deserialize<List<TaskItem>>(json);
                if (vysledek == null)
                {
                    return new List<TaskItem>();
                }
                return vysledek;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Chyba při načítání JSON: {ex.Message}");
                return new List<TaskItem>();
            }
        }

        public void SaveTasks(IEnumerable<TaskItem> tasks)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

                string jsonString = JsonSerializer.Serialize(tasks, options);
                File.WriteAllText(_tasksPath, jsonString);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Chyba při ukládání JSON: {ex.Message}");
            }
        }
    }
}