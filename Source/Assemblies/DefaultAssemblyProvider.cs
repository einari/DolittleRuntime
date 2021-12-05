﻿// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyModel;

namespace Dolittle.Runtime.Assemblies;

/// <summary>
/// Represents an implementation of <see cref="ICanProvideAssemblies"/> that provides assemblies from the current context.
/// </summary>
public class DefaultAssemblyProvider : ICanProvideAssemblies
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultAssemblyProvider"/> class.
    /// </summary>
    /// <param name="logger">Logger for logging.</param>
    /// <param name="entryAssembly"><see cref="Assembly">Entry assembly</see> - if null, it will try to get entry assembly.</param>
    public DefaultAssemblyProvider(ILogger logger, Assembly entryAssembly = null)
    {
        if (entryAssembly == null)
        {
            entryAssembly = Assembly.GetEntryAssembly();
        }
        var dependencyModel = DependencyContext.Load(entryAssembly);

        Log.NumberOfLibraries(logger, dependencyModel.RuntimeLibraries.Count);
        Libraries = dependencyModel.RuntimeLibraries.Where(_ => _.RuntimeAssemblyGroups.Count > 0).ToArray();
        Log.NumberOfLibrariesInAssemblyGroup(logger, Libraries.Count());

        foreach (var library in Libraries)
        {
            Log.ProvidingLibrary(logger, library.Name, library.Version);
        }
    }

    /// <inheritdoc/>
    public IEnumerable<Library> Libraries { get; }

    /// <inheritdoc/>
    public Assembly GetFrom(Library library)
    {
        return Assembly.Load(new AssemblyName(library.Name));
    }
}
