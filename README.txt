##################################    README   ################################

This file is part of the MAI framework, distributed under The M License.
Copyright © 2008-2010, David Bond, All Rights Resevered

To make this program simply type make. This should work with any implementation
of make but gmake is the only one it is tested on. The make runs on a recursive
cs file complilation so make sure there isn't any random cs files laying around.

To run the program refer to the command line documentation below.

This program requires the mono c-sharp complier/.NET framework or the Microsoft
c-sharp complier/.NET 3.5 framework. The GUI requires the Tao Framework Freeglut
and OpenGl bindings and your system must have freeglut and open gl installed on
it. Some older versions of mono will not build this. The fetch.sh script will
pull and build the latest mono daily snapshot if you are having trouble building
it on your operating system.

As of last count the MAI framework was around 12,000 lines including the MU
files that it utilizes.

If you wish to use this framework with another License please contact David Bond
and we can most likely figure something out.

##############################    DOCUMENTATION   #############################

This documentation is out of date as of 12/29/2009. Please bug me to update it.

This framework provides a means to anayalis search. This documenation assumes
a basic knownledge of search. To begin consider the concept of a search space.
A search space is a graph with nodes representing states and the arcs
representing the operators that transition between states. In this OO design the
StaticState is represents an unchanging portion of a state. This means it will
never change through operators, random actions, or external actions. The
DynamicState object on the other hand represents the portion of the state that
can change. This division is done to save space by not repeating data. Both of
these classes are abstract. A concrete implementation of them represents some
domain for search. The Operator class represents an action. The dynamic state
class has an expand method which takes a list of operators and if they are
applicable on the state will result in a list of new states formed by the
action. Normally this will be just one new state.

There is a complication defintion you can use, OpenGl, which enabled graphical
rendering of the agent moving around the map. To do this you need the Tao.OpenGl
dll's installed.

Here are some more classes:

Search Package
--------------
Path: Provides a list of action and states taken to get to a state.
PathVisualizer: An interface to implement a graphical representation of some
  domain.
BookKeeping: An interface for additional information to be stored in a
  DynamicState.
Goal: An interface which provides a test if a state is a goal.
Algorithm: An abstract class which provides a method to solve some search
  problem.
AlgorithmFactory: An interface for a factory design pattern to create
  algorithms.
GenericAlgorithmFactory: A domain independant factory for many implemented
  search algorithms.
Heuristic: An abstract class for testing the heurisitc value of a state.
HeuristicFactory: An interface for a factory design pattern to create
  heuristics.
GComputer: An abstract class for testing the G value of a state;

Search/Algorithm Package:
-------------------------
AStar: The AStar algorithm implemented with a best first search.
BestFirstSearch: An implementation of the best first search algorithm.
BrandAndBoundSearch: @Deprecated
DepthFirstSearch: The Depth First algorithm implemented with a best first
  search.
Greedy: The Greedy algorithm implemented with a best first search.
IterativeDeepingSearch: An implementation of the iterative deeping search
  algorithm.
LimitableAlgorithm: An abstract class for algorithms that can be limited in
  depth.
ParentBookKeeping: An implementation of	book keeping to keep track of the
  parent node in the search tree.
UniformedCostSearch: The uniformed cost search algorithm implemented with a
  best first search.
WeightedAStar: The Weighted AStar algorithm implemented with a best first
  search.

Search/GComputers Package:
--------------------------
PathCostGComputer: A G value computer that returns the cost of a path.
PathLengthGComputer: A G value computer that returns the path length.

Search/Heuristics Package:
--------------------------
ZeroHeuristic: A heuristic that always returns zero.

Search/RealTime Package:
------------------------
Transformer: A interface for a class that takes a goal and dyanmic state
  and converts them into a goal dynamic state.
SingleUnitTransformer: A transformer for a single unit gridworld problem.
SingleUnitManhattenDistanceHeuristic: A manhatten distance heursitic
  algorithm for when there is only one unit on the board.
Results: A DataStructure to hold the results of running an algorithm.
RealTimeSearchHarness: The main program. See below for command option
  and output description.
RealTimeAlgorithm: A generic interface for real time algorithms. These
  return an IEnuerable to be used in a foreach loop via using the yield
  statements. The should only return 1 action unless they have found the
  rest of the path.
InStateGoal: A goal to see if one state is the same as another.
DestinationsReachedGoal: A goal to see if all units have reached their
  destinations.
AlgorithmRunner: A class which knows how to run algorithms and retrieve
  results and statistics.
MapConverter: Converts a map from HOG to gwmap format.

Search/RealTime/Algorithm Package:
----------------------------------
AStar: AStar implemented as a repeating non informed real time
  algorithm.
BestFirstSearch: AStar implemented as a repeating non informed real
  time algorithm.
