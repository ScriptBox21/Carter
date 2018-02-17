namespace Botwin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Extensions.DependencyModel;

    public class DependencyContextAssemblyCatalog
    {
        private static readonly Assembly BotwinAssembly = typeof(BotwinExtensions).Assembly;

        private readonly DependencyContext dependencyContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContextAssemblyCatalog"/> class,
        /// using <see cref="Assembly.GetEntryAssembly()"/>.
        /// </summary>
        public DependencyContextAssemblyCatalog()
            : this(Assembly.GetEntryAssembly())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContextAssemblyCatalog"/> class,
        /// using <paramref name="entryAssembly"/>.
        /// </summary>
        public DependencyContextAssemblyCatalog(Assembly entryAssembly)
        {
            this.dependencyContext = DependencyContext.Load(entryAssembly);
        }

        /// <summary>
        /// Gets all <see cref="Assembly"/> instances in the catalog.
        /// </summary>
        /// <returns>An <see cref="IReadOnlyCollection{T}"/> of <see cref="Assembly"/> instances.</returns>
        public virtual IReadOnlyCollection<Assembly> GetAssemblies()
        {
            var results = new HashSet<Assembly>
            {
                typeof(DependencyContextAssemblyCatalog).GetTypeInfo().Assembly
            };

            foreach (var library in this.dependencyContext.RuntimeLibraries)
            {
                if (IsReferencingBotwin(library))
                {
                    foreach (var assemblyName in library.GetDefaultAssemblyNames(this.dependencyContext))
                    {
                        results.Add(SafeLoadAssembly(assemblyName));
                    }
                }
            }

            return results.ToArray();
        }

        private static Assembly SafeLoadAssembly(AssemblyName assemblyName)
        {
            try
            {
                return Assembly.Load(assemblyName);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static bool IsReferencingBotwin(Library library)
        {
            return library.Dependencies.Any(dependency => dependency.Name.Equals(BotwinAssembly.GetName().Name));
        }
    }
}