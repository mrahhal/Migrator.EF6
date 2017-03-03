namespace Migrator.EF6.Tools
{
	public static class Worker
	{
#if NET462
		public static int Execute(string[] args) => ExecuteCommand.Create().Execute(args);
#else
		public static int Execute(string[] args)
		{
			return 0;
		}
#endif
	}
}
