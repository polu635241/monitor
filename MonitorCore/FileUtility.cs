using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorCore
{
    public class FileUtility
    {
        static FileUtility ()
        {
            CurrentDriectory = Directory.GetCurrentDirectory ();
        }

        public static string CurrentDriectory { get; private set; }
    }
}
