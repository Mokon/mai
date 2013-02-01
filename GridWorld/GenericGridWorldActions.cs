/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System.Collections.Generic;
using Net.Mokon.Edu.Unh.CS.AI.Search;
using Net.Mokon.Edu.Unh.CS.AI.GridWorld;
using Net.Mokon.Utilities;
using System;

namespace Net.Mokon.Edu.Unh.CS.AI.GridWorld {

  /// <summary>
  /// an abstract class the other grid world operators use to simplify their
  /// operation.
  /// </summary>
  public abstract class GridWorldOperator :
    Operator<GenericGridWorldStaticState, GenericGridWorldDynamicState> {

    public static GridWorldOperator[] Ops = new GridWorldOperator[] {
          new MoveEast( 0 ),
          new MoveNorth( 0 ),
          new MoveSouth( 0 ),
          new MoveWest( 0 ),
          new MoveNorthEast( 0 ),
          new MoveNorthWest( 0 ),
          new MoveSouthEast( 0 ),
          new MoveSouthWest( 0 ),
      };

    /// <summary>
    /// The diagnal cost of movement.
    /// </summary>
    public static readonly Metric DiagnalCost =
      new Metric( Distance.OctileDistance( 0, 1, 0, 1 ) );

    public static readonly Metric StraightCost = new Metric( 1.0d );

    /// <summary>
    /// The unit this operator acts on.
    /// </summary>
    protected int Unit;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="Unit"></param>
    public GridWorldOperator( int Unit ) {
      this.Unit = Unit;
    }

    /// <summary>
    /// Tests if the action is valid.
    /// </summary>
    /// <param name="DynamicState"></param>
    /// <param name="ss"></param>
    /// <param name="all"></param>
    /// <param name="SideCheck"></param>
    /// <param name="Y"></param>
    /// <param name="X"></param>
    /// <returns></returns>
    public bool IsValidOn( GenericGridWorldDynamicState DynamicState,
      GenericGridWorldStaticState ss, bool all, bool SideCheck, int Y, int X ) {
      return ( SideCheck && ss.Tiles[Y][X].IsPassable( ss, all ) );
    }

  }


  public class MoveNorthWest : GridWorldOperator {

    public MoveNorthWest( int Unit )
      : base( Unit ) {
    }

    public override bool IsValidOn( GenericGridWorldDynamicState DynamicState,
      GenericGridWorldStaticState ss, bool all ) {
      Unit U = DynamicState.FindUnit( Unit );
      return base.IsValidOn( DynamicState, ss, all,
        U.Y > 0 && U.X > 0, U.Y - 1, U.X - 1 );
    }

    public override bool IsNeighborValidOn( GenericGridWorldDynamicState DynamicState,
      GenericGridWorldStaticState ss, bool all ) {
      Unit U = DynamicState.FindUnit( Unit );
      return U.Y > 0 && U.X > 0;
    }

    public override IEnumerable<GenericGridWorldDynamicState> PerformOn(
      GenericGridWorldDynamicState
      DynamicState, GenericGridWorldStaticState ss ) {
      GenericGridWorldDynamicState ds =
        DynamicState.Clone( ) as GenericGridWorldDynamicState;
      
      Unit U = ds.FindUnit( Unit );
      ds.hash ^= U.GetHashCode( );
      U.Y--;
      U.X--;
      ds.hash ^= U.GetHashCode( );
      var l = new List<GenericGridWorldDynamicState>( );
      l.Add( ds );
      return l;
    }

    public override string ToString( ) {
      return "Unit Move North West";
    }

    public override IEnumerable<GenericGridWorldDynamicState> ReverseOn(
      GenericGridWorldDynamicState
      DynamicState, GenericGridWorldStaticState ss ) {
      GenericGridWorldDynamicState ds =
        DynamicState.Clone( ) as GenericGridWorldDynamicState;
      Unit U = ds.FindUnit( Unit );
      ds.hash ^= U.GetHashCode( );
      U.Y++;
      U.X++;
      ds.hash ^= U.GetHashCode( );
      var l = new List<GenericGridWorldDynamicState>( );
      l.Add( ds );
      return l;
    }

