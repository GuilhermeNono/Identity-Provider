﻿namespace Idp.Infrastructure.EFCore.Query.CustomQuery.Interfaces;

public interface IBaseQuery<TResult>
{
    public bool IsCountable { get; }
}