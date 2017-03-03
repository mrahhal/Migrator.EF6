#if NET462

using System;
using System.Reflection;

namespace Migrator.EF6.Tools.Extensions
{
	public static class TypeExtensions
	{
		/// <summary>
		/// Checks if the type has a public constructor with no parameters, and is constructable
		/// (is not abstract and not a generic type).
		/// </summary>
		public static bool IsConstructable(this Type type)
		{
			var typeInfo = type.GetTypeInfo();
			return
				typeInfo.GetConstructor(
					BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null) != null
				&& !typeInfo.IsAbstract
				&& !typeInfo.IsGenericType;
		}
	}
}

#endif
