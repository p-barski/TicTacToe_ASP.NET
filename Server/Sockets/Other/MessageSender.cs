using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Server.Sockets.Messages;

namespace Server.Sockets.Other
{
	public class MessageSender : IMessageSender
	{
		private readonly ILogger<MessageSender> logger;
		private readonly IMessageDeserializer deserializer;

		public MessageSender(ILogger<MessageSender> logger,
			IMessageDeserializer deserializer)
		{
			this.logger = logger;
			this.deserializer = deserializer;
		}
		public async Task SendMessageAsync(IWebSocket socket, ISendMessage message)
		{
			var buffer = deserializer.SerializeToBuffer(message);
			logger.LogInformation($"Sending msg: {message.GetType()}");
			if (socket.State == WebSocketState.Open)
				await socket.SendAsync(buffer, WebSocketMessageType.Text,
					true, CancellationToken.None);
		}
	}
}