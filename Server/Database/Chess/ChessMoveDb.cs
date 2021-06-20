using System;

namespace Server.Database.Chess
{
	public class ChessMoveDb
	{
		public int Id { get; private set; }
		public int X_StartPosition { get; private set; }
		public int Y_StartPosition { get; private set; }
		public int X_FinishedPosition { get; private set; }
		public int Y_FinishedPosition { get; private set; }
		public string PawnPromotion { get; private set; }
		public DateTime Date { get; private set; }
		public ChessMoveDb() { }
		public ChessMoveDb(int xStartPosition,
			int yStartPosition, int xFinishedPosition,
			int yFinishedPosition, string pawnPromotion,
			DateTime date)
		{
			X_StartPosition = xStartPosition;
			Y_StartPosition = yStartPosition;
			X_FinishedPosition = xFinishedPosition;
			Y_FinishedPosition = yFinishedPosition;
			PawnPromotion = pawnPromotion;
			Date = date;
		}
	}
}