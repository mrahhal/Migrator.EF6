namespace Migrator.EF6.Tools
{
	public static class Worker
	{
		public static int Execute(string[] args) => ExecuteCommand.Create().Execute(args);
	}
}
