using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication5.Models
{
    public class Export : IDisposable
    {
        //List<List<object>> dataObjects = new();
        public string Id;

        public Export()
        {
            Id = GenerateString();
        }

        public List<object> GetDummyData(int length)
        {
            var data = new List<object>();
            // tmp never gets cleared by the garbage collector, even if its not used after the call is finished
            for (int i = 0; i < length; i++)
            {
                dynamic @object = new ExpandoObject();
                @object.GatewayName = GenerateString();
                @object.Message = GenerateString();
                @object.Status = GenerateString();
                data.Add(@object);

                if (i % 500000 == 0)
                {
                    Console.WriteLine(i);
                }
            }
            return data;
        }

        public byte[] Test(int amount)
        {
            List<object> data = GetDummyData(amount);
            Console.WriteLine($"Finished generating items: {DateTime.Now.ToLongTimeString()}");
            Console.WriteLine($"Generating CSV: {DateTime.Now.ToLongTimeString()}");
            // 1048574 is max, excel says 1048576 is max but because of header and seperater line it needs to be minussed with 2
            ExportDataAsCSV(data, $"wwwroot/reports/{Id}.csv");
            return null;
        }

        private readonly Random random = new ();
        private string GenerateString(int length = 50)
        {
            StringBuilder str_build = new();

            char letter;

            for (int i = 0; i < length; i++)
            {
                double flt = random.NextDouble();
                int shift = Convert.ToInt32(Math.Floor(25 * flt));
                letter = Convert.ToChar(shift + 65);
                str_build.Append(letter);
            }
            var str = str_build.ToString();
            str_build.Clear();
            return str;
        }

        private static void ExportDataAsCSV(IEnumerable<object> listToExport, string fileName)
        {
            if (listToExport is null || !listToExport.Any())
                throw new ArgumentNullException(nameof(listToExport));

            using var file = File.Open(fileName, FileMode.Append, FileAccess.Write, FileShare.Write);
            using var streamWriter = new StreamWriter(file, Encoding.UTF8);
            if (file.Length == 0)
            {
                Console.WriteLine("Adding seperator");
                streamWriter.Write("sep=;");
                streamWriter.Write(Environment.NewLine);
            }

            var headerNames = listToExport.First().GetType().GetProperties();
            foreach (var header in headerNames)
            {
                var displayAttribute = header.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.DisplayAttribute), true);
                if (displayAttribute.Length != 0)
                {
                    var attribute = displayAttribute.Single() as System.ComponentModel.DataAnnotations.DisplayAttribute;
                    streamWriter.Write(attribute.Name + ";");
                }
                else
                    streamWriter.Write(header.Name + ";");
            }
            streamWriter.Write(Environment.NewLine);
            var j = 0;
            foreach (var item in listToExport)
            {
                var itemProperties = item.GetType().GetProperties();
                for (int i = 0; i < itemProperties.Length; i++)
                {
                    var a = itemProperties[i].GetValue(item);
                    if (a != null)
                        streamWriter.Write(a + ";");
                    else
                        streamWriter.Write(";");
                }
                streamWriter.Write(Environment.NewLine);
                j++;
                if (j % 500000 == 0)
                    Console.WriteLine(j);
            }

            //Helpers.LogHelper.Log(Helpers.LogHelper.LogType.Info, GetType(), $"User {User.Identity.Name} downloaded {fileName}");
            streamWriter.Flush();
            file.Flush();

            streamWriter.Close();
            file.Close();
        }

        void IDisposable.Dispose()
        {
            GC.SuppressFinalize(this);
            Cleanup();
        }


        public void Cleanup()
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
        }
    }
}
