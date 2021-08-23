﻿using HtmlAgilityPack;
using MailKit.Net.Pop3;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;

namespace WhiteListEmail
{
    public class WhiteListing
    {
        private readonly Timer _timer;

        public void Start()
        {
          
        }

        public void Stop()
        {
            
        }

        public WhiteListing()
        {
            const string hostelastic = "http://172.17.1.88:9200";
            string hostName = "172.17.1.41";
            int port = 110;
            bool useSsl = false;
            string userName = "publish@emmares";
            string password = "publish123!";
            using (var client = new Pop3Client())
            {
                client.Connect(hostName, port, useSsl);
                client.Authenticate(userName, password);
                for (int i = 4; i < client.Count; i++)
                {
                    var uID = client.GetMessageUid(i);
                    var message = client.GetMessage(i);
                    var subject = message.Subject;
                    var addrfrom = message.From;
                    //  MessageBox.Show(message.TextBody.Trim());
                    var excerpt = "";
                    if (message.TextBody == null)
                    {
                        excerpt = message.HtmlBody.Trim();
                    }
                    else
                    {
                        excerpt = message.TextBody.Trim();
                    }
                    var cid = "";
                    var match = Regex.Match(excerpt, "Emmarescid###(.*)###");
                    cid = match.Groups[1].Value; // Campaign ID
                    excerpt = Regex.Replace(excerpt, @"\r\n?|\n", " ");
                    excerpt = Regex.Replace(excerpt, "&#x10C;", "Č");
                    excerpt = Regex.Replace(excerpt, "&zwnj;", "");


                    /*string pattern0 = "(?<= style)(.*)(?= style)";
                    Regex rgx0 = new Regex(pattern0);
                    excerpt = rgx0.Replace(excerpt, "");*/
                    string pattern0s = "<script(?:\r|\n|.)+</script>";
                    Regex rgx0s = new Regex(pattern0s);
                    excerpt = rgx0s.Replace(excerpt, "");
                    string pattern0 = "<style(?:\r|\n|.)+</style>";
                    Regex rgx0 = new Regex(pattern0);
                    excerpt = rgx0.Replace(excerpt, "");
                    string pattern = "<[^<>]+>";
                    //  string pattern = "<[^>]*> ";
                    Regex rgx = new Regex(pattern);
                    excerpt = rgx.Replace(excerpt, "");
                    string pattern2 = "{[^{}]+}";
                    Regex rgx2 = new Regex(pattern2);
                    excerpt = rgx2.Replace(excerpt, "");
                    excerpt = rgx2.Replace(excerpt, "");
                    string pattern3 = "#.*? ";
                    Regex rgx3 = new Regex(pattern3);
                    excerpt = rgx3.Replace(excerpt, "");
                    /* string pattern4 = "\\..*? ";
                     Regex rgx4 = new Regex(pattern4);
                     excerpt = rgx4.Replace(excerpt, "");*/

                    excerpt = Regex.Replace(excerpt, "<", " ");
                    excerpt = Regex.Replace(excerpt, ">", " ");
                    excerpt = Regex.Replace(excerpt, "/", " ");

                    // excerpt = Regex.Replace(excerpt, "\\\\<*>", " ");
                    // excerpt = Regex.Replace(excerpt, ">", " ");
                    excerpt = Regex.Replace(excerpt, "\"", "'");


                    excerpt = Regex.Replace(excerpt, @"\t", " ");
                    excerpt = Regex.Replace(excerpt, @"\r", " ");
                    excerpt = Regex.Replace(excerpt, @"\s+", " ");

                    subject = Regex.Replace(subject, "\"", "'");
                    subject = Regex.Replace(subject, @"\t", " ");


                    float score = 0.0F;
                    var messageId = message.MessageId;
                    var preview = message.TextBody;
                    var campaignname = "nocampaignname";
                    var descriptionofcampaign = "nodescriptionofcampaign";
                    var publisher = "nopublisher";
                    var fieldofinterests = "fieldofinterests";
                    var region = "noregion";
                    var contenttype = "nocontenttype";
                    var optin = "";
                    var optout = "";
                    var affiliatelink = "";
                    var enddate = message.To.ToString();

                    string[] enddate1 = enddate.Split(new string[] { "-enddate-" }, StringSplitOptions.None);  //emmares-enddate-2019-06-06@emmares.com
                    string[] enddate2 = enddate1.Length > 1 ? enddate1[1].Split(new string[] { "@emmares" }, StringSplitOptions.None) : new string[] { DateTime.Today.AddYears(2).ToString("yyyy-MM-dd") };
                    //enddate = 2 leti od dneva vpisa
                    enddate = enddate2[0]; //"2019-12-09";

                    string content = !string.IsNullOrEmpty(message.HtmlBody) ? message.HtmlBody : message.TextBody;

                    //addr "n" <n@n.com> -> n@n.com
                    //
                    string addrfrom2 = addrfrom.ToString();
                    if (addrfrom2.Contains("<"))
                    {
                        string[] addrsplit = addrfrom2.Split('<');
                        addrfrom2 = addrsplit[1];
                        addrfrom2 = addrfrom2.Replace(">", "");
                    }

                    if (cid != "")
                    {
                        WebClient savetodb = new WebClient();
                        try
                        {
                            var addressuri = "https://emmares.com/SearchAPI/SaveToDB?";
                            var datacidaddr = "CampaignID=" + cid + "&Sender_Email=" + addrfrom2;
                            //string method = "POST";
                            savetodb.DownloadString(addressuri + datacidaddr);
                        }
                        catch (Exception ex)
                        {
                            if (!ex.Message.ToLower().Contains("violation")) { }

                        }
                    }


                    //
                    string mailelastic = "";
                    /*WebClient wc = new WebClient();
                     try
                      { mailelastic = wc.DownloadString(hostelastic + "/blacklist/_search?q=" + addrfrom.ToString() + "&filter_path=hits.hits._source.query.term.email"); } 
                      catch { mailelastic = "Do not use \", (, ), : and other special characters"; }*/
                    string json = "{\"query\": {\"term\": {\"email\": \"" + addrfrom2 + "\" }}}";
                    WebClient wc2 = new WebClient();
                    wc2.Headers.Add("Content-Type", "application/json");
                    try
                    {
                        mailelastic = wc2.UploadString(hostelastic + "/blacklist/_search?", json);
                    }
                    catch
                    {
                        mailelastic = "Error";
                    }

                    string mailelastic2 = "";
                    string jsonw = "{\"query\": {\"term\": {\"email\": \"" + addrfrom2 + "\" }}}";
                    WebClient wc3 = new WebClient();
                    wc3.Headers.Add("Content-Type", "application/json");
                    try
                    {
                        mailelastic2 = wc3.UploadString(hostelastic + "/whitelist/_search?", json);
                    }
                    catch
                    {
                        mailelastic2 = "Error";
                    }

                    try
                    {
                        var xblacklist = Newtonsoft.Json.JsonConvert.DeserializeObject(mailelastic);

                        var emailclass = new Emailclass();
                        emailclass = JsonConvert.DeserializeObject<Emailclass>(mailelastic);

                        if (emailclass.Hits.Total != 0)
                        {
                            // MessageBox.Show("ta mail je na blacklisti " + emailclass.Hits.HitsHits[0]?.Source.Email.ToString()); //.Query.Term.Email.ToString());
                            DeleteMessageByUID(uID);
                        }
                        else
                        {
                            //  MessageBox.Show(Regex.Replace(excerpt, @"\r\n?|\n", " "));
                            try
                            {
                                var xwhitelist = Newtonsoft.Json.JsonConvert.DeserializeObject(mailelastic2);
                                var emailclass2 = new Emailclass();
                                emailclass2 = JsonConvert.DeserializeObject<Emailclass>(mailelastic2);
                                string todaysdate = DateTime.Today.ToString("yyyy-MM-dd");
                                var htmlDoc = new HtmlDocument();
                                htmlDoc.LoadHtml(content);
                                var htmlNodes = htmlDoc.DocumentNode.SelectNodes("//a");
                                if (htmlNodes != null)
                                {
                                    for (int i2 = htmlNodes.Count - 1; i2 >= 0; --i2)
                                    {
                                        if (htmlNodes[i2].InnerHtml.ToLower().Contains("unsubscribe") ||
                                            htmlNodes[i2].InnerHtml.ToLower().Contains("opt out") ||
                                            htmlNodes[i2].InnerHtml.ToLower().Contains("subscription") ||
                                            htmlNodes[i2].InnerHtml.ToLower().Contains("naročnine") ||
                                            htmlNodes[i2].InnerHtml.ToLower().Contains("odjavi") ||
                                            htmlNodes[i2].InnerHtml.ToLower().Contains("subscriber options") ||
                                            htmlNodes[i2].InnerHtml.ToLower().Contains("uredi profil") ||
                                            htmlNodes[i2].InnerHtml.ToLower().Contains("manage email preferences") ||
                                            htmlNodes[i2].InnerHtml.ToLower().Contains("posodobi želje"))
                                        {
                                            htmlNodes[i2].InnerHtml = " ";
                                        }
                                    }
                                }
                                using (StringWriter writer = new StringWriter())
                                {
                                    htmlDoc.Save(writer);
                                    content = writer.ToString();
                               
                                }

                                Regex r5 = new Regex(@"(?i)unsubscribe.*?</a>");
                                content = r5.Replace(content, " ");
                                Regex r6 = new Regex(@"(?i)opt out.*?</a>");
                                content = r6.Replace(content, " ");
                                Regex r7 = new Regex(@"(?i)subscription.*?</a>");
                                content = r7.Replace(content, " ");
                                Regex r8 = new Regex(@"(?i)odjavi.*?</a>");
                                content = r8.Replace(content, " ");
                                Regex r11 = new Regex(@"publish.*?@emmares.net");
                                content = r11.Replace(content, "");
                                Regex r12 = new Regex(@"(?i)uredi profil.*?</a>");
                                content = r12.Replace(content, " ");
                                Regex r13 = new Regex(@"(?i)posodobi želje .*?</a>");
                                content = r13.Replace(content, " ");
                                Regex r14 = new Regex(@"(?i)manage email preferences.*?</a>");
                                content = r14.Replace(content, " ");
                                if (!addrfrom.ToString().Contains("@emmares"))
                                {
                                    Regex r9 = new Regex(@"Emmares Emmares");
                                    content = r9.Replace(content, "Reader");
                                    Regex r10 = new Regex(@"Emmares");
                                    content = r10.Replace(content, "Reader");
                                }

                                if (emailclass2.Hits.Total != 0 && emailclass2.Hits.HitsHits[0].Source?.Publish == "true")
                                {
                                    // MessageBox.Show("ta mail je na whitelisti " + emailclass2.Hits.HitsHits[0]?.Source.Email.ToString()); //.Query.Term.Email.ToString());
                                    //upload to elasticsearch
                                    if (emailclass2.Hits.HitsHits[0].Source?.Duration != null)
                                        enddate = DateTime.Today.AddDays(Convert.ToDouble(emailclass2.Hits.HitsHits[0].Source?.Duration)).ToString("yyyy-MM-dd");
                                    //MessageBox.Show("main enddate " + enddate);
                                    // MessageBox.Show(emailclass2.Hits.HitsHits[0].Source.Email + " o " + emailclass2.Hits.HitsHits[0].Source.Optin + " p " + emailclass2.Hits.HitsHits[0].Source.Publish);
                                    string jsonbody = "{ \"subject\" : \"" + subject + "\", \"addrfrom\" : \"" + addrfrom2 + "\", \"excerpt\" : \"" + excerpt + "\", \"score\" : \"0.0\", \"messageid\" : \"" + messageId + "\", \"preview\" : \"!!!preview!!!\", \"campaignname\" : \"Campaign name\", \"descriptionofcampaign\" : \"Description of campaign\", \"publisher\" : \"publisher1\", \"fieldofinterest\" : \"News\", \"region\" : \"Europe\", \"contenttype\" : \"Newsletter\", \"optin\" : \"" + emailclass2.Hits.HitsHits[0]?.Source.Optin + "\", \"optout\" : \"" + emailclass2.Hits.HitsHits[0]?.Source.Optout + "\", \"affiliatelink\" : \"" + emailclass2.Hits.HitsHits[0]?.Source.Affiliatelink + "\", \"enddate\" : \"" + enddate + "\", \"date\" : \"" + todaysdate + "\" }   ";
                                    WebClient wc4 = new WebClient();
                                    wc4.Encoding = Encoding.UTF8;
                                    wc4.Headers.Add("Content-Type", "application/json");
                                    try
                                    {
                                        wc4.UploadString(hostelastic + "/emmares_search_test/_doc", jsonbody);
                                        var urlwithoutsecureprotocol = "http://emmares.com/SearchAPI/Get_File/" + messageId;

                                        WaitForItToWork(urlwithoutsecureprotocol, messageId + ".jpg", "test");

                                        //delete from pop
                                        DeleteMessageByUID(uID);
                                        string potdomaila = "C:/inetpub/wwwroot/App_Data/pages/";
                                        System.IO.File.WriteAllText(potdomaila + messageId + ".html", content, Encoding.UTF8);
                                        i++;
                                    }
                                    catch (Exception ex)
                                    {
                                        string logerror = "C:/inetpub/wwwroot/App_Data/errors/";
                                        System.IO.File.WriteAllText(logerror + messageId + ".txt", jsonbody, Encoding.UTF8);
                                    }

                                }

                            }
                            catch
                            {
                                i++;
                            }
                        }
                    }
                    catch
                    {
                        i++;
                    }








                }
            }
            //_timer = new Timer(1000) { AutoReset = true };
            //_timer.Elapsed += TimerElapsed;

        }

