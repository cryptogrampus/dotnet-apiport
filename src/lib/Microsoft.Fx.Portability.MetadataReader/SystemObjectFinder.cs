﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Analyzer.Exceptions;
using Microsoft.Fx.Portability.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;

namespace Microsoft.Fx.Portability.Analyzer
{
    public class SystemObjectFinder
    {
        private static readonly HashSet<string> SystemObjectAssemblies = new HashSet<string>(StringComparer.Ordinal)
        {
            "mscorlib",
            "netstandard",
            "System.Private.CoreLib",
            "System.Runtime"
        };

        private readonly IDependencyFilter _assemblyFilter;

        public SystemObjectFinder(IDependencyFilter assemblyFilter)
        {
            _assemblyFilter = assemblyFilter;
        }

        /// <summary>
        /// Tries to locate the assembly containing <see cref="object"/>.
        /// </summary>
        public AssemblyReferenceInformation GetSystemRuntimeAssemblyInformation(MetadataReader reader)
        {
            var microsoftAssemblies = reader.AssemblyReferences
                .Select(handle =>
                {
                    var assembly = reader.GetAssemblyReference(handle);
                    return reader.FormatAssemblyInfo(assembly);
                })
                .Where(_assemblyFilter.IsFrameworkAssembly)
                .Where(assembly => SystemObjectAssemblies.Contains(assembly.Name))
                .OrderByDescending(assembly => assembly.Version);

            var matchingAssembly = microsoftAssemblies.FirstOrDefault();

            if (matchingAssembly != default(AssemblyReferenceInformation))
            {
                return matchingAssembly;
            }
            else
            {
                throw new SystemObjectNotFoundException(microsoftAssemblies);
            }
        }
    }
}
