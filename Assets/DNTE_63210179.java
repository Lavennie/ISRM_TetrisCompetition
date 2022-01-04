import java.util.Scanner;
import java.lang.Math;

public class DNTE_63210179 {
	
	public static void main(String[] args) {
		Scanner sc = new Scanner(System.in);
		byte[] sequence = new byte[sc.nextShort()];
		for (int i = 0; i < sequence.length; i++) {
			sequence[i] = sc.nextByte();
		}
		Tetris tetris = new Tetris(sequence);
		tetris.calculateDrops();
	}
	
	public static class Tetris {
		// common variable names
		// x - grid x pos
		// y - grid y pos
		// p - piece type index { 0, 1, 2, 3, 4, 5, 6 }
		// 0 - orientation of piece {0, 1, 2, 3}
		// i - index of piece in sequence
		private Piece[] pieces = new Piece[]
		{
			new Piece(
				new byte[] { 0, 1, 0, 1 }, 
				new byte[][] { new byte[] { 0, 0, 0, 0 }, new byte[] { 0 } },
				new byte[][] { new byte[] { 1, 1, 1, 1 }, new byte[] { 4 } },
				new byte[][] { new byte[] { 0, 0, 0 }, new byte[] { } }, 
				new byte[] { 0, -2 },
				new byte[] { 0, 2 }),
			new Piece(
				new byte[] { 0, 1, 2, 3 },
				new byte[][] { new byte[] { 0, 0, 0 }, new byte[] { 0, 2 }, new byte[] { 1, 1, 0 }, new byte[] { 0, 0 } },
				new byte[][] { new byte[] { 2, 1, 1 }, new byte[] { 3, 1 }, new byte[] { 1, 1, 2 }, new byte[] { 1, 3 } },
				new byte[][] { new byte[] { 0, 0 }, new byte[] { 2 }, new byte[] { 0, -1 }, new byte[] { 0 } },
				new byte[] { 0, 0, 0, 0 },
				new byte[] { 0, 0, 0, 0 }),
			new Piece(
				new byte[] { 0, 1, 2, 3 },
				new byte[][] { new byte[] { 0, 0, 0 }, new byte[] { 0, 0 }, new byte[] { 0, 1, 1 }, new byte[] { 2, 0 } },
				new byte[][] { new byte[] { 1, 1, 2 }, new byte[] { 3, 1 }, new byte[] { 2, 1, 1 }, new byte[] { 1, 3 } },
				new byte[][] { new byte[] { 0, 0 }, new byte[] { 0 }, new byte[] { 1, 0 }, new byte[] { -2 } },
				new byte[] { 0, 0, 0, 0 },
				new byte[] { 0, 0, 0, 0 }),
			new Piece(
				new byte[] { 0, 0, 0, 0 },
				new byte[][] { new byte[] { 0, 0 } },
				new byte[][] { new byte[] { 2, 2 } },
				new byte[][] { new byte[] { 0 } },
				new byte[] { 0 },
				new byte[] { 0 }),
			new Piece(
				new byte[] { 0, 1, 0, 1 },
				new byte[][] { new byte[] { 0, 0, 1 }, new byte[] { 1, 0 } },
				new byte[][] { new byte[] { 1, 2, 1 }, new byte[] { 2, 2 } },
				new byte[][] { new byte[] { 0, 1 }, new byte[] { -1 } },
				new byte[] { 0, 0 },
				new byte[] { 0, 0 }),
			new Piece(
				new byte[] { 0, 1, 2, 3 },
				new byte[][] { new byte[] { 0, 0, 0 }, new byte[] { 0, 1 }, new byte[] { 1, 0, 1 }, new byte[] { 1, 0 } },
				new byte[][] { new byte[] { 1, 2, 1 }, new byte[] { 3, 1 }, new byte[] { 1, 2, 1 }, new byte[] { 1, 3 } },
				new byte[][] { new byte[] { 0, 0 }, new byte[] { 1 }, new byte[] { -1, 1 }, new byte[] { -1 } },
				new byte[] { 0, -1, 0, 0 },
				new byte[] { 0, 0, 0, 1 }),
			new Piece(
				new byte[] { 0, 1, 0, 1 },
				new byte[][] { new byte[] { 1, 0, 0 }, new byte[] { 0, 1 } },
				new byte[][] { new byte[] { 1, 2, 1 }, new byte[] { 2, 2 } },
				new byte[][] { new byte[] { -1, 0 }, new byte[] { 1 } },
				new byte[] { 0, 0 },
				new byte[] { 0, 0 }),
		};
		
