/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using Net.Mokon.Utilities;

namespace Net.Mokon.Edu.Unh.CS.AI.Search.Algorithms {

  /// <summary>
  /// This is an algorithm which implements the depth first search over the
  /// state space.
  /// </summary>
  public class DepthFirstSearch<SS, DS> :
    BestFirstSearch<SS, DS>
    where SS : StaticState<SS, DS>
    where DS : DynamicState<SS, DS> {

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="DepthBound">The dpeth</param>
    /// <param name="UseClosedList">Whether a closed list or duplicate
    /// detection should be used for cycle detection.</param>
    public DepthFirstSearch( Metric DepthBound, bool UseClosedList, StateVisualizer<SS, DS> sv )
      : base( null, UseClosedList, sv ) {
      throw new System.Exception( "Not Implemented DFS" );
    }

  }

}
