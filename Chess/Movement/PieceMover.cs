using System.Linq;
using System.Collections.Generic;
using Chess.Pieces;

namespace Chess.Movement
{
	public class PieceMover : IPieceMover
	{
		public IReadOnlyMovementHistory History => history;
		private readonly IMovementHistory history;
		private readonly IPiecePromoter piecePromoter;
		private readonly ICastlingMover castlingMover;
		private readonly IEnPassantMover enPassantMover;
		public PieceMover(IMovementHistory history, IPiecePromoter piecePromoter,
			ICastlingMover castlingMover, IEnPassantMover enPassantMover)
		{
			this.history = history;
			this.piecePromoter = piecePromoter;
			this.castlingMover = castlingMover;
			this.enPassantMover = enPassantMover;
		}
		public IChessPiece Move(ChessMove chessMove, IEnumerable<IChessPiece> pieces)
		{
			if (piecePromoter.PromoteIfPromotionMove(chessMove, pieces))
			{
				return null;
			}
			if (castlingMover.PerformCastlingIfCastlingMove(chessMove, pieces))
			{
				return null;
			}

			var pieceToRemove = enPassantMover.PerformEnPassantIfApplicable(chessMove, pieces);
			if (pieceToRemove != null)
			{
				return pieceToRemove;
			}

			var pieceToMove = pieces
				.First(p => p.Position == chessMove.StartingPosition);
			pieceToRemove = pieces
				.FirstOrDefault(p => p.Position == chessMove.FinishedPosition);
			pieceToMove.Position = chessMove.FinishedPosition;
			pieceToMove.IncrementMoveCounter();
			if (pieceToRemove != null)
			{
				chessMove = chessMove.ReturnWithCaptureAsTrue();
			}
			history.Add(chessMove);
			return pieceToRemove;
		}
		public bool ReverseLastMove(IEnumerable<IChessPiece> pieces)
		{
			var chessMove = history.RemoveLastMove();
			if (castlingMover.UndoCastlingIfCastlingMove(chessMove, pieces))
			{
				return false;
			}
			chessMove = piecePromoter
				.DepromoteIfPromotionMove(chessMove, pieces);
			var pieceMovedPreviously = pieces
				.First(p => p.Position == chessMove.FinishedPosition);
			pieceMovedPreviously.Position = chessMove.StartingPosition;
			pieceMovedPreviously.DecrementMoveCounter();
			return chessMove.IsCapture;
		}
	}
}