    public override Metric Cost( GenericGridWorldDynamicState DynamicState ) {
      return GridWorldOperator.DiagnalCost;
    }

  }

  public class MoveNorthEast : GridWorldOperator {

    public MoveNorthEast( int Unit )
      : base( Unit ) {
    }

    public override bool IsValidOn( GenericGridWorldDynamicState DynamicState,
      GenericGridWorldStaticState ss, bool all ) {
      Unit U = DynamicState.FindUnit( Unit );
      return base.IsValidOn( DynamicState, ss, all,
        U.Y > 0 && U.X < ss.Width - 1, U.Y - 1, U.X + 1 );
    }

    public override bool IsNeighborValidOn( GenericGridWorldDynamicState DynamicState,
     GenericGridWorldStaticState ss, bool all ) {
      Unit U = DynamicState.FindUnit( Unit );
      return U.Y > 0 && U.X < ss.Width - 1;
    }

    public override IEnumerable<GenericGridWorldDynamicState> PerformOn(
      GenericGridWorldDynamicState
      DynamicState, GenericGridWorldStaticState ss ) {
      GenericGridWorldDynamicState ds =
        DynamicState.Clone( ) as GenericGridWorldDynamicState;
      Unit U = ds.FindUnit( Unit );
      ds.hash ^= U.GetHashCode( );
      U.Y--;
      U.X++;
      ds.hash ^= U.GetHashCode( );
      var l = new List<GenericGridWorldDynamicState>( );
      l.Add( ds );
      return l;
    }

    public override string ToString( ) {
      return "Unit Move North East";
    }

    public override IEnumerable<GenericGridWorldDynamicState> ReverseOn(
      GenericGridWorldDynamicState
      DynamicState, GenericGridWorldStaticState ss ) {
      GenericGridWorldDynamicState ds =
        DynamicState.Clone( ) as GenericGridWorldDynamicState;
      Unit U = ds.FindUnit( Unit );
      ds.hash ^= U.GetHashCode( );
      U.Y++;
      U.X--;
      ds.hash ^= U.GetHashCode( );
      var l = new List<GenericGridWorldDynamicState>( );
      l.Add( ds );
      return l;
    }

    public override Metric Cost( GenericGridWorldDynamicState DynamicState ) {
      return GridWorldOperator.DiagnalCost;
    }

  }

  public class MoveSouthWest : GridWorldOperator {

    public MoveSouthWest( int Unit )
      : base( Unit ) {
    }

    public override bool IsValidOn( GenericGridWorldDynamicState DynamicState,
      GenericGridWorldStaticState ss, bool all ) {
      Unit U = DynamicState.FindUnit( Unit );
      return base.IsValidOn( DynamicState, ss, all,
        U.Y < ss.Height - 1 && U.X > 0, U.Y + 1, U.X - 1 );
    }

    public override bool IsNeighborValidOn( GenericGridWorldDynamicState DynamicState,
      GenericGridWorldStaticState ss, bool all ) {
      Unit U = DynamicState.FindUnit( Unit );
      return U.Y < ss.Height - 1 && U.X > 0;
    }

    public override IEnumerable<GenericGridWorldDynamicState> PerformOn(
      GenericGridWorldDynamicState
      DynamicState, GenericGridWorldStaticState ss ) {
      GenericGridWorldDynamicState ds =
        DynamicState.Clone( ) as GenericGridWorldDynamicState;
      Unit U = ds.FindUnit( Unit );
      ds.hash ^= U.GetHashCode( );
      U.Y++;
      U.X--;
      ds.hash ^= U.GetHashCode( );
      var l = new List<GenericGridWorldDynamicState>( );
      l.Add( ds );
      return l;
    }

    public override string ToString( ) {
      return "Unit Move South West";
    }

