﻿using Idp.Domain.Database.Entity.Interfaces;

namespace Idp.Domain.Database.Repository;

public interface IAuditRepository<TEntity, in TId> : ICrudRepository<TEntity, TId>
    where TEntity : class, IEntity<TId>, new()
{
    Task<int> Delete(TEntity entity, string userWhoDeleted, CancellationToken cancellationToken);
    Task<int> Delete(TId id, string userWhoDeleted, CancellationToken cancellationToken);
}
