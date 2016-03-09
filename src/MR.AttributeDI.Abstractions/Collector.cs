using System;
using System.Linq;
using System.Reflection;

namespace MR.AttributeDI
{
	/// <summary>
	/// Collects types decorated with <see cref="AddToServicesAttribute"/> and presents them to an <see cref="IApplier"/>.
	/// </summary>
	public class Collector
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Collector"/> class.
		/// </summary>
		/// <param name="assemblies">The assemblies to collect from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assemblies"/> is null.</exception>
		/// <exception cref="ArgumentException">Assemblies to check should not be empty.</exception>
		public Collector(params Assembly[] assemblies)
		{
			if (assemblies == null)
				throw new ArgumentNullException(nameof(assemblies));
			if (assemblies.Length == 0)
				throw new ArgumentException("Assemblies to check should not be empty.", nameof(assemblies));

			Assemblies = assemblies;
		}

		/// <summary>
		/// Gets the assemblies to collect from.
		/// </summary>
		public Assembly[] Assemblies { get; }

		/// <summary>
		/// Collects types decorated with <see cref="AddToServicesAttribute"/> and user the <paramref name="applier"/> to apply it.
		/// </summary>
		/// <param name="applier">The applier to use.</param>
		/// <exception cref="System.ArgumentNullException"><paramref name="applier"/> is null.</exception>
		public void Collect(IApplier applier)
		{
			if (applier == null)
				throw new ArgumentNullException(nameof(applier));

			var implementations = Assemblies
				.SelectMany(a => a.ExportedTypes)
				.Where(t =>
					t.GetTypeInfo()
					 .CustomAttributes
					 .Any(cd => cd.AttributeType == typeof(AddToServicesAttribute)));

			foreach (var implementation in implementations)
			{
				var attributes = implementation.GetTypeInfo().GetCustomAttributes<AddToServicesAttribute>();
				foreach (var attribute in attributes)
				{
					var context = new ApplierContext(implementation, attribute);
					applier.Apply(context);
				}
			}
		}
	}
}
