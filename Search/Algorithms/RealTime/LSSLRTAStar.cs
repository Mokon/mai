/*

 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;
using System.Collections.Generic;
using Net.Mokon.Edu.Unh.CS.AI.GridWorld;
using Net.Mokon.Edu.Unh.CS.AI.Harness;
using Net.Mokon.Edu.Unh.CS.AI.Search.Goals;
using Net.Mokon.Edu.Unh.CS.AI.Search.Heuristics;
using Net.Mokon.Utilities;

namespace Net.Mokon.Edu.Unh.CS.AI.Search.Algorithms.RealTime {

  public class LSSLRTAStar<SS, DS> : RealTimeAlgorithm<SS, DS>
    where SS : StaticState<SS, DS>
    where DS : DynamicState<SS, DS> {

    private static void AStar( ref ExtendedDictionary<DS, Metric> G,
        ExtendedDictionary<DS,Metric> H, DS Sstart, DS Sgoal,
        ExtendedDictionary<DS, DS> tree, ref PriorityQueue<Metric, DS> OPEN,
        ref HashSet<DS> CLOSED, int lookahead, Operator<SS, DS>[] Ops, SS ss,
        Func<DS, DS, Metric> h0, ref uint Expansions, StateVisualizer<SS, DS> sv ) {
      G = new ExtendedDictionary<DS, Metric>( k => Metric.Infinity );
      G.Set( Sstart, Metric.Zero );
      OPEN = new PriorityQueue<Metric, DS>( );
      CLOSED = new HashSet<DS>( );
      OPEN.Enqueue( H.Lookup( Sstart ), Sstart ) ;
        
      int expansions = 0;

      while ( !OPEN.IsEmpty( ) && G.Lookup( Sgoal ) > OPEN.TopKey( ) &&
          expansions < lookahead ) {
#if OpenGl
        if ( sv != null ) {
          HashSet<DS> hs = new HashSet<DS>( );
          OPEN.AddToHashSet( hs );
          sv.VisualizeAlg( ss, ss.InitialDynamicState, new OpenGLStateVisualizer.OpenGlVisulizationData( ) {
            List = hs as HashSet<GenericGridWorldDynamicState>,
            HAdjusted = H,
          } );
        }
#endif
        expansions++;
        DS s = OPEN.Dequeue( );
        CLOSED.Add( s );
        Expansions++;
        foreach ( DS sp in s.Expand( Ops, ss, false ) ) {
          Metric cost = G.Lookup( s)  + c( s, sp, h0 );
          if ( G.Lookup( sp ) > cost ) {
            G.Set( sp, cost );
            tree.Set( sp, s );
            if ( OPEN.Contains( sp ) ) {
              OPEN.Remove( sp );
            }
            OPEN.Enqueue( G.Lookup( sp ) + H.Lookup( sp ), sp );
          }
        }
      }
    }

    private static void Dijkstra( ref ExtendedDictionary<DS, Metric> G,
        ExtendedDictionary<DS, Metric> H, DS Sstart, DS Sgoal, 
        ref ExtendedDictionary<DS, DS> tree,
        ref PriorityQueue<Metric, DS> OPEN, ref HashSet<DS> CLOSED,
        Operator<SS, DS>[] Ops, SS ss, Func<DS, DS, Metric> h0 ) {
      foreach ( DS s in CLOSED ) {
        H.Set( s, Metric.Infinity );
      }
      OPEN = OPEN.Reorder( s => H.Lookup( s ) );
      while ( CLOSED.Count != 0 && !OPEN.IsEmpty( ) ) {
        DS s = OPEN.Dequeue( );
        if ( CLOSED.Contains( s ) ) {
          CLOSED.Remove( s );
        }
        foreach ( DS sp in s.Expand( Ops, ss, false ) ) {
          Metric cost = c( sp, s, h0 ) + H.Lookup( s );
          if ( CLOSED.Contains( sp ) && H.Lookup( sp ) > cost ) {
            H.Set( sp, cost );
            if ( !OPEN.Contains( sp ) ) {
              OPEN.Enqueue( cost, sp );
            }
          }
        }
      }
    }

    private static Metric c( DS s, DS sp, Func<DS, DS, Metric> h ) {
      return h( s, sp );
    }

    public LSSLRTAStar( Heuristic<SS, DS> Heuristic,
      StateVisualizer<SS, DS> sv, Transformer<SS, DS> Transformer ) {
      this.Heuristic = Heuristic;
      this.Transformer = Transformer;
      this.sv = sv;
    }

    private Transformer<SS, DS> Transformer;
    private StateVisualizer<SS, DS> sv;
    private Heuristic<SS, DS> Heuristic;

    public static Path<SS, DS> LSSLRTAStarLookahead( int ComputationLimit, SS ss,
      DS Sstart, DS Sgoal, Operator<SS, DS>[] Ops, Func<DS, DS, Metric> h0,
      ref Path<SS, DS> Incumbant, ref uint Expansions, StateVisualizer<SS, DS> sv,
      ExtendedDictionary<DS, DS> tree, ExtendedDictionary<DS, Metric> H ) {
      if ( !PathGood( Incumbant, ss ) ) {
        ExtendedDictionary<DS, Metric> G = null;
        PriorityQueue<Metric, DS> OPEN = null;
        HashSet<DS> CLOSED = null;
        AStar( ref G, H, Sstart, Sgoal, tree, ref OPEN, ref CLOSED, ComputationLimit, Ops, ss, h0, ref Expansions, sv );
        if ( OPEN.IsEmpty( ) ) {
          return null;
        }
        DS SPgoal = OPEN.Top( );
        Dijkstra( ref G, H, Sstart, Sgoal, ref tree, ref OPEN, ref CLOSED, Ops, ss, h0 );

        Incumbant = ExtractPath( Sstart, SPgoal, tree, Ops, ss );
      }
      if ( Incumbant == null || Incumbant.ActionCount == 0 ) {
        return null;
      } else {
        var IHead = Incumbant.PopHead( );
        if ( IHead.Actions.First.Value.IsValidOn( ss.InitialDynamicState, ss, false ) ) {
          return IHead;
        } else {
          Incumbant = null;
          return null;
        }
      }
    }

    public override IEnumerable<Path<SS, DS>> Compute( SS ss,
      DS Sstart, Goal<SS, DS> Goal, Operator<SS, DS>[] Ops ) {
      DS Sgoal = Transformer.Transform( Goal, Sstart );
      Func<DS, DS, Metric> h0 = ( x, y ) => Heuristic.H( ss, x, y, Ops );
      ExtendedDictionary<DS, DS> tree = new ExtendedDictionary<DS, DS>( x => null );
      ExtendedDictionary<DS, Metric> H = new ExtendedDictionary<DS, Metric>(
        x => h0( x, Sgoal ) );
      Path<SS, DS> Incumbant = null;

      while ( !Sstart.Equals( Sgoal ) ) {
        yield return LSSLRTAStarLookahead( ComputationLimit, ss, Sstart, Sgoal,
          Ops, h0, ref Incumbant, ref Expansions, sv, tree, H );
        Sstart = ss.InitialDynamicState;
      }
      yield break;
    }

    private static bool PathGood( Path<SS, DS> Incumbant, SS ss ) {
      if ( Incumbant == null || Incumbant.Actions.Count == 0 ) {
        return false;
      }
      if ( ss.HasRecentChanges( ) ) {
        foreach ( var Change in ss.Changes ) {
          if ( Incumbant.StateHashTable.Contains( Change ) ) {
            return false;
          }
        }
      }
      return true;
    }
  }

}