		public final float POINTS_CORRECT_SHAPE_MUL = 2.5f;
		public final float POINTS_BAD_SHAPE_MUL = -3.25f;
		public final float POINTS_SIDE_SHAPE_MUL = 1.25f;
		public final float POINTS_CLEAR_LINE_MUL = 3.75f;
		public final float POINTS_HEIGHT_MUL = -2.5f;
		
		private byte[] sequence;
		public short maxHeight = 0;
		private boolean[][] map = new boolean[10][4001];
		
		public Tetris(byte[] sequence) {
			this.sequence = sequence;
		}
		
		public void calculateDrops() {
			for (short i = 0; i < sequence.length; i++) {
				SimulatePieceResult res = this.simulatePiece(i);
				this.drop(i, res.getX(), res.getY(), res.getO());
				System.out.println(res.getO() + " " + res.getX());
			}
		}
		private void drop(short i, byte x, short y, byte o) {
			short newMaxHeight = 0;
			// update map and max height
			for (byte mx = 0; mx < pieces[sequence[i]].width(o); mx++) {
				for (byte my = 0; my < pieces[sequence[i]].height(mx, o); my++) {
					map[x + mx][y + pieces[sequence[i]].min(mx, o) + my] = true;
					newMaxHeight = (short)Math.max(newMaxHeight, y + pieces[sequence[i]].min(mx, o) + my + 1);
				}
			}
			
			// clear rows
			for (short my = y; my < newMaxHeight; my++) {
				if (this.isRowFull(my)) {
					// clear line my
					for (short ny = (short)(my + 1); ny < Math.max(maxHeight, newMaxHeight); ny++) {
						for (byte nx = 0; nx < 10; nx++) {
							map[nx][ny - 1] = map[nx][ny];
							map[nx][ny] = false;
						}
					}
					my--;
					newMaxHeight--;
				}
			}
			maxHeight = (short)Math.max(maxHeight, newMaxHeight);
		}
		
		public short getHeight(byte x) {
			for (short y = maxHeight; y >= 0; y--) {
				if (map[x][y]) {
					return (short)(y + 1);
				}
			}
			return 0;
		}
		public short getDerivative(byte x) {
			// |     0  1  2  3  4  5  6  7  8  9     |
			//  d0:-4 d1 d2 d3 d4 d5 d6 d7 d8 d9 d10:4
			if(x == 0) {
				return -4;
			}
			else if (x == 10) {
				return 4;
			}
			else {
				return (short)(this.getHeight(x) - this.getHeight((byte)(x - 1)));
			}
		}
		
		private boolean isRowFull(short y) {
			for (byte x = 0; x < 10; x++) {
				if (!map[x][y]) {
					return false;
				}
			}
			return true;
		}
		private boolean willRowBeFull(short y, byte p, byte o, byte px, short py) {
			for (byte x = 0; x < 10; x++) {
				if (!map[x][y] && !pieces[p].hasSolid((byte)(x - px), (byte)(y - py), o))
				{
					return false;
				}
			}
			return true;
		}
		
