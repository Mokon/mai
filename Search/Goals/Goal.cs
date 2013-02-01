/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
namespace Net.Mokon.Edu.Unh.CS.AI.Search.Goals {

  /// <summary>
  /// An interface which represents a goal state to be reached.
  /// </summary>
  public interface Goal<SS, DS>
    where SS : StaticState<SS, DS>
    where DS : DynamicState<SS, DS> {

    /// <summary>
    /// A preticate function which returns whether or not the Goal state has
    /// been reached.
    /// </summary>
    /// <param name="StaticState">The static state of this node.</param>
    /// <param name="DynamicState">The dynamic state of this node.</param>
    /// <returns>Whether or not the goal has been reached.</returns>
    bool IsSatisfiedBy( SS StaticState, DS DynamicState );

  }

}