    public override IEnumerable<GenericGridWorldDynamicState> ReverseOn(
      GenericGridWorldDynamicState
      DynamicState, GenericGridWorldStaticState ss ) {
      GenericGridWorldDynamicState ds =
        DynamicState.Clone( ) as GenericGridWorldDynamicState;
      Unit U = ds.FindUnit( Unit );
      ds.hash ^= U.GetHashCode( );
      U.Y--;
      U.X++;
      ds.hash ^= U.GetHashCode( );
      var l = new List<GenericGridWorldDynamicState>( );
      l.Add( ds );
      return l;
    }

    public override Metric Cost( GenericGridWorldDynamicState DynamicState ) {
      return GridWorldOperator.DiagnalCost;
    }

  }

  public class MoveSouthEast : GridWorldOperator {

    public MoveSouthEast( int Unit )
      : base( Unit ) {
    }

    public override bool IsValidOn( GenericGridWorldDynamicState DynamicState,
      GenericGridWorldStaticState ss, bool all ) {
      Unit U = DynamicState.FindUnit( Unit );
      return base.IsValidOn( DynamicState, ss, all,
        U.Y < ss.Height - 1 && U.X < ss.Width - 1, U.Y + 1, U.X + 1 );
    }

    public override bool IsNeighborValidOn( GenericGridWorldDynamicState DynamicState,
      GenericGridWorldStaticState ss, bool all ) {
      Unit U = DynamicState.FindUnit( Unit );
      return U.Y < ss.Height - 1 && U.X < ss.Width - 1;
    }

    public override IEnumerable<GenericGridWorldDynamicState> PerformOn(
      GenericGridWorldDynamicState
      DynamicState, GenericGridWorldStaticState ss ) {
      GenericGridWorldDynamicState ds =
        DynamicState.Clone( ) as GenericGridWorldDynamicState;
      Unit U = ds.FindUnit( Unit );
      ds.hash ^= U.GetHashCode( );
      U.Y++;
      U.X++;
      ds.hash ^= U.GetHashCode( );
      var l = new List<GenericGridWorldDynamicState>( );
      l.Add( ds );
      return l;
    }

    public override string ToString( ) {
      return "Unit Move South East";
    }

    public override IEnumerable<GenericGridWorldDynamicState> ReverseOn(
      GenericGridWorldDynamicState
      DynamicState, GenericGridWorldStaticState ss ) {
      GenericGridWorldDynamicState ds =
        DynamicState.Clone( ) as GenericGridWorldDynamicState;
      Unit U = ds.FindUnit( Unit );
      ds.hash ^= U.GetHashCode( );
      U.Y--;
      U.X--;
      ds.hash ^= U.GetHashCode( );
      var l = new List<GenericGridWorldDynamicState>( );
      l.Add( ds );
      return l;
    }

    public override Metric Cost( GenericGridWorldDynamicState DynamicState ) {
      return GridWorldOperator.DiagnalCost;
    }

  }

  /// <summary>
  /// An action that moves the bot one square north.
  /// </summary>
  public class MoveNorth : GridWorldOperator {

    /// <summary>
    /// A basic constructor.
    /// </summary>
    /// <param name="StaticState">The static state for this action to work on.
    /// </param>
    public MoveNorth( int Unit )
      : base( Unit ) {
    }

    /// <summary>
    /// Tests to see if this action can be performed on the given dynamic state.
    /// </summary>
    /// <param name="DynamicState">The dynamic state to test against.</param>
    /// <returns>Whether this is currently a valid action on the given
    /// state.</returns>
    public override bool IsValidOn( GenericGridWorldDynamicState DynamicState,
      GenericGridWorldStaticState ss, bool all ) {
      Unit U = DynamicState.FindUnit( Unit );
      return base.IsValidOn( DynamicState, ss, all, U.Y > 0, U.Y - 1, U.X );
    }

    public override bool IsNeighborValidOn( GenericGridWorldDynamicState DynamicState,
     GenericGridWorldStaticState ss, bool all ) {
      Unit U = DynamicState.FindUnit( Unit );
      return U.Y > 0;
    }

