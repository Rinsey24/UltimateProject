using UltimateProject.Data;
using UltimateProject.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
namespace UltimateProject
{
    internal class Program
    {
        static void Main()
        {

            using var context = new GenericContext();
            
            // 🗑️ Очистка и создание БД
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            var connection = context.Database.GetDbConnection();
            if (connection.DataSource != null)
                Console.WriteLine($"База данных: {Path.GetFullPath(connection.DataSource)}");

            // 📝 СОЗДАНИЕ ДАННЫХ
            var mainEntities = new List<MainEntity>
            {
                new() { Name = "Технологическая Конференция", Description = "IT события", Location = "Москва" },
                new() { Name = "Музыкальный Фестиваль", Description = "Живая музыка", Location = "Санкт-Петербург" },
                new() { Name = "Научный Симпозиум", Description = "Исследования", Location = "Новосибирск" }
            };

            var relatedEntities = new List<RelatedEntity>
            {
                new() { Name = "Анна Иванова", Email = "anna@test.com" },
                new() { Name = "Борис Петров", Email = "boris@test.com" },
                new() { Name = "Светлана Сидорова", Email = "svetlana@test.com" },
                new() { Name = "Дмитрий Козлов", Email = "dmitry@test.com" }
            };

            context.MainEntities.AddRange(mainEntities);
            context.RelatedEntities.AddRange(relatedEntities);
            context.SaveChanges();

            // 🔗 СОЗДАНИЕ СВЯЗЕЙ
            mainEntities[0].RelatedEntities.Add(relatedEntities[0]); // Конференция + Анна
            mainEntities[0].RelatedEntities.Add(relatedEntities[1]); // Конференция + Борис
            mainEntities[0].RelatedEntities.Add(relatedEntities[2]); // Конференция + Светлана
            
            mainEntities[1].RelatedEntities.Add(relatedEntities[1]); // Фестиваль + Борис
            mainEntities[1].RelatedEntities.Add(relatedEntities[3]); // Фестиваль + Дмитрий
            
            mainEntities[2].RelatedEntities.Add(relatedEntities[0]); // Симпозиум + Анна
            mainEntities[2].RelatedEntities.Add(relatedEntities[2]); // Симпозиум + Светлана
            mainEntities[2].RelatedEntities.Add(relatedEntities[3]); // Симпозиум + Дмитрий

            context.SaveChanges();

            // 🔥 LINQ ЗАПРОСЫ

            Console.WriteLine("1. ВСЕ ОСНОВНЫЕ СУЩНОСТИ:");
            var allMain = context.MainEntities.ToList();
            allMain.ForEach(m => Console.WriteLine($"   {m.Name} - {m.Location}"));

            Console.WriteLine("\n2. ВСЕ СВЯЗАННЫЕ СУЩНОСТИ:");
            var allRelated = context.RelatedEntities.ToList();
            allRelated.ForEach(r => Console.WriteLine($"   {r.Name} - {r.Email}"));

            Console.WriteLine("\n3. СУЩНОСТИ С КОЛИЧЕСТВОМ СВЯЗЕЙ:");
            var entitiesWithCount = context.MainEntities
                .Select(m => new 
                { 
                    m.Name, 
                    Count = m.RelatedEntities.Count  // 🔥 LAZY LOADING работает!
                })
                .OrderByDescending(x => x.Count)
                .ToList();
            
            entitiesWithCount.ForEach(x => Console.WriteLine($"   {x.Name}: {x.Count} связей"));

            Console.WriteLine("\n4. ПОПУЛЯРНЫЕ СУЩНОСТИ (2+ связей):");
            var popularEntities = context.MainEntities
                .Where(m => m.RelatedEntities.Count >= 2)
                .OrderByDescending(m => m.RelatedEntities.Count)
                .ToList();
            
            popularEntities.ForEach(m => 
                Console.WriteLine($"   {m.Name}: {m.RelatedEntities.Count} участников"));

            Console.WriteLine("\n5. АКТИВНЫЕ УЧАСТНИКИ (2+ событий):");
            var activeParticipants = context.RelatedEntities
                .Where(r => r.MainEntities.Count >= 2)
                .OrderByDescending(r => r.MainEntities.Count)
                .ToList();
            
            activeParticipants.ForEach(r => 
                Console.WriteLine($"   {r.Name}: {r.MainEntities.Count} событий"));

            Console.WriteLine("\n6. ГРУППИРОВКА ПО МЕСТОПОЛОЖЕНИЮ:");
            var byLocation = context.MainEntities
                .GroupBy(m => m.Location)
                .Select(g => new { Location = g.Key, Count = g.Count() })
                .ToList();
            
            byLocation.ForEach(x => Console.WriteLine($"   {x.Location}: {x.Count} событий"));

            Console.WriteLine("\n7. ПЕРВЫЕ 2 СУЩНОСТИ С УЧАСТНИКАМИ:");
            var firstTwoWithParticipants = context.MainEntities
                .Take(2)
                .ToList();
            
            foreach (var entity in firstTwoWithParticipants)
            {
                Console.WriteLine($"   {entity.Name}:");
                foreach (var participant in entity.RelatedEntities) // 🔥 LAZY LOADING
                {
                    Console.WriteLine($"     - {participant.Name}");
                }
            }

            Console.WriteLine("\n8. ПОИСК ПО ИМЕНИ:");
            var searchResult = context.MainEntities
                .Where(m => m.Name.Contains("Конференция"))
                .ToList();
            
            searchResult.ForEach(m => Console.WriteLine($"   Найдено: {m.Name}"));

            Console.WriteLine("\n9. СОРТИРОВКА ПО ИМЕНИ:");
            var sorted = context.MainEntities
                .OrderBy(m => m.Name)
                .ToList();
            
            sorted.ForEach(m => Console.WriteLine($"   {m.Name}"));

            Console.WriteLine("\n10. СУЩНОСТИ БЕЗ СВЯЗЕЙ:");
            var entitiesWithoutRelations = context.MainEntities
                .Where(m => !m.RelatedEntities.Any())
                .ToList();
            
            if (entitiesWithoutRelations.Any())
                entitiesWithoutRelations.ForEach(m => Console.WriteLine($"   {m.Name}"));
            else
                Console.WriteLine("   Все сущности имеют связи");
        }
    }
}