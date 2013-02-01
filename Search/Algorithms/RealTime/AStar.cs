/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using Net.Mokon.Edu.Unh.CS.AI.Search.Goals;
using Net.Mokon.Edu.Unh.CS.AI.Search.Heuristics;

namespace Net.Mokon.Edu.Unh.CS.AI.Search.Algorithms.RealTime {

  /// <summary>
  /// This is an algorithm which implements the astar search alg.
  /// </summary>
  public class AStar<SS, DS> : BestFirstSearch<SS, DS>
    where SS : StaticState<SS, DS>
    where DS : DynamicState<SS, DS> {

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="GComputer">The G function computer object</param>
    /// <param name="Heuristic">The Heuristic function object</param>
    /// <param name="DepthBound">The dpeth</param>
    /// <param name="UseClosedList">Whether a closed list or duplicate
    /// detection should be used for cycle detection.</param>
    public AStar( Heuristic<SS, DS> Heuristic ) 
      : base( Heuristic ) {
    }

  }

}
