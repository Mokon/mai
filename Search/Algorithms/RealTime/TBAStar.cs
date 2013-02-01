/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;
using System.Linq;
using System.Collections.Generic;
using Net.Mokon.Utilities;
using Net.Mokon.Edu.Unh.CS.AI.Search.Goals;
using Net.Mokon.Edu.Unh.CS.AI.Search.Bookkeepings;

namespace Net.Mokon.Edu.Unh.CS.AI.Search.Algorithms.RealTime {

  /// <summary>
  /// This is an algorithm which implements the greedy search alg.
  /// </summary>
  public class TBAStar<SS, DS> : RealTimeAlgorithm<SS, DS>
    where SS : StaticState<SS, DS>
    where DS : DynamicState<SS, DS> {

    /// <summary>
    /// The best expression for sorting elements in the open list.
    /// </summary>
    protected Func<SS, DS, Goal<SS, DS>, Operator<SS, DS>[], Metric>
      BestExpresion;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="BestExpresion">The best expression for sorting elements
    /// in the open list.</param>
    /// <param name="Limit">The limit bound. Default to 0 if no bound
    /// wanted.</param>
    /// <param name="UseClosedList">Whether or not a closed list vs upward
    /// travesal loop detection will be used to detect cycles.</param>
    public TBAStar( Func<SS, DS, Goal<SS, DS>, Operator<SS, DS>[],
      Metric> BestExpresion ) {
      this.BestExpresion = BestExpresion;
    }

    public override IEnumerable<Path<SS, DS>> Compute( SS StaticState, DS DynamicState,
      Goal<SS, DS> Goal, Operator<SS, DS>[] Actions ) {
      bool solutionFound = false;
      DS loc = DynamicState;
      Operator<SS, DS> opLast = null;
      PriorityQueue<Metric, DS> OpenList = new PriorityQueue<Metric, DS>( );
      HashSet<DS> ClosedList = new HashSet<DS>( );
      OpenList.Enqueue( Metric.Zero, DynamicState );
      LinkedList<DS> pathFollow = null;
      Path<SS, DS> actionPathFollow = null;
      Path<SS, DS> pathTaken = new Path<SS, DS>( (DS)null );
      while ( !Goal.IsSatisfiedBy( StaticState, loc ) ) {
        if ( !solutionFound ) {
          /********************/
          int IterationExpansions = 0;
          while ( true ) {
            if ( OpenList.IsEmpty( ) ) {
              throw new PathNotFoundException( "Path Not Found" );
            }
            DS Cur = OpenList.Dequeue( );
            ClosedList.Add( Cur );
            if ( Goal.IsSatisfiedBy( StaticState, Cur ) ) {
              OpenList.Enqueue( BestExpresion( StaticState, Cur, Goal, Actions ), Cur );
              solutionFound = true;
              break;
            } else {
              this.Expansions++;
              foreach ( DS e in Cur.Expand( Actions, StaticState, false ) ) {
                e.BookKeeping = new ParentBookKeeping<SS, DS>( );
                ( e.BookKeeping as ParentBookKeeping<SS, DS> ).Parent = Cur;
                if ( !ClosedList.Contains( e ) ) {
                  OpenList.Enqueue( BestExpresion( StaticState, e, Goal, Actions ), e );
                }
                this.Generations++;
              }
              if ( ++IterationExpansions >= ComputationLimit ) {
                solutionFound = false;
                break;
              }
            }
          } /********************/
        }

        pathFollow = new LinkedList<DS>( );
        var pN = OpenList.DequeueNode( );
        OpenList.Enqueue( pN.First, pN.Second );
        DS p = pN.Second;
        var path = p.Path( );
        actionPathFollow = new Path<SS, DS>( (DS)null );
        pathFollow.AddFirst( p );
        var i = path.Actions.Reverse( ).GetEnumerator( );
        while ( ( p.BookKeeping as ParentBookKeeping<SS, DS> ) != null && ( p = ( p.BookKeeping as
          ParentBookKeeping<SS, DS> ).Parent ) != null && ( !p.Equals( DynamicState ) || loc.Equals( DynamicState ) ) ) {
          pathFollow.AddFirst( p );
          if ( i.MoveNext( ) ) {
            actionPathFollow.Actions.AddFirst( i.Current );
          }
          if ( p.Equals( loc ) ) {
            break;
          }
        }

        Operator<SS, DS> action;
        if ( pathFollow.Contains( loc ) ) {
          action = actionPathFollow.Actions.First.Value;
          if ( !action.IsValidOn( loc, StaticState, false ) ) {
            System.Console.Error.WriteLine( "ERROR" );
          }
          pathFollow.RemoveFirst( );
          actionPathFollow.Actions.RemoveFirst( );
          pathTaken.Push( action, loc, null );
        } else {
          if ( !loc.Equals( DynamicState ) ) {
            action = new ReverseOperator<SS, DS>( pathTaken.Actions.Last.Value );
            pathTaken.Actions.RemoveLast( );
          } else {
            action = new ReverseOperator<SS, DS>( opLast );
            pathTaken.Push( action, loc, null );
          }
        }
        if ( !action.IsValidOn( loc, StaticState, false ) ) {
          System.Console.Error.WriteLine( "ERROR" );
        }
        loc = action.PerformOn( loc, StaticState ).First( );
        opLast = action;
        yield return new Path<SS, DS>( action );
      }
      yield break;
    }
  }
}