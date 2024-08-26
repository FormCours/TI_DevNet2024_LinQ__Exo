using Exo_Linq_Context;
using System.Text.RegularExpressions;

Console.WriteLine("Exercice Linq");
Console.WriteLine("*************");

DataContext context = new DataContext();

// Exercice 2.5 Ecrire une requête pour présenter le nom complet (nom et prénom séparés par un espace) et le résultat annuel classé par nom croissant sur le nom de tous les étudiants appartenant à la section 1110.
var result2_5 = from student in context.Students
                where student.Section_ID == 1110
                orderby student.Last_Name
                select new
                {
                    NomCompelet = $"{student.Last_Name} {student.First_Name}",
                    Resultat = student.Year_Result
                };

var result2_5bis = context.Students.Where(s => s.Section_ID == 1110)
                                   .OrderBy(s => s.Last_Name)
                                   .Select(s => new
                                   {
                                       NomCompelet = $"{s.Last_Name} {s.First_Name}",
                                       Resultat = s.Year_Result
                                   });

// Exercice 2.6 Ecrire une requête pour présenter le nom, l’id de section et le résultat annuel. Classé par ordre croissant sur la section de tous les étudiants appartenant aux sections 1010 et 1020 ayant un résultat annuel qui n’est pas compris entre 12 et 18.

var result2_6 = from student in context.Students
                where (student.Section_ID == 1010 || student.Section_ID == 1020)
                    && (student.Year_Result < 12 || student.Year_Result > 18)
                orderby student.Section_ID
                select new
                {
                    Nom = student.Last_Name,
                    Section = student.Section_ID,
                    Resultat = student.Year_Result
                };

var result2_6bis = context.Students.Where(st => (st.Section_ID == 1010 || st.Section_ID == 1020)
                                                && (st.Year_Result < 12 || st.Year_Result > 18))
                                   .OrderBy(st => st.Section_ID)
                                   .Select(st => new
                                   {
                                       Nom = st.Last_Name,
                                       Section = st.Section_ID,
                                       Resultat = st.Year_Result
                                   });

// Exercice 2.7 Ecrire une requête pour présenter le nom, l’id de section et le résultat annuel sur 100 (nommer la colonne ‘result_100’) classé par ordre décroissant du résultat de tous les étudiants appartenant aux sections commençant par 13 et ayant un résultat annuel sur 100 inférieur ou égal à 60.

double min_result = 60;
var result2_7 = from student in context.Students
                where student.Section_ID.ToString().StartsWith("13")
                    && student.Year_Result * 5 < min_result
                orderby student.Year_Result descending
                select new
                {
                    Nom = student.Last_Name,
                    Section = student.Section_ID,
                    Result = student.Year_Result * 5
                };

var result2_7bis = context.Students.Where(s => Regex.IsMatch(s.Section_ID.ToString(), "^13\\d*$")
                                                && s.Year_Result * 5 < min_result)
                                   .OrderByDescending(s => s.Year_Result)
                                   .Select(s => new
                                   {
                                       Nom = s.Last_Name,
                                       Section = s.Section_ID,
                                       Result = s.Year_Result * 5
                                   });


foreach (var item in result2_7bis)
{
    Console.WriteLine(item);
}