BRTS: A generic implementation of BRTS.
BRTSAStar: BRTS implemented with A*.
BRTSAStarSorting: A* BRTS with "free" sorting.
BRTSSotring: Generic BRTS with "free" sorting.
BRTSwAStar: BRTS implemented with wA*
LimitableAlgorithm: A generic interface for depth limitable real time
  algorithms.
ParentBookKeeping: @deprated, remove.
RTAStar: RTA* which expanse out to a given computation limit.
RTAStarSingle: RTA* which only expands once.
TBAStar: A first shot at implementing TBA*
TBAStar2: A second shot at implementing TBA*

GridWorld Package:
------------------
GenericGridWorldActions: Implements the basic operators for the grid world
  domain.
GenericGridWorldDynamicState: An implementation of the GridWorld domain's
  dynamic state with unit additions.
GenericGridWorldLoader: A grid world loader to load maps in std form.
GenericGridWorldPathVisualizer: A grid world path visualizer.
GenericGridWorldStaticState: An implementation of the GridWorld domain's
  static state.
GridWorldDynamicState: An implementation of the GridWorld domain's dynamic
  state.
GridWorldFormat: Formatting data and methods for a GridWorld.
GridWorldLoader: An interface for loading grid world maps from a stream.
GridWorldObject: A GridWorld object which represents something that can
  reside on a grid world.
GridWorldStaticState: An implementation of the GridWorld domain's static
  state.
HOGGridWorldLoader: A GridWorld Loader from the HOG file format.
Tile: A GridWorld Tile.
Unit: A GridWorld object that has a destination and can move.

Utilties Package:
-----------------
Distance: A class that has functions to calcualte distance.
IEnumerableExtensions: Some extensions to IEnumerable.
PriorirtyQueue: A priority queue data structure.
Tuple: A tupling data structure. Advoid using this when possible.

###############################    RUNTIME OPS   ##############################

When compiled the RealTimeSearchHarness has a number of modes it can run in and
it has man options for each of those modes. These are as follows:

  batch: Runs algorithms many time as specified by the options. The results are
    printed to stdout.
  demo: Runs one algorithm once as specified by the options and visilizes this
    on the screen.
  convert: Converts maps from one format to another.
  batch-playlist: not documented
  organize: Organizes batch printout into an easilier graphable form.

The options each mode takes are as follows:
  batch:
   -clm <int> : the computation limit max value.
   -clmin <int> : the computation limit min value.
   -w <double> : a weight for algorithms needing it.
   -minpl <int> : the min path length. optimal paths lower then this will be excluded
     from the results.
   -icl <int> : the iteration count limit. This makes sure only so many maps are proccesed.
   -maxpl <int> : the max path length. optimal paths higher then this will be excluded
   -maxpl <int> : the max offline astar expansions. offline astar expansions
     higher then this will be excluded
   -cli <int> : the computation limit increment. Each map is run from with a computation
     limit of 1 to computation limit max by increments of this value.
   -clcr <double> : this is a ratio which when given the optimal path length times this
     value is the maximum path length before the simulator will stop running an algorithm.
   -o <filename> a filename to output results to.
   -d <directory> a directory in which gwmap formated maps are located.
   output: The output will be in the form of Alogoritm, Computation Limit,
    Expansions, Generations, Computation Time, Path Length, and then
    Optimal Path Length in a cvs file. The first line will be a header file.
    The following errors are also possible. They will be on a line by themselves
    and should be considered normal. Mostly these should be ignored by parsers.
    No Path Posible, A* Error, Min Path Length Violated
    Max Path Length Violated, Min Expansions Violated,
    Max Expansions Violated, All Iterations Complete
  organize:
    -m ct : sub-optimality calcualted based on calulation time.
    -m pl : sub-optimality calcualted based on path length.
    output: csv with header row givening algorithms and then a first colomn
      giving computation limit and the following rows giving average
      sub-optimality.
  convert:
    <dir> : A directory to recursively load files from. The files are
      converted as specified by the map converter class. The resultant
      maps will have the same file name with a .gwmap appended. If
      unknown formatated maps are in the directory they are ignored.
  demo:
    -a <alg> : the alg to demo
    -rr <long> : the milliseconds between frame in the demo
    -w <double> : a weight for algorithms needing it.
    -cl <int> : the computation limit to run the demo with.
    -f <filename> : a file name with a map to run. If this is stdin
      the map will be run from stdin.
  batch-playlist: not documented


###############################################################################
RANDOM STUFF FOR ME

