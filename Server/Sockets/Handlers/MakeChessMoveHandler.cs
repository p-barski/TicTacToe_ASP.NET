using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Server.Games;
using Server.Games.Chess;
using Server.Sockets.Other;
using Server.Sockets.Messages;

namespace Server.Sockets.Handlers
{
	public class MakeChessMoveHandler : IMessageHandler
	{
		private readonly ICollections collections;
		private readonly ILogger<MakeChessMoveHandler> logger;
		private readonly IMessageSender messageSender;
		public MakeChessMoveHandler(ICollections collections,
			ILogger<MakeChessMoveHandler> logger, IMessageSender messageSender)
		{
			this.collections = collections;
			this.logger = logger;
			this.messageSender = messageSender;
		}
		public async Task HandleMessageAsync(IPlayer player, IReceivedMessage msg)
		{
			IGameMove chessGameMove = null;
			if (msg is MakeChessMoveMessage)
			{
				var chessMoveMsg = (MakeChessMoveMessage)msg;
				chessGameMove = new ChessGameMove(
					chessMoveMsg.X_StartPosition, chessMoveMsg.Y_StartPosition,
					chessMoveMsg.X_FinishedPosition, chessMoveMsg.Y_FinishedPosition);
			}
			else
			{
				var chessMoveMsg = (PawnPromotionMessage)msg;
				chessGameMove = new PromotionMove(chessMoveMsg.PromotionPiece);
			}

			ChessGameSession session;
			try
			{
				session = (ChessGameSession)collections.FindSessionOfAPlayer(player);
			}
			catch (InvalidOperationException)
			{
				var msgText = "This player is not connected to any game session.";
				await messageSender.SendMessageAsync(player.Socket,
					new InvalidStateMessage(msgText));
				return;
			}

			var result = session.Play(player, chessGameMove);
			switch (result)
			{
				case PlayResult.Success:
				case PlayResult.Check:
					await SendToBothAsync(session, player, result);
					break;
				case PlayResult.Draw:
					await SendStalemateAsync(session);
					collections.RemoveSession(session);
					break;
				case PlayResult.YouWin:
					await SendGameFinishedAsync(session, player);
					collections.RemoveSession(session);
					break;
				case PlayResult.PromotionRequired:
					await SendPromotionAsync(player);
					break;
				default:
					await SendErrorMsgToSenderAsync(player, result);
					break;
			}
		}
		private async Task SendToBothAsync(ChessGameSession session, IPlayer sender,
			PlayResult result)
		{
			var otherPlayer = GetOtherPlayer(session, sender);
			var msgToSend = new ChessPlayResultMessage()
			{
				Message = result.ToString()
			};

			await messageSender.SendMessageAsync(sender.Socket, msgToSend);
			msgToSend.IsClientTurn = true;
			await messageSender.SendMessageAsync(otherPlayer.Socket, msgToSend);

			await SendChessPiecesAndMovesMessageAsync(session);
		}
		private async Task SendErrorMsgToSenderAsync(IPlayer player, PlayResult result)
		{
			var msgToSend = new ChessPlayResultMessage() { Message = result.ToString() };
			await messageSender.SendMessageAsync(player.Socket, msgToSend);
		}
		private async Task SendGameFinishedAsync(ChessGameSession session,
			IPlayer winner)
		{
			var loser = GetOtherPlayer(session, winner);

			var msgToWinner = new ChessPlayResultMessage()
			{
				Message = PlayResult.YouWin.ToString()
			};
			var msgToLoser = new ChessPlayResultMessage()
			{
				Message = PlayResult.YouLose.ToString()
			};

			await messageSender.SendMessageAsync(winner.Socket, msgToWinner);
			await messageSender.SendMessageAsync(loser.Socket, msgToLoser);

			await SendChessPiecesAndMovesMessageAsync(session);
		}
		private async Task SendStalemateAsync(ChessGameSession session)
		{
			var stalemateMsg = new ChessPlayResultMessage()
			{
				Message = PlayResult.Draw.ToString()
			};

			await messageSender.SendMessageAsync(session.PlayerOne.Socket, stalemateMsg);
			await messageSender.SendMessageAsync(session.PlayerTwo.Socket, stalemateMsg);
			await SendChessPiecesAndMovesMessageAsync(session);
		}
		private async Task SendPromotionAsync(IPlayer player)
		{
			var promotionMsg = new ChessPlayResultMessage()
			{
				Message = PlayResult.PromotionRequired.ToString(),
				IsClientTurn = true,
				IsPromotionRequired = true
			};

			await messageSender.SendMessageAsync(player.Socket, promotionMsg);
		}
		private async Task SendChessPiecesAndMovesMessageAsync(ChessGameSession session)
		{
			var movesAndPiecesMessage = new ChessPiecesAndMovesMessage()
			{
				AvailableMoves = session.GetAvailableMoves(),
				Pieces = session.GetAvailablePieces()
			};

			await messageSender.SendMessageAsync(session.PlayerOne.Socket, movesAndPiecesMessage);
			await messageSender.SendMessageAsync(session.PlayerTwo.Socket, movesAndPiecesMessage);
		}
		private IPlayer GetOtherPlayer(IGameSession session, IPlayer player)
		{
			if (session.PlayerOne == player)
			{
				return session.PlayerTwo;
			}
			return session.PlayerOne;
		}
	}
}