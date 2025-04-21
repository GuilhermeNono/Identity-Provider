﻿using System.ComponentModel.DataAnnotations.Schema;
using Idp.Domain;
using Idp.Domain.Annotations;
using Idp.Domain.Database.Context;
using Idp.Domain.Database.Entity;
using Idp.Domain.Database.Entity.Interfaces;
using Idp.Domain.Enums;
using Idp.Domain.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenIddict.EntityFrameworkCore.Models;

namespace Idp.Infrastructure.EFCore.Database.Context;

public sealed class MainContext(DbContextOptions<MainContext> options, ILogger<MainContext> logger)
    : BaseContext<MainContext>(options, logger), IMainContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if(!EnvironmentHelper.IsProductionEnvironment)
            optionsBuilder.EnableSensitiveDataLogging();
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        logger.LogWarning("| Initializing EF Core Main Database |");
        var entities = GetEntitiesTypes(EntityType.Entity);
        var views = GetEntitiesTypes(EntityType.View);

        foreach (var (entity, entityName) in entities)
            modelBuilder.Entity(entity).ToTable(entityName);
        logger.LogInformation("| EF Main Entities Loaded |");

        foreach (var (view, viewName) in views)
            modelBuilder.Entity(view)
                .HasNoKey().ToView(viewName);
        logger.LogInformation("| EF Main Views Loaded |");

        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly,
            t => !t.Name.Contains("log", StringComparison.CurrentCultureIgnoreCase));
        logger.LogInformation("| EF Main Mappers Loaded |");
    }

    private Dictionary<Type, string> GetEntitiesTypes(EntityType type) => JoinAllChildren(type);

    private Dictionary<Type, string> JoinAllChildren(EntityType type)
    {
        var children = new Dictionary<Type, string>();

        switch (type)
        {
            case EntityType.Entity:
            {
                var entities = ChildrenOfBaseEntity.ToList();

                foreach (Type entity in entities)
                    children.Add(entity,
                        ((TableAttribute)Attribute.GetCustomAttribute(entity, typeof(TableAttribute))!).Name);

                return children;
            }
            case EntityType.View:
            {
                var views = ChildrenOfBaseEntityView.ToList();

                foreach (Type view in views)
                    children.Add(view,
                        ((ViewAttribute)Attribute.GetCustomAttribute(view, typeof(ViewAttribute))!).Name);

                return children;
            }
            default:
                return children;
        }
    }

    private IEnumerable<Type> ChildrenOfBaseEntity => AssemblyReference.Assembly.GetTypes()
        .Where(t => t.IsClass && !t.IsAbstract)
        .Where(InheritsFromEntity)
        .Where(t => t.GetInterface(nameof(IEntityLog)) is null)
        .Where(t => t.GetInterface(nameof(IEntityView)) is null)
        .ToList();

    static bool InheritsFromEntity(Type type)
    {
        while (type != null && type != typeof(object))
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Entity<>))
                return true;

            type = type.BaseType;
        }
        return false;
    }

    private IEnumerable<Type> ChildrenOfBaseEntityView => AssemblyReference
        .Assembly.GetTypes()
        .Where(t => t.GetInterface(nameof(IEntityView)) is not null)
        .Where(t => t.GetInterface(nameof(IEntityLog)) is null);
}