demo -cl 10000 -rr 250 -a RealTimeD*Lite
demo -cl 10 -a RealTimeD*Lite -rr 25 -f ../../maps/demo-maps/hard2.map  -f ../../maps/game/instance/battleground/130/9.gwmap -a TBA*-LPA*
batch -clm 500 -minpl 10 -icl 50 -maxpl 200 -cli 100 -clcr 10 -d maps/game
batch -clm 500 -minpl 10 -icl 50 -maxpl 200 -cli 100 -clcr 10 -d ../../maps/game -a 1 RealTimeD*Lite
demo -cl 10 -a RealTimeD*Lite -rr 25 -f ../../maps/game/instance/battleground/130/9.gwmap
batch -clm 250 -minpl 10 -icl 50 -maxpl 200 -cli 100 -clcr 10 -d ../../maps/game -a 2 BRTA* RealTimeD*Lite

demo -cl 50 -a RealTimeD*Lite -rr 25 -f ../../maps/game\\instance\\moonglade\\190\\0.gwmap
batch -clm 500 -minpl 10 -icl 50 -maxpl 200 -cli 100 -clcr 10 -d ../../maps/game -a 2 BRTA* RealTimeD*Lite -clmin 50

batch -clm 250 -minpl 10 -icl 50 -maxpl 200 -cli 100 -clcr 10 -d ../../maps/game -a 2 D*Lite RealTimeD*Lite
demo -cl 50 -a D*Lite -rr 25 -f ../../maps/game\\instance\\moonglade\\190\\0.gwmap
batch -clm 1000 -minpl 10 -icl 50 -maxpl 200 -cli 50 -clcr 10 -d ../../maps/game -a 3 D*Lite BRTA* RealTimeD*Lite
batch -clm 1000 -minpl 10 -icl 50 -maxpl 200 -cli 50 -clcr 10 -d ./maps/game -a 11 D*Lite BRTA* RealTimeD*Lite RealTimeD*Lite-USample RealTimeD*Lite-DeadAdjusted RealTimeD*Lite-USample RealTimeD*Lite-DeadAdjusted  RealTimeD*Lite-Adjusted RTA* RealTimeD*Lite-Dead TBA* -o out-raw.csv
organize -m ct -i out-raw2.csv -o out-org2-ct.csv

batch -clm 1000 -minpl 10 -icl 50 -maxpl 200 -cli 50 -clcr 10 -d ../../maps/game/instance/battleground/130/ -a 11 D*Lite BRTA* RealTimeD*Lite RealTimeD*Lite-USample RealTimeD*Lite-DeadAdjusted RealTimeD*Lite-USample RealTimeD*Lite-DeadAdjusted  RealTimeD*Lite-Adjusted RTA* RealTimeD*Lite-Dead TBA* -o out-raw3.csv
batch -clm 1000 -minpl 10 -icl 50 -maxpl 200 -cli 50 -clcr 10 -d ../../maps/game/instance/battleground/130/ -a 11 D*Lite BRTA* RealTimeD*Lite RealTimeD*Lite-USample RealTimeD*Lite-DeadAdjusted RealTimeD*Lite-USample RealTimeD*Lite-DeadAdjusted  RealTimeD*Lite-Adjusted RTA* RealTimeD*Lite-Dead TBA* -o out-raw3.csv
batch -clm 100 -icl 50 -cli 50 -clcr 10 -d ../../maps/game/instance/battleground/130/ -a 3 D*Lite BRTA* RealTimeD*Lite -o out-raw4.csv

