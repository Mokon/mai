/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;
using System.Collections.Generic;
using Net.Mokon.Edu.Unh.CS.AI.Search.Bookkeepings;

namespace Net.Mokon.Edu.Unh.CS.AI.Search {

  /// <summary>
  /// A node object which represents the constantly changing portion of a 
  /// state.
  /// </summary>
  public abstract class DynamicState<SS, DS>
    where SS : StaticState<SS, DS>
    where DS : DynamicState<SS, DS> {

    /// <summary>
    /// A hash value concrete implementations should alter to update the hash
    /// code to be returned.
    /// </summary>
    internal int hash;

    /// <summary>
    /// Returns a boolean on whether this dynamic state is invalid. IE it should
    /// not be possible to reach.
    /// </summary>
    /// <param name="ss"></param>
    /// <returns></returns>
    public abstract bool IsInvalid( SS ss, bool all );

    public Operator<SS, DS> LastOperator {
      get;
      private set;
    }

    /// <summary>
    /// A constructor which takes a set of actions and expands the node
    /// based on which ever of those actions can be performed on the current
    /// node to produce additional nodes.
    /// </summary>
    /// <param name="Actions">A set of actions to work with.</param>
    /// <returns>The nodes created by the expansion function.</returns>
    public LinkedList<DS> Expand( Operator<SS, DS>[] Actions, SS ss,
      bool all ) {
      LinkedList<DS> Nodes = new LinkedList<DS>( );
      if ( this.IsInvalid( ss, all ) ) {
        return Nodes;
      }
      foreach ( Operator<SS, DS> act in Actions ) {
        if ( act.IsValidOn( (DS)this, ss, all ) ) {
          var result = act.PerformOn( (DS)this, ss );
          foreach ( var ds in result ) {
            ds.LastOperator = act;
            Nodes.AddLast( ds );
          }
        }
      }
      return Nodes;
    }

    public LinkedList<DS> Neighbors( Operator<SS, DS>[] Actions, SS ss,
      bool all ) {
      LinkedList<DS> Nodes = new LinkedList<DS>( );
      foreach ( Operator<SS, DS> act in Actions ) {
        if ( act.IsNeighborValidOn( (DS)this, ss, all ) ) {
          var result = act.PerformOn( (DS)this, ss );
          foreach ( var ds in result ) {
            Nodes.AddLast( ds );
          }
        }
      }
      return Nodes;
    }

    /// <summary>
    /// A getter for the hash value of this object.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode( ) {
      return hash;
    }

    /// <summary>
    /// Returns a clone of the current state. I am not sure if this is legal
    /// with the current laws in the US but oh well. Cloning is fun.
    /// </summary>
    /// <returns>A clone.</returns>
    public virtual object Clone( ) {
      DynamicState<SS, DS> DS = this.MemberwiseClone( ) as DynamicState<SS, DS>;
      return DS;
    }

    /// <summary>
    /// A BookKeeping object that takes care of information storage.
    /// </summary>
    public BookKeeping<SS, DS> BookKeeping;

  }

}
