using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorCore
{
    public static class MonitorResultUtility
    {
        public static MonitorModifyResult GetModify (this MonitorResult oldResult, MonitorResult newResult)
        {
            MonitorModifyResult result = new MonitorModifyResult ();

            var oldDatas = oldResult.monitorDatas.ToList ();

            var newDatas = newResult.monitorDatas.ToList ();

            oldDatas.ToList ().ForEach (oldData=> 
            {
                var findMapping = newDatas.Find (data => data.appName == oldData.appName);

                if (findMapping != null) 
                {
                    if (findMapping.pids.SequenceEqual (oldData.pids) == false) 
                    {
                        result.beModifys.Add (oldData.appName);
                    }

                    newDatas.Remove (findMapping);
                }
                else
                {
                    result.beRemoves.Add (oldData.appName);
                }
            });

            result.beAdds = newDatas.ConvertAll (data => data.appName);

            return result;
        }
    }

    [Serializable]
    public class MonitorModifyResult 
    {
        //只保留app名稱
        public List<string> beRemoves = new List<string> ();
        public List<string> beAdds = new List<string> ();
        public List<string> beModifys = new List<string> ();

        public bool HasValue => beRemoves.Count > 0 || beAdds.Count > 0 || beModifys.Count > 0;
    }
}
