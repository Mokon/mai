/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using Net.Mokon.Utilities;
using Net.Mokon.Edu.Unh.CS.AI.Search.Goals;

namespace Net.Mokon.Edu.Unh.CS.AI.Search.Algorithms {

  /// <summary>
  /// This is an algorithm which implements the uniformed cost search alg.
  /// </summary>
  public class UniformedCostSearch<SS, DS> : BestFirstSearch<SS, DS>
    where SS : StaticState<SS, DS>
    where DS : DynamicState<SS, DS> {

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="GComputer">The G function computer object</param>
    /// <param name="DepthBound">The dpeth</param>
    /// <param name="UseClosedList">Whether a closed list or duplicate
    /// detection should be used for cycle detection.</param>
    public UniformedCostSearch( bool UseClosedList, StateVisualizer<SS, DS> sv ) 
      : base( null, true, sv ) {
      throw new System.Exception( "Not Implemented UCS" );
    }

  }

}
