﻿using Idp.Domain.Database.Entity.Interfaces;

namespace Idp.Domain.Database.Entity;

public abstract class StatefulEntity<TId> : Entity<TId>, IStateable 
{
    public bool IsActive { get; protected set; } = true;
    public virtual void ChangeActiveStatus(bool newStatus = default) => IsActive = newStatus;
}
