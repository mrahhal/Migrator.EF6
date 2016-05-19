namespace Migrator.EF6.Tools
{
	public static class Worker
	{
#if NET451
		public static int Execute(string[] args) => ExecuteCommand.Create().Execute(args);
#else
		public static int Execute(string[] args)
		{
			return 0;
		}
#endif
	}
}
