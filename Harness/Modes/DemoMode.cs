/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System.IO;
using Net.Mokon.Edu.Unh.CS.AI.GridWorld;
using Net.Mokon.Utilities;
using Net.Mokon.Utilities.CommandLine;
using Net.Mokon.Edu.Unh.CS.AI.Search;
using Net.Mokon.Edu.Unh.CS.AI.Search.Algorithms;
using System.Collections.Generic;

namespace Net.Mokon.Edu.Unh.CS.AI.Harness {

  /// <summary>
  /// A mode for demoing an algorithm.
  /// </summary>
  public class DemoMode : Mode {

    public override string GetHelp( ) {
      return "Runs a graphical simulation of an algorithm running.";
    }

    // Command Line Argument Fields
    protected GridWorldLoader<GenericGridWorldStaticState,
      GenericGridWorldDynamicState> Loader = new GenericGridWorldLoader( );
    private string Algorithm = "RealTimeD*Lite";
    private long refreshRate = 0;
    private int computationLimit = 50;
    private double Weight = 1.0d;
    private int LOS = 7;
    private TextReader filestream = null;
    private bool Dynamic = false;
    private int Seed = 0;
    internal StateVisualizer<GenericGridWorldStaticState,
      GenericGridWorldDynamicState> vis = null;
    internal Metric OptimalPathLenth = Metric.Zero;
    internal bool Solve = true;
    internal string file = "stdin";
    private string Demo = "none";

    /// <summary>
    /// Constructor to initilize the command line handler.
    /// </summary>
    public DemoMode( ) {
      base.CLH = new CommandLineHandler(
        new GridWorldLoaderFlag( this, "", "Loader", "l", "loader" ),
         new CommandLineFlag( this, "The algorithm to demo.", "Algorithm", "a", "algorithm" ),
         new CommandLineFlag( this, "The demo mode", "Demo", "d", "demo" ),
         new CommandLineFlag( this, "The seed for dynamic changes.", "Seed", "seed" ),
         new CommandLineFlag( this, "The refresh rate for the screen.", "refreshRate", "rr", "refreshRate" ),
         new CommandLineFlag( this, "The weight for algorithms that require a weight.", "Weight", "w", "Weight" ),
         new CommandLineFlag( this, "The computation limit to demo.", "computationLimit", "cl", "computationLimit" ),
         new CommandLineFlag( this, "The file to demo.", "file", "f", "file" ),
         new CommandLineFlag( this, "The LOS of the base unit to demo.", "LOS", "LOS", "lineOfSight" ),
         new CommandLineFlag( this, "Whether or not the world should dynamically change.", "Dynamic", "dyn", "dynamic" ),
         new VisFlag( this, "The visiulizer to use.", "vis", "v", "visilizer" )
         );
    }

    /// <summary>
    /// Finishes the required parsing
    /// </summary>
    public override void FinishParse( ) {
      // This must be done here otherwise opengl double start error...
      if ( vis == null ) {
        vis = new OpenGLStateVisualizer( );
      }
      if ( !Directory.Exists( this.file ) ) {
        SetFilename( this.file );
      }
    }

    /// <summary>
    /// Sets the file name
    /// </summary>
    /// <param name="filename"></param>
    public void SetFilename( string filename ) {
      if ( file.Equals( "stdin" ) ) {
        filestream = System.Console.In;
      } else {
        this.file = filename;
        filestream = new StreamReader( new FileStream( file,
          FileMode.Open, FileAccess.Read ) );
      }
    }

    /// <summary>
    /// Runs the demo mode.
    /// </summary>
    /// <param name="args"></param>
    public override void Run( string[] args ) {
      if ( Solve ) {
        this.ParseArgs( args );
      }
      if ( Demo.Equals( "presentation" ) ) {
        ( vis as OpenGLStateVisualizer ).Presentation( false, false, false );
      } else if ( Demo.Equals( "presentation-doors" ) ) {
        ( vis as OpenGLStateVisualizer ).Presentation( true, true, false );
      } else if ( Demo.Equals( "presentation-questions" ) ) {
        ( vis as OpenGLStateVisualizer ).Presentation( false, true, true );
      } else if ( Demo.Equals( "none" ) ) {
        
      }
      try {
        Loader.LOS = LOS;
        var AR = new AlgorithmRunner( filestream,
          Loader, Algorithm,
          new Metric( Weight ), false, vis, Seed );
        if ( Solve ) {
          var SOL = AR.Solve( null );
          OptimalPathLenth = SOL.Path.Cost;
        }
        AR.Resultss = Resultss;
        Results = AR.Run( refreshRate, false, 1000, Algorithm,
          computationLimit, OptimalPathLenth, file, Dynamic );
        System.Console.WriteLine( Results );
      } catch ( PathNotFoundException ) {
        System.Console.Out.WriteLine( "Path Not Found" );
      }
    }

    public List<Results> Resultss = null;
    public Results Results;

    /// <summary>
    /// Gets the name of the mode.
    /// </summary>
    /// <returns></returns>
    public override string Name( ) {
      return "demo";
    }

  }

}
