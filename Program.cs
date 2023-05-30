using HtmlAgilityPack;
using ScrapySharp.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace Buscador_de_precios
{
    class Program
    {
        static ITelegramBotClient _botClient;
        static void Main(string[] args)
        {

            _botClient = new TelegramBotClient("6218585144:AAGHlD78x8FCPdsZbxmgEEWOJEnKwIIkaBw");
            var me = _botClient.GetMeAsync().Result;
            Console.WriteLine($"Hi, I am {me.Id} and my name is:  {me.FirstName}");

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new UpdateType[]
                {
                    UpdateType.Message,
                    UpdateType.EditedMessage,
                }
            };

            


            int cantidadArticulos = 0;
            int pivot = -1;
            int destino = -1;
            Console.Write("Ingrese URL: ");
            string url = Console.ReadLine();
            bool paso = true;
            int segundos = 10;
           
            do
            {
                paso = true;
                Console.Write("Ingrese Intervalos de tiempo [segundos] [min: 10]: ");
                string s = Console.ReadLine();
                try
                {
                    segundos = Convert.ToInt32(s);
                    if (segundos < 10)
                        throw new Exception();
                    
                }
                catch
                {
                    Console.Write("Ingrese un tiempo valido\n");
                    paso = false;
                }
                if (paso)
                {
                    Console.Write("Ingrese cantidad de articulos a consultar [maximo 20]: ");
                    s = Console.ReadLine();
                    try
                    {
                        cantidadArticulos = Convert.ToInt32(s);
                        if (cantidadArticulos < 1 || cantidadArticulos > 20)
                            throw new Exception();

                    }
                    catch
                    {
                        Console.Write("Ingrese una cantidad de articulos valida\n");
                        paso = false;
                    }
                    if (paso)
                    {
                        Console.Write("Ingrese nro de articulo pivot: ");
                        s = Console.ReadLine();
                        try
                        {
                            pivot = Convert.ToInt32(s);
                            if (pivot < 1 || pivot > cantidadArticulos)
                                throw new Exception();
                        }
                        catch
                        {
                            Console.Write("Ingrese un pivot valido\n");
                            paso = false;
                        }
                        Console.Write("Ingrese ID de destino de telegram (ej: 1018644491): ");
                        s = Console.ReadLine();
                        try
                        {
                            destino = Convert.ToInt32(s);
                        }
                        catch
                        {
                            Console.Write("Ingrese un ID de destino valido\n");
                            paso = false;
                        }
                    }
                }
            } while (paso == false);

            Bucle(url, segundos, cantidadArticulos, pivot, destino);
        }

        

        public static void Bucle(string url, int segundos, int cantidadArticulos, int pivot, int destino)
        {
            Double precioMinimo = 99999999999;
            Double precioMaximo = 0;
            string directorioSonido1;
            string directorioSonido2;
            string diaHsPrecioMinimo="";
            string diaHsPrecioSubio = "";
            SoundPlayer player;

            while (true)
            {
                string textoTelegram="";
                string diaHs;
                Estructura articulos = SeleccionarArticulos(url, cantidadArticulos, pivot);
                diaHs = DateTime.Now.ToString();

                textoTelegram = "";
                textoTelegram += "request: " + diaHs + "\n";        
                textoTelegram += articulos.getArticulo()+"\n";   

                if (articulos.getPrecio() < precioMinimo)
                {
                    precioMinimo = articulos.getPrecio();
                    diaHsPrecioMinimo = DateTime.Now.ToString(); ;
                    textoTelegram += "El precio minimo hasta ahora: " + precioMinimo + " obtenida: " + diaHsPrecioMinimo+"\n";
                    textoTelegram += "El precio minimo aumento a: " + precioMaximo + " obtenida: " + diaHsPrecioSubio+"\n";
                    directorioSonido1 = Path.Combine(Environment.CurrentDirectory, "alerta.wav");
                    directorioSonido2 = Path.Combine(Environment.CurrentDirectory, "glados.wav");
                    player = new SoundPlayer(directorioSonido1);
                    player.Play();
                    Thread.Sleep(2000);
                    player = new SoundPlayer(directorioSonido2);
                    player.Play();
                    _botClient.SendTextMessageAsync(destino, "Se detecto una bajada de precio!\n"+textoTelegram, parseMode: ParseMode.Html);
                }
                else if (articulos.getPrecio() > precioMinimo && articulos.getPrecio() != precioMaximo)
                {
                    if (articulos.getPrecio() < precioMaximo)
                    {
                        directorioSonido1 = Path.Combine(Environment.CurrentDirectory, "alerta.wav");
                        directorioSonido2 = Path.Combine(Environment.CurrentDirectory, "glados.wav");
                        player = new SoundPlayer(directorioSonido1);
                        player.Play();
                        Thread.Sleep(2000);
                        player = new SoundPlayer(directorioSonido2);
                        player.Play();
                        _botClient.SendTextMessageAsync(destino, "Se detecto una bajada de precio!\n"+textoTelegram, parseMode: ParseMode.Html);
                    }
                    else
                    {
                        directorioSonido1 = Path.Combine(Environment.CurrentDirectory, "alerta.wav");
                        directorioSonido2 = Path.Combine(Environment.CurrentDirectory, "glados2.wav");
                        player = new SoundPlayer(directorioSonido1);
                        player.Play();
                        Thread.Sleep(2000);
                        player = new SoundPlayer(directorioSonido2);
                        player.Play();
                        _botClient.SendTextMessageAsync(destino, "Se detecto una subida de precio!\n" + textoTelegram, parseMode: ParseMode.Html);
                    }
                    diaHsPrecioSubio = DateTime.Now.ToString();
                    precioMaximo = articulos.getPrecio();
                }

                diaHs = DateTime.Now.ToString();
                textoTelegram = "";
                Console.WriteLine("request: " + diaHs + "\n");
                textoTelegram += "request: " + diaHs + "\n";
                Console.WriteLine(articulos.getArticulo());
                textoTelegram += articulos.getArticulo();
                Console.WriteLine("El precio minimo hasta ahora: " + precioMinimo + " obtenida: " + diaHsPrecioMinimo);
                textoTelegram += "El precio minimo hasta ahora: " + precioMinimo + " obtenida: " + diaHsPrecioMinimo;
                Console.WriteLine("El precio minimo aumento a: " + precioMaximo + " obtenida: " + diaHsPrecioSubio);
                textoTelegram += "El precio minimo aumento a: " + precioMaximo + " obtenida: " + diaHsPrecioSubio;
                Console.WriteLine("------------------------------------------------------------------------------------------------");
                Thread.Sleep((segundos - 3) * 1000);
            }

        }
        public static Estructura SeleccionarArticulos(string url, int cantidadArticulos, int pivot)
        {
            List<string> precios = new List<string>();
            List<string> descripciones = new List<string>();
            string precio = "";
            string descripcion = "";
            string articulo = "";
            Double precioMinimo = 999999999999;
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);
            
            int i = 0;
            foreach (var Nodo in doc.DocumentNode.CssSelect(".product-price"))
            {
                if (i < cantidadArticulos*2)
                {
                    precio = Nodo.InnerHtml;
                    if (!precio.Contains("$"))
                    {
                        precios.Add(Nodo.InnerHtml);
                    }
                }
                else
                    break;
                i++;
            }
            i = 0;
            foreach (var Nodo in doc.DocumentNode.CssSelect(".product-title"))
            {
                if (i <= cantidadArticulos)
                {
                    descripcion = Nodo.InnerHtml;
                    descripciones.Add(descripcion);
                }
                i++;
            }
            i = 0;
            descripcion = "";
            precio = "";

            for (int x = 0; x < precios.Count; x++)
            {
                articulo += descripciones[x];
                articulo += precios[x];
                articulo += "\n";
                string formateado = precios[x].Replace("\n", "").Replace(" ", "");
                Double precioFormateado;
                bool parseo = Double.TryParse(formateado, out precioFormateado);
                if (parseo)
                {
                    if (precioFormateado < precioMinimo && x == pivot-1)
                        precioMinimo = precioFormateado;
                }
            }

            Estructura est = new Estructura(articulo, precioMinimo);
            return est;
        }
        public class Estructura{
            private string articulo;
            private Double precio;
            public Estructura(string articulo, Double precio)
            {
                this.articulo = articulo;
                this.precio = precio;
            }

            public string getArticulo()
            {
                return articulo;
            }
            public Double getPrecio()
            {
                return precio;
            }
        }

     
    }

}
