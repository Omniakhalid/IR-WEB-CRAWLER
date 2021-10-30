using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.IO;
using mshtml;
using System.Data;
using System.Data.SqlClient;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace IR_Task_1
{
    class Program
    {
        static Queue<string> visited;
        static Queue<string> links;
        static Queue<string> stored_db;
        static Queue<string> contents;

        static void Main(string[] args)
        {
            visited = new Queue<string>();
            links = new Queue<string>();
            stored_db = new Queue<string>();
            contents = new Queue<string>();

            WebRequest myWebRequest;
            WebResponse myWebResponse;
            string URL = "http://www.bbc.com/";

            links.Enqueue(URL);
            try
            {
                for (int i = 0; i < links.Count(); i++)
                {

                    string l = links.Dequeue();
                    //visited.Enqueue(l);
                    myWebRequest = WebRequest.Create(l);
                    myWebResponse = myWebRequest.GetResponse();//Returns a response from an Internet resource

                    Stream streamResponse = myWebResponse.GetResponseStream();//return the data stream from the internet                                                                          //and save it in the stream
                    StreamReader sreader = new StreamReader(streamResponse);//reads the data stream
                    String Rstring = sreader.ReadToEnd();//reads it to the end
                    GetUrls(Rstring);

                    streamResponse.Close();
                    sreader.Close();
                    myWebResponse.Close();

                }
                Console.ReadLine();




            }
            catch (WebException ex)
            {
                Console.WriteLine("Exception is : " + ex.Message);
            }

        }
        //////////////////////////////////////////////////////////////////////////////////

        private static void GetUrls(string Rstring)
        {
            HTMLDocument d = new HTMLDocument();
            IHTMLDocument2 doc = (IHTMLDocument2)d;

            var mdoc = new HtmlDocument();

            doc.write(Rstring);
            IHTMLElementCollection L = doc.links;

            foreach (IHTMLElement link in L)
            {
                string url = link.getAttribute("href", 0);
                string CheckLang = CheckLanguage(Rstring);

                if (visited.Count() > 3001)
                { goto end_loop; }

                // url not visited in queue
                if (RemoteFileExists(url) && (!visited.Contains(url)))
                {
                    if ((CheckLang == "continue" || CheckLang == "NOTEXIST") && Rstring != string.Empty)
                    {
                        Console.WriteLine(url);
                        contents.Enqueue(Rstring.ToString());

                        //Element-Tag-Name[@Attribute-Name='Value-Desired']

                        //string txt = mdoc.DocumentNode.SelectNodes("//body")[0].InnerText;
                        //Console.WriteLine(txt);

                        stored_db.Enqueue(url);
                        visited.Enqueue(url);
                        //SqlConn();
                        links.Enqueue(url);
                    }
                    else
                    {
                        Console.WriteLine("Sorry web Page "+ url+" is not english");
                    }
                }
            }
            end_loop:;
        }
        //////////////////////////////////////////////////////////////////////////////////

        private static string CheckLanguage(string UrlContent)
        {
            var doc = new HtmlDocument();
            //Convert string to HtmlDocument
            doc.LoadHtml(UrlContent);
            bool isEnglish = false;
            try
            {
                HtmlNode lang = doc.DocumentNode.SelectSingleNode("(//html[@lang])[0]");
                isEnglish = Regex.IsMatch(UrlContent, @"^[a-zA-Z1-9\s]+$");

                if (lang == null)
                {
                    throw new Exception("Oops! it is null");
                }
                else if (lang.Attributes["lang"].Value.ToLower().Contains("en") || !isEnglish)
                {
                    return "continue";
                }
                return "stop";
            }
            catch (Exception ex)
            {
                return "NOTEXIST";
            }
        }
        ////////////////////////////////////////////////////////////////////////////////

        private static bool RemoteFileExists(string url)
        {
            try
            {
                //Creating the HttpWebRequest
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                //Setting the Request method HEAD, you can also use GET too.
                request.Method = "HEAD";
                //Getting the Web Response.
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                //Returns TRUE if the Status code == 200
                response.Close();
                return (response.StatusCode == HttpStatusCode.OK);
            }
            catch
            {
                //Any exception will returns false.
                return false;
            }
        }
        ///////////////////////////////////////////////////////////////////////////////
        private static void SqlConn()
        {
            string connetionString = null;
            //string sql = null;
            SqlCommand command;
            SqlConnection cnn;

            connetionString = "Data Source=DESKTOP-P84ETB9\\SQLEXPRESS;" +
                "Initial Catalog=IrProject;" +
                "Integrated Security=true";

            using (cnn = new SqlConnection(connetionString))
            {
                cnn.Open();
                //sql = "INSERT INTO links (url,content) VALUES (@url,@content)";
                command = new SqlCommand("INSERT INTO links (url,content) VALUES (@url,@content)"
                    , cnn);
                command.Parameters.AddWithValue("@url", stored_db.Dequeue());
                command.Parameters.AddWithValue("@content", contents.Dequeue());

                int x = command.ExecuteNonQuery();
                if (x < 0)
                {
                    System.Console.WriteLine("Error inserting data into Database!");
                    cnn.Close();
                }
            }
            //System.Console.WriteLine("SUCCESSFULLY insert data into Database!");
            cnn.Close();
        }
        ////////////////////////////////////////////////////////////////////////////

    }

}

