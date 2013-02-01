/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;
using System.Collections.Generic;
using System.Linq;
using Net.Mokon.Edu.Unh.CS.AI.GridWorld;
using Net.Mokon.Edu.Unh.CS.AI.Harness;
using Net.Mokon.Edu.Unh.CS.AI.Search.Goals;
using Net.Mokon.Edu.Unh.CS.AI.Search.Heuristics;
using Net.Mokon.Utilities;

namespace Net.Mokon.Edu.Unh.CS.AI.Search.Algorithms.RealTime {
     
  public class AnytimeDStar<SS, DS> : Algorithm<SS, DS>
    where SS : StaticState<SS, DS>
    where DS : DynamicState<SS, DS> {

    public AnytimeDStar( Heuristic<SS, DS> H, Transformer<SS, DS> Transformer,
      StateVisualizer<SS, DS> sv ) {
      this.Transformer = Transformer;
      this.H = H;
      this.sv = sv;
    }

    private void init( Goal<SS, DS> Goal, SS ss, DS Sstart, Operator<SS, DS>[] Ops ) {
      RHS = new Dictionary<string, Metric>( );
      G = new Dictionary<string, Metric>( );
      this.Sgoal = Transformer.Transform( Goal, Sstart );
      this.Sstart = Sstart;
      this.ss = ss;
      this.Ops = Ops;
    }

    private StateVisualizer<SS, DS> sv;
    private Transformer<SS, DS> Transformer;
    protected Heuristic<SS, DS> H;
    private Dictionary<string, Metric> RHS;
    private Dictionary<string, Metric> G;
    private PriorityQueue<SvenKey, DS> OPEN;
    private Metric epsilon;
    private static readonly Metric epsilon0 = new Metric( 3.0d ) ;
    private static readonly Metric epsilonFactor = new Metric( 0.5d );
    private DS Sgoal;
    private DS Sstart;
    private SS ss;
    private Operator<SS, DS>[] Ops;
    private HashSet<DS> CLOSED;
    private HashSet<DS> INCONS;    

    private Metric h( DS Sstart, DS Sgoal ) {
      return H.H( ss, Sstart, Sgoal, Ops );
    }

    public Metric c( DS s1, DS s2 ) {
      if ( s1.IsInvalid( ss, false ) || s2.IsInvalid( ss, false ) ) {
        return Metric.Infinity;
      } else {
        return h( s1, s2 );
      }
    }

    private Metric rhs( DS ds ) {
      Metric r;
      if ( RHS.TryGetValue( ds.ToString( ), out r ) ) {
        return r;
      } else {
        return Metric.PositiveInfinity;
      }
    }

    private void rhs( DS ds, Metric value ) {
      string key = ds.ToString( );
      if ( RHS.ContainsKey( key ) ) {
        RHS.Remove( key );
      }
      RHS.Add( key, value );
    }

    private Metric g( DS ds ) {
      Metric r;
      if ( G.TryGetValue( ds.ToString( ), out r ) ) {
        return r;
      } else {
        return Metric.PositiveInfinity;
      }
    }

    private void g( DS ds, Metric value ) {
      string key = ds.ToString( );
      if ( G.ContainsKey( key ) ) {
        G.Remove( key );
      }
      G.Add( key, value );
    }

    private SvenKey key( DS s ) {
      if ( g( s ) > rhs( s ) ) {
        return new SvenKey( ) {
          k1 = rhs( s ) + epsilon*h( Sstart, s ),
          k2 = rhs( s )
        };
      } else {
        return new SvenKey( ) {
          k1 = g( s ) + h( Sstart, s ),
          k2 = g( s )
        };
      }
    }

    private void UpdateState( DS s ) {
      if ( !s.Equals( Sgoal ) ) {
        rhs( s, Min( sp => c( s, sp ) + g( sp ), Succ( s ) ) );
      }
      if ( OPEN.Contains( s ) ) {
        OPEN.Remove( s );
      }
      if ( g( s ) != rhs( s ) ) {
        if ( !CLOSED.Contains( s ) ) {
          OPEN.Enqueue( key( s ), s );
        } else {
          INCONS.Add( s );
        }
      }
    }

    private SvenKey TopKey( ) {
      var _ = OPEN.TopKey( );
      if ( _ == null ) {
        _ = new SvenKey( ) {
          k1 = Metric.PositiveInfinity,
          k2 = Metric.PositiveInfinity
        };
      }
      return _;
    }
     
    private void ComputeorImprovePath( ) {
      while ( TopKey( ) < key( Sstart ) || rhs( Sstart ) != g( Sstart ) ) {
        this.Expansions++;
#if OpenGl
        if ( sv != null ) {
          this.sv.VisualizeAlg( ss, ss.InitialDynamicState, new OpenGLStateVisualizer.OpenGlVisulizationData( ) {
            G = G, RHS = this.RHS, U = this.OPEN as PriorityQueue<SvenKey, GenericGridWorldDynamicState>
          } );
        }
#endif
        var s = OPEN.Dequeue( );
        if ( g( s ) > rhs( s ) ) {
          g( s, rhs( s ) );
          CLOSED.Add( s );
          foreach ( DS sp in Pred( s ) ) {
            this.Generations++;
            UpdateState( sp );
          }
        } else {
          g( s, Metric.Infinity );
          foreach ( DS sp in PredUnionSelf( s ) ) {
            this.Generations++;
            UpdateState( sp );
          }
        }
        
      }
    }

    public override IEnumerable<Path<SS, DS>> Compute( SS ss, DS ds,
      Goal<SS, DS> Goal, Operator<SS, DS>[] Actions ) {

      init( Goal, ss, ds, Actions );

      g(Sstart, Metric.Infinity ) ;
      rhs( Sstart,  Metric.Infinity ) ;
      g(Sgoal, Metric.Infinity ) ;
      rhs( Sgoal, Metric.Zero );
      epsilon = epsilon0;

      CLOSED = new HashSet<DS>( );
      OPEN = new PriorityQueue<SvenKey, DS>( );
      INCONS = new HashSet<DS>( );
      OPEN.Enqueue( key( Sgoal ), Sgoal );
      ComputeorImprovePath( );
      yield return MoveAgent( );
      while ( Sstart != Sgoal ) {
        bool Changes = ss.HasRecentChanges( );
        if ( Changes ) {
          foreach ( var u in ss.Changes ) {
            foreach ( var v in Succ( u ) ) {
              UpdateState( u );
              UpdateState( v );
            }
          }
        }
        if ( Changes ) {
          epsilon += epsilonFactor;
        } else if ( epsilon > Metric.One ) {
          epsilon -= epsilonFactor;
          if ( epsilon < Metric.One ) {
            epsilon= Metric.One;
          }
        }
        MoveINCONSintoOPENAndResort( );
        CLOSED = new HashSet<DS>( );
        if ( Changes || epsilon != Metric.One ) {
          ComputeorImprovePath( );
        }
        yield return MoveAgent();

      }
      yield break;
    }

    private void MoveINCONSintoOPENAndResort( ) {
      var OldOPEN = OPEN ;
      OPEN = new PriorityQueue<SvenKey,DS>( );
      foreach ( DS s in INCONS ) {
        OPEN.Enqueue( key( s ), s );
      }
      while ( !OldOPEN.IsEmpty( ) ) {
        DS s = OldOPEN.Dequeue( ) ;
        OPEN.Enqueue( key( s ), s ); 
      }
    }

    private Path<SS, DS> MoveAgent( ) {
      DS next = ArgMin( sp => c( Sstart, sp ) + g( sp ), Succ( Sstart ) );
      Operator<SS, DS> Op = null;
      foreach ( var action in Ops ) {
        if ( action.IsValidOn( Sstart, ss, false ) && action.PerformOn( Sstart, ss ).First( ).Equals( next ) ) {
          Op = action;
          break;
        }
      }
      if ( Op == null ) {
        return null;
      } else {
        Sstart = next;
        return Operator<SS, DS>.MakePath( Op );
      }
    }

     private Metric Min( Func<DS, Metric> func, params DS[] args ) {
       Metric min = Metric.PositiveInfinity;
       foreach ( DS d in args ) {
         Metric v = func( d );
         if ( v < min ) {
           min = v;
         }
       }
       return min;
     }
        /*
    private Metric Min( params Metric[] args ) {
      Metric min = Metric.PositiveInfinity;
      foreach ( Metric d in args ) {
        if ( d < min ) {
          min = d;
        }
      }
      return min;
    }       * */

    private DS ArgMin( Func<DS, Metric> func, params DS[] args ) {
      DS arg = null;
      Metric min = Metric.PositiveInfinity;
      foreach ( DS d in args ) {
        Metric v = func( d );
        if ( v <= min ) {
          min = v;
          arg = d;
        }
      }
      return arg;
    }
    

    private DS[] Pred( DS u ) {
      return u.Neighbors( Ops, ss, false ).ToArray( );
    }

    private DS[] PredUnionSelf( DS u ) {
      var _ = u.Neighbors( Ops, ss, false );
      _.AddFirst( u );
      return _.ToArray( );
    }

    private DS[] Succ( DS u ) {
      return u.Neighbors( Ops, ss, false ).ToArray( );
    }  

  }    
}
