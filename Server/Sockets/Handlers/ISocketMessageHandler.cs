using System.Net.WebSockets;
using System.Threading.Tasks;
using Server.TicTacToe;

namespace Server.Sockets.Handlers
{
	public interface ISocketMessageHandler
	{
		Task HandleMessageAsync(IPlayer player, WebSocketReceiveResult result, byte[] buffer);
	}
}