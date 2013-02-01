/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using Net.Mokon.Utilities.CommandLine;

namespace Net.Mokon.Edu.Unh.CS.AI.Harness {

  /// <summary>
  /// A abstract class that implements a mode of how to start the harness.
  /// </summary>
  public abstract class Mode {

    /// <summary>
    /// The command line handler.
    /// </summary>
    internal CommandLineHandler CLH;

    /// <summary>
    /// Runs the Mode.
    /// </summary>
    /// <param name="args"></param>
    public virtual void Run( string[] args ) {
      this.ParseArgs( args );
      this.Run( );
    }

    /// <summary>
    /// Parses the args with the command line handler.
    /// </summary>
    /// <param name="args"></param>
    public virtual void ParseArgs( string[] args ) {
      CLH.Parse( args );
      this.FinishParse( );
    }

    /// <summary>
    /// A method to finish parses for concrete modes to override.
    /// </summary>
    public virtual void FinishParse( ) {

    }

    /// <summary>
    /// A method to run the mode for concrete modes to override.
    /// </summary>
    public virtual void Run( ) {

    }

    /// <summary>
    /// Gets the name of the mode.
    /// </summary>
    /// <returns></returns>
    public abstract string Name( );

    /// <summary>
    /// Returns a string describing the mode.
    /// </summary>
    /// <returns></returns>
    public abstract string GetHelp( );

  }

}
