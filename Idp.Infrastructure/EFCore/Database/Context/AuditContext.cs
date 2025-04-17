﻿using System.ComponentModel.DataAnnotations.Schema;
using Idp.Domain.Annotations;
using Idp.Domain.Database.Context;
using Idp.Domain.Database.Entity.Interfaces;
using Idp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Idp.Infrastructure.EFCore.Database.Context;

public sealed class AuditContext(DbContextOptions<AuditContext> options, ILogger<AuditContext> logger)
    : BaseContext<AuditContext>(options, logger), IAuditContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        logger.LogWarning("| Initializing EF Core Audit Database |");

        var entities = GetEntitiesTypes(EntityType.Entity);
        var views = GetEntitiesTypes(EntityType.View);

        foreach (var (entity, name) in entities)
            modelBuilder.Entity(entity).ToTable(name);
        logger.LogInformation("| EF Audit Entities Loaded |");

        foreach (var (view, name) in views)
            modelBuilder.Entity(view).HasNoKey().ToView(name);
        logger.LogInformation("| EF Audit Views Loaded |");

        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly,
            t => t.Name.Contains("log", StringComparison.CurrentCultureIgnoreCase));
        logger.LogInformation("| EF Audit Mappers Loaded |");
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

    private static IEnumerable<Type> ChildrenOfBaseEntity => Domain.AssemblyReference.Assembly.GetTypes()
        .Where(t => t is { IsClass: true, IsAbstract: false })
        .Where(t => t.GetInterface(nameof(IEntityLog)) is not null)
        .Where(t => t.GetInterface(nameof(IEntityView)) is null)
        .ToList();

    private static IEnumerable<Type> ChildrenOfBaseEntityView => Domain.AssemblyReference
        .Assembly.GetTypes()
        .Where(t => t.GetInterface(nameof(IEntityView)) is not null)
        .Where(t => t.GetInterface(nameof(IEntityLog)) is not null);
}
