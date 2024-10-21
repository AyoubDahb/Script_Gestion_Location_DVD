using System;
using System.Collections.Generic;
using System.Linq;

public class Individu
{
    public string Nom { get; set; }
    public string Prenom { get; set; }

    public Individu() { }

    public Individu(string nom, string prenom)
    {
        Nom = nom;
        Prenom = prenom;
    }

    public override string ToString()
    {
        return $"{Nom} {Prenom}";
    }
}

public class Adherent : Individu
{
    private static int compteur = 0;
    public int CodeAdherent { get; }
    public DateTime DateAdhesion { get; }

    public Adherent(string nom, string prenom) : base(nom, prenom)
    {
        CodeAdherent = ++compteur;
        DateAdhesion = DateTime.Now;
    }

    public override string ToString()
    {
        return $"{CodeAdherent}_{Nom}_{Prenom}_{DateAdhesion:dd MM yyyy}";
    }
}

public class Realiseur : Individu
{
    public int CodeRealisateur { get; }

    public Realiseur(string nom, string prenom) : base(nom, prenom)
    {
        CodeRealisateur = new Random().Next(1000, 9999);
    }

    public override string ToString()
    {
        return $"{Nom} {Prenom}";
    }
}

public class DVD
{
    public int CodeDVD { get; }
    public string Titre { get; }
    public Realiseur DVDRealisateur { get; }
    public int ExemplairesTotal { get; }
    public int ExemplairesDisponibles { get; private set; }

    public DVD(string titre, Realiseur realisateur, int exemplairesTotal)
    {
        CodeDVD = new Random().Next(1000, 9999);
        Titre = titre;
        DVDRealisateur = realisateur;
        ExemplairesTotal = exemplairesTotal;
        ExemplairesDisponibles = exemplairesTotal;
    }

    public bool DVDDisponible() => ExemplairesDisponibles > 0;

    public void MettreAJourExemplairesDisponibles(int nombre)
    {
        ExemplairesDisponibles += nombre;
    }

    public override string ToString()
    {
        return $"{CodeDVD}_{Titre}_{DVDRealisateur.ToString()}_{ExemplairesTotal}_{ExemplairesDisponibles}";
    }
}

public class Location
{
    private static int compteur = 0;
    public int CodeLocation { get; }
    public DVD DVDLoué { get; }
    public Adherent Locataire { get; }
    public DateTime DateLocation { get; }
    public DateTime DateRetourPrevu { get; }
    public DateTime? DateRetourEffective { get; set; }

    public Location(DVD dvd, Adherent adherent)
    {
        CodeLocation = ++compteur;
        DVDLoué = dvd;
        Locataire = adherent;
        DateLocation = DateTime.Now;
        DateRetourPrevu = DateLocation.AddDays(7);
    }

    public string EtatLocation()
    {
        if (DateRetourEffective.HasValue)
            return "Rendu";
        return DateTime.Now > DateRetourPrevu ? "Non rendu" : "En cours";
    }

    public override string ToString()
    {
        return $"{CodeLocation}_{DVDLoué.ToString()}_{Locataire.ToString()}_{DateLocation:dd MM yyyy}_{DateRetourPrevu:dd MM yyyy}_{DateRetourEffective?.ToString("dd MM yyyy")}";
    }
}

public class CDI
{
    private List<DVD> dvds;
    private List<Adherent> adherents;
    private List<Location> locations;

    public CDI()
    {
        dvds = new List<DVD>();
        adherents = new List<Adherent>();
        locations = new List<Location>();
    }

    public void AjouterDVD(DVD dvd) => dvds.Add(dvd);

    public void AjouterAdherent(Adherent adherent) => adherents.Add(adherent);

    public Adherent RechercherAdherent(int code) => adherents.FirstOrDefault(a => a.CodeAdherent == code);

    public DVD RechercherDVD(int code) => dvds.FirstOrDefault(d => d.CodeDVD == code);

    public void AjouterLocation(DVD dvd, Adherent adherent)
    {
        if (dvd.DVDDisponible())
        {
            var location = new Location(dvd, adherent);
            locations.Add(location);
            dvd.MettreAJourExemplairesDisponibles(-1);
        }
        else
        {
            Console.WriteLine("DVD non disponible pour location.");
        }
    }

    public void RetourLocation(int codeLocation)
    {
        var location = locations.FirstOrDefault(l => l.CodeLocation == codeLocation);
        if (location != null)
        {
            location.DateRetourEffective = DateTime.Now;
            location.DVDLoué.MettreAJourExemplairesDisponibles(1);
        }
        else
        {
            Console.WriteLine("Location non trouvée.");
        }
    }

    public DVD TopLocation()
    {
        return locations.GroupBy(l => l.DVDLoué)
                       .OrderByDescending(g => g.Count())
                       .FirstOrDefault()?.Key;
    }

    public List<Adherent> Locataires()
    {
        return locations.Where(l => l.EtatLocation() == "En cours")
                        .Select(l => l.Locataire)
                        .Distinct()
                        .ToList();
    }

