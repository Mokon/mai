/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;
using System.Collections.Generic;
using System.IO;

namespace Net.Mokon.Edu.Unh.CS.AI.GridWorld {

  /// <summary>
  /// This is an implementation of the grid world loader which has its file
  /// format defined by the A1 info sheet.
  /// </summary>
  public class RandomGridWorldMaker {

    /// <summary>
    /// A constructor with LOS.
    /// </summary>
    /// <param name="LOS"></param>
    public RandomGridWorldMaker( int LOS, double Blocked, int Height, int Width ) {
      this.LOS = LOS;
      this.Blocked = Blocked;
      this.Width = Width;
      this.Height = Height;
    }

    /// <summary>
    /// The LOS of units read in.
    /// </summary>
    private int LOS;

    private double Blocked;

    private int Height;

    private int Width;

    /// <summary>
    /// The load method to parse the file format. This will throw a format
    /// exception if it fails.
    /// </summary>
    /// <param name="Stream">The stream to parse from.</param>
    /// <returns>The grid world.</returns>
    public void Make( int Seed, String Filename ) {
      Random Gen = new Random( Seed );
      TextWriter IO = new StreamWriter( File.Open( Filename, FileMode.Create ) );
      IO.WriteLine( Width );
      IO.WriteLine( Height );
      int UX =  Gen.Next( Width - 1 );
      int UY =  Gen.Next( Height - 1 );
      IO.WriteLine( UX + " " + UY + " " 
        + Gen.Next( Width - 1 ) + " " + Gen.Next( Height - 1 ) + " " + LOS );
      IO.WriteLine( "--- end units ---" );
      for ( int Y = 0 ; Y < Height ; Y++ ) {
        for ( int X = 0 ; X < Width ; X++ ) {
          if ( Y == UY && X == UX ) {
            IO.Write( (char)GridWorldFormat.TileKind.UNIT );
          } else {
            IO.Write( Gen.NextDouble( ) > this.Blocked ?
              (char)GridWorldFormat.TileKind.BLANK :
              (char)GridWorldFormat.TileKind.BLOCKED );
          }

        }
        IO.WriteLine( );
      }
      IO.Close( );

    }

  }

}