        private void DeleteMessageByUID(string messageId)
        {
            string hostName = "172.17.1.41";
            int port = 110;
            bool useSsl = false;
            string userName = "publish@emmares";
            string password = "publish123!";

            using (var client = new Pop3Client())
            {
                client.Connect(hostName, port, useSsl);
                client.Authenticate(userName, password);
                for (int i = 0; i < client.Count; i++)
                {

                    // If the Message ID of the current message is the same as the parameter given, delete that message
                    if (client.GetMessageUid(i) == messageId)
                    {
                        // Delete
                        client.DeleteMessage(i);
                    }
                }
                client.Disconnect(true);
            }
        }

        private bool WaitForItToWork(string Url, string FileName, string PathToPicture)
        {
            var argument = @"C:\\Users\emmaresmvp\Desktop\Thumbnail\bin\Release\GetSiteThumbnail.exe" + " " + @Url + " " + @"C:\\inetpub\wwwroot\wwwroot\images\" + FileName;
            var final = "/C" + argument;
            // var argument = @"/C C:\Users\emmaresmvp\Desktop\GetSiteThumbnail.exe" +Url + "C:/Users\\emmaresmvp\\Desktop\\cognifis.jpg 1280 1024 640 480";

            // var argument = @"/C C:\Users\emmaresmvp\Desktop\GetSiteThumbnail.exe" +Url + "C:/Users\\emmaresmvp\\Desktop\\cognifis.jpg 1280 1024 640 480";
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = final;
            process.StartInfo = startInfo;
            process.Start();

            return true;

        }

