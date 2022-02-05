using System;
using PuppeteerSharp;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using Microsoft.Win32;

namespace DiscordTokenLogin
{
    class Program
    {

        static string GetJsCode(string strToken)
        {
            Regex TokenPattern = new Regex(@"(mfa\.[a-z0-9_-]{20,})|([a-z0-9_-]{23,28}\.[a-z0-9_-]{6,7}\.[a-z0-9_-]{27})", RegexOptions.None);
            MatchCollection Match = TokenPattern.Matches(strToken);
            if (Match == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("    Token is invalid.");
                Console.ReadKey();
            }
            return "function login(token) { setInterval(() => { document.body.appendChild(document.createElement `iframe`).contentWindow.localStorage.token = `\"${token}\"` }, 50); setTimeout(() => { location.reload(); }, 2500); } login('" + strToken + "')";
        }
        static string BrowserPath()
        {
            string strBrowserPath = "";
            try
            {
                RegistryKey RegistryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\chrome.exe");
                using RegistryKey ChromeKey = RegistryKey;
                if (ChromeKey != null)
                {
                    strBrowserPath = ChromeKey.GetValue(name: "").ToString();
                }
                else
                {
                    RegistryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Clients\\StartMenuInternet\\Microsoft Edge\\shell\\open\\command");
                    using RegistryKey EdgeKey = RegistryKey;
                    if (EdgeKey != null)
                    {
                        strBrowserPath = EdgeKey.GetValue(name: "").ToString();

                    }
                }
            }
            catch (Exception) { }
            return strBrowserPath.Replace("\"", String.Empty);
        }
        static void UserDetails(string strToken)
        {
            HttpWebRequest Request = (HttpWebRequest)WebRequest.Create("https://discord.com/api/v9/users/@me");
            Request.Headers.Add("authorization", strToken);
            using HttpWebResponse Response = (HttpWebResponse)Request.GetResponse();
            Stream DataStream = Response.GetResponseStream();
            StreamReader Reader = new StreamReader(DataStream);
            var strData = Reader.ReadToEnd();
            JObject objData = JObject.Parse(strData);
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"\n\n    ID:   {objData.GetValue("id")}\n    USERNAME:    {objData.GetValue("username")}#{objData.GetValue("discriminator")}\n    EMAIL:    {objData.GetValue("email")}\n    PHONE:    {objData.GetValue("phone")}\n    VERIFIED:    {objData.GetValue("verified")}");
            Reader.Close();
            DataStream.Close();
        }
        [Obsolete]
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            Console.Write("\n");
            string EHSNDVR = @"
    ███████╗██╗  ██╗███████╗███╗   ██╗██████╗ ██╗   ██╗██████╗ 
    ██╔════╝██║  ██║██╔════╝████╗  ██║██╔══██╗██║   ██║██╔══██╗
    █████╗  ███████║███████╗██╔██╗ ██║██║  ██║██║   ██║██████╔╝
    ██╔══╝  ██╔══██║╚════██║██║╚██╗██║██║  ██║╚██╗ ██╔╝██╔══██╗
    ███████╗██║  ██║███████║██║ ╚████║██████╔╝ ╚████╔╝ ██║  ██║
    ╚══════╝╚═╝  ╚═╝╚══════╝╚═╝  ╚═══╝╚═════╝   ╚═══╝  ╚═╝  ╚═╝
";
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(EHSNDVR);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("\n    Discord Login Token    Github Page : iamehsandvr    Version: 1.0.0\n");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("\n    Token:    ");
            Console.ForegroundColor = ConsoleColor.White;
            string strToken = Console.ReadLine();
            string strJsCode = GetJsCode(strToken);
            UserDetails(strToken);
            string strBrowserPath = BrowserPath();
            var Option = new LaunchOptions();
            if (strBrowserPath == "")
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("\n    Downloading Chromium ... Please Wait ...    ");
                await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);
                Option = new LaunchOptions
                {
                    Headless = false,
                };
            }
            else
            {
                Option = new LaunchOptions
                {
                    Headless = false,
                    ExecutablePath = strBrowserPath
                };
            }
            Console.ForegroundColor = ConsoleColor.Black;
            var Browser = await Puppeteer.LaunchAsync(Option);
            var DiscordLoginPage = (await Browser.PagesAsync())[0];
            await DiscordLoginPage.GoToAsync("https://discord.com/login");
            try
            {
                await DiscordLoginPage.EvaluateExpressionAsync(strJsCode.ToString());
            }
            catch (Exception)
            { }
        }
    }
}