		private SimulatePieceResult simulatePiece(short i) {
			byte dropX = 0;
			short dropY = 0;
			byte dropO = 0;
			float dropPoints = -Float.MAX_VALUE;
			for (byte x = 0; x < 10; x++) {
				for (byte o = 0; o < 4; o++) {
					SimulateDropResult res = this.simulateDrop(x, o, sequence[i]);
					if (res.getSuccess() && res.getPoints() > dropPoints) {
						dropPoints = res.getPoints();
						dropX = x;
						dropY = res.getY();
						dropO = o;
					}
				}
			}
			return new SimulatePieceResult(dropX, dropY, dropO, dropPoints);
		}
		private SimulateDropResult simulateDrop(byte x, byte o, byte p) {
			if (x + pieces[p].width(o) - 1 >= 10) { return new SimulateDropResult((short)0, -Float.MAX_VALUE, false); }
			short dropY = 0;
			float dropPoints = 0;
			for (byte mx = 0; mx < pieces[p].width(o); mx++) {
				dropY = (short)Math.max(dropY, this.getHeight((byte)(x + mx)) - pieces[p].min(mx, o));
			}
			
			// add point for row clears
			for (short my = dropY; my < dropY + 4; my++) {
				if (this.willRowBeFull(my, p, o, x, dropY)) {
					dropPoints += this.POINTS_CLEAR_LINE_MUL;
				}
			}
			// deduct points for higher height
			dropPoints += dropY * this.POINTS_HEIGHT_MUL;
			// add/deduct points for following shape (not leaving holes)
			dropPoints += pieces[p].points(this, x, dropY, o);
			
			return new SimulateDropResult(dropY, dropPoints, true);
		}
	}
	public static class Piece {
		private byte[] oTOi;
		private byte[][] mins;
		private byte[][] heights;
		private byte[][] derivatives;
		private byte[] lefts;
		private byte[] rights;
		
		public Piece(byte[] orientationToIndex, byte[][] mins, byte[][] heights, byte[][] derivatives, byte[] lefts, byte[] rights) {
			this.oTOi = orientationToIndex;
			this.mins = mins;
			this.heights = heights;
			this.derivatives = derivatives;
			this.lefts = lefts;
			this.rights = rights;
		}
		
		public byte min(byte x, byte o) {
			return mins[oTOi[o]][x];
		}
		public byte width(byte o) {
			return (byte)mins[oTOi[o]].length;
		}
		public byte height(byte x, byte o) {
			return this.heights[oTOi[o]][x];
		}
		
		public boolean hasSolid(byte x, byte y, byte o) {
			if (x < 0 || x >= this.width(o)) { return false; }
			return y - this.min(x, o) < this.height(x, o);
		}
		public float points(Tetris map, byte x, short y, byte o) {
			return shapePointsBase(map, x, y, o) + shapePointsSide(map, x, y, o);
		}
		
		private float shapePointsBase(Tetris map, byte x, short y, byte o) {
			if (x + derivatives[oTOi[o]].length >= 10) { return -Float.MAX_VALUE; }
			float score = 0;
			short d = (short)(y + this.min((byte)0, o) - map.getHeight(x));
			for (byte j = 0; j < derivatives[oTOi[o]].length; j++) {
				d = (short)(map.getDerivative((byte)(x + j + 1)) - derivatives[oTOi[o]][j] + d);
				if (d == 0) {
					score += map.POINTS_CORRECT_SHAPE_MUL;
				}
				else {
					score += Math.abs(d) * map.POINTS_BAD_SHAPE_MUL;
				}
			}
			return score;
		}
		private float shapePointsSide(Tetris map, byte x, short y, byte o) {
			short sL = (short)(map.getDerivative(x) + (y + this.min((byte)0, o) - map.getHeight(x)));
			short sR = (short)(map.getDerivative((byte)(x + this.width(o))) - (y + this.min((byte)(this.width(o) - 1), o) - map.getHeight((byte)(x + this.width(o) - 1))));
			return (Math.max(0, -sL + lefts[oTOi[o]]) + Math.max(0, sR - rights[oTOi[o]])) * map.POINTS_SIDE_SHAPE_MUL;
		}
	}
	
	public static class SimulateDropResult {
		private short y;
		private float points;
		private boolean success;
		
		public SimulateDropResult(short y, float points, boolean success) {
			this.y = y;
			this.points = points;
			this.success = success;
		}
		
		public short getY() { return y; }
		public float getPoints() { return points; }
		public boolean getSuccess() { return success; }
	}
	public static class SimulatePieceResult extends SimulateDropResult {
		private byte x;
		private byte o;
		
		public SimulatePieceResult(byte x, short y, byte o, float points) {
			super(y, points, true);
			this.x = x;
			this.o = o;
		}
		
		public byte getX() { return x; }
		public byte getO() { return o; }
	}
}