    /// <summary>
    /// Performs the action on the given state. This function is not safe and
    /// it therefore assumes the IsValidOn function has been called in the past.
    /// </summary>
    /// <param name="DynamicState">The state to act upon.</param>
    /// <returns>The resultant state after this action has been performed.
    /// </returns>
    public override IEnumerable<GenericGridWorldDynamicState> PerformOn(
      GenericGridWorldDynamicState
      DynamicState, GenericGridWorldStaticState ss ) {
      GenericGridWorldDynamicState ds =
        DynamicState.Clone( ) as GenericGridWorldDynamicState;
      Unit U = ds.FindUnit( Unit );
      ds.hash ^= U.GetHashCode( );
      U.Y--;
      ds.hash ^= U.GetHashCode( );
      var l = new List<GenericGridWorldDynamicState>( );
      l.Add( ds );
      return l;
    }

    /// <summary>
    /// Converts the action to a human readable string.
    /// </summary>
    /// <returns>A string representing the action.</returns>
    public override string ToString( ) {
      return "Unit Move North";
    }

    /// <summary>
    /// A method that reverses the action on the current state. There is on
    /// corroponding IsValidOn method and the implementations of this are not
    /// required to check all bound so per careful about ensuring only valid
    /// states are given to this.
    /// </summary>
    /// <param name="DynamicState">The state to reverse this action on.</param>
    /// <returns>The new resulant state.</returns>
    public override IEnumerable<GenericGridWorldDynamicState> ReverseOn(
      GenericGridWorldDynamicState
      DynamicState, GenericGridWorldStaticState ss ) {
      GenericGridWorldDynamicState ds =
        DynamicState.Clone( ) as GenericGridWorldDynamicState;
      Unit U = ds.FindUnit( Unit );
      ds.hash ^= U.GetHashCode( );
      U.Y++;
      ds.hash ^= U.GetHashCode( );
      var l = new List<GenericGridWorldDynamicState>( );
      l.Add( ds );
      return l;
    }

    /// <summary>
    /// Computes the cost to perform this action.
    /// </summary>
    /// <returns>The cost to perform this action.</returns>
    public override Metric Cost( GenericGridWorldDynamicState DynamicState ) {
      return GridWorldOperator.StraightCost;
    }

  }

  /// <summary>
  /// An action which moves the bot one square east.
  /// </summary>
  public class MoveEast : GridWorldOperator {

    /// <summary>
    /// A basic constructor.
    /// </summary>
    /// <param name="StaticState">The static state for this action to work on.
    /// </param>
    public MoveEast( int Unit )
      : base( Unit ) {
    }

    /// <summary>
    /// Tests to see if this action can be performed on the given dynamic state.
    /// </summary>
    /// <param name="DynamicState">The dynamic state to test against.</param>
    /// <returns>Whether this is currently a valid action on the given
    /// state.</returns>
    public override bool IsValidOn( GenericGridWorldDynamicState DynamicState,
      GenericGridWorldStaticState ss, bool all ) {
      Unit U = DynamicState.FindUnit( Unit );
      return base.IsValidOn( DynamicState, ss, all,
        U.X < ss.Width - 1, U.Y, U.X + 1 );
    }

    public override bool IsNeighborValidOn( GenericGridWorldDynamicState DynamicState,
     GenericGridWorldStaticState ss, bool all ) {
      Unit U = DynamicState.FindUnit( Unit );
      return U.X < ss.Width - 1;
    }

    /// <summary>
    /// Performs the action on the given state. This function is not safe and
    /// it therefore assumes the IsValidOn function has been called in the past.
    /// </summary>
    /// <param name="DynamicState">The state to act upon.</param>
    /// <returns>The resultant state after this action has been performed.
    /// </returns>
    public override IEnumerable<GenericGridWorldDynamicState> PerformOn(
      GenericGridWorldDynamicState
      DynamicState, GenericGridWorldStaticState ss ) {
      GenericGridWorldDynamicState ds =
        DynamicState.Clone( ) as GenericGridWorldDynamicState;
      Unit U = ds.FindUnit( Unit );
      ds.hash ^= U.GetHashCode( );
      U.X++;
      ds.hash ^= U.GetHashCode( );
      var l = new List<GenericGridWorldDynamicState>( );
      l.Add( ds );
      return l;
    }

