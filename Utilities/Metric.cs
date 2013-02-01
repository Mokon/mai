/*
 * This file is part of the Mokon.Net Utilties Package,
 * distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;

namespace Net.Mokon.Utilities {

  /// <summary>
  /// A floating point comparable class.
  /// </summary>
  public class Metric : IComparable<Metric> {

    public static Metric Min( Metric Arg1, Metric Arg2 ) {
      return new Metric( Math.Min( Arg1.Value, Arg2.Value ) );
    }

    private readonly double Value;

    public Metric( double Value ) {
      this.Value = Value;
    }

    public override string ToString( ) {
      return this.Value.ToString( );
    }

    public int CompareTo( Metric o ) {
      if ( this.Equals( o ) ) {
        return 0;
      } else if ( this < o ) {
        return -1;
      } else {
        return 1;
      }
    }

    public override bool Equals( object obj ) {
      if ( obj is Metric ) {
        Metric o = obj as Metric;
        if ( ( o.Value.Equals( double.PositiveInfinity ) && this.Value.Equals( double.PositiveInfinity ) ) ||
          ( o.Value.Equals( double.NegativeInfinity ) && this.Value.Equals( double.NegativeInfinity ) ) ) {
          return true;
        } else {
          return Math.Abs( ( (Metric)obj ).Value - this.Value ) < 0.0000001;
        }
      } else {
        return false;
      }
    }

    public override int GetHashCode( ) {
      return (int)Value;
    }

    public static readonly Metric NegativeInfinity = new Metric( double.NegativeInfinity );
    public static readonly Metric PositiveInfinity = new Metric( double.PositiveInfinity );
    public static readonly Metric Infinity = new Metric( double.PositiveInfinity );
    public static readonly Metric One = new Metric( 1.0d );
    public static readonly Metric MaxValue = new Metric( double.MaxValue );
    public static readonly Metric Zero = new Metric( 0.0d );

    public static bool operator>( Metric arg1, Metric arg2 ) {
      return arg1.Value > arg2.Value && !arg1.Equals( arg2 );
    }

    public static bool operator==( Metric arg1, Metric arg2 ) {
      if ( System.Object.ReferenceEquals( arg1, arg2 ) ) {
        return true;
      }
      if ( ( (object)arg1 == null ) || ( (object)arg2 == null ) ) {
        return false;
      }
      return arg1.Equals( arg2 );
    }

    public static bool operator!=( Metric arg1, Metric arg2 ) {
      if ( System.Object.ReferenceEquals( arg1, arg2 ) ) {
        return false;
      }
      if ( ( (object)arg1 == null ) || ( (object)arg2 == null ) ) {
        return true;
      }

      return !arg1.Equals( arg2 );
    }

    public static bool operator<( Metric arg1, Metric arg2 ) {
      return arg1.Value < arg2.Value && !arg1.Equals( arg2 );
    }

    public static bool operator>=( Metric arg1, Metric arg2 ) {
      return arg1.Value > arg2.Value || arg1.Equals( arg2 );
    }

    public static bool operator<=( Metric arg1, Metric arg2 ) {
      return arg1.Value < arg2.Value || arg1.Equals( arg2 );
    }

    public static Metric operator+( Metric arg1, Metric arg2 ) {
      return new Metric( arg1.Value + arg2.Value );
    }

    public static Metric operator-( Metric arg1, Metric arg2 ) {
      return new Metric( arg1.Value - arg2.Value );
    }

    public static Metric operator-( Metric arg1 ) {
      return new Metric( -arg1.Value );
    }

    public static Metric operator+( Metric arg1 ) {
      return new Metric( +arg1.Value );
    }

    public static Metric operator++( Metric arg1 ) {
      return new Metric( arg1.Value+1 );
    }


    public static Metric operator--( Metric arg1 ) {
      return new Metric( arg1.Value-1 );
    }

    public static Metric operator*( Metric arg1, Metric arg2 ) {
      return new Metric( arg1.Value * arg2.Value );
    }

    public static Metric operator/( Metric arg1, Metric arg2 ) {
      return new Metric( arg1.Value / arg2.Value );
    }
    public double ToDouble( ) {
      return this.Value;
    }
  }
}
