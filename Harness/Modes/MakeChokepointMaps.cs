/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;
using System.IO;
using Net.Mokon.Edu.Unh.CS.AI.GridWorld;
using Net.Mokon.Utilities.CommandLine;

namespace Net.Mokon.Edu.Unh.CS.AI.Harness {

  public class MakeChokepointMaps : Mode {

    public override string GetHelp( ) {
      return "Makes a set of chokepoint maps.";
    }

    private int Height = 100;
    private int Width = 100;
    private int NumChokepoint = 30;
    private int NumAgents = 20;
    private int ChokepointRadius = 3;
    private int AgentRadius = 3;
    private string Dir = "./maps/chokepoints";
    private int LOS = 7;
    private int Num = 5;
    private int NumSteps = 1000;
    private int Seed = (int)DateTime.Now.Ticks;
    private double MoveRate = 0.1;


    /// <summary>
    /// Constructor whihc initilizes data fields to their default values.
    /// </summary>
    public MakeChokepointMaps( ) {
      base.CLH = new CommandLineHandler(
         new CommandLineFlag( this, "The directory to make choke maps in.", "Dir", "d", "dir" ),
         new CommandLineFlag( this, "The height of the choke maps to make.", "Height", "h", "height" ),
         new CommandLineFlag( this, "The width of the choke maps to make.", "Width", "w", "width" ),
         new CommandLineFlag( this, "The LOS of the choke maps to make.", "LOS", "l", "LOS" ),
         new CommandLineFlag( this, "The number of choke maps to make.", "Num", "n", "num" ),
         new CommandLineFlag( this, "The seed to use for the creation of choke maps.", "Seed", "s", "seed" ),
         new CommandLineFlag( this, "The number of choke points to create", "NumChokepoint", "c", "chokes" ),
         new CommandLineFlag( this, "The number of agents to create", "NumAgents", "a", "agents" ),
         new CommandLineFlag( this, "The number of steps to create changes for", "NumSteps", "S", "steps" ), 
         new CommandLineFlag( this, "The radius of the choke points to create", "ChokepointRadius", "r", "radius" ),
         new CommandLineFlag( this, "The movement rate of blockages", "MoveRate", "m", "moverate" ),
         new CommandLineFlag( this, "The radius of the agents to create", "AgentRadius", "ar", "aradius" )
         );
    }

    /// <summary>
    /// Runs the Mode.
    /// </summary>
    /// <param name="args"></param>
    public override void Run( ) {
      ChokepointGridWorldMaker mkr = new ChokepointGridWorldMaker( LOS, 
        NumChokepoint, Height, Width, ChokepointRadius, NumAgents, NumSteps, AgentRadius, MoveRate );
      Random Gen = new Random( Seed );
      string bdir = Dir + "/";
      int d = 0;
      while ( Directory.Exists( bdir + "/" + d ) ) {
        d++;                                                                                                                                    
      }
      string rdir = bdir + "/" + d + "/";
      Directory.CreateDirectory( rdir );

      for ( int i = 0 ; i < Num ; i++ ) {
        bool redo = true ;
        while ( redo ) {
          try {
            mkr.Make( Gen.Next(), rdir + i + ".xml" );
            redo = false;
          } catch ( ChokepointGridWorldMaker.MapCreationFailed ) {
            redo = true;
          }
        }
      }
    }

    /// <summary>
    /// Gets the name of the mode.
    /// </summary>
    /// <returns></returns>
    public override string Name( ) {
      return "make-choke";
    }

  }

}
