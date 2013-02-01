/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;
using System.Collections.Generic;
using System.Linq;
using Net.Mokon.Edu.Unh.CS.AI.Search;

namespace Net.Mokon.Edu.Unh.CS.AI.GridWorld {

  /// <summary>
  /// This class is an extension of the dynamic state class for use in the grid
  /// world domain.
  /// </summary>
  [Serializable]
  public abstract class GridWorldDynamicState<SS, DS> : DynamicState<SS, DS>
    where SS : GridWorldStaticState<SS, DS>
    where DS : GridWorldDynamicState<SS, DS> {

    /// <summary>
    /// Constructor
    /// </summary>
    public GridWorldDynamicState( ) {
      this.WorldObjects = new HashSet<GridWorldObject<SS, DS>>( );
    }

    /// <summary>
    /// Finds a unit with the given UID and returns that unit.
    /// </summary>
    /// <param name="UID"></param>
    /// <returns></returns>
    public Unit FindUnit( int UID ) {
      foreach ( var obj in this.WorldObjects ) {
        if ( obj is Unit && ( obj as Unit ).UID == UID ) {
          return obj as Unit;
        }
      }
      throw new ArgumentException( );
    }

    /// <summary>
    /// A dictionary of key and grid world objects. A dictionary converts the
    /// given key into a hash value and therefore has the same properties of a
    /// hash table.
    /// </summary>
    protected HashSet<GridWorldObject<SS, DS>> WorldObjects;

    /// <summary>
    /// A method to get to the world objects. This should be done with care and
    /// the world object should NOT be altered at all.
    /// </summary>
    /// <returns></returns>
    protected internal HashSet<GridWorldObject<SS, DS>> GetWorldObjects( ) {
      return this.WorldObjects;
    }

    /// <summary>
    /// Adds the given world object to the world.
    /// </summary>
    /// <param name="GWO">The object to add.</param>
    public virtual void AddWorldObject( GridWorldObject<SS, DS> GWO ) {
      this.hash ^= GWO.GetHashCode( );
      this.WorldObjects.Add( GWO );
    }

    /// <summary>
    /// Removes the world object based on the given key.
    /// </summary>
    /// <param name="Key">The key value of the object to remove.</param>
    /// <returns>Whether or not that object was succesfull removed.</returns>
    public virtual bool RemoveWorldObject( Type Type, int X, int Y ) {
      GridWorldObject<SS, DS> GWO =
        this.WorldObjects.First( x => x.X == X && x.Y == Y
        && x.GetType( ).Equals( Type ) );
      this.hash ^= GWO.GetHashCode( );
      return this.WorldObjects.Remove( GWO );
    }

    /// <summary>
    /// A tester function to see if the dynamic state has the object with the
    /// given key.
    /// </summary>
    /// <param name="Key">The key of the object being searched for.</param>
    /// <returns>Whether or not this object is in the state.</returns>
    public virtual bool ContainsWorldObject( Type Type, int X, int Y ) {
      try {
        this.WorldObjects.First( x => x.X == X && x.Y == Y
        && ( Type == null || x.GetType( ).Equals( Type ) ) );
        return true;
      } catch( InvalidOperationException ) {
        return false;
      }
    }

    public virtual bool ContainsWorldObject( int Exclude, int X, int Y ) {
      return this.WorldObjects.FirstOrDefault( x => x.X == X && x.Y == Y
        && x.UID != Exclude ) != null;
        
    }

    /// <summary>
    /// Returns a clone of the current state. I am not sure if this is legal
    /// with the current laws in the US but oh well. Cloning is fun.
    /// 
    /// NOTE!: This does not clone the book keeping.
    /// </summary>
    /// <returns>A clone.</returns>
    public override object Clone( ) {
      GridWorldDynamicState<SS, DS> GS = base.Clone( )
        as GridWorldDynamicState<SS, DS>;
      GS.WorldObjects = new HashSet<GridWorldObject<SS, DS>>( );
      GS.BookKeeping = null;
      foreach( var e in this.WorldObjects ) {
        GS.WorldObjects.Add( e.Clone( ) as GridWorldObject<SS, DS> );
      }
      return GS;
    }

  }

}
