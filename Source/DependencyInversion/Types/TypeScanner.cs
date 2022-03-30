// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BaselineTypeDiscovery;
using Dolittle.Runtime.DependencyInversion.Actors;
using Dolittle.Runtime.DependencyInversion.Lifecycle;
using Dolittle.Runtime.DependencyInversion.Scoping;

namespace Dolittle.Runtime.DependencyInversion.Types;

/// <summary>
/// Represents a system that can scan all Runtime assemblies for classes to be registered in a DI container.
/// </summary>
public static class TypeScanner
{
    /// <summary>
    /// Scans all the Runtime assemblies to find exported classes.
    /// </summary>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Type"/> with all classes in Runtime assemblies.</returns>
    public static IEnumerable<Type> GetAllClassesInRuntimeAssemblies()
    {
        var assemblies = AssemblyFinder.FindAssemblies(
            _ => { },
            assembly => assembly.FullName != default 
                        && assembly.FullName.StartsWith("Dolittle.Runtime", StringComparison.InvariantCulture)
                        && !assembly.FullName.Contains("Contracts", StringComparison.InvariantCulture),
            true);
        return assemblies.SelectMany(_ => _.ExportedTypes).Where(_ => _.IsClass);
    }
    
    /// <summary>
    /// Groups classes from an <see cref="IEnumerable{T}"/> of <see cref="Type"/> by scope and lifetime.
    /// </summary>
    /// <param name="classes">The classes to group.</param>
    /// <returns>The <see cref="ClassesByScope"/> grouped by scope and lifetime.</returns>
    public static ClassesByScope GroupClassesByScopeAndLifecycle(IEnumerable<Type> classes)
    {
        var classesByScope = classes.ToLookup(_ => _.GetScope());

        return new ClassesByScope(
            GroupClassesByLifecycle(classesByScope[Scopes.Global]),
            GroupClassesByLifecycle(classesByScope[Scopes.PerTenant]));
    }
    
    /// <summary>
    /// Groups classes from an <see cref="IEnumerable{T}"/> of <see cref="Type"/> by scope and actor type.
    /// </summary>
    /// <param name="classes">The classes to group.</param>
    /// <returns>The <see cref="ClassesByScopeAndActorType"/> grouped by scope and actor type.</returns>
    public static ClassesByScopeAndActorType GroupClassesByScopeAndActorType(IEnumerable<Type> classes)
    {
        var classesByScope = classes.ToLookup(_ => _.GetScope());
        return new ClassesByScopeAndActorType(
            GroupClassesByActorType(classesByScope[Scopes.Global]),
            GroupClassesByActorType(classesByScope[Scopes.PerTenant]));
    }

    static ClassesByLifecycle GroupClassesByLifecycle(IEnumerable<Type> classes)
    {
        var classesByLifecycle = classes.ToLookup(_ => _.GetLifecycle());
        
        return new ClassesByLifecycle(
            classesByLifecycle[Lifecycles.Singleton].ToArray(),
            classesByLifecycle[Lifecycles.Scoped].ToArray(),
            classesByLifecycle[Lifecycles.Transient].ToArray());
    }

    static ClassesByActorType GroupClassesByActorType(IEnumerable<Type> classes)
    {
        var classesByActorType = classes.ToLookup(_ => _.GetActorType());
        return new ClassesByActorType(
            classesByActorType[ActorType.Actor].ToArray(),
            classesByActorType[ActorType.Grain]
                .Select(grainType => new GrainAndActor(grainType, grainType.GetCustomAttribute<GrainAttribute>()!.ActorType))
                .ToArray());
    }
}
