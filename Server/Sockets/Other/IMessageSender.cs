using System.Threading.Tasks;
using Server.Sockets.Messages;

namespace Server.Sockets.Other
{
	public interface IMessageSender
	{
		Task SendMessageAsync(IWebSocket socket, ISendMessage message);
	}
}