/*
 * This file is part of the Mokon.Net Utilties Package,
 * distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;

namespace Net.Mokon.Utilities {

  /// <summary>
  /// This is a utility class to compute distances between points.
  /// </summary>
  public static class Distance {

    /// <summary>
    /// Computes the euclian distance between two points.
    /// </summary>
    /// <param name="X1">P1.X</param>
    /// <param name="X2">P2.X</param>
    /// <param name="Y1">P1.Y</param>
    /// <param name="Y2">P2.Y</param>
    /// <returns>The euclian distance between those two points.</returns>
    public static double EuclianDistance( int X1, int X2, int Y1, int Y2 ) {
      return ( Math.Sqrt(
                  Math.Pow( System.Convert.ToDouble( X1 - X2 ), 2 ) +
                  Math.Pow( System.Convert.ToDouble( Y1 - Y2 ), 2 ) )
             );
    }

    private static readonly double ROOT_TWO = System.Math.Sqrt( 2.0d );
    private static readonly double ROOT_TWO_M1 = ROOT_TWO - 1 ;

    public static double OctileDistance( int x1, int x2, int y1, int y2 ) {
      double answer = 0.0;
      if ( Math.Abs( x1-x2 ) < Math.Abs( y1-y2 ) )
        answer = ROOT_TWO_M1*Math.Abs( x1-x2 )+Math.Abs( y1-y2 );
      else
        answer = ROOT_TWO_M1*Math.Abs( y1-y2 )+Math.Abs( x1-x2 );
      return answer;
    }

    /// <summary>
    /// Computes the manhattan distance between two points.
    /// </summary>
    /// <param name="X1">P1.X</param>
    /// <param name="X2">P2.X</param>
    /// <param name="Y1">P1.Y</param>
    /// <param name="Y2">P2.Y</param>
    /// <returns>The manhattan distance between those two points.</returns>
    public static double ManhattanDistance( int X1, int X2, int Y1, int Y2 ) {
      return ( System.Math.Abs( System.Convert.ToDouble( X1 - X2 ) )
        + System.Math.Abs( System.Convert.ToDouble( Y1 - Y2 ) ) );
    }

  }

}
