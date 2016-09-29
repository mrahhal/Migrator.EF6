using System;

namespace Migrator.EF6.Tools
{
	public class OperationException : Exception
	{
		public OperationException(string message) : base(message)
		{
		}
	}
}
