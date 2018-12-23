using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPGPExplorer
{
    public static class TempFiles
    {
        private static List<string> _TempFiles = new List<string>();

        public static string GetNewTempFile()
        {
            string NewTempFile = Path.GetTempPath() + Guid.NewGuid().ToString() + ".gpg.bin";
            _TempFiles.Add(NewTempFile);
            return NewTempFile;
        }

        public static void Clear()
        {
            try
            {
                foreach (string s in _TempFiles)
                    File.Delete(s);
            }
            catch { }
        }

    }
}