        /// <summary>
        /// Wait for it to work.
        /// </summary>
        /// <param name="Url"></param>
        /// <param name="FileName"></param>
        /// <param name="PathToPicture"></param>
        /// <returns></returns>
        /// 

        public partial class Emailclass
        {
            [JsonProperty("hits")]
            public Hits Hits { get; set; }

        }

        public partial class Hits
        {
            [JsonProperty("total")]
            public int Total { get; set; }
            [JsonProperty("hits")]
            public Hit[] HitsHits { get; set; }
        }

        public partial class Hit
        {
            [JsonProperty("_id")]
            public string Id { get; set; }
            [JsonProperty("_source")]
            public Source Source { get; set; }
        }

        public partial class Source
        {
            [JsonProperty("email")]
            public string Email { get; set; }
        }

        public partial class Source
        {
            [JsonProperty("optin")]
            public string Optin { get; set; }
        }
        public partial class Source
        {
            [JsonProperty("optout")]
            public string Optout { get; set; }
        }

        public partial class Source
        {
            [JsonProperty("affiliatelink")]
            public string Affiliatelink { get; set; }
        }

        public partial class Source
        {
            [JsonProperty("publish")]
            public string Publish { get; set; }
        }

        public partial class Source
        {
            [JsonProperty("duration")]
            public string Duration { get; set; }
        }

