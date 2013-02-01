/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System.Collections.Generic;
using Net.Mokon.Edu.Unh.CS.AI.Search.Goals;
using Net.Mokon.Edu.Unh.CS.AI.Search.Heuristics;
using Net.Mokon.Utilities;

namespace Net.Mokon.Edu.Unh.CS.AI.Search.Algorithms.RealTime {

  /// <summary>
  /// This is an algorithm which implements the greedy search alg.
  /// </summary>
  public class BestFirstSearch<SS, DS> : RealTimeAlgorithm<SS, DS>
    where SS : StaticState<SS, DS>
    where DS : DynamicState<SS, DS> {

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="BestExpresion">The best expression for sorting elements
    /// in the open list.</param>
    /// <param name="Limit">The limit bound. Default to 0 if no bound
    /// wanted.</param>
    /// <param name="UseClosedList">Whether or not a closed list vs upward
    /// travesal loop detection will be used to detect cycles.</param>
    public BestFirstSearch( Heuristic<SS, DS> Heuristic ) {
      this.Heuristic = Heuristic;
    }

    private Heuristic<SS, DS> Heuristic;

    /// <summary>
    /// The compute method which uses greedy search.
    /// </summary>
    /// <param name="StaticState">A static state to work on.</param>
    /// <param name="DynamicState">A dynamic state to work on.</param>
    /// <param name="Goal">The goal state(s) to search for.</param>
    /// <returns>Computes a path.</returns>
    public override IEnumerable<Path<SS, DS>> Compute( SS StaticState,
      DS DynamicState, Goal<SS, DS> Goal, Operator<SS, DS>[] Actions ) {
      ExtendedDictionary<DS, DS> tree = new ExtendedDictionary<DS, DS>( x => null );
      ExtendedDictionary<DS, Metric> G = new ExtendedDictionary<DS, Metric>( k => Metric.Infinity );
      G.Set( DynamicState, Metric.Zero );
      uint IterationExpansions = 0;
      PriorityQueue<Metric, DS> OpenList = new PriorityQueue<Metric, DS>( );
      HashSet<DS> ClosedList = new HashSet<DS>( );
      OpenList.Enqueue( Metric.Zero, DynamicState );
      while ( true ) {
        if ( OpenList.IsEmpty( ) ) {
          throw new PathNotFoundException( "Path Not Found" );
        }
        DS Cur = OpenList.Dequeue( );
        ClosedList.Add( Cur );
        if ( Goal.IsSatisfiedBy( StaticState, Cur ) ) {
          yield return Algorithm<SS, DS>.ExtractPath( StaticState.InitialDynamicState,
            Cur, tree, Actions, StaticState );
          yield break;
        } else {
          this.Expansions++;
          IterationExpansions++;
          if ( IterationExpansions > ComputationLimit ) {
            yield return Algorithm<SS, DS>.ExtractPath( StaticState.InitialDynamicState,
            Cur, tree, Actions, StaticState );
            IterationExpansions = 1;
          }
          foreach ( DS e in Cur.Expand( Actions, StaticState, true ) ) {
            if ( !ClosedList.Contains( e ) ) {
              Metric cost = G.Lookup( Cur )  + e.LastOperator.Cost( Cur );
              if ( G.Lookup( e ) > cost ) {
                G.Set( e, cost );
                tree.Set( e, Cur );
                OpenList.Enqueue( cost, e );
              }
            }
            this.Generations++;
          }
        }
      }
    }

  }

}
