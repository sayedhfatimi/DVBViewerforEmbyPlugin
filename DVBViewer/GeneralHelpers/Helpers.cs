using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DVBViewer.TunerHost;
using MediaBrowser.Model.Serialization;

namespace DVBViewer.GeneralHelpers
{
    public static class Helpers
    {
        public static IEnumerable<Type> GetTypesInNamespace(Assembly assembly, string nameSpace)
        {
            return assembly.GetTypes().Where(t => String.Equals(t.Namespace, nameSpace, StringComparison.Ordinal) && typeof(ITunerHost).IsAssignableFrom(t) && t.IsPublic);
        }

        public static void CreateFileCopy(object obj, string filePath, IJsonSerializer serializer)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            if (obj != null)
            {
                serializer.SerializeToFile(obj, filePath);
            }
        }

        public static void CreateFileCopy(object obj, string filePath, IXmlSerializer serializer)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            if (obj != null)
            {
                serializer.SerializeToFile(obj, filePath);
            }
        }

        public static void DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public static DateTime convertToDateTime(string s)
        {
            char[] c = s.ToCharArray();

            string y = new string(c, 0, 4);
            string m = new string(c, 4, 2);
            string d = new string(c, 6, 2);
            string h = new string(c, 8, 2);
            string mi = new string(c, 10, 2);
            string se = new string(c, 12, 2);

            return DateTime.Parse(y + "/" + m + "/" + d + " " + h + ":" + mi + ":" + se);
        }
    }
}