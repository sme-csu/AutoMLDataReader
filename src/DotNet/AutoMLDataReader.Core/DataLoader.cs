using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutoMLDataReader.Core
{
    public class DataLoader
    {

        public async Task<DataTable> LoadData(string csvPath,string endpointUrl,string key,int batchSize=200, IProgress<float> progress=null)
        {
            var dt=loadCSV(csvPath);
            
            EndpointClient client = new EndpointClient(endpointUrl);
            client.APIKey = key;
            
            DataTable result = new DataTable();
            int count = dt.Rows.Count;
            int processed = 0;
            while (count>0)
            {
                int batchStart = dt.Rows.Count - count;
                int size = Math.Min(batchSize, dt.Rows.Count - processed);
                var x = genDictFromDataTable(dt,batchStart,size);
                var s = await client.InvokeRequestResponseService(x);
                processResult(result, s);
                processed += size;
                count -= size;
                progress?.Report((float)processed / (float)dt.Rows.Count);
            }
            
            return result;
        }

        private void processResult(DataTable table, string responseText)
        {
            string r1 = JsonConvert.DeserializeObject<string>(responseText);
            var tmp = JsonConvert.DeserializeObject<AMLPredictResult>(r1);
            if (table.Columns.Count==0)//first batch, need to init columns
            {
                foreach (var item in tmp.index[0])
                {
                    table.Columns.Add(item.Key, item.Value is long ? typeof(DateTime) : typeof(string));
                }
                table.Columns.Add("predict", typeof(double));
            }
            for (int i = 0; i < tmp.forecast.Count; i++)
            {
                DataRow row = table.NewRow();
                foreach (var item in tmp.index[i])
                {
                    if (item.Value is long)
                    {
                        row[item.Key] = DateTime.UnixEpoch.AddMilliseconds((long)item.Value);
                    }
                    else
                    {
                        row[item.Key] = item.Value.ToString();
                    }
                }
                row["predict"] = tmp.forecast[i];
                table.Rows.Add(row);
            }

        }

        private IEnumerable<Dictionary<string,string>> genDictFromDataTable(DataTable dt,int start=0, int count=0)
        {
            if (count==0)
            {
                count = dt.Rows.Count - start;
            }

            foreach (DataRow row in dt.AsEnumerable().Skip(start).Take(count))
            {
                Dictionary<string, string> item = new Dictionary<string, string>();
                foreach (DataColumn c in dt.Columns)
                {
                    item.Add(c.ColumnName, row.Field<string>(c));
                }
                yield return item;
            }
        }

        private DataTable loadCSV(string csvPath)
        {
            using (var reader = new StreamReader(csvPath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                // Do any configuration to `CsvReader` before creating CsvDataReader.
                using (var dr = new CsvDataReader(csv))
                {
                    var dt = new DataTable();
                    dt.Load(dr);
                    return dt;
                }
            }
        }

        public static void SaveDatatableToCSV(Stream fileStream,DataTable table)
        {
            using StreamWriter writer = new StreamWriter(fileStream,Encoding.UTF8,1024*1024*10 );//10MB buffer size
            var headers = from DataColumn header in table.Columns
                          select header.ColumnName;
            writer.WriteLine(string.Join(',', headers));

            foreach (DataRow row in table.Rows)
            {
                writer.WriteLine(string.Join(',', row.ItemArray));
            }
            writer.Flush();
        }
    }
}