        public partial class Emailclass
        {
            public static Emailclass FromJson(string json) => JsonConvert.DeserializeObject<Emailclass>(json, Converter.Settings);
        }
        internal static class Converter
        {
            public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
            {
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                DateParseHandling = DateParseHandling.None,
                Converters =
                {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
                },
            };
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            const string hostelastic = "http://172.17.1.88:9200";
            string hostName = "172.17.1.41";
            int port = 110;
            bool useSsl = false;
            string userName = "publish@emmares";
            string password = "publish123!";
            using (var client = new Pop3Client())
            {
                client.Connect(hostName, port, useSsl);
                client.Authenticate(userName, password);
                for (int i = 4; i < client.Count; i++)
                {
                    var uID = client.GetMessageUid(i);
                    var message = client.GetMessage(i);
                    var subject = message.Subject;
                    var addrfrom = message.From;
                    //  MessageBox.Show(message.TextBody.Trim());
                    var excerpt = "";
                    if (message.TextBody == null)
                    {
                        excerpt = message.HtmlBody.Trim();
                    }
                    else
                    {
                        excerpt = message.TextBody.Trim();
                    }
                    var cid = "";
                    var match = Regex.Match(excerpt, "Emmarescid###(.*)###");
                    cid = match.Groups[1].Value; // Campaign ID
                    excerpt = Regex.Replace(excerpt, @"\r\n?|\n", " ");
                    excerpt = Regex.Replace(excerpt, "&#x10C;", "Č");
                    excerpt = Regex.Replace(excerpt, "&zwnj;", "");


                    /*string pattern0 = "(?<= style)(.*)(?= style)";
                    Regex rgx0 = new Regex(pattern0);
                    excerpt = rgx0.Replace(excerpt, "");*/
                    string pattern0s = "<script(?:\r|\n|.)+</script>";
                    Regex rgx0s = new Regex(pattern0s);
                    excerpt = rgx0s.Replace(excerpt, "");
                    string pattern0 = "<style(?:\r|\n|.)+</style>";
                    Regex rgx0 = new Regex(pattern0);
                    excerpt = rgx0.Replace(excerpt, "");
                    string pattern = "<[^<>]+>";
                    //  string pattern = "<[^>]*> ";
                    Regex rgx = new Regex(pattern);
                    excerpt = rgx.Replace(excerpt, "");
                    string pattern2 = "{[^{}]+}";
                    Regex rgx2 = new Regex(pattern2);
                    excerpt = rgx2.Replace(excerpt, "");
                    excerpt = rgx2.Replace(excerpt, "");
                    string pattern3 = "#.*? ";
                    Regex rgx3 = new Regex(pattern3);
                    excerpt = rgx3.Replace(excerpt, "");
                    /* string pattern4 = "\\..*? ";
                     Regex rgx4 = new Regex(pattern4);
                     excerpt = rgx4.Replace(excerpt, "");*/

                    excerpt = Regex.Replace(excerpt, "<", " ");
                    excerpt = Regex.Replace(excerpt, ">", " ");
                    excerpt = Regex.Replace(excerpt, "/", " ");

                    // excerpt = Regex.Replace(excerpt, "\\\\<*>", " ");
                    // excerpt = Regex.Replace(excerpt, ">", " ");
                    excerpt = Regex.Replace(excerpt, "\"", "'");


                    excerpt = Regex.Replace(excerpt, @"\t", " ");
                    excerpt = Regex.Replace(excerpt, @"\r", " ");
                    excerpt = Regex.Replace(excerpt, @"\s+", " ");

                    subject = Regex.Replace(subject, "\"", "'");
                    subject = Regex.Replace(subject, @"\t", " ");


                    float score = 0.0F;
                    var messageId = message.MessageId;
                    var preview = message.TextBody;
                    var campaignname = "nocampaignname";
                    var descriptionofcampaign = "nodescriptionofcampaign";
                    var publisher = "nopublisher";
                    var fieldofinterests = "fieldofinterests";
                    var region = "noregion";
                    var contenttype = "nocontenttype";
                    var optin = "";
                    var optout = "";
                    var affiliatelink = "";
                    var enddate = message.To.ToString();

                    string[] enddate1 = enddate.Split(new string[] { "-enddate-" }, StringSplitOptions.None);  //emmares-enddate-2019-06-06@emmares.com
                    string[] enddate2 = enddate1.Length > 1 ? enddate1[1].Split(new string[] { "@emmares" }, StringSplitOptions.None) : new string[] { DateTime.Today.AddYears(2).ToString("yyyy-MM-dd") };
                    //enddate = 2 leti od dneva vpisa
                    enddate = enddate2[0]; //"2019-12-09";

                    string content = !string.IsNullOrEmpty(message.HtmlBody) ? message.HtmlBody : message.TextBody;

                    //addr "n" <n@n.com> -> n@n.com
                    //
                    string addrfrom2 = addrfrom.ToString();
                    if (addrfrom2.Contains("<"))
                    {
                        string[] addrsplit = addrfrom2.Split('<');
                        addrfrom2 = addrsplit[1];
                        addrfrom2 = addrfrom2.Replace(">", "");
                    }

                    if (cid != "")
                    {
                        WebClient savetodb = new WebClient();
                        try
                        {
                            var addressuri = "https://emmares.com/SearchAPI/SaveToDB?";
                            var datacidaddr = "CampaignID=" + cid + "&Sender_Email=" + addrfrom2;
                            //string method = "POST";
                            savetodb.DownloadString(addressuri + datacidaddr);
                        }
                        catch (Exception ex)
                        {
                            if (!ex.Message.ToLower().Contains("violation")) { }

                        }
                    }


                    //
                    string mailelastic = "";
                    /*WebClient wc = new WebClient();
                     try
                      { mailelastic = wc.DownloadString(hostelastic + "/blacklist/_search?q=" + addrfrom.ToString() + "&filter_path=hits.hits._source.query.term.email"); } 
                      catch { mailelastic = "Do not use \", (, ), : and other special characters"; }*/
                    string json = "{\"query\": {\"term\": {\"email\": \"" + addrfrom2 + "\" }}}";
                    WebClient wc2 = new WebClient();
                    wc2.Headers.Add("Content-Type", "application/json");
                    try
                    {
                        mailelastic = wc2.UploadString(hostelastic + "/blacklist/_search?", json);
                    }
                    catch
                    {
                        mailelastic = "Error";
                    }

                    string mailelastic2 = "";
                    string jsonw = "{\"query\": {\"term\": {\"email\": \"" + addrfrom2 + "\" }}}";
                    WebClient wc3 = new WebClient();
                    wc3.Headers.Add("Content-Type", "application/json");
                    try
                    {
                        mailelastic2 = wc3.UploadString(hostelastic + "/whitelist/_search?", json);
                    }
                    catch
                    {
                        mailelastic2 = "Error";
                    }

                    try
                    {
                        var xblacklist = Newtonsoft.Json.JsonConvert.DeserializeObject(mailelastic);

                        var emailclass = new Emailclass();
                        emailclass = JsonConvert.DeserializeObject<Emailclass>(mailelastic);

                        if (emailclass.Hits.Total != 0)
                        {
                            // MessageBox.Show("ta mail je na blacklisti " + emailclass.Hits.HitsHits[0]?.Source.Email.ToString()); //.Query.Term.Email.ToString());
                            DeleteMessageByUID(uID);
                        }
                        else
                        {
                            //  MessageBox.Show(Regex.Replace(excerpt, @"\r\n?|\n", " "));
                            try
                            {
                                var xwhitelist = Newtonsoft.Json.JsonConvert.DeserializeObject(mailelastic2);
                                var emailclass2 = new Emailclass();
                                emailclass2 = JsonConvert.DeserializeObject<Emailclass>(mailelastic2);
                                string todaysdate = DateTime.Today.ToString("yyyy-MM-dd");
                                var htmlDoc = new HtmlDocument();
                                htmlDoc.LoadHtml(content);
                                var htmlNodes = htmlDoc.DocumentNode.SelectNodes("//a");
                                if (htmlNodes != null)
                                {
                                    for (int i2 = htmlNodes.Count - 1; i2 >= 0; --i2)
                                    {
                                        if (htmlNodes[i2].InnerHtml.ToLower().Contains("unsubscribe") ||
                                            htmlNodes[i2].InnerHtml.ToLower().Contains("opt out") ||
                                            htmlNodes[i2].InnerHtml.ToLower().Contains("subscription") ||
                                            htmlNodes[i2].InnerHtml.ToLower().Contains("naročnine") ||
                                            htmlNodes[i2].InnerHtml.ToLower().Contains("odjavi") ||
                                            htmlNodes[i2].InnerHtml.ToLower().Contains("subscriber options") ||
                                            htmlNodes[i2].InnerHtml.ToLower().Contains("uredi profil") ||
                                            htmlNodes[i2].InnerHtml.ToLower().Contains("manage email preferences") ||
                                            htmlNodes[i2].InnerHtml.ToLower().Contains("posodobi želje"))
                                        {
                                            htmlNodes[i2].InnerHtml = " ";
                                        }
                                    }
                                }
                                using (StringWriter writer = new StringWriter())
                                {
                                    htmlDoc.Save(writer);
                                    content = writer.ToString();
                                }

                                Regex r5 = new Regex(@"(?i)unsubscribe.*?</a>");
                                content = r5.Replace(content, " ");
                                Regex r6 = new Regex(@"(?i)opt out.*?</a>");
                                content = r6.Replace(content, " ");
                                Regex r7 = new Regex(@"(?i)subscription.*?</a>");
                                content = r7.Replace(content, " ");
                                Regex r8 = new Regex(@"(?i)odjavi.*?</a>");
                                content = r8.Replace(content, " ");
                                Regex r11 = new Regex(@"publish.*?@emmares.net");
                                content = r11.Replace(content, "");
                                Regex r12 = new Regex(@"(?i)uredi profil.*?</a>");
                                content = r12.Replace(content, " ");
                                Regex r13 = new Regex(@"(?i)posodobi želje .*?</a>");
                                content = r13.Replace(content, " ");
                                Regex r14 = new Regex(@"(?i)manage email preferences.*?</a>");
                                content = r14.Replace(content, " ");
                                if (!addrfrom.ToString().Contains("@emmares"))
                                {
                                    Regex r9 = new Regex(@"Emmares Emmares");
                                    content = r9.Replace(content, "Reader");
                                    Regex r10 = new Regex(@"Emmares");
                                    content = r10.Replace(content, "Reader");
                                }

                                if (emailclass2.Hits.Total != 0 && emailclass2.Hits.HitsHits[0].Source?.Publish == "true")
                                {
                                    // MessageBox.Show("ta mail je na whitelisti " + emailclass2.Hits.HitsHits[0]?.Source.Email.ToString()); //.Query.Term.Email.ToString());
                                    //upload to elasticsearch
                                    if (emailclass2.Hits.HitsHits[0].Source?.Duration != null)
                                        enddate = DateTime.Today.AddDays(Convert.ToDouble(emailclass2.Hits.HitsHits[0].Source?.Duration)).ToString("yyyy-MM-dd");
                                    //MessageBox.Show("main enddate " + enddate);
                                    // MessageBox.Show(emailclass2.Hits.HitsHits[0].Source.Email + " o " + emailclass2.Hits.HitsHits[0].Source.Optin + " p " + emailclass2.Hits.HitsHits[0].Source.Publish);
                                    string jsonbody = "{ \"subject\" : \"" + subject + "\", \"addrfrom\" : \"" + addrfrom2 + "\", \"excerpt\" : \"" + excerpt + "\", \"score\" : \"0.0\", \"messageid\" : \"" + messageId + "\", \"preview\" : \"!!!preview!!!\", \"campaignname\" : \"Campaign name\", \"descriptionofcampaign\" : \"Description of campaign\", \"publisher\" : \"publisher1\", \"fieldofinterest\" : \"News\", \"region\" : \"Europe\", \"contenttype\" : \"Newsletter\", \"optin\" : \"" + emailclass2.Hits.HitsHits[0]?.Source.Optin + "\", \"optout\" : \"" + emailclass2.Hits.HitsHits[0]?.Source.Optout + "\", \"affiliatelink\" : \"" + emailclass2.Hits.HitsHits[0]?.Source.Affiliatelink + "\", \"enddate\" : \"" + enddate + "\", \"date\" : \"" + todaysdate + "\" }   ";
                                    WebClient wc4 = new WebClient();
                                    wc4.Encoding = Encoding.UTF8;
                                    wc4.Headers.Add("Content-Type", "application/json");
                                    try
                                    {
                                        string responese = wc4.UploadString(hostelastic + "/emmares_search_test/_doc", jsonbody);
                                        //delete from pop
                                        var test = true;
                                        DeleteMessageByUID(uID);
                                        string potdomaila = "C:/inetpub/wwwroot/App_Data/pages/";
                                        System.IO.File.WriteAllText(potdomaila + messageId + ".html", content, Encoding.UTF8);
                                    }
                                    catch (Exception ex)
                                    {
                                        string logerror = "C:/inetpub/wwwroot/App_Data/errors/";
                                        System.IO.File.WriteAllText(logerror + messageId + ".txt", jsonbody, Encoding.UTF8);
                                        throw new NotImplementedException();

                                    }

                                }

                            }
                            catch
                            {
                                i++;
                            }
                    }
                    } catch
                    {
                        i++;
                    }








        }
            }
        }
    }
}
    
