using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using SpolecenskaDeskovaHraSUkoly.Models;

namespace SpolecenskaDeskovaHraSUkoly.Services
{
    public class TaskService
    {
        private readonly string _tasksPath;
        public TaskService()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            _tasksPath = Path.Combine(baseDir, "Data", "tasks.json");
        }
        public List<TaskItem> LoadTasks()
        {
            if (!File.Exists(_tasksPath))
            {
                return new List<TaskItem>();
            }
                
            var json = File.ReadAllText(_tasksPath, Encoding.UTF8);
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                var serializer = new DataContractJsonSerializer(typeof(List<TaskItem>));
                var obj = serializer.ReadObject(ms) as List<TaskItem>;
                return obj ?? new List<TaskItem>();
            }
        }
    }
}