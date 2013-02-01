/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;
using System.Collections.Generic;
using Net.Mokon.Utilities;

namespace Net.Mokon.Edu.Unh.CS.AI.Search {

  /// <summary>
  /// An action works with a dynamics state to transform it into another dynamic
  /// state in some way.
  /// </summary>
  public abstract class Operator<SS, DS>
    where SS : StaticState<SS, DS>
    where DS : DynamicState<SS, DS> {

    public static Path<SS, DS> MakePath( Operator<SS, DS> Op ) {
      var act = new LinkedList<Operator<SS, DS>>( );
      act.AddFirst( Op );
      return new Path<SS, DS>( (DS)null ) {
        Actions = act
      };
    }

    /// <summary>
    /// Tests to see if a dynamic state will currently accept the given action.
    /// </summary>
    /// <param name="DynamicState">The state to test.</param>
    /// <returns>A new state.</returns>
    public abstract bool IsValidOn( DS DynamicState, SS ss, bool all );

    public abstract bool IsNeighborValidOn( DS DynamicState, SS ss, bool all );

    /// <summary>
    /// Transforms the given state into a new state based on this actions
    /// properties. This will throw an IllegalActionOnStateException
    /// if the current state does not accept the given action. To prevent this
    /// use the isvalid on test first.
    /// </summary>
    /// <param name="DynamicState">The state to act on.</param>
    /// <returns>A new state.</returns>
    public abstract IEnumerable<DS> PerformOn( DS DynamicState, SS ss );

    /// <summary>
    /// A two string method which converts the action into a readable string.
    /// </summary>
    /// <returns>A string rep of the action.</returns>
    public abstract override string ToString( );

    /// <summary>
    /// A method that reverses the action on the current state. There is on
    /// corroponding IsValidOn method and the implementations of this are not
    /// required to check all bound so per careful about ensuring only valid
    /// states are given to this.
    /// </summary>
    /// <param name="DynamicState">The state to reverse this action on.</param>
    /// <returns>The new resulant state.</returns>
    public abstract IEnumerable<DS> ReverseOn( DS DynamicState, SS ss );

    /// <summary>
    /// Returns the cost of this action.
    /// </summary>
    /// <returns></returns>
    public abstract Metric Cost( DS DynamicState );

    /// <summary>
    /// Returns a clone.
    /// </summary>
    /// <returns>A clone.</returns>
    public virtual object Clone( ) {
      Operator<SS, DS> DS = this.MemberwiseClone( ) as Operator<SS, DS>;
      return DS;
    }

  }

  /// <summary>
  /// An exception indicating the given dynamic state can not be transformed 
  /// with the given action.
  /// </summary>
  public class IllegalActionOnStateException : Exception {

    /// <summary>
    /// A constructor based on an error message.
    /// </summary>
    /// <param name="Message">The error message to pass along.</param>
    public IllegalActionOnStateException( string Message )
      : base( Message ) {
    }

  }

  /// <summary>
  /// An exception class to indicate the action can not be performed on this
  /// method. IsValidOn should confirm that this exception will not be thrown.
  /// </summary>
  public class ActionStateException : Exception {

    /// <summary>
    /// A constructor based on an error message.
    /// </summary>
    /// <param name="Message">The error message to pass along.</param>
    public ActionStateException( string Message )
      : base( Message ) {
    }

  }

  /// <summary>
  /// An operator which reverses the base operator.
  /// </summary>
  /// <typeparam name="SS"></typeparam>
  /// <typeparam name="DS"></typeparam>
  public class ReverseOperator<SS, DS> : Operator<SS, DS>
    where SS : StaticState<SS, DS>
    where DS : DynamicState<SS, DS> {

    /// <summary>
    /// The base operator for this reverse operator.
    /// </summary>
    public readonly Operator<SS, DS> BaseOperator;

    /// <summary>
    /// Constructor which takes the static state and sets it up.
    /// </summary>
    /// <param name="StaticState">The static state to associate with this
    /// action.</param>
    public ReverseOperator( Operator<SS, DS> BaseOperator ) {
      this.BaseOperator = BaseOperator;
    }

    /// <summary>
    /// Tests to see if a dynamic state will currently accept the given action.
    /// </summary>
    /// <param name="DynamicState">The state to test.</param>
    /// <returns>A new state.</returns>
    public override bool IsValidOn( DS DynamicState, SS ss, bool all ) {
      return true;
    }

    public override bool IsNeighborValidOn( DS DynamicState, SS ss, bool all ) {
      return true;
    }

    /// <summary>
    /// Transforms the given state into a new state based on this actions
    /// properties. This will throw an IllegalActionOnStateException
    /// if the current state does not accept the given action. To prevent this
    /// use the isvalid on test first.
    /// </summary>
    /// <param name="DynamicState">The state to act on.</param>
    /// <returns>A new state.</returns>
    public override IEnumerable<DS> PerformOn( DS DynamicState, SS ss ) {
      return this.BaseOperator.ReverseOn( DynamicState, ss );
    }

    /// <summary>
    /// A two string method which converts the action into a readable string.
    /// </summary>
    /// <returns>A string rep of the action.</returns>
    public override string ToString( ) {
      return "Reverse ( " + this.BaseOperator.ToString( ) + " ) ";
    }

    /// <summary>
    /// A method that reverses the action on the current state. There is on
    /// corroponding IsValidOn method and the implementations of this are not
    /// required to check all bound so per careful about ensuring only valid
    /// states are given to this.
    /// </summary>
    /// <param name="DynamicState">The state to reverse this action on.</param>
    /// <returns>The new resulant state.</returns>
    public override IEnumerable<DS> ReverseOn( DS DynamicState, SS ss ) {
      return this.BaseOperator.PerformOn( DynamicState, ss );
    }

    /// <summary>
    /// Returns the cost of this action.
    /// </summary>
    /// <returns></returns>
    public override Metric Cost( DS DynamicState ) {
      return this.BaseOperator.Cost( DynamicState );
    }

  }

}
