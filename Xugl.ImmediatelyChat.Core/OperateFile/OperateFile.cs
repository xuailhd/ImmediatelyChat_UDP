using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xugl.ImmediatelyChat.Core
{
    public class OperateFile:IOperateFile
    {
        public void SaveConfig(string fileName, string fieldName, string fieldValue)
        {
            try
            {
                if (!File.Exists(fileName))
                {
                    File.Create(fileName).Close();
                }

                string templine;
                string tempkey;
                string tempvalue;
                IDictionary<string, string> fieldNames = new Dictionary<string, string>();

                using (StreamReader rd = new StreamReader(fileName, Encoding.UTF8))
                {
                    while (!string.IsNullOrEmpty(templine = rd.ReadLine()))
                    {
                        tempkey = templine.Substring(0, templine.IndexOf("="));
                        if (tempkey.ToUpper() == fieldName.ToUpper())
                        {
                            tempvalue = fieldValue;
                        }
                        else
                        {
                            tempvalue = templine.Remove(0, tempkey.Length + 1);
                        }
                        fieldNames.Add(tempkey.ToUpper(), tempvalue);
                    }
                }

                if (!fieldNames.Keys.Contains(fieldName.ToUpper()))
                {
                    fieldNames.Add(fieldName.ToUpper(), fieldValue);
                }

                using (StreamWriter wr = new StreamWriter(fileName, false))
                {
                    foreach (string key in fieldNames.Keys)
                    {

                        wr.WriteLine(key + "=" + fieldNames[key]);
                    }
                }

            }
            catch (Exception ex)
            {

            }
        }


        public string GetConfig(string fileName, string fieldName)
        {
            if (!File.Exists(fileName))
            {
                return string.Empty;
            }

            string templine;
            string tempkey;
            string tempvalue = string.Empty;

            using (StreamReader rd = new StreamReader(fileName, Encoding.UTF8))
            {
                while (!string.IsNullOrEmpty(templine = rd.ReadLine()))
                {
                    tempkey = templine.Substring(0, templine.IndexOf("="));
                    if (tempkey.ToUpper() == fieldName.ToUpper())
                    {
                        tempvalue = templine.Remove(0, tempkey.Length + 1);
                        break;
                    }
                }
            }
            return tempvalue;
        }
    }
}
