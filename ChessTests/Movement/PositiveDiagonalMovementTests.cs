using System.Linq;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Chess;
using Chess.Pieces;
using Chess.Board;
using Chess.Movement;

namespace ChessTests
{
	public class PositiveDiagonalMovementTests
	{
		[Test]
		public void BishopCanMoveOnPositiveDiagonal()
		{
			//WB - white bishop
			//PM - possible move
			//7                      PM
			//6                   PM   
			//5                PM      
			//4             PM         
			//3          PM            
			//2       PM               
			//1    WB                  
			//0 PM                     
			//  0  1  2  3  4  5  6  7 
			var boardMock = new Mock<IChessBoard>(MockBehavior.Strict);
			var bishopMock = new Mock<IReadOnlyChessPiece>(MockBehavior.Strict);

			var bishopPosition = new Position(1, 1);

			var availableFinishPosition01 = new Position(0, 0);
			var availableFinishPosition02 = new Position(2, 2);
			var availableFinishPosition03 = new Position(3, 3);
			var availableFinishPosition04 = new Position(4, 4);
			var availableFinishPosition05 = new Position(5, 5);
			var availableFinishPosition06 = new Position(6, 6);
			var availableFinishPosition07 = new Position(7, 7);

			bishopMock
				.SetupGet(p => p.Color)
				.Returns(ChessColor.White);
			bishopMock
				.SetupGet(p => p.PieceType)
				.Returns(ChessPieceType.Bishop);
			bishopMock
				.SetupGet(p => p.Position)
				.Returns(bishopPosition);
			bishopMock
				.SetupGet(p => p.HasMoved)
				.Returns(true);

			boardMock
				.Setup(b => b.IsPositionTaken(It.IsAny<Position>()))
				.Returns(false);
			boardMock
				.Setup(b => b.IsEnemyOnPosition(It.IsAny<Position>(), It.IsAny<ChessColor>()))
				.Returns(false);

			var movement = new PositiveDiagonalMovement(boardMock.Object);
			var availableMoves = movement.GetAvailableMoves(bishopMock.Object);

			Assert.AreEqual(7, availableMoves.Count());
			Assert.IsTrue(availableMoves.All(m => m.StartingPosition == bishopPosition));

			var finishedPositions = availableMoves
				.Select(m => m.FinishedPosition)
				.ToList();

			Assert.Contains(availableFinishPosition01, finishedPositions);
			Assert.Contains(availableFinishPosition02, finishedPositions);
			Assert.Contains(availableFinishPosition03, finishedPositions);
			Assert.Contains(availableFinishPosition04, finishedPositions);
			Assert.Contains(availableFinishPosition05, finishedPositions);
			Assert.Contains(availableFinishPosition06, finishedPositions);
			Assert.Contains(availableFinishPosition07, finishedPositions);
		}

		[Test]
		public void QueenCanCaptureEnemiesButCantJumpThroughThem()
		{
			//BQ - black queen
			//PM - possible move
			//EP - enemy piece
			//7                        
			//6                      PM
			//5                   BQ   
			//4                PM      
			//3             PM         
			//2          EP            
			//1                        
			//0                        
			//  0  1  2  3  4  5  6  7 
			var boardMock = new Mock<IChessBoard>(MockBehavior.Strict);
			var queenMock = new Mock<IReadOnlyChessPiece>(MockBehavior.Strict);

			var queenPosition = new Position(6, 5);
			var enemyPosition = new Position(3, 2);

			var availableFinishPosition1 = new Position(4, 3);
			var availableFinishPosition2 = new Position(5, 4);
			var availableFinishPosition3 = new Position(7, 6);

			var availableFinishPositions = new List<Position>(){
				availableFinishPosition1, availableFinishPosition2,
				availableFinishPosition3
			};

			queenMock
				.SetupGet(p => p.Color)
				.Returns(ChessColor.Black);
			queenMock
				.SetupGet(p => p.PieceType)
				.Returns(ChessPieceType.Queen);
			queenMock
				.SetupGet(p => p.Position)
				.Returns(queenPosition);
			queenMock
				.SetupGet(p => p.HasMoved)
				.Returns(true);

			boardMock
				.Setup(b => b.IsPositionTaken(enemyPosition))
				.Returns(true);
			boardMock
				.Setup(b => b.IsPositionTaken(It.IsIn<Position>(availableFinishPositions)))
				.Returns(false);
			boardMock
				.Setup(b => b.IsEnemyOnPosition(enemyPosition, ChessColor.White))
				.Returns(true);
			boardMock
				.Setup(b => b.IsEnemyOnPosition(It.IsIn<Position>(availableFinishPositions),
					ChessColor.White))
				.Returns(false);

			var movement = new PositiveDiagonalMovement(boardMock.Object);
			var availableMoves = movement.GetAvailableMoves(queenMock.Object);

			Assert.AreEqual(4, availableMoves.Count());
			Assert.IsTrue(availableMoves.All(m => m.StartingPosition == queenPosition));

			var finishedPositions = availableMoves
				.Select(m => m.FinishedPosition)
				.ToList();

			Assert.Contains(availableFinishPosition1, finishedPositions);
			Assert.Contains(availableFinishPosition2, finishedPositions);
			Assert.Contains(availableFinishPosition3, finishedPositions);
			Assert.Contains(enemyPosition, finishedPositions);
		}
		[Test]
		public void QueenCantJumpThroughFriendlyPieces()
		{
			//WQ - white queen
			//PM - possible move
			//WP - white piece
			//7                        
			//6                        
			//5                WP      
			//4             PM         
			//3          WQ            
			//2       WP               
			//1                        
			//0                        
			//  0  1  2  3  4  5  6  7 
			var boardMock = new Mock<IChessBoard>(MockBehavior.Strict);
			var queenMock = new Mock<IReadOnlyChessPiece>(MockBehavior.Strict);

			var queenPosition = new Position(3, 3);
			var pawn1Position = new Position(2, 2);
			var pawn2Position = new Position(5, 5);

			var availableFinishPosition = new Position(4, 4);

			queenMock
				.SetupGet(p => p.Color)
				.Returns(ChessColor.White);
			queenMock
				.SetupGet(p => p.PieceType)
				.Returns(ChessPieceType.Queen);
			queenMock
				.SetupGet(p => p.Position)
				.Returns(queenPosition);
			queenMock
				.SetupGet(p => p.HasMoved)
				.Returns(true);

			boardMock
				.Setup(b => b.IsPositionTaken(pawn1Position))
				.Returns(true);
			boardMock
				.Setup(b => b.IsPositionTaken(pawn2Position))
				.Returns(true);
			boardMock
				.Setup(b => b.IsPositionTaken(availableFinishPosition))
				.Returns(false);
			boardMock
				.Setup(b => b.IsEnemyOnPosition(It.IsAny<Position>(), ChessColor.Black))
				.Returns(false);

			var movement = new PositiveDiagonalMovement(boardMock.Object);
			var availableMoves = movement.GetAvailableMoves(queenMock.Object);

			Assert.AreEqual(1, availableMoves.Count());
			Assert.IsTrue(availableMoves.All(m => m.StartingPosition == queenPosition));

			var finishedPositions = availableMoves
				.Select(m => m.FinishedPosition)
				.ToList();

			Assert.Contains(availableFinishPosition, finishedPositions);
		}
	}
}