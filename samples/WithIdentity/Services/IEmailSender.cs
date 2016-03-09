using System.Threading.Tasks;

namespace WithIdentity.Services
{
	public interface IEmailSender
	{
		Task SendEmailAsync(string email, string subject, string message);
	}
}
