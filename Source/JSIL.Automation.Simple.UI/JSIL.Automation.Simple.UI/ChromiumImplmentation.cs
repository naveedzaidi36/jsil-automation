using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading;

namespace JSIL.Automation.Simple.UI
{
    public class ChromiumImplmentation
    {

        private readonly IConfiguration Configuration;

        public ChromiumImplmentation(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void Run()
        {
            bool result = false;
            int retryCount = -1;

            using (ChromeDriver driver = new ChromeDriver())
            {
                try
                {
                    var navigate = driver.Navigate();
                    navigate.GoToUrl(@"https://online.jsil.com/login.xhtml");
                    driver.Manage().Window.Maximize();

                    IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                    js.ExecuteScript("document.getElementById('onlineSecurityTipDialog').remove(); document.getElementById('onlineSecurityTipDialog_modal').remove();");

                    while (retryCount++ < 10)
                    {
                        try
                        {
                            IWebElement userName = driver.FindElement(By.Id("loginForm:username"));
                            userName.Clear();
                            userName.SendKeys(Configuration["JSIL:username"]);

                            userName.SendKeys(Keys.Tab);

                            Thread.Sleep(2000);

                            IWebElement password = driver.FindElement(By.Id("loginForm:password"));
                            password.Clear();
                            password.SendKeys(Configuration["JSIL:password"]);

                            Thread.Sleep(2000);

                            IWebElement loginButton = driver.FindElement(By.Id("loginForm:btn1"));
                            loginButton.Click();
                        }
                        catch (NoSuchElementException)
                        {
                            result = true;
                            break;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            Thread.Sleep(5000);
                            continue;
                        }
                    }

                    if (result)
                    {
                        IWebElement mainTable = driver.FindElement(By.CssSelector("table[class='classback portfolioImage col-md-12 col-md-offset-1'"));

                        var splitted = mainTable.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                        File.AppendAllText(Path.Combine(Directory.GetCurrentDirectory(), "StatusFile.txt"), Environment.NewLine + splitted[0] + " " + splitted[1] + Environment.NewLine);

                        SendEmail(splitted[0] + " " + splitted[1]);

                        IWebElement logout = driver.FindElement(By.Id("menuLinkForm:logout_link"));
                        logout.Click();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    File.AppendAllText(Path.Combine(Directory.GetCurrentDirectory(), "ErrorFile.txt"), ex.Message);
                }
                finally
                {
                    driver.Quit();
                }
            }
        }

        private void SendEmail(string message)
        {
            using (var client = new SmtpClient(Configuration["SMTP:hostname"], Convert.ToInt32(Configuration["SMTP:Port"]))
            {
                Credentials = new NetworkCredential(Configuration["SMTP:username"], Configuration["SMTP:password"]),
                EnableSsl = true
            })
            {
                var subject = "Daily JS Fund Status - " + DateTime.Now.ToString("dd-MM-yyyy");
                var body = "Hi User,\n\t" + "Please find the following status for JS Fund: \n\t" + message;

                client.Send(Configuration["SMTP:from"], Configuration["SMTP:recipient"], subject, body);
            }
        }
    }
}
