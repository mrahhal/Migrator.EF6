#if NET451

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Migrator.EF6.Tools.Extensions
{
	public static class TypeExtensions
	{
		/// <summary>
		/// Checks if the type has a public constructor with no parameters, and is constructable (is not abstract and not a generic type).
		/// </summary>
		public static bool IsConstructable(this Type type)
		{
			return type.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null) != null
				   && !type.IsAbstract
				   && !type.IsGenericType;
		}
	}
}

#endif