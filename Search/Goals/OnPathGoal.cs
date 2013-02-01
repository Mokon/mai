/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using Net.Mokon.Edu.Unh.CS.AI.GridWorld;

namespace Net.Mokon.Edu.Unh.CS.AI.Search.Goals {

  /// <summary>
  /// A goal that classifies based on being in a single state.
  /// </summary>
  /// <typeparam name="SS"></typeparam>
  /// <typeparam name="DS"></typeparam>
  public class OnPathGoal<SS, DS> : Goal<SS, DS>
    where SS : StaticState<SS, DS>
    where DS : DynamicState<SS, DS> {

    /// <summary>
    /// The path to test being in.
    /// </summary>
    public Path<SS,DS> GoalPath ;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="GoalState"></param>
    public OnPathGoal( Path<SS,DS> GoalPath ) {
      this.GoalPath = GoalPath;
    }

    /// <summary>
    /// A preticate function which returns whether or not the Goal state has
    /// been reached.
    /// </summary>
    /// <param name="StaticState">The static state of this node.</param>
    /// <param name="DynamicState">The dynamic state of this node.</param>
    /// <returns>Whether or not the goal has been reached.</returns>
    public bool IsSatisfiedBy( SS StaticState, DS DynamicState ) {
      return this.GoalPath.StateHashTable.Contains( DynamicState );
    }

  }

}
