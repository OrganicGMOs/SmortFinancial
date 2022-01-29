using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banker.Extensions
{
    internal static class Functions
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
        /// <summary>
        /// Attempts to asyn read a file at a given path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns>
        ///     string - file contents <br />
        ///     empty string - couldnt read file <br />
        ///     null - file doesnt exist
        /// </returns>
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
                    return null;

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
