/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System.Collections.Generic;

namespace Net.Mokon.Edu.Unh.CS.AI.Search {

  /// <summary>
  /// A class which represents a possible state of the world. The static
  /// state shall be where state information that never changes, aka is static,
  /// shall be stored.
  /// </summary>
  public abstract class StaticState<SS, DS>
    where SS : StaticState<SS, DS>
    where DS : DynamicState<SS, DS> {

    /// <summary>
    /// A consturctor which initilizes the initial dynamic state.
    /// </summary>
    /// <param name="InitialDynamicState"></param>
    public StaticState( DS InitialDynamicState ) {
      this.InitialDynamicState = InitialDynamicState;
    }

    /// <summary>
    /// The initial dynamic state for this algorithm.
    /// </summary>
    public DS InitialDynamicState;

    /// <summary>
    /// A list of changes in this static state during the the last time tick.
    /// This is for dynamic algorithms.
    /// </summary>
    public LinkedList<DS> Changes;

    /// <summary>
    /// Returns whether their are changes waiting to be processed.
    /// </summary>
    /// <returns></returns>
    public bool HasRecentChanges( ) {
      return Changes != null && Changes.Count != 0;
    }

  }

}
