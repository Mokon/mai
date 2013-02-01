/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */           
namespace Net.Mokon.Edu.Unh.CS.AI.Search.Goals {

  /// <summary>
  /// A goal that classifies based on being in a single state.
  /// </summary>
  /// <typeparam name="SS"></typeparam>
  /// <typeparam name="DS"></typeparam>
  public class InStateGoal<SS, DS> : Goal<SS, DS>
    where SS : StaticState<SS, DS>
    where DS : DynamicState<SS, DS> {

    /// <summary>
    /// The state to test being in.
    /// </summary>
    public DS GoalState;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="GoalState"></param>
    public InStateGoal( DS GoalState ) {
      this.GoalState = GoalState;
    }

    /// <summary>
    /// A preticate function which returns whether or not the Goal state has
    /// been reached.
    /// </summary>
    /// <param name="StaticState">The static state of this node.</param>
    /// <param name="DynamicState">The dynamic state of this node.</param>
    /// <returns>Whether or not the goal has been reached.</returns>
    public bool IsSatisfiedBy( SS StaticState, DS DynamicState ) {
      return this.GoalState.Equals( DynamicState );
    }

  }

}
