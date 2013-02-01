/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;
using System.Collections.Generic;
using System.Linq;
using Net.Mokon.Utilities;

namespace Net.Mokon.Edu.Unh.CS.AI.Search {

  /// <summary>
  /// An class which represents a set of actions which can be take upon
  /// a state space to get from an initial state to a goal state. A path does
  /// not store the orginal state and therefore if one wishes to recreate this
  /// the initial state must be stored elsewhere.
  /// </summary>
  public class Path<SS, DS>
    where SS : StaticState<SS, DS>
    where DS : DynamicState<SS, DS> {

    public override int GetHashCode( ) {
      return base.GetHashCode( );
    }

    public IEnumerable<Path<SS, DS>> GetIEnumerable( ) {
      return new Path<SS, DS>[] { this };
    }

    /// <summary>
    /// A list of the action to follow this path. These should be in the order
    /// front to last in order of time. In other words the first action in the
    /// list is the first action to take upon the initial state.
    /// </summary>
    internal LinkedList<Operator<SS, DS>> Actions;

    /// <summary>
    /// A list of the states in this path.
    /// </summary>
    internal LinkedList<DS> States;

    /// <summary>
    /// A hash table of states in this path.
    /// </summary>
    internal HashSet<DS> StateHashTable;

    /// <summary>
    /// Constructor of a path.
    /// </summary>
    /// <param name="Actions">The actions to perform the path.</param>
    public Path( DS InitialState ) {
      this.Actions = new LinkedList<Operator<SS, DS>>( );
      this.cost = Metric.Zero ;
      States = new LinkedList<DS>( );
      StateHashTable = new HashSet<DS>( );
      if ( InitialState != null ) {
        StateHashTable.Add( InitialState );
        States.AddFirst( InitialState );
      }
    }

    public Path( Operator<SS, DS> o ) : this((DS)null) {
      this.Actions.AddFirst( o );
    }

    /// <summary>
    /// Add an action to the end of the path.
    /// </summary>
    /// <param name="Action"></param>
    public void Push( Operator<SS, DS> Action, DS ToPush, DS newState ) {
      this.Actions.AddLast( Action );
      this.cost += Action.Cost( ToPush );
      States.AddLast( newState );
      StateHashTable.Add( newState );
    }

    /// <summary>
    /// Add an action to the end of the path.
    /// </summary>
    /// <param name="Action"></param>
    public void PushFirst( Operator<SS, DS> Action, DS ToPush, DS newState ) {
      this.Actions.AddFirst( Action );
      this.cost += Action.Cost( ToPush );
      States.AddFirst( newState );
      StateHashTable.Add( newState );
    }

    public Path<SS, DS> PopHead( ) {
      var ret = Operator<SS, DS>.MakePath( this.Actions.First( ) );
      this.Actions.RemoveFirst( ) ;
      this.StateHashTable.Remove( this.States.First.Value );
      this.States.RemoveFirst( );

      return ret;
    }

    /// <summary>
    /// Converts the path into a simple human readable string.
    /// </summary>
    /// <returns>A string representation of the path.</returns>
    public override string ToString( ) {
      if( this.Actions.Any( ) ) {
        return ( from x in Actions
                 select x.ToString( ) + "\r\n" ).
                 Aggregate( ( x, y ) => y + x );
      } else {
        return "<Empty>";
      }
    }

    /// <summary>
    /// Returns a clone of the path.
    /// </summary>
    /// <returns>A clone.</returns>
    public virtual object Clone( ) {
      Path<SS, DS> Path = this.MemberwiseClone( ) as Path<SS, DS>;
      Path.Actions = new LinkedList<Operator<SS, DS>>( );
      foreach( var e in this.Actions ) {
        Path.Actions.AddLast( e );
      }
      Path.StateHashTable = new HashSet<DS>( );
      Path.States = new LinkedList<DS>( );
      foreach ( var e in this.States ) {
        Path.States.AddLast( e ) ;
        Path.StateHashTable.Add( e );
      }
      return Path;
    }

    /// <summary>
    /// Pops the front of the path.
    /// </summary>
    /// <returns></returns>
    public STuple<DS,Operator<SS,DS>> popFront( ) {
      var firstA = this.Actions.First.Value;
      DS firstDS = this.States.First.Value;

      this.Actions.RemoveFirst();
      this.States.RemoveFirst( ) ;
      this.StateHashTable.Remove(firstDS);

      return new STuple<DS,Operator<SS,DS>> ( firstDS, firstA ) ;
    }

    /// <summary>
    /// Pops the back of the path.
    /// </summary>
    /// <returns></returns>
    public STuple<DS, Operator<SS, DS>> popBack()
    {
        var lastA = this.Actions.Last.Value;
        DS lastDS = this.States.Last.Value;

        this.Actions.RemoveLast();
        this.States.RemoveLast();
        this.StateHashTable.Remove(lastDS);

        return new STuple<DS, Operator<SS, DS>>( lastDS, lastA );
    }

    public override bool Equals( object obj ) {
      return this.ToString( ) .Equals( obj.ToString( ) ) ;
    }

    /// <summary>
    /// Peeks at the front of the path.
    /// </summary>
    /// <returns></returns>
    public STuple<DS, Operator<SS, DS>> peekFront()
    {
        var firstA = this.Actions.First.Value;
        DS firstDS = this.States.First.Value;

        return new STuple<DS, Operator<SS, DS>>(firstDS, firstA);
    }

    /// <summary>
    /// Peeks at the back of the path.
    /// </summary>
    /// <returns></returns>
    public STuple<DS, Operator<SS, DS>> peekBack()
    {
        var lastA = this.Actions.Last.Value;
        DS lastDS = this.States.Last.Value;

        return new STuple<DS, Operator<SS, DS>>(lastDS, lastA);
    }

    /// <summary>
    /// Book keeping values for path work.
    /// </summary>
    internal bool rdy = false;
    internal IEnumerator<DS> StatesEnumerator = null;
    internal IEnumerator<Operator<SS, DS>> ActionsEnumerator = null;

    /// <summary>
    /// Computes the cost of the path based on the action costs.
    /// </summary>
    /// <returns>The cost of the path.</returns>
    public Metric Cost {
      get {
        return this.cost;
      }
    }

    /// <summary>
    /// The internal cost.
    /// </summary>
    private Metric cost;

    /// <summary>
    /// Computes the cost of the path based on the action costs.
    /// </summary>
    /// <returns>The cost of the path.</returns>
    public int ActionCount {
      get {
        return this.Actions.Count;
      }
    }

  }

}
