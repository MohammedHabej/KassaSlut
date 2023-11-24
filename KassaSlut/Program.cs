using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Kassa
{
    private const string ProdukterFilvag = @"produktLista.txt";
    private const string KvittonFilvag = "Kvittenser/";
    private static int kvittoNummer = 0;

    class Produkt
    {
        public int ProduktID { get; set; }
        public string ProduktNamn { get; set; }
        public decimal Pris { get; set; }
        public string PrisEnhet { get; set; }
    }

    class SkannadProdukt
    {
        public Produkt Produkt { get; set; }
        public decimal Antal { get; set; }
    }

    static List<SkannadProdukt> skannadeProdukter = new List<SkannadProdukt>();
    static List<Produkt> produkterna = new List<Produkt>();

    static void Main()
    {
        while (true)
        {
            Console.WriteLine("KASSA");
            Console.WriteLine("1. Ny kund");
            Console.WriteLine("2. Avsluta");

            if (!int.TryParse(Console.ReadLine(), out int val))
            {
                Console.WriteLine("Ogiltigt val. Försök igen.");
                continue;
            }

            switch (val)
            {
                case 1:
                    NyKund();
                    break;
                case 2:
                    Console.WriteLine("Avslutar...");
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Ogiltigt val. Försök igen.");
                    break;
            }
        }
    }

    static void NyKund()
    {
        LaddaProdukter();

        Console.WriteLine("1. Skanna varor");
        Console.WriteLine("2. Visa produktlista");
        Console.WriteLine("3. Gå tillbaka");
        Console.WriteLine("4. Avsluta");

        if (!int.TryParse(Console.ReadLine(), out int val))
        {
            Console.WriteLine("Ogiltigt val. Försök igen.");
            NyKund();
            return;
        }

        switch (val)
        {
            case 1:
                SkannaProdukter();
                break;
            case 2:
                VisaProduktLista();
                break;
            case 3:
                Main();
                break;
            case 4:
                Console.WriteLine("Avslutar...");
                Environment.Exit(0);
                break;
            default:
                Console.WriteLine("Ogiltigt val. Försök igen.");
                NyKund();
                break;
        }
    }

    static void LaddaProdukter()
    {
        string produkterFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ProdukterFilvag);

        produkterna = File.ReadLines(produkterFilePath)
            .Select(line => line.Split(','))
            .Where(columns => columns.Length >= 4 && int.TryParse(columns[0], out _))
            .Select(columns => new Produkt
            {
                ProduktID = int.Parse(columns[0]),
                ProduktNamn = columns[1],
                Pris = decimal.TryParse(columns[2], out decimal pris) ? pris : 0,
                PrisEnhet = columns[3]
            })
            .ToList();
    }

    static void SkannaProdukter()
    {
        Console.WriteLine("Kommandon:");
        Console.WriteLine("<produktid> <antal>");
        Console.WriteLine("Exempel: 100 2");

        Console.Write("Kommando: ");
        string input = Console.ReadLine();

        string[] kommandos = input.Split(' ');

        if (kommandos.Length == 2 && int.TryParse(kommandos[0], out int produktID) && decimal.TryParse(kommandos[1], out decimal antal))
        {
            ProduktKod(produktID, antal);
        }
        else
        {
            Console.WriteLine("Ogiltigt kommando. Försök igen.");
            NyKund();
        }
    }

    static void ProduktKod(int produktKod, decimal antal)
    {
        Produkt hittadProdukt = produkterna.FirstOrDefault(p => p.ProduktID == produktKod);

        if (hittadProdukt != null)
        {
            SkannadProdukt skannadProdukt = new SkannadProdukt
            {
                Produkt = hittadProdukt,
                Antal = antal
            };

            skannadeProdukter.Add(skannadProdukt);

            VisaSkannadeProdukter();

            Console.WriteLine("1. Fortsätt skanna produkter");
            Console.WriteLine("2. Betala");
            Console.WriteLine("3. Gå tillbaka");
            Console.WriteLine("4. Visa produktlista");

            if (!int.TryParse(Console.ReadLine(), out int fortsattVal))
            {
                Console.WriteLine("Ogiltigt val. Avslutar.");
                return;
            }

            switch (fortsattVal)
            {
                case 1:
                    SkannaProdukter();
                    break;
                case 2:
                    Betalningen();
                    break;
                case 3:
                    Main();
                    break;
                case 4:
                    VisaProduktLista();
                    break;
                default:
                    Console.WriteLine("Ogiltigt val. Avslutar.");
                    break;
            }
        }
        else
        {
            Console.WriteLine("Ogiltig produktkod. Försök igen.");
            NyKund();
        }
    }

    static void VisaProduktLista()
    {
        Console.WriteLine("\n--- Produktlista ---");

        foreach (var produkt in produkterna)
        {
            Console.WriteLine($"Kod: {produkt.ProduktID}, Namn: {produkt.ProduktNamn}, Pris: {produkt.Pris} {produkt.PrisEnhet}");
        }

        Console.WriteLine("---------------------\n");

        Console.WriteLine("1. Ny kund");
        Console.WriteLine("2. Avsluta");

        if (!int.TryParse(Console.ReadLine(), out int visaVal))
        {
            Console.WriteLine("Ogiltigt val. Avslutar.");
            return;
        }

        switch (visaVal)
        {
            case 1:
                NyKund();
                break;
            case 2:
                Console.WriteLine("Avslutar...");
                Environment.Exit(0);
                break;
            default:
                Console.WriteLine("Ogiltigt val. Avslutar.");
                break;
        }
    }

    static void VisaSkannadeProdukter()
    {
        Console.WriteLine("\n--- Skannade Produkter ---");

        foreach (var skannadProdukt in skannadeProdukter)
        {
            decimal produktPris = skannadProdukt.Antal * skannadProdukt.Produkt.Pris;
            Console.WriteLine($"{skannadProdukt.Produkt.ProduktNamn} ({skannadProdukt.Antal}*{skannadProdukt.Produkt.Pris} kr): {produktPris} kr");
        }

        decimal totalPris = skannadeProdukter.Sum(sp => sp.Antal * sp.Produkt.Pris);
        Console.WriteLine($"--- Totalt Pris: {totalPris} kr ---\n");
    }

    static void Betalningen()
    {
        Console.Clear();

        Console.WriteLine("--------------------------------------------------");
        Console.WriteLine($"|             KVITTO #{++kvittoNummer:D6}              |");
        Console.WriteLine("--------------------------------------------------");

        foreach (var skannadProdukt in skannadeProdukter)
        {
            decimal produktPris = skannadProdukt.Antal * skannadProdukt.Produkt.Pris;
            Console.WriteLine($"{skannadProdukt.Produkt.ProduktNamn,-25} {skannadProdukt.Antal,5} * {skannadProdukt.Produkt.Pris,8:C} = {produktPris,10:C}");
        }

        Console.WriteLine("--------------------------------------------------");

        decimal totalPris = skannadeProdukter.Sum(sp => sp.Antal * sp.Produkt.Pris);
        Console.WriteLine($"TOTAL: {totalPris,40:C}");

        Console.WriteLine("--------------------------------------------------");

        SparaKvittoFil();

        skannadeProdukter.Clear();

        Console.WriteLine("1. Ny kund");
        Console.WriteLine("2. Avsluta");

        if (!int.TryParse(Console.ReadLine(), out int betalningsVal))
        {
            Console.WriteLine("Ogiltigt val. Avslutar.");
            return;
        }

        switch (betalningsVal)
        {
            case 1:
                NyKund();
                break;
            case 2:
                Console.WriteLine("Avslutar...");
                Environment.Exit(0);
                break;
            default:
                Console.WriteLine("Ogiltigt val. Avslutar.");
                break;
        }
    }

    static void SparaKvittoFil()
    {
        string kvittoFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, KvittonFilvag);

        if (!Directory.Exists(kvittoFolderPath))
        {
            Console.WriteLine($"Creating folder: {kvittoFolderPath}");
            Directory.CreateDirectory(kvittoFolderPath);
        }


        DateTime now = DateTime.Now;


        string separator = "==================================================";


        string kvittoFile = Path.Combine(kvittoFolderPath, "AllaKvitton.txt");

        try
        {
            using (StreamWriter writer = new StreamWriter(kvittoFile, true))
            {
                writer.WriteLine(separator);
                writer.WriteLine($"|             KVITTO #{kvittoNummer:D6}              |");
                writer.WriteLine($"|             Date: {now.ToString("yyyy-MM-dd")} Time: {now.ToString("HH:mm:ss")} |");
                writer.WriteLine(separator);

                foreach (var skannadProdukt in skannadeProdukter)
                {
                    decimal produktPris = skannadProdukt.Antal * skannadProdukt.Produkt.Pris;
                    writer.WriteLine($"{skannadProdukt.Produkt.ProduktNamn,-25} {skannadProdukt.Antal,5} * {skannadProdukt.Produkt.Pris,8:C} = {produktPris,10:C}");
                }

                writer.WriteLine(separator);

                decimal totalPris = skannadeProdukter.Sum(sp => sp.Antal * sp.Produkt.Pris);
                writer.WriteLine($"TOTAL: {totalPris,40:C}");

                writer.WriteLine(separator);
            }

            Console.WriteLine($"Kvitto sparad till: {kvittoFile}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Gick ej att spara kvitto: {ex.Message}");
        }
    }
}
