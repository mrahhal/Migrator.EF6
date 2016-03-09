using System.Threading.Tasks;

namespace WithIdentity.Services
{
	public interface ISmsSender
	{
		Task SendSmsAsync(string number, string message);
	}
}
