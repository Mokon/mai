/*
 * This file is part of the Mokon.Net Utilties Package,
 * distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Net.Mokon.Utilities {

  /// <summary>
  /// The .NET framework does not have a good pQueue implementation so this
  /// is a simple one.
  /// </summary>
  /// <typeparam name="PriorityType">The priority type, IE double, int etc.
  /// This is what the queue will be sorted based on.</typeparam>
  /// <typeparam name="DataType">The type of data to store.</typeparam>
  public class PriorityQueue<PriorityType, DataType> {


    public void AddToHashSet( HashSet<DataType> HashSet ) {
      foreach ( var p in Priorities ) {
        HashSet.UnionWith( p.Value );
      }
    }

    public static HashSet<DataType> MakeMacroHashSet( Dictionary<DataType, PriorityQueue<PriorityType, DataType>> OpenLists ) {
      HashSet<DataType> hss = new HashSet<DataType>( );
      foreach ( var ol in OpenLists ) {
        ol.Value.AddToHashSet( hss );
      }
      return hss;
    }

    public PriorityQueue<PriorityType, DataType> Reorder( Func<DataType,PriorityType> NewOrder ) {
      PriorityQueue<PriorityType, DataType> NewQueue = new PriorityQueue<PriorityType, DataType>( );
      while ( !this.IsEmpty( ) ) {
        DataType dt = this.Dequeue( );
        NewQueue.Enqueue( NewOrder( dt ), dt );
      }
      return NewQueue;
    }

    public PriorityType TopKey( ) {
      if ( IsEmpty( ) ) {
        return default( PriorityType );
      }
      var fElem = Priorities.First( );
      return fElem.Key;
    }

    public DataType Top( ) {
      if ( !Priorities.Any( ) ) {
        throw new InvalidOperationException( "Priority Queue Empty" );
      }
      var fElem = Priorities.First( );
      return fElem.Value.First( );
    }

    public void Insert( DataType dt, PriorityType pt ) {
      this.Enqueue( pt, dt );
    }

    public void Remove( DataType dt ) {
      bool found = false;
      PriorityType pt = default( PriorityType );
      foreach ( var List in Priorities ) {
        if ( List.Value.Contains( dt ) ) {
          if ( List.Value.Remove( dt ) ) {
            found = true;
            if ( List.Value.Count == 0 ) {
              pt = List.Key;
            }
            break;
          } else {
            throw new Exception( "PQ Value Remove Failed" );
          }
        } else {
          continue;
        }
      }
      if ( !found ) {
        throw new InvalidOperationException(
          "Priority Queue Does Not Contain " + dt );
      } else {
        if ( pt != null ) {
          if ( !Priorities.ContainsKey( pt ) ) {
            System.Console.WriteLine( "PQ Priority Remove Will Fail" );
          }

          if ( !Priorities.Keys.ToArray( ).Contains( pt ) ) {
            System.Console.WriteLine( "PQ Priority Remove Will Fail" );
          }
          if ( !Priorities.Remove( pt ) ) {
            throw new Exception( "PQ Priority Remove Failed" );
          }
        }
      }
    }

    public void Update( DataType dt, PriorityType pt ) {
      this.Remove( dt );
      this.Insert( dt, pt );
    }

    /// <summary>
    /// A hash table of the priorities.
    /// </summary>
    private SortedDictionary<PriorityType, HashSet<DataType>> Priorities;

    /// <summary>
    /// Constructor.
    /// </summary>
    public PriorityQueue( ) {
      this.Priorities = new SortedDictionary<PriorityType,
        HashSet<DataType>>( );
    }

    public bool Contains( PriorityType Priority, DataType ToCheck ) {
      HashSet<DataType> List = null;
      if ( !Priorities.TryGetValue( Priority, out List ) ) {
        return false;
      } else {
        return List.Contains( ToCheck );
      }
    }

    public bool Contains( DataType ToCheck ) {
      foreach ( var List in Priorities.Values ) {
        if ( List.Contains( ToCheck ) ) {
          return true;
        }
      }
      return false;
    }

    public bool Contains( string ToCheck ) {
      foreach ( var List in Priorities.Values ) {
        if ( List.Any( x => x.ToString( ).Equals( ToCheck ) ) ) {
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Adds to the pQueue.
    /// </summary>
    /// <param name="Priority">How to sort the value.</param>
    /// <param name="Value">The value to add.</param>
    public void Enqueue( PriorityType Priority, DataType Value ) {
      HashSet<DataType> List = null;
      if ( !Priorities.TryGetValue( Priority, out List ) ) {
        List = new HashSet<DataType>( );
        Priorities.Add( Priority, List );
      }
      if ( !List.Contains( Value ) ) {
        List.Add( Value );
      }
    }

    /// <summary>
    /// Removes the first element on the queue.
    /// </summary>
    /// <returns>The highest priorty (lowest number) value on the pQueue.
    /// </returns>
    public DataType Dequeue( ) {
      if ( IsEmpty( ) ) {
        throw new InvalidOperationException( "Priority Queue Empty" );
      }
      var fElem = Priorities.First( );
      if ( fElem.Value.Count == 0 ) {
        throw new InvalidOperationException( "Priority Queue ERROR" );
      }
      DataType _return = fElem.Value.First( );
      fElem.Value.Remove( _return );
      if ( fElem.Value.Count == 0 ) {
        if ( !Priorities.Remove( fElem.Key ) ) {
          System.Console.WriteLine( "fElem.Value is count 0 " + Priorities.Count );
          throw new InvalidOperationException( "PQ Priority Remove Failed" );
        }
      }
      return _return;
    }

    public DataType Get( PriorityType Pt, DataType Elem, Random Gen ) {
      HashSet<DataType> r;
      if ( Priorities.TryGetValue( Pt, out r ) ) {
        r = new HashSet<DataType>( r );
        r.Add( Elem );
      } else {
        r = new HashSet<DataType>( );
        r.Add( Elem );
      }
      int i = Gen.Next( r.Count );
      System.Console.WriteLine( "C = {1} i == {0}", i, r.Count );
      if ( r.Count == 2 ) {
        System.Console.WriteLine( "\t = {1} = {0}", r.ToArray( )[1], r.ToArray( )[0] );
      }
      return r.ToArray( )[i];
    }

    public STuple<PriorityType, DataType> DequeueNode( ) {
      if ( IsEmpty( ) ) {
        return null;
      }
      var fElem = Priorities.First( );
      if ( fElem.Value.Count == 0 ) {
        System.Console.WriteLine( "fElem.Value is count 0 " + Priorities.Count );
        throw new InvalidOperationException( "Priority Queue ERROR" );
      }
      var _return = fElem.Value.First( );
      fElem.Value.Remove( _return );
      if ( fElem.Value.Count == 0 ) {
        if ( !Priorities.Remove( fElem.Key ) ) {
          throw new Exception( "PQ Priority Remove Failed" );
        }
      }
      return new STuple<PriorityType, DataType>( fElem.Key, _return );
    }

    /// <summary>
    /// Test to see if the pQueue is empty.
    /// </summary>
    /// <returns>True IFF the pQueue does not contain any elements.</returns>
    public bool IsEmpty( ) {
      return Priorities.Count == 0;
    }

    /// <summary>
    /// A debugging method to convert the pQueue to a string given a lamba
    /// expression to convert the datatype elements to a string.
    /// </summary>
    /// <param name="Stringer"></param>
    /// <returns></returns>
    public string ToString( Func<PriorityType, DataType, String> Stringer ) {
      StringBuilder sb = new StringBuilder( );
      foreach ( var e in Priorities ) {
        foreach ( var i in e.Value ) {
          sb.Append( Stringer( e.Key, i ) );
          sb.Append( '\n' );
        }
      }
      return sb.ToString( );
    }

    public DataType[] ToArray( ) {
      LinkedList<DataType> list = new LinkedList<DataType>( );
      foreach ( var i in Priorities ) {
        foreach ( var j in i.Value ) {
          list.AddFirst( j );
        }
      }
      return list.ToArray( );
    }
  }
}