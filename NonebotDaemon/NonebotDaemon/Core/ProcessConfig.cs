using System.ComponentModel;
using System.Reflection;
using System.Xml;
using log4net;

namespace NonebotDaemon.Core
{
    public class ProcessConfig
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProcessConfig));
        
        private static void AppendElement(XmlDocument doc, XmlNode root, string name, string value, string description)
        {
            var element = doc.CreateElement(name);
            element.InnerText = value;
            
            foreach (var line in description.Split('\n'))
            {
                var comment = doc.CreateComment(line);
                root.AppendChild(comment);
            }

            root.AppendChild(element);
        }
        
        public static bool ParseConfig()
        {
            try
            {
                if (!File.Exists(Program.ConfigFilePath))
                {
                    Log.Error("Config file not exist.");
                    File.WriteAllText(Program.ConfigFilePath, string.Empty);
                    
                    SaveToXml(Program.ConfigFilePath);
                }

                LoadFromXml(Program.ConfigFilePath);
                
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message + ex.StackTrace);
                return false;
            }
        }
        
        private static void LoadFromXml(string filePath)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(filePath);

                XmlNode root = doc.DocumentElement;

                foreach (var prop in typeof(GlobalVariables).GetProperties())
                {
                    if (prop.GetCustomAttribute<IgnoreSettingAttribute>() != null)
                    {
                        Log.Debug($"Property '{prop.Name}' is marked with IgnoreSettingAttribute. Skipping.");
                        continue;
                    }
                    
                    var node = root.SelectSingleNode(prop.Name);
                    if (node != null)
                    {
                        var value = node.InnerText;
                        if (prop.PropertyType == typeof(ushort[]))
                        {
                            prop.SetValue(null, value.Split(',').Select(ushort.Parse).ToArray());
                        }
                        else if (prop.PropertyType == typeof(bool))
                        {
                            prop.SetValue(null, bool.Parse(value));
                        }
                        else if (prop.PropertyType == typeof(ushort))
                        {
                            prop.SetValue(null, ushort.Parse(value));
                        }
                        else
                        {
                            prop.SetValue(null, value);
                        }
                    }
                    else
                    {
                        Log.Debug($"Node '{prop.Name}' not found in XML.");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error loading settings from XML: {ex.Message}");
            }
        }
        
        private static void SaveToXml(string filePath)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement root = doc.CreateElement("Settings");
            doc.AppendChild(root);

            foreach (var prop in typeof(GlobalVariables).GetProperties())
            {
                var value = prop.GetValue(null);
                var descriptionAttr = (DescriptionAttribute)prop.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault();
                var description = descriptionAttr?.Description ?? string.Empty;
                
                if (prop.GetCustomAttribute<IgnoreSettingAttribute>() != null)
                {
                    Log.Debug($"Property '{prop.Name}' is marked with IgnoreSettingAttribute. Skipping.");
                    continue;
                }

                if (prop.PropertyType == typeof(ushort[]))
                {
                    AppendElement(doc, root, prop.Name, string.Join(",", (ushort[])value), description);
                }
                else
                {
                    AppendElement(doc, root, prop.Name, value.ToString(), description);
                }
            }

            doc.Save(filePath);
        }
    }
    
    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreSettingAttribute : Attribute
    {
    }
}