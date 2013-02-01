/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;
using System.IO;
using Net.Mokon.Edu.Unh.CS.AI.GridWorld;
using Net.Mokon.Utilities.CommandLine;

namespace Net.Mokon.Edu.Unh.CS.AI.Harness {

  public class MakeRoomsMaps : Mode {

    public override string GetHelp( ) {
      return "Makes a set of room maps.";
    }

    private int Height = 1000;
    private int Width = 1000;
    private int NumVertRooms = 50;
    private int NumHorzRooms = 50;
    private string Dir = "./maps/rooms";
    private int LOS = 7;
    private int Num = 5;
    private int NumSteps = 1000 ;
    private int Seed = (int)DateTime.Now.Ticks;
    private int HorzRoomSkip = 0;
    private int VertRoomSkip = 0;
    private double ChanceOpen = 0.01;
    private double ChanceClose = 0.01;
    private double MinAStarDistance = 0.0d;
    private double MaxAStarDistance = double.PositiveInfinity;
    private double MinHDistance = 0.0d;
    private double MaxHDistance = double.PositiveInfinity;
    private bool UseCorners = true;
    private bool GuarenteeSolveability = true;


    /// <summary>
    /// Constructor whihc initilizes data fields to their default values.
    /// </summary>
    public MakeRoomsMaps( ) {
      base.CLH = new CommandLineHandler(
         new CommandLineFlag( this, "The Min A* Distance", "MinAStarDistance", "mina", "min-a*-distance" ),
         new CommandLineFlag( this, "The Max A* Distance", "MaxAStarDistance", "maxa", "max-a*-distance" ),
         new CommandLineFlag( this, "The Min H Distance", "MinHDistance", "minh", "min-h-distance" ),
         new CommandLineFlag( this, "The Max H Distance", "MaxHDistance", "maxh", "max-h-distance" ),
         new CommandLineFlag( this, "Whether to use corners for start and goal locations", "UseCorners", "uc", "use-corners" ),
         new CommandLineFlag( this, "The directory to make choke maps in.", "Dir", "d", "dir" ),
         new CommandLineFlag( this, "The height of the choke maps to make.", "Height", "h", "height" ),
         new CommandLineFlag( this, "The width of the choke maps to make.", "Width", "w", "width" ),
         new CommandLineFlag( this, "The LOS of the choke maps to make.", "LOS", "l", "LOS" ),
         new CommandLineFlag( this, "The number of choke maps to make.", "Num", "n", "num" ),
         new CommandLineFlag( this, "The seed to use for the creation of choke maps.", "Seed", "s", "seed" ),
         new CommandLineFlag( this, "The number of vertical rooms to create", "NumVertRooms", "vr", "vertical-rooms" ),
         new CommandLineFlag( this, "The number of horizontial rooms to create", "NumHorzRooms", "hr", "horizontal-rooms" ),
         new CommandLineFlag( this, "The number of steps to create changes for", "NumSteps", "S", "steps" ),
         new CommandLineFlag( this, "The movement rate of blockages", "MoveRate", "m", "moverate" ),
         new CommandLineFlag( this, "The chance of a door opening", "ChanceOpen", "co", "chance-open" ),
         new CommandLineFlag( this, "The chance of a door closing", "ChanceClose", "cc", "chance-close" ),
         new CommandLineFlag( this, "The number of horz rooms to skip between swinging doors", "HorzRoomSkip", "hrs", "horizontal-room-skip" ),
         new CommandLineFlag( this, "The number of vert rooms to skip between swinging doors", "VertRoomSkip", "vrs", "vertical-room-skip" )
         );
    }

    /// <summary>
    /// Runs the Mode.
    /// </summary>
    /// <param name="args"></param>
    public override void Run( ) {
      RoomsGridWorldMaker mkr = new RoomsGridWorldMaker( LOS, Height, Width,
        NumHorzRooms, NumVertRooms, NumSteps, ChanceOpen, ChanceClose, HorzRoomSkip,
        VertRoomSkip, GuarenteeSolveability, MinAStarDistance, MaxAStarDistance, UseCorners,
        MinHDistance, MaxHDistance );
      Random Gen = new Random( Seed );
      string bdir = Dir + "/";
      int d = 0;
      while ( Directory.Exists( bdir + "/" + d ) ) {
        d++;
      }
      string rdir = bdir + "/" + d + "/";
      Directory.CreateDirectory( rdir );

      for ( int i = 0 ; i < Num ; i++ ) {
        bool redo = true;
        while ( redo ) {
          try {
            mkr.Make( Gen.Next( ), rdir + i + ".xml" );
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
      return "make-rooms";
    }

  }

}
