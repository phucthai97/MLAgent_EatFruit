#!/bin/bash

file_path="results/configuration.yaml"
run_id=$1
time_sleep=1

#max_steps
# Extract the max_steps value from the configuration file and assign it to the current_max_steps variable
current_max_steps=$(grep 'max_steps:' "$file_path" | awk '{print $2}')
interval_max_steps=300000
max_intervals=$2
max_steps_of_flow=$((current_max_steps + max_intervals*interval_max_steps))

#Check available
folder="results/$1"
if [ -d "$folder" ]; then

  

  echo "Initial max_steps: $current_max_steps"
  
  ###  FIRST RUN!  ####
  #echo -e "\n"
  echo "  ______   _________     _       _______   _________   _________  _______          _       _____  ____  _____  _____  ____  _____   ______   "
  echo ".' ____ \ |  _   _  |   / \     |_   __ \ |  _   _  | |  _   _  ||_   __ \        / \     |_   _||_   \|_   _||_   _||_   \|_   _|.' ___  |  "
  echo "| (___ \_||_/ | | \_|  / _ \      | |__) ||_/ | | \_| |_/ | | \_|  | |__) |      / _ \      | |    |   \ | |    | |    |   \ | | / .'   \_|  "
  echo " _.____\`.     | |     / ___ \     |  __ /     | |         | |      |  __ /      / ___ \     | |    | |\ \| |    | |    | |\ \| | | |   ____  "
  echo "| \____) |   _| |_  _/ /   \ \_  _| |  \ \_  _| |_       _| |_    _| |  \ \_  _/ /   \ \_  _| |_  _| |_\   |_  _| |_  _| |_\   |_\ \`.___]  | "
  echo " \______.'  |_____||____| |____||____| |___||_____|     |_____|  |____| |___||____| |____||_____||_____|\____||_____||_____|\____|\`._____.'  "

  #echo -e "\n"
  
  echo -e "\n|---------------------------------------------|"
  echo "|   Author: PhucThai                          |"
  echo "|   Date create: June 12 2024                 |"
  echo "|   Version: 1.0                              |"
  echo "|   Folder run: results/$1              |"
  echo "|   Interation incremental: $2                 |"
  echo -e "|   Max_step increase of flow: $max_steps_of_flow steps |"
  echo -e "|---------------------------------------------|\n"
  echo -e "\nStart interval - max_steps: $current_max_steps - id=$1 \n"

  # Loop n intervals
  for ((i=1; i<=$max_intervals; i++))
  do
      new_max_steps=$((current_max_steps + i*interval_max_steps))

      ##Replace max_step value
      sed -i "s/    max_steps: [0-9]*/    max_steps: $new_max_steps/g" "$file_path"
      
      echo -e "\nInterval $i - max_steps: $new_max_steps - id=$1 \n"
      mlagents-learn $file_path --run-id=$1 --env=Build/MLAgentBlock --resume

      if [ $i -ne $max_intervals ]; then
        echo -e "\nSleep for a while!"
        sleep $time_sleep
      fi
  done
else
  echo "Directory $folder not exists. Please check again for run incremental!"
fi
 