    /// <summary>
    /// Converts the action to a human readable string.
    /// </summary>
    /// <returns>A string representing the action.</returns>
    public override string ToString( ) {
      return "Bot Move East";
    }

    /// <summary>
    /// A method that reverses the action on the current state. There is on
    /// corroponding IsValidOn method and the implementations of this are not
    /// required to check all bound so per careful about ensuring only valid
    /// states are given to this.
    /// </summary>
    /// <param name="DynamicState">The state to reverse this action on.</param>
    /// <returns>The new resulant state.</returns>
    public override IEnumerable<GenericGridWorldDynamicState> ReverseOn(
      GenericGridWorldDynamicState
      DynamicState, GenericGridWorldStaticState ss ) {
      GenericGridWorldDynamicState ds =
        DynamicState.Clone( ) as GenericGridWorldDynamicState;
      Unit U = ds.FindUnit( Unit );
      ds.hash ^= U.GetHashCode( );
      U.X--;
      ds.hash ^= U.GetHashCode( );
      var l = new List<GenericGridWorldDynamicState>( );
      l.Add( ds );
      return l;
    }

    /// <summary>
    /// Computes the cost to perform this action.
    /// </summary>
    /// <returns>The cost to perform this action.</returns>
    public override Metric Cost( GenericGridWorldDynamicState DynamicState ) {
      return GridWorldOperator.StraightCost;
    }

  }

  /// <summary>
  /// An action that moves the bot one square south.
  /// </summary>
  public class MoveSouth : GridWorldOperator {

    /// <summary>
    /// A basic constructor.
    /// </summary>
    /// <param name="StaticState">The static state for this action to work on.
    /// </param>
    public MoveSouth( int Unit )
      : base( Unit ) {
    }

    /// <summary>
    /// Tests to see if this action can be performed on the given dynamic state.
    /// </summary>
    /// <param name="DynamicState">The dynamic state to test against.</param>
    /// <returns>Whether this is currently a valid action on the given
    /// state.</returns>
    public override bool IsValidOn( GenericGridWorldDynamicState DynamicState,
      GenericGridWorldStaticState ss, bool all ) {
      Unit U = DynamicState.FindUnit( Unit );
      return base.IsValidOn( DynamicState, ss, all,
        U.Y < ss.Height - 1, U.Y + 1, U.X );
    }

    public override bool IsNeighborValidOn( GenericGridWorldDynamicState DynamicState,
  GenericGridWorldStaticState ss, bool all ) {
      Unit U = DynamicState.FindUnit( Unit );
      return U.Y < ss.Height - 1;
    }

    /// <summary>
    /// Performs the action on the given state. This function is not safe and
    /// it therefore assumes the IsValidOn function has been called in the past.
    /// </summary>
    /// <param name="DynamicState">The state to act upon.</param>
    /// <returns>The resultant state after this action has been performed.
    /// </returns>
    public override IEnumerable<GenericGridWorldDynamicState> PerformOn(
      GenericGridWorldDynamicState DynamicState,
      GenericGridWorldStaticState ss ) {
      GenericGridWorldDynamicState ds =
        DynamicState.Clone( ) as GenericGridWorldDynamicState;
      Unit U = ds.FindUnit( Unit );
      ds.hash ^= U.GetHashCode( );
      U.Y++;
      ds.hash ^= U.GetHashCode( );
      var l = new List<GenericGridWorldDynamicState>( );
      l.Add( ds );
      return l;
    }

    /// <summary>
    /// Converts the action to a human readable string.
    /// </summary>
    /// <returns>A string representing the action.</returns>
    public override string ToString( ) {
      return "Bot Move South";
    }

    /// <summary>
    /// A method that reverses the action on the current state. There is on
    /// corroponding IsValidOn method and the implementations of this are not
    /// required to check all bound so per careful about ensuring only valid
    /// states are given to this.
    /// </summary>
    /// <param name="DynamicState">The state to reverse this action on.</param>
    /// <returns>The new resulant state.</returns>
    public override IEnumerable<GenericGridWorldDynamicState> ReverseOn(
      GenericGridWorldDynamicState DynamicState,
      GenericGridWorldStaticState ss ) {
      GenericGridWorldDynamicState ds =
        DynamicState.Clone( ) as GenericGridWorldDynamicState;
      Unit U = ds.FindUnit( Unit );
      ds.hash ^= U.GetHashCode( );
      U.Y--;
      ds.hash ^= U.GetHashCode( );
      var l = new List<GenericGridWorldDynamicState>( );
      l.Add( ds );
      return l;
    }

