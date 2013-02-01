/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System.Collections.Generic;
using Net.Mokon.Edu.Unh.CS.AI.GridWorld;
using Net.Mokon.Edu.Unh.CS.AI.Harness;
using Net.Mokon.Edu.Unh.CS.AI.Search.Bookkeepings;
using Net.Mokon.Edu.Unh.CS.AI.Search.Goals;
using Net.Mokon.Edu.Unh.CS.AI.Search.Heuristics;
using Net.Mokon.Utilities;

namespace Net.Mokon.Edu.Unh.CS.AI.Search.Algorithms {

  /// <summary>
  /// This is an algorithm which implements the greedy search alg.
  /// </summary>
  public class BestFirstSearch<SS, DS> : UnlimitedStaticAlgorithm<SS, DS>
    where SS : StaticState<SS, DS>
    where DS : DynamicState<SS, DS> {

    private StateVisualizer<SS, DS> sv;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="BestExpresion">The best expression for sorting elements
    /// in the open list.</param>
    /// <param name="Limit">The limit bound. Default to 0 if no bound
    /// wanted.</param>
    /// <param name="UseClosedList">Whether or not a closed list vs upward
    /// travesal loop detection will be used to detect cycles.</param>
    public BestFirstSearch( Heuristic<SS, DS> Heuristic, bool UseClosedList, StateVisualizer<SS, DS> sv ) {
      this.UseClosedList = UseClosedList;
      this.sv = sv;
      this.Heuristic = Heuristic;
    }

    private Heuristic<SS, DS> Heuristic;


    /// <summary>
    /// Whether or not a closed list vs upward travesal loop detection will be
    /// used to detect cycles.
    /// </summary>
    protected bool UseClosedList;

    /// <summary>
    /// The compute method which uses greedy search.
    /// </summary>
    /// <param name="StaticState">A static state to work on.</param>
    /// <param name="DynamicState">A dynamic state to work on.</param>
    /// <param name="Goal">The goal state(s) to search for.</param>
    /// <returns>Computes a path.</returns>
    public override Path<SS, DS> ComputeComplete( SS StaticState,
      DS DynamicState, Goal<SS, DS> Goal, Operator<SS, DS>[] Actions ) {
      PriorityQueue<Metric, DS> OpenList = new PriorityQueue<Metric, DS>( );
      ExtendedDictionary<DS, DS> tree = new ExtendedDictionary<DS, DS>( x => null );
      ExtendedDictionary<DS, Metric> G = new ExtendedDictionary<DS, Metric>( k => Metric.Infinity );
      G.Set( DynamicState, Metric.Zero );
      HashSet<DS> ClosedList = new HashSet<DS>( );
      OpenList.Enqueue( Metric.Zero, DynamicState );
      while ( true ) {
#if OpenGl
        if ( sv != null ) {
          this.sv.VisualizeAlg( StaticState, StaticState.InitialDynamicState,
            new OpenGLStateVisualizer.OpenGlVisulizationData( ) {
              OL = OpenList as PriorityQueue<Metric, GenericGridWorldDynamicState>,
            } );
        }
#endif
        if ( OpenList.IsEmpty( ) ) {
          throw new PathNotFoundException( "Path Not Found" );
        }
        DS Cur = OpenList.Dequeue( );
        if ( UseClosedList ) {
          ClosedList.Add( Cur );
        }
        if ( Goal.IsSatisfiedBy( StaticState, Cur ) ) {
          return Algorithm<SS, DS>.ExtractPath( StaticState.InitialDynamicState,
            Cur, tree, Actions, StaticState );
        } else {
          this.Expansions++;
          foreach ( DS e in Cur.Expand( Actions, StaticState, true ) ) {
            e.BookKeeping = new ParentBookKeeping<SS, DS>( );
            if ( !UseClosedList ) {
              ( e.BookKeeping as ParentBookKeeping<SS, DS> ).Parent = Cur;
            }
            if ( ( UseClosedList && !ClosedList.Contains( e ) && !OpenList.Contains( e ) ) ||
              ( !UseClosedList && !CheckReversePathForDuplicates( e ) ) ) {
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

    /// <summary>
    /// This method takes a node and interates in a reverse direction on its
    /// path performing the actions to get back to the orginal state. If any
    /// duplicates exist on this path it will return true else it will return
    /// false.
    /// </summary>
    /// <param name="e">The node to check for duplicates of on its reverse
    /// path.</param>
    /// <returns>Whether or not any duplicates were found.</returns>
    protected static bool CheckReversePathForDuplicates( DS e ) {
      ParentBookKeeping<SS, DS> bk;
      DS Cur = e;
      while ( Cur != null ) {
        bk = Cur.BookKeeping as ParentBookKeeping<SS, DS>;
        if ( bk == null ) {
          return false;
        }
        Cur = bk.Parent;

        if ( e.Equals( Cur ) ) {
          return true;
        }
      }
      return false;
    }

  }

}
