/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;
using System.Collections.Generic;
using System.Linq;
using Net.Mokon.Utilities;
using Net.Mokon.Edu.Unh.CS.AI.Search.Goals;

namespace Net.Mokon.Edu.Unh.CS.AI.Search.Algorithms {

  /// <summary>
  /// This is an abstract class which all Algorithms must implement. The key
  /// point is the Compute method which returns a path from the given
  /// DynamicState to the final Goal state.
  /// </summary>
  public abstract class Algorithm<SS, DS>
    where SS : StaticState<SS, DS>
    where DS : DynamicState<SS, DS> {

    /// <summary>
    /// Returns weather this algorithm returns actions in real time.
    /// </summary>
    /// <returns></returns>
    public virtual bool IsRealTime( ) {
      return false;
    }

    /// <summary>
    /// Returns whether this algorithm works on dynamic world.
    /// </summary>
    /// <returns></returns>
    public virtual bool IsDynamic( ) {
      return false;
    }

    /// <summary>
    /// A field for book keeping on the number of node expansions done in this
    /// algorithm. Concrete algorithms should define what this means.
    /// </summary>
    protected internal uint Expansions = 0;

    /// <summary>
    /// A field for book keeping on the number of node generations done in this
    /// algorithm. Concrete algorithms should define what this means.
    /// </summary>
    protected internal uint Generations = 0;

    /// <summary>
    /// This method attempt  to compute a path from the given state information
    /// to the given goal. This function will throw a PathNotFoundException if
    /// no valid path can be found.
    /// </summary>
    /// <param name="StaticState">Some static state information that will not
    /// change with actions performed upon the state space.</param>
    /// <param name="DynamicState">A starting dynamic state to be transformed
    /// by actions to reach the goal state.</param>
    /// <param name="Goal">A goal object to evaluate if a given goal has been
    /// reached.</param>
    /// <returns>A path from the starting state to the goal state. Whether this
    /// is optiminal will depend upon the concrete implementation.</returns>
    public abstract IEnumerable<Path<SS, DS>> Compute( SS StaticState,
      DS DynamicState, Goal<SS, DS> Goal, Operator<SS, DS>[] Actions );

    /// <summary>
    /// Creates a textual summary of the alorithms stats.
    /// </summary>
    /// <returns></returns>
    public string Summary( ) {
      return Generations + " nodes generated\n" + Expansions + " nodes expanded\n";
    }

     public static Path<SS, DS> ExtractPath( DS Sstart, DS SPgoal,
      ExtendedDictionary<DS, DS> tree, Operator<SS, DS>[] Ops, SS ss ) {
      DS Prev;
      DS Cur = SPgoal;
      Path<SS, DS> Path = new Path<SS, DS>( (DS)null );
      do {         
        Prev = Cur;
        Cur = tree.Lookup( Prev );
        if ( Path.StateHashTable.Contains( Cur ) ) {
          return Path;
        }
        var Op = GetOp( Cur, Prev, Ops, ss );
        if ( Op == null ) {
          return Path;
        }
        Path.PushFirst( Op, Cur, Prev );
      } while ( !Cur.Equals( Sstart ) );

      return Path;
    }

    public static Operator<SS, DS> GetOp( DS From, DS To, Operator<SS, DS>[] Ops, SS ss ) {
      foreach ( var op in Ops ) {
        if ( op.IsValidOn( From, ss, false ) && op.PerformOn( From, ss ).First( ).Equals( To ) ) {
          return op;
        }
      }
      return null;
    }

  }

  /// <summary>
  /// An exception class to indicate a path could not be found during a
  /// computation.
  /// </summary>
  public class PathNotFoundException : Exception {

    /// <summary>
    /// A constructor based on an error message.
    /// </summary>
    /// <param name="Message">The error message to pass along.</param>
    public PathNotFoundException( string Message )
      : base( Message ) {
    }

  }

}
