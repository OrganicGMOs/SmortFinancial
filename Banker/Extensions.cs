using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banker
{
    internal static class Extensions
    {
        public static async Task WriteFile(string path, string content)
        {
            try
            {
                await File.WriteAllTextAsync(path, content);
            }
            catch (Exception ex)
            {
                //todo: exception handling
                Console.WriteLine(ex.Message);
            }
        }
        public static async Task<string> ReadFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    var s = string.Empty;
                    s = await File.ReadAllTextAsync(path);
                    if (s.Length > 0)
                        return s;
                    else
                    {
                        //todo: expcetion handling
                        return String.Empty;
                    }
                }
                else
                    return String.Empty;
                
            }
            catch (Exception ex)
            {
                //todo: expcetion handling
                return String.Empty;
            }
        }
        public static T? ParseJson<T>(string json)
        {
            try
            {
                if (json.Length > 0)
                    return JsonConvert.DeserializeObject<T>(json);
                else
                    return default(T);
            }
            catch (Exception ex)
            {
                //todo: exception and logging
                return default(T);
            }
        }
    }
}