demo -cl 10 -a RealTimeD*Lite-Dead -rr 0  -f ../../maps/game/instance/battleground/130/9.gwmap
 batch -clmin 10 -clm 11 -cli 50 -minpl 10 -icl 1 -maxpl 200 -clcr 10 -d ../../maps/game/instance/battleground/130/ -a 1 D*Lite
 demo -cl 10 -a D*Lite -rr 25 -f .../../maps/game\\instance\\moonglade\\190\\0.gwmap

 batch -clmin 101 -clm 102 -cli 50 -minpl 10 -icl 1 -maxpl 200 -clcr 10 -d ../../maps/game/instance/battleground/130/ -a 2 D*Lite RealTimeD*Lite
 batch -clmin 1 -clm 300 -cli 25 -minpl 10 -icl 1 -maxpl 200 -clcr 10 -d ../../maps/game/instance/battleground/ -a 3 D*Lite RealTimeD*Lite RealTimeD*Lite-Dead -o out-raw5.cs
 demo -a TBA*-LPA* -cl 25 -f ../../maps/game/instance/battleground/130/1.gwmap -rr 0
 demo -a RealTimeD*Lite-AStar -cl 25 -f ../../maps/game/instance/darkforest/130/1.gwmap -rr 0

 batch -clmin 1 -clm 100 -cli 25 -minpl 10 -icl 10 -maxpl 200 -clcr 10 -d ../../maps/game/instance/battleground/ -a 2 RealTimeD*Lite RealTimeD*Lite-Dead -o out-raw5.csv -mt 1

 batch -clmin 1 -clm 500 -cli 25 -minpl 10 -icl 10 -maxpl 200 -clcr 10 -d ../../maps/game/instance/battleground/ -a 3 RealTimeD*Lite RealTimeD*Lite-Dead RealTimeD*Lite-AStar -o out-raw5.csv
 organize -i out-raw5.csv -o out-org5.csv
 organize -i Raw.csv -o Org3.csv -g  GnuPlot3.csv -d 1.1 -p 0 -m 3

 ** For Presentation **
 demo -a RealTimeTBA*-LPA* -cl 25 -f ../../maps/game/instance/darkforest/110/2.gwmap -rr 0
 demo -a RealTimeD*Lite -cl 100 -f ../../maps/game/instance/darkforest/170/3.gwmap -rr 0
 demo -a RealTimeTBA*-LPA* -cl 25 -f ../../maps/game/instance/battleground/130/1.gwmap -rr 0
 batch-single -clmin 1 -clm 1200 -cli 25 -clcr 50 -O 99.5 -a 1 RealTimeD*Lite-AStar-Ratio=0.2 -i ./maps/game/instance/moonglade/190/0.gwmap -o out.tmp.txt -clmethod exp-1.5 -LOS 7 -mt 2
 demo -a RealTimeD*Lite-AStar-Ratio=0.2 -f ./maps/game/instance/moonglade/190/0.gwmap
  -v sonic
  organize -i Output-Raw.csv -o Output-Org1.csv -g  Output-GnuPlot1.csv -d 0 -p 0 -m 3
  demo-playlist -a RealTimeD*Lite-Adjusted-Lock-Goal -f playlist-battleground-alpha.txt  -cl 1000 -rr 0 -LOS 7
  make-playlist -d ./maps/random -o Playlist/playlist-random.txt  -mt 4
  demo-playlist -a RealTimeD*Lite-Adjusted-Lock-Goal -f Playlist/playlist-random.txt  -cl 1000 -rr 0 -LOS 7
  demo-playlist -a RealTimeD*Lite-AStarAdjusted-Lock-Goal-Weight=500.0 -f Playlist/playlist-random-0.5-2.txt  -cl 50 -rr 0 -LOS 7
  make-random -h 112 -w 112
  make-random -h 512 -w 512 -b 0.5 -n 10
  make-playlist -o Playlist\playlist-moonglade-upscaled.txt -d .\maps\scaled\512x512\maps\game\instance\moonglade

 demo -a LRTA*-dyn -f maps/demo-maps/bug2.map -cl 2 -rr 0 -LOS 7 -dyn false
 demo-playlist -a LRTA*-dyn -f Playlist/Paper/pl-moonglade-he.txt  -cl 50 -rr 0 -LOS 7 -dyn false
 demo -a LRTA*-dyn -f maps/chokepoints/0/0.xml -cl 2 -rr 0 -LOS 7 -dyn false -l xml
 demo -a RealTimeD*Lite-LRTA*-Goal -f maps/chokepoints/8/1.xml -cl 100 -rr 0 -LOS 1000 -dyn false -l xml

 // Good Example of LRTA* doing bad.
 demo -a LRTA*-dyn -f maps/chokepoints/12/4.xml -cl 100 -rr 0 -LOS 1000 -dyn false -l xml
  demo -a LRTA*-dyn -f maps/chokepoints/12/4.xml -cl 1000 -rr 0 -LOS 1000 -dyn false -l xml
  demo -a RealTimeD*Lite-NoOp -f maps/game/instance/battleground/120/21.gwmap -cl 1 -rr 0 -LOS 7 -dyn false

  demo-playlist -a Anytime-D* -f maps/rooms/1 -cl 50 -rr 0 -LOS 1000 -dyn false -l xml
  make-rooms -vr 10 -hr 10 -s 0 -S 1000
  demo-playlist -a LSS-LRTA* -f maps/rooms/ -cl 50 -rr 0 -LOS 1000 -dyn false -l xml
  demo-playlist -a D*Lite -f maps/rooms/1 -cl 50 -rr 0 -LOS 1000 -dyn false -l xml
  make-rooms -n 2 -cc 0.001 -co 0.001 -s 0 -S 1000
  RealTimeAnytimeD*-Ratio=0.50

  demo-playlist -a RealTimeAnytimeD*-Ratio=0.90 -f maps/rooms/0 -cl 50 -rr 0 -LOS 1000 -dyn false -l xml

  demo-playlist -a LSS-LRTA* -f maps/rooms/3/10.xml -cl 100 -rr 0 -LOS 1000 -dyn false -l xml
