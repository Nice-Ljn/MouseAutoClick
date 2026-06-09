using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using MouseRecorderWpf.Models;

namespace MouseRecorderWpf.Services
{
    public class DataStorageService
    {
        private static readonly string AppDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MouseRecorderWpf");

        private static readonly string ScriptsFolder = Path.Combine(AppDataFolder, "Scripts");
        private static readonly string ConfigPath = Path.Combine(AppDataFolder, "config.json");

        static DataStorageService()
        {
            if (!Directory.Exists(AppDataFolder))
            {
                Directory.CreateDirectory(AppDataFolder);
            }
            if (!Directory.Exists(ScriptsFolder))
            {
                Directory.CreateDirectory(ScriptsFolder);
            }
        }

        public static List<Script> LoadAllScripts()
        {
            var scripts = new List<Script>();
            try
            {
                var files = Directory.GetFiles(ScriptsFolder, "*.json");
                foreach (var file in files)
                {
                    try
                    {
                        string json = File.ReadAllText(file);
                        var script = JsonSerializer.Deserialize<Script>(json);
                        if (script != null)
                        {
                            scripts.Add(script);
                        }
                    }
                    catch
                    {
                    }
                }
                scripts.Sort((a, b) => b.CreatedAt.CompareTo(a.CreatedAt));
            }
            catch
            {
            }
            return scripts;
        }

        public static void SaveScript(Script script)
        {
            try
            {
                var fileName = $"{script.CreatedAt:yyyyMMdd_HHmmss}_{script.Name.Replace(' ', '_')}.json";
                var filePath = Path.Combine(ScriptsFolder, fileName);
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string json = JsonSerializer.Serialize(script, options);
                File.WriteAllText(filePath, json);
            }
            catch
            {
            }
        }

        public static void DeleteScript(Script script)
        {
            try
            {
                var files = Directory.GetFiles(ScriptsFolder, "*.json");
                foreach (var file in files)
                {
                    try
                    {
                        string json = File.ReadAllText(file);
                        var loadedScript = JsonSerializer.Deserialize<Script>(json);
                        if (loadedScript != null &&
                            loadedScript.CreatedAt == script.CreatedAt &&
                            loadedScript.Name == script.Name)
                        {
                            File.Delete(file);
                            break;
                        }
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }
        }

        public static string GetActiveScriptName()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    var json = File.ReadAllText(ConfigPath);
                    var config = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                    if (config != null && config.TryGetValue("ActiveScriptName", out var name))
                    {
                        return name;
                    }
                }
            }
            catch
            {
            }
            return string.Empty;
        }

        public static void SaveActiveScriptName(string? name)
        {
            try
            {
                var config = new Dictionary<string, string>
                {
                    ["ActiveScriptName"] = name ?? string.Empty
                };
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string json = JsonSerializer.Serialize(config, options);
                File.WriteAllText(ConfigPath, json);
            }
            catch
            {
            }
        }

        public static void ExportScript(Script script, string filePath)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string json = JsonSerializer.Serialize(script, options);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"导出失败: {ex.Message}");
            }
        }

        public static Script? ImportScript(string filePath)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                var script = JsonSerializer.Deserialize<Script>(json);
                if (script != null)
                {
                    // 检查是否有重复的脚本，如果有则修改名称
                    var existingScripts = LoadAllScripts();
                    int duplicateCount = 0;
                    var originalName = script.Name;
                    while (existingScripts.Any(s => s.CreatedAt == script.CreatedAt && s.Name == script.Name))
                    {
                        duplicateCount++;
                        script.Name = $"{originalName} ({duplicateCount})";
                    }
                    SaveScript(script);
                }
                return script;
            }
            catch (Exception ex)
            {
                throw new Exception($"导入失败: {ex.Message}");
            }
        }
    }
}