    /// <summary>
    /// Computes the cost to perform this action.
    /// </summary>
    /// <returns>The cost to perform this action.</returns>
    public override Metric Cost( GenericGridWorldDynamicState DynamicState ) {
      return GridWorldOperator.StraightCost;
    }

  }

  /// <summary>
  /// An action that moves the bot one square west.
  /// </summary>
  public class MoveWest : GridWorldOperator {

    /// <summary>
    /// A basic constructor.
    /// </summary>
    /// <param name="StaticState">The static state for this action to work on.
    /// </param>
    public MoveWest( int Unit )
      : base( Unit ) {
    }

    /// <summary>
    /// Tests to see if this action can be performed on the given dynamic state.
    /// </summary>
    /// <param name="DynamicState">The dynamic state to test against.</param>
    /// <returns>Whether this is currently a valid action on the given
    /// state.</returns>
    public override bool IsValidOn( GenericGridWorldDynamicState DynamicState,
      GenericGridWorldStaticState ss, bool all ) {
      Unit U = DynamicState.FindUnit( Unit );
      return base.IsValidOn( DynamicState, ss, all, U.X > 0, U.Y, U.X - 1 );
    }

    public override bool IsNeighborValidOn( GenericGridWorldDynamicState DynamicState,
      GenericGridWorldStaticState ss, bool all ) {
      Unit U = DynamicState.FindUnit( Unit );
      return U.X > 0;
    }

    /// <summary>
    /// Performs the action on the given state. This function is not safe and
    /// it therefore assumes the IsValidOn function has been called in the past.
    /// </summary>
    /// <param name="DynamicState">The state to act upon.</param>
    /// <returns>The resultant state after this action has been performed.
    /// </returns>
    public override IEnumerable<GenericGridWorldDynamicState> PerformOn(
      GenericGridWorldDynamicState
      DynamicState, GenericGridWorldStaticState ss ) {
      GenericGridWorldDynamicState ds =
        DynamicState.Clone( ) as GenericGridWorldDynamicState;
      Unit U = ds.FindUnit( Unit );
      ds.hash ^= U.GetHashCode( );
      U.X--;
      ds.hash ^= U.GetHashCode( );
      var l = new List<GenericGridWorldDynamicState>( );
      l.Add( ds );
      return l;
    }

    /// <summary>
    /// Converts the action to a human readable string.
    /// </summary>
    /// <returns>A string representing the action.</returns>
    public override string ToString( ) {
      return "Bot Move West";
    }

    /// <summary>
    /// A method that reverses the action on the current state. There is on
    /// corroponding IsValidOn method and the implementations of this are not
    /// required to check all bound so per careful about ensuring only valid
    /// states are given to this.
    /// </summary>
    /// <param name="DynamicState">The state to reverse this action on.</param>
    /// <returns>The new resulant state.</returns>
    public override IEnumerable<GenericGridWorldDynamicState> ReverseOn(
      GenericGridWorldDynamicState
      DynamicState, GenericGridWorldStaticState ss ) {
      GenericGridWorldDynamicState ds =
        DynamicState.Clone( ) as GenericGridWorldDynamicState;
      Unit U = ds.FindUnit( Unit );
      ds.hash ^= U.GetHashCode( );
      U.X++;
      ds.hash ^= U.GetHashCode( );
      var l = new List<GenericGridWorldDynamicState>( );
      l.Add( ds );
      return l;
      ;
    }

    /// <summary>
    /// Computes the cost to perform this action.
    /// </summary>
    /// <returns>The cost to perform this action.</returns>
    public override Metric Cost( GenericGridWorldDynamicState DynamicState ) {
      return GridWorldOperator.StraightCost;
    }

  }

}
