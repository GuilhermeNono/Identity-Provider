﻿namespace Idp.Domain.Annotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class IgnoreFilterPropertyAttribute : Attribute
{
    public IgnoreFilterPropertyAttribute()
    {
    }
}