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

            // 🔥 LINQ ЗАПРОСЫ В ПРАВИЛЬНОМ СИНТАКСИСЕ

            Console.WriteLine("1. ВСЕ ОСНОВНЫЕ СУЩНОСТИ:");
            var allMain = (from entity in context.MainEntities
                           select entity).ToList();
            foreach (var m in allMain)
                Console.WriteLine($"   {m.Name} - {m.Location}");

            Console.WriteLine("\n2. ВСЕ СВЯЗАННЫЕ СУЩНОСТИ:");
            var allRelated = (from entity in context.RelatedEntities
                              select entity).ToList();
            foreach (var r in allRelated)
                Console.WriteLine($"   {r.Name} - {r.Email}");

            Console.WriteLine("\n3. СУЩНОСТИ С КОЛИЧЕСТВОМ СВЯЗЕЙ:");
            var entitiesWithCount = (from entity in context.MainEntities
                                     select new
                                     {
                                         entity.Name,
                                         Count = entity.RelatedEntities.Count
                                     } into result
                                     orderby result.Count descending
                                     select result).ToList();

            foreach (var x in entitiesWithCount)
                Console.WriteLine($"   {x.Name}: {x.Count} связей");

            Console.WriteLine("\n4. ПОПУЛЯРНЫЕ СУЩНОСТИ (2+ связей):");
            var popularEntities = (from entity in context.MainEntities
                                   where entity.RelatedEntities.Count >= 2
                                   orderby entity.RelatedEntities.Count descending
                                   select entity).ToList();

            foreach (var m in popularEntities)
                Console.WriteLine($"   {m.Name}: {m.RelatedEntities.Count} участников");

            Console.WriteLine("\n5. АКТИВНЫЕ УЧАСТНИКИ (2+ событий):");
            var activeParticipants = (from participant in context.RelatedEntities
                                      where participant.MainEntities.Count >= 2
                                      orderby participant.MainEntities.Count descending
                                      select participant).ToList();

            foreach (var r in activeParticipants)
                Console.WriteLine($"   {r.Name}: {r.MainEntities.Count} событий");

            Console.WriteLine("\n6. ГРУППИРОВКА ПО МЕСТОПОЛОЖЕНИЮ:");
            var byLocation = (from entity in context.MainEntities
                              group entity by entity.Location into g
                              select new { Location = g.Key, Count = g.Count() }).ToList();

            foreach (var x in byLocation)
                Console.WriteLine($"   {x.Location}: {x.Count} событий");

            Console.WriteLine("\n7. ПЕРВЫЕ 2 СУЩНОСТИ С УЧАСТНИКАМИ:");
            var firstTwoWithParticipants = (from entity in context.MainEntities
                                            select entity).Take(2).ToList();

            foreach (var entity in firstTwoWithParticipants)
            {
                Console.WriteLine($"   {entity.Name}:");
                foreach (var participant in entity.RelatedEntities)
                {
                    Console.WriteLine($"     - {participant.Name}");
                }
            }

            Console.WriteLine("\n8. ПОИСК ПО ИМЕНИ:");
            var searchResult = (from entity in context.MainEntities
                                where entity.Name.Contains("Конференция")
                                select entity).ToList();

            foreach (var m in searchResult)
                Console.WriteLine($"   Найдено: {m.Name}");

            Console.WriteLine("\n9. СОРТИРОВКА ПО ИМЕНИ:");
            var sorted = (from entity in context.MainEntities
                          orderby entity.Name
                          select entity).ToList();

            foreach (var m in sorted)
                Console.WriteLine($"   {m.Name}");

            Console.WriteLine("\n10. СУЩНОСТИ БЕЗ СВЯЗЕЙ:");
            var entitiesWithoutRelations = (from entity in context.MainEntities
                                            where !entity.RelatedEntities.Any()
                                            select entity).ToList();

            if (entitiesWithoutRelations.Any())
                foreach (var m in entitiesWithoutRelations)
                    Console.WriteLine($"   {m.Name}");
            else
                Console.WriteLine("   Все сущности имеют связи");

            // 🎯 ДОПОЛНИТЕЛЬНЫЕ ЗАПРОСЫ В ПРАВИЛЬНОМ СИНТАКСИСЕ

            Console.WriteLine("\n11. СЛОЖНАЯ ГРУППИРОВКА С АГРЕГАЦИЕЙ:");
            var complexGroup = (from entity in context.MainEntities
                                group entity by entity.Location into g
                                select new
                                {
                                    Location = g.Key,
                                    TotalEvents = g.Count(),
                                    AvgParticipants = g.Average(e => e.RelatedEntities.Count),
                                    EventNames = string.Join(", ", g.Select(e => e.Name))
                                }).ToList();

            foreach (var group in complexGroup)
            {
                Console.WriteLine($"   {group.Location}:");
                Console.WriteLine($"     Событий: {group.TotalEvents}");
                Console.WriteLine($"     Среднее участников: {group.AvgParticipants:F1}");
                Console.WriteLine($"     События: {group.EventNames}");
            }

            Console.WriteLine("\n12. СОЕДИНЕНИЕ СУЩНОСТЕЙ:");
            var joinedQuery = (from main in context.MainEntities
                               from related in main.RelatedEntities
                               select new
                               {
                                   Event = main.Name,
                                   Participant = related.Name,
                                   Location = main.Location
                               }).ToList();

            foreach (var item in joinedQuery.Take(5)) // Покажем первые 5
            {
                Console.WriteLine($"   {item.Event} -> {item.Participant} ({item.Location})");
            }

            Console.WriteLine("\n13. УНИКАЛЬНЫЕ ЛОКАЦИИ:");
            var uniqueLocations = (from entity in context.MainEntities
                                   select entity.Location).Distinct().ToList();

            foreach (var location in uniqueLocations)
            {
                Console.WriteLine($"   {location}");
            }
        }
    }
}