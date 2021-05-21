using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication5.Models
{
    public class Export : IDisposable
    {

        public dynamic[] GetDummyData(int length)
        {
            dynamic[] tmp = new dynamic[length];
            // tmp never gets cleared by the garbage collector, even if its not used after the call is finished
            for (int i = 0; i < length; i++)
            {
                dynamic test = new System.Dynamic.ExpandoObject();

                test.GatewayName = GenerateString();
                test.Message = GenerateString();
                test.Status = GenerateString();
                tmp[i] = test;
            }
            return tmp;
        }
        public async Task<byte[]> test()
        {
            dynamic[] tmp = GetDummyData(5000000);
            Console.WriteLine($"Finished generating items: {DateTime.Now.ToLongTimeString()}");
            Console.WriteLine($"Generating CSV: {DateTime.Now.ToLongTimeString()}");
            // 1048574 is max, excel says 1048576 is max but because of header and seperater line it needs to be minussed with 2
            return await ExportDataAsCSV(tmp, $"Report.csv");
        }

        private string GenerateString(int length = 50)
        {
            StringBuilder str_build = new StringBuilder();
            Random random = new Random();

            char letter;

            for (int i = 0; i < length; i++)
            {
                double flt = random.NextDouble();
                int shift = Convert.ToInt32(Math.Floor(25 * flt));
                letter = Convert.ToChar(shift + 65);
                str_build.Append(letter);
            }
            return str_build.ToString();
        }

        private async Task<byte[]> ExportDataAsCSV(IEnumerable<object> listToExport, string fileName)
        {
            if (listToExport is null || !listToExport.Any())
                throw new ArgumentNullException(nameof(listToExport));

            System.IO.File.Delete("Reports/" + GenerateString(30) + fileName);
            var file = System.IO.File.Create("Reports/" + GenerateString(30) + fileName, 4096, FileOptions.DeleteOnClose);

            byte[] bytes;
            using (var streamWriter = new StreamWriter(file, Encoding.UTF8))
            {
                await streamWriter.WriteAsync("sep=;");
                await streamWriter.WriteAsync(Environment.NewLine);

                var headerNames = listToExport.First().GetType().GetProperties();
                foreach (var header in headerNames)
                {
                    var displayAttribute = header.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.DisplayAttribute), true);
                    if (displayAttribute.Length != 0)
                    {
                        var attribute = displayAttribute.Single() as System.ComponentModel.DataAnnotations.DisplayAttribute;
                        await streamWriter.WriteAsync(attribute.Name + ";");
                    }
                    else
                        await streamWriter.WriteAsync(header.Name + ";");
                }
                await streamWriter.WriteAsync(Environment.NewLine);

                var newListToExport = listToExport.ToArray();

                for (int j = 0; j < newListToExport.Length; j++)
                {
                    object item = newListToExport[j];
                    var itemProperties = item.GetType().GetProperties();
                    for (int i = 0; i < itemProperties.Length; i++)
                    {
                        await streamWriter.WriteAsync(itemProperties[i].GetValue(item)?.ToString() + ";");
                    }
                    await streamWriter.WriteAsync(Environment.NewLine);
                }



                //Helpers.LogHelper.Log(Helpers.LogHelper.LogType.Info, GetType(), $"User {User.Identity.Name} downloaded {fileName}");
                await file.FlushAsync();
                file.Position = 0;

                bytes = new byte[file.Length];
                file.Read(bytes, 0, bytes.Length);
            }
            return bytes;
            //return File(bytes, "text/csv", fileName);
        }

        public void Dispose()
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
        }
    }
}
