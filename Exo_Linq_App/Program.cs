using Exo_Linq_Context;
using System.Diagnostics;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

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


/*******************************************************************************************/


// 4.3 - Donner le résultat moyen (« AVGResult ») et le mois en chiffre (« BirthMonth ») pour les étudiants né le même mois entre 1970 et 1985.

var result4_3 = context.Students.Where(st => st.BirthDate.Year >= 1970 && st.BirthDate.Year <= 1985)
                                .GroupBy(st => st.BirthDate.Month)
                                .Select(grp => new
                                {
                                    BirthMonth = grp.Key,
                                    AVGResult = grp.Average(st => st.Year_Result)
                                });

var result4_3bis = from st in context.Students
                   where st.BirthDate.Year >= 1970 && st.BirthDate.Year <= 1985
                   group st by st.BirthDate.Month into grp
                   select new
                   {
                       BirthMonth = grp.Key,
                       AVGResult = grp.Average(st => st.Year_Result)
                   };



// 4.4 - Donner pour toutes les sections qui compte plus de 3 étudiants, la moyenne des résultats annuels (« AVGResult »).

var result4_4 = context.Students.GroupBy(st => st.Section_ID)
                                .Where(grp => grp.Count() > 3)
                                .Select(grp => new
                                {
                                    SectionId = grp.Key,
                                    AVGResult = grp.Average(st => st.Year_Result)
                                });


// 4.9 - Donner à chaque étudiant ayant obtenu un résultat annuel supérieur ou égal à 12 son grade en fonction de son résultat annuel et sur base de la table grade. La liste doit être classée dans l’ordre alphabétique des grades attribués.

// -> Non equi-join via une Cross join

var result4_9 = context.Students.Where(st => st.Year_Result >= 12)
                                .Join(
                                    context.Grades,
                                    st => true,  // Hack des conditions de la jointure, pour faire une cross join
                                    g => true,   // Comment -> On lui donne 2 valeurs constantes identique :o
                                    (st, g) => new
                                    {
                                        Student = st,
                                        Grade = g
                                    }
                                 )
                                 .Where(res => res.Student.Year_Result >= res.Grade.Lower_Bound
                                            && res.Student.Year_Result <= res.Grade.Upper_Bound)
                                 .Select(res => new
                                 {
                                     Name = res.Student.Last_Name,
                                     Result = res.Student.Year_Result,
                                     Grade = res.Grade.GradeName
                                 })
                                 .OrderBy(res => res.Grade);


var result4_9bis = from st in context.Students
                   from g in context.Grades
                   where st.Year_Result >= 12 && st.Year_Result >= g.Lower_Bound && st.Year_Result <= g.Upper_Bound
                   orderby g.GradeName
                   select new
                   {
                       Name = st.Last_Name,
                       Result = st.Year_Result,
                       Grade = g.GradeName
                   };




// 4.10 - Donner la liste des professeurs et la section à laquelle ils se rapportent ainsi que le(s) cour(s)(nom du cours et crédits) dont le professeur est responsable. La liste est triée par ordre décroissant des crédits attribués à un cours.

var result4_10 = context.Professors.GroupJoin(                                      // Groupe join entre Prof et course
                                        context.Courses,
                                        p => p.Professor_ID,
                                        c => c.Professor_ID,
                                        (prof, courses) => new
                                        {
                                            Prof = prof,                            // Un prof
                                            TempCourses = courses                   // Une liste de course (Potentiellement vide)
                                        }
                                   )
                                   .SelectMany(                                     // Fonction pour applanir le resultat
                                        res => res.TempCourses.DefaultIfEmpty(),    // → Permet d'obtenir un resultat comme "left join"
                                        (res, c) => new                             //   Le prof sans course, ne sont pas perdu
                                        {                                           //   contrairement à un Join (Inner join)
                                            res.Prof,
                                            Course = c
                                        }
                                   )
                                   .GroupJoin(                                      // Groupe join entre le resultat précédent et les section
                                        context.Sections,
                                        res => res.Prof.Section_ID,
                                        s => s.Section_ID,
                                        (res, sections) => new
                                        {
                                            Prof = res.Prof,
                                            Course = res.Course,
                                            TempSections = sections
                                        }
                                   )
                                   .SelectMany(                                     // Fonction pour applanir le resultat
                                        res => res.TempSections.DefaultIfEmpty(),
                                        (res, s) => new
                                        {
                                            ProfessorName = res.Prof.Professor_Name,
                                            Section = s?.Section_Name,
                                            CourseName = res.Course?.Course_Name,
                                            CourseEcts = res.Course?.Course_Ects
                                        }
                                   )
                                   .OrderByDescending(res => res.CourseEcts);

Console.Clear();
foreach (var item in result4_10)
{
    Console.WriteLine(item);
}
Console.WriteLine();



int count = 1_000_000;
Stopwatch sw1 = new Stopwatch();
Stopwatch sw2 = new Stopwatch();

sw1.Start();
for (int i = 0; i < count; i++)
{
    var result4_10bis = from p in context.Professors
                        join _c in context.Courses on p.Professor_ID equals _c.Professor_ID into tempCourses
                        join _s in context.Sections on p.Section_ID equals _s.Section_ID into tempSections
                        from c in tempCourses.DefaultIfEmpty()
                        from s in tempSections.DefaultIfEmpty()
                        orderby c?.Course_Ects descending
                        select new
                        {
                            ProfessorName = p.Professor_Name,
                            Section = s?.Section_Name,
                            CourseName = c?.Course_Name,
                            CourseEcts = c?.Course_Ects,
                        };
}
sw1.Stop();

sw2.Start();
for (int i = 0; i < count; i++)
{
    var result4_10tier = context.Professors
                            .GroupJoin(context.Courses, p => p.Professor_ID, c => c.Professor_ID, (prof, courses) => new { Prof = prof, TempCourses = courses })
                            .SelectMany(res => res.TempCourses.DefaultIfEmpty(), (res, c) => new { res.Prof, Course = c })
                            .GroupJoin(context.Sections, res => res.Prof.Section_ID, s => s.Section_ID, (res, sections) => new { Prof = res.Prof, Course = res.Course, TempSections = sections })
                            .SelectMany(res => res.TempSections.DefaultIfEmpty(), (res, s) => new
                            {
                                ProfessorName = res.Prof.Professor_Name,
                                Section = s?.Section_Name,
                                CourseName = res.Course?.Course_Name,
                                CourseEcts = res.Course?.Course_Ects
                            })
                            .OrderByDescending(res => res.CourseEcts);
}
sw2.Stop();

Console.WriteLine("Test de la même requete en méthode et en expression");
Console.WriteLine($"Expression : {sw1.ElapsedTicks} ({sw1.ElapsedMilliseconds} ms)");
Console.WriteLine($"Methode    : {sw2.ElapsedTicks} ({sw2.ElapsedMilliseconds} ms)");

