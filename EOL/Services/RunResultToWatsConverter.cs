using EOL.Models;
using Microsoft.Xaml.Behaviors.Media;
using Newtonsoft.Json.Linq;
using ScriptHandler.Interfaces;
using ScriptHandler.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using WatsReportModels;
using static System.Net.Mime.MediaTypeNames;

namespace EOL.Services
{
    public class RunResultToWatsConverter
    {

        public GeneratedProjectData ProjectData;

        private string Location = String.Empty;
        
        public RunResultToWatsConverter()
        {
            //Location = GetLocalIPAddress();

        }

        public void SaveRunResultToXml(Reports reports)
        {
            //string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
          string filePath = Path.Combine("C:\\ProgramData\\Virinco\\WATS\\WatsStandardXmlFormat", "ReportWats.xml");


            XmlSerializer serializer = new XmlSerializer(typeof(Reports));
            //namespaces.Add(string.Empty, "http://wats.virinco.com/schemas/WATS/Report/wsxf");

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                serializer.Serialize(writer, reports );
            }
        }

        //public async Task PostRunResultToServerAsync(Reports reports, string token)
        //{
        //    string url = "https://irpsystems.wats.com/api/report/wsxf";
        //    XmlSerializer serializer = new XmlSerializer(typeof(Reports));

        //    using (var stringWriter = new StringWriter())
        //    {
        //        serializer.Serialize(stringWriter, reports);
        //        string xmlContent = stringWriter.ToString();
        //        using (var httpClient = new HttpClient())
        //        {
        //            // Add the token to the Authorization header
        //            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", token);

        //            var content = new StringContent(xmlContent, Encoding.UTF8, "application/xml");
        //            HttpResponseMessage response = await httpClient.PostAsync(url, content);

        //            if (response.IsSuccessStatusCode)
        //            {
        //                MessageBox.Show("Data posted successfully.");
        //            }
        //            else
        //            {
        //                string responseContent = await response.Content.ReadAsStringAsync();
        //                MessageBox.Show($"Failed to post data. Status code: {response.StatusCode}\nResponse: {responseContent}");
        //            }
        //        }
        //    }
        //}




        private static string GetLocalIPAddress()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet && ni.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            return ip.Address.ToString();
                        }
                    }
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }


    }
}
