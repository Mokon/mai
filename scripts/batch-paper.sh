#!/bin/sh
export CompLimitMin=1
export CompLimitMax=1001
export CompLimitMethod=arr
export CompLimitIncrement=25
export CeilingRatio=1000
export LOS=7
export MaxThreads=1
export PlayList=$1
export Dynamic=false
export Algs="10\
 D*Lite\
 LSS-LRTA*\
 LRTA*-dyn\
 RealTimeD*Lite-NoOp\
 RealTimeD*Lite-LRTA*-Goal-Ratio=0.25\
 RealTimeD*Lite-LRTA*-Goal-Ratio=0.50\
 RealTimeD*Lite-LRTA*-Goal-Ratio=0.75\
 RealTimeD*Lite-LSS-LRTA*-Goal-Ratio=0.25\
 RealTimeD*Lite-LSS-LRTA*-Goal-Ratio=0.50\
 RealTimeD*Lite-LSS-LRTA*-Goal-Ratio=0.75\
"
export TSTAMP=`date +%s`-$PlayList
if [[ ! -e Output ]] ; then
  mkdir Output
fi
export OutputFolder=Output/Output-$TSTAMP/
mkdir $OutputFolder
export OutputStatusPrefix=Output-Status-
export OutputRaw=Output-Raw
export OutputOrg=Output-Org
export OutputCsvExtension=.csv
export OutputTxtExtension=.txt


exec 3<Playlist/Paper/$PlayList
while read -u 3 Filename OptimalCost
do
FILENAME=`echo $Filename | sed -e 's/\//-/g'`
./mai.sh batch-single -clmin $CompLimitMin -clm $CompLimitMax -cli $CompLimitIncrement \
	-clcr $CeilingRatio -O $OptimalCost -a $Algs -i $Filename \
	-o $OutputFolder$OutputRaw$FILENAME$OutputCsvExtension -clmethod $CompLimitMethod -LOS $LOS -mt $MaxThreads -dyn $Dynamic\
     > $OutputFolder$OutputStatusPrefix$FILENAME$OutputTxtExtension
if [[ $? == 0 ]] ; then
  cat $OutputFolder$OutputRaw$OutputCsvExtension $OutputFolder$OutputRaw$FILENAME$OutputCsvExtension > tmp
  more tmp > $OutputFolder$OutputRaw$OutputCsvExtension 
  rm -f tmp
fi
done

./mai.sh organize -i $OutputFolder$OutputRaw$OutputCsvExtension  -o $OutputFolder$OutputOrg$OutputCsvExtension 