    public DateTime DatePossibiliteLocation(int codeLocation)
    {
        var location = locations.FirstOrDefault(l => l.CodeLocation == codeLocation);
        if (location != null)
        {
            return location.DateRetourPrevu;
        }
        throw new Exception("Location non trouvée.");
    }

    public List<DVD> DVDs => dvds;
    public List<Adherent> Adherents => adherents;
    public List<Location> Locations => locations;
}

public class Program
{
    static void Main(string[] args)
    {
        CDI cdi = new CDI();
        while (true)
        {
            Console.WriteLine("=== Menu ===");
            Console.WriteLine("1. Ajouter un DVD");
            Console.WriteLine("2. Ajouter un Adhérent");
            Console.WriteLine("3. Louer un DVD");
            Console.WriteLine("4. Retourner un DVD");
            Console.WriteLine("5. Afficher tous les DVDs");
            Console.WriteLine("6. Afficher tous les adhérents");
            Console.WriteLine("7. Afficher toutes les locations");
            Console.WriteLine("8. Afficher le DVD le plus loué");
            Console.WriteLine("0. Quitter");
            Console.Write("Choisissez une option: ");
            string choix = Console.ReadLine();

            switch (choix)
            {
                case "1":
                    AjouterDVD(cdi);
                    break;
                case "2":
                    AjouterAdherent(cdi);
                    break;
                case "3":
                    LouerDVD(cdi);
                    break;
                case "4":
                    RetournerDVD(cdi);
                    break;
                case "5":
                    AfficherDVDs(cdi);
                    break;
                case "6":
                    AfficherAdherents(cdi);
                    break;
                case "7":
                    AfficherLocations(cdi);
                    break;
                case "8":
                    AfficherTopDVD(cdi);
                    break;
                case "0":
                    return;
                default:
                    Console.WriteLine("Option invalide, veuillez réessayer.");
                    break;
            }
            Console.WriteLine();
        }
    }

    static void AjouterDVD(CDI cdi)
    {
        Console.Write("Titre du DVD: ");
        string titre = Console.ReadLine();
        Console.Write("Nom du réalisateur: ");
        string nomReal = Console.ReadLine();
        Console.Write("Prénom du réalisateur: ");
        string prenomReal = Console.ReadLine();
        Console.Write("Nombre total d'exemplaires: ");
        int totalExemplaires = int.Parse(Console.ReadLine());

        Realiseur realisateur = new Realiseur(nomReal, prenomReal);
        DVD dvd = new DVD(titre, realisateur, totalExemplaires);
        cdi.AjouterDVD(dvd);
        Console.WriteLine("DVD ajouté avec succès !");
    }

    static void AjouterAdherent(CDI cdi)
    {
        Console.Write("Nom de l'adhérent: ");
        string nom = Console.ReadLine();
        Console.Write("Prénom de l'adhérent: ");
        string prenom = Console.ReadLine();

        Adherent adherent = new Adherent(nom, prenom);
        cdi.AjouterAdherent(adherent);
        Console.WriteLine("Adhérent ajouté avec succès !");
    }

    static void LouerDVD(CDI cdi)
    {
        Console.Write("Code de l'adhérent: ");
        int codeAdherent = int.Parse(Console.ReadLine());
        Adherent adherent = cdi.RechercherAdherent(codeAdherent);
        if (adherent == null)
        {
            Console.WriteLine("Adhérent non trouvé.");
            return;
        }

        Console.Write("Code du DVD: ");
        int codeDVD = int.Parse(Console.ReadLine());
        DVD dvd = cdi.RechercherDVD(codeDVD);
        if (dvd == null)
        {
            Console.WriteLine("DVD non trouvé.");
            return;
        }

        cdi.AjouterLocation(dvd, adherent);
        Console.WriteLine("DVD loué avec succès !");
    }

    static void RetournerDVD(CDI cdi)
    {
        Console.Write("Code de la location: ");
        int codeLocation = int.Parse(Console.ReadLine());
        cdi.RetourLocation(codeLocation);
        Console.WriteLine("Retour du DVD enregistré avec succès !");
    }

    static void AfficherDVDs(CDI cdi)
    {
        foreach (var dvd in cdi.DVDs)
        {
            Console.WriteLine(dvd);
        }
    }

    static void AfficherAdherents(CDI cdi)
    {
        foreach (var adherent in cdi.Adherents)
        {
            Console.WriteLine(adherent);
        }
    }

    static void AfficherLocations(CDI cdi)
    {
        foreach (var location in cdi.Locations)
        {
            Console.WriteLine(location);
        }
    }

    static void AfficherTopDVD(CDI cdi)
    {
        DVD topDVD = cdi.TopLocation();
        if (topDVD != null)
        {
            Console.WriteLine($"Le DVD le plus loué est: {topDVD.Titre} (Code: {topDVD.CodeDVD})");
        }
        else
        {
            Console.WriteLine("Aucun DVD n'a été loué.");
        }
    }
}
