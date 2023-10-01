using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CycleLauncher
{
    [Serializable]
    public class Config
    {
        public List<string> paths = new List<string> ();
        public int rate;
    }
}
