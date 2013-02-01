/*
 * This file is part of the Mokon.Net Utilties Package,
 * distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;
using System.Collections.Generic;

namespace Net.Mokon.Utilities {

  /// <summary>
  /// This is a simple tuple class which allows for multiple elements to be passed as
  /// one object. The Tuple class is comparable and must contain comparable elements.
  /// The STuple is not and can contain anything.
  /// </summary>
  public class Tuple<T> : IComparable where T : IComparable {
    public Tuple( T first ) {
      First = first;
    }
    public T First {
      get;
      set;
    }

    virtual public int CompareTo( object obj ) {
      if( obj is Tuple<T> ) {
        Tuple<T> o = (Tuple<T>)obj;
        return o.First.CompareTo( First );
      }
      throw new ArgumentException( "" );
    }
  }
  public class Tuple<T, T2> : Tuple<T>
    where T : IComparable
    where T2 : IComparable {
    public Tuple( T first, T2 second )
      : base( first ) {
      Second = second;
    }
    public T2 Second {
      get;
      set;
    }

    override public int CompareTo( object obj ) {
      if( obj is Tuple<T, T2> ) {
        Tuple<T, T2> o = (Tuple<T, T2>)obj;
        int i = -base.CompareTo( new Tuple<T>( o.First ) );
        if( i == 0 ) {
          return o.Second.CompareTo( Second );
        } else {
          return i;
        }
      }
      throw new ArgumentException( "" );
    }
    public override string ToString( ) {
      return "< " + First + ", " + Second + " > ";
    }

    public override bool Equals( object obj ) {
      bool ret = this.First.Equals( ( obj as Tuple<T, T2> ).First ) &&
        this.Second.Equals( ( obj as Tuple<T, T2> ).Second );
      return ret;
    }

    public override int GetHashCode( ) {
      return this.First.GetHashCode( ) ^ this.Second.GetHashCode( );
    }
  }
  public class Tuple<T, T2, T3> : Tuple<T, T2>
    where T : IComparable
    where T2 : IComparable
    where T3 : IComparable {
    public Tuple( T first, T2 second, T3 third )
      : base( first, second ) {
      Third = third;
    }
    public T3 Third {
      get;
      set;
    }

    override public int CompareTo( object obj ) {
      if( obj is Tuple<T, T2, T3> ) {
        Tuple<T, T2, T3> o = (Tuple<T, T2, T3>)obj;
        int i = base.CompareTo( new Tuple<T, T2>( o.First, o.Second ) );
        if( i == 0 ) {
          return o.Third.CompareTo( Third );
        } else {
          return i;
        }
      }
      throw new ArgumentException( "" );
    }
  }
  public class Tuple<T, T2, T3, T4> : Tuple<T, T2, T3>
    where T : IComparable
    where T2 : IComparable
    where T3 : IComparable
    where T4 : IComparable {
    public Tuple( T first, T2 second, T3 third, T4 fourth )
      : base( first, second, third ) {
      Fourth = fourth;
    }
    public T4 Fourth {
      get;
      set;
    }

    override public int CompareTo( object obj ) {
      if( obj is Tuple<T, T2, T3, T4> ) {
        Tuple<T, T2, T3, T4> o = (Tuple<T, T2, T3, T4>)obj;
        int i = base.CompareTo( new Tuple<T, T2, T3>( o.First, o.Second, o.Third ) );
        if( i == 0 ) {
          return o.Fourth.CompareTo( Fourth );
        } else {
          return i;
        }
      }
      throw new ArgumentException( "" );
    }
  }

  public class STuple<T> {
    public STuple( T first ) {
      First = first;
    }
    public T First {
      get;
      set;
    }
  }

  public class STuple<T, T2> : STuple<T> {
    public STuple( T first, T2 second )
      : base( first ) {
      Second = second;
    }
    public T2 Second {
      get;
      set;
    }
  }

  public class STuple<T, T2, T3> : STuple<T, T2> {
    public STuple( T first, T2 second, T3 third )
      : base( first, second ) {
      Third = third;
    }
    public T3 Third {
      get;
      set;
    }   
  }

}
