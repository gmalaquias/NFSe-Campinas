using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.PhantomJS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

/**
* OBSERVAÇÔES:
*   Abrir o site da NFSe no servidor e liberar o popup, senão vai dar erro na hora de pegar a pagina
*   Colar os arquivos chromedriver e phantomdriver no local descrito na varival LOCAL_DRIVER
*   versao do chrome deve ser maior que 51
*/

namespace GetUrlNF
{
    class Program
    {
        public const string LOCAL_DRIVER = "C:/";

        static void Main(string[] args)
        {
            //instancia o phantom para rodar por tras as requisições
            using (var driver = new PhantomJSDriver(LOCAL_DRIVER))
            {
                //abre o site da NFSe
                driver.Navigate().GoToUrl("http://nfse.campinas.sp.gov.br/NotaFiscal/verificarAutenticidade.php");

                //procura a imagem do captcha
                var img = driver.FindElements(By.XPath("//*[@id='coluna5B']/form/table/tbody/tr[5]/td[4]/img"));
                //pega o url do captcha
                var url = img.FirstOrDefault().GetAttribute("src");
                //pega o base64 que esta sendo passado por parametro na imagem 
                var texto = url.Split('?').LastOrDefault().Split('?').LastOrDefault();
                //decifra de base64 
                byte[] data = Convert.FromBase64String(texto);
                var captcha = Encoding.UTF8.GetString(data);

                //campo CNPJ
                driver.FindElementByXPath("//*[@id='rPrest']").SendKeys("04298309000403");
                //numero da nota
                driver.FindElementByXPath("//*[@id='rNumNota']").SendKeys("00234867");
                //codigo de verificação
                driver.FindElementByXPath("//*[@id='rCodigoVerificacao']").SendKeys("094e7a5b");
                //incrição do municipio
                driver.FindElementByXPath("//*[@id='rInsMun']").SendKeys("000736325");
                //captcha
                driver.FindElementByXPath("//*[@id='rSelo']").SendKeys(captcha);
                //clica no botão
                driver.FindElementByXPath("//*[@id='btnVerificar']").Click();

                //espera para abrir o popup
                Thread.Sleep(2000);
                //pega as paginas abertas
                var windows = driver.WindowHandles;

                //altera para a pagina do popup
                driver.SwitchTo().Window(windows[1]);

                if(driver.Url.Contains("action"))
                    driver.SwitchTo().Window(windows[2]);

                //pega a url da pagina substituindo o arquivo responsavel pela abertura da nota
                var urlNota = driver.Url.Replace("notaFiscal.php", "visualizarNota.php");
                var idNota = Regex.Split(urlNota, "id_nota_fiscal=").LastOrDefault().Split('&').FirstOrDefault();

                //decifra id da nota
                byte[] dataId = Convert.FromBase64String(idNota);
                idNota = Encoding.UTF8.GetString(dataId);

                //abre o chrome para testar a URL
                ChromeDriver chrome = new ChromeDriver(LOCAL_DRIVER);
                chrome.Navigate().GoToUrl(urlNota);

                Console.Clear();
                Console.WriteLine("Id da nota: {0}", idNota);
                Console.WriteLine("Url da nota: {0}", urlNota);
            }
        }
    }